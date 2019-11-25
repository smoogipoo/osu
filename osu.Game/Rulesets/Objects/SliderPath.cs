// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using osu.Framework.Bindables;
using osu.Framework.Caching;
using osu.Framework.MathUtils;
using osu.Game.Rulesets.Objects.Types;
using osuTK;

namespace osu.Game.Rulesets.Objects
{
    public class SliderPath
    {
        private readonly Bindable<int> version = new Bindable<int>();
        public IBindable<int> Version => version;

        private double? expectedDistance;

        /// <summary>
        /// The distance to lengthen or shorten the path to. If null, the path will use its true distance.
        /// </summary>
        public double? ExpectedDistance
        {
            get => expectedDistance;
            set
            {
                expectedDistance = value;

                pathCache.Invalidate();

                version.Value++;
            }
        }

        private PathType type;

        /// <summary>
        /// The type of the path.
        /// </summary>
        public PathType Type
        {
            get => type;
            set
            {
                type = value;

                pathCache.Invalidate();

                version.Value++;
            }
        }

        private ReadOnlyMemory<Vector2> controlPoints;

        /// <summary>
        /// The control points of the path.
        /// </summary>
        public ReadOnlyMemory<Vector2> ControlPoints
        {
            get => controlPoints;
            set
            {
                controlPoints = value;

                pathCache.Invalidate();

                version.Value++;
            }
        }

        private readonly Cached pathCache = new Cached();

        private List<Vector2> calculatedPath;
        private List<double> cumulativeLength;

        /// <summary>
        /// The distance of the path after lengthening/shortening to account for <see cref="ExpectedDistance"/>.
        /// </summary>
        [JsonIgnore]
        public double Distance
        {
            get
            {
                ensureValid();
                return cumulativeLength.Count == 0 ? 0 : cumulativeLength[cumulativeLength.Count - 1];
            }
        }

        /// <summary>
        /// Computes the slider path until a given progress that ranges from 0 (beginning of the slider)
        /// to 1 (end of the slider) and stores the generated path in the given list.
        /// </summary>
        /// <param name="path">The list to be filled with the computed path.</param>
        /// <param name="p0">Start progress. Ranges from 0 (beginning of the slider) to 1 (end of the slider).</param>
        /// <param name="p1">End progress. Ranges from 0 (beginning of the slider) to 1 (end of the slider).</param>
        public void GetPathToProgress(List<Vector2> path, double p0, double p1)
        {
            ensureValid();

            double d0 = progressToDistance(p0);
            double d1 = progressToDistance(p1);

            path.Clear();

            int i = 0;

            for (; i < calculatedPath.Count && cumulativeLength[i] < d0; ++i)
            {
            }

            path.Add(interpolateVertices(i, d0));

            for (; i < calculatedPath.Count && cumulativeLength[i] <= d1; ++i)
                path.Add(calculatedPath[i]);

            path.Add(interpolateVertices(i, d1));
        }

        /// <summary>
        /// Computes the position on the slider at a given progress that ranges from 0 (beginning of the path)
        /// to 1 (end of the path).
        /// </summary>
        /// <param name="progress">Ranges from 0 (beginning of the path) to 1 (end of the path).</param>
        /// <returns></returns>
        public Vector2 PositionAt(double progress)
        {
            ensureValid();

            double d = progressToDistance(progress);
            return interpolateVertices(indexOfDistance(d), d);
        }

        private void ensureValid()
        {
            if (pathCache.IsValid)
                return;

            calculatedPath = new List<Vector2>();
            cumulativeLength = new List<double>();

            calculatePath();
            calculateCumulativeLength();

            pathCache.Validate();
        }

        private List<Vector2> calculateSubpath(ReadOnlySpan<Vector2> subControlPoints)
        {
            switch (Type)
            {
                case PathType.Linear:
                    return PathApproximator.ApproximateLinear(subControlPoints);

                case PathType.PerfectCurve:
                    //we can only use CircularArc iff we have exactly three control points and no dissection.
                    if (ControlPoints.Length != 3 || subControlPoints.Length != 3)
                        break;

                    // Here we have exactly 3 control points. Attempt to fit a circular arc.
                    List<Vector2> subpath = PathApproximator.ApproximateCircularArc(subControlPoints);

                    // If for some reason a circular arc could not be fit to the 3 given points, fall back to a numerically stable bezier approximation.
                    if (subpath.Count == 0)
                        break;

                    return subpath;

                case PathType.Catmull:
                    return PathApproximator.ApproximateCatmull(subControlPoints);
            }

            return PathApproximator.ApproximateBezier(subControlPoints);
        }

        private void calculatePath()
        {
            calculatedPath.Clear();

            // Sliders may consist of various subpaths separated by two consecutive vertices
            // with the same position. The following loop parses these subpaths and computes
            // their shape independently, consecutively appending them to calculatedPath.

            int start = 0;
            int end = 0;

            for (int i = 0; i < ControlPoints.Length; ++i)
            {
                end++;

                if (i == ControlPoints.Length - 1 || ControlPoints.Span[i] == ControlPoints.Span[i + 1])
                {
                    ReadOnlySpan<Vector2> cpSpan = ControlPoints.Span.Slice(start, end - start);

                    foreach (Vector2 t in calculateSubpath(cpSpan))
                    {
                        if (calculatedPath.Count == 0 || calculatedPath.Last() != t)
                            calculatedPath.Add(t);
                    }

                    start = end;
                }
            }
        }

        private void calculateCumulativeLength()
        {
            double l = 0;

            cumulativeLength.Clear();
            cumulativeLength.Add(l);

            for (int i = 0; i < calculatedPath.Count - 1; ++i)
            {
                Vector2 diff = calculatedPath[i + 1] - calculatedPath[i];
                double d = diff.Length;

                // Shorted slider paths that are too long compared to the expected distance
                if (ExpectedDistance.HasValue && ExpectedDistance - l < d)
                {
                    calculatedPath[i + 1] = calculatedPath[i] + diff * (float)((ExpectedDistance - l) / d);
                    calculatedPath.RemoveRange(i + 2, calculatedPath.Count - 2 - i);

                    l = ExpectedDistance.Value;
                    cumulativeLength.Add(l);
                    break;
                }

                l += d;
                cumulativeLength.Add(l);
            }

            // Lengthen slider paths that are too short compared to the expected distance
            if (ExpectedDistance.HasValue && l < ExpectedDistance && calculatedPath.Count > 1)
            {
                Vector2 diff = calculatedPath[calculatedPath.Count - 1] - calculatedPath[calculatedPath.Count - 2];
                double d = diff.Length;

                if (d <= 0)
                    return;

                calculatedPath[calculatedPath.Count - 1] += diff * (float)((ExpectedDistance - l) / d);
                cumulativeLength[calculatedPath.Count - 1] = ExpectedDistance.Value;
            }
        }

        private int indexOfDistance(double d)
        {
            int i = cumulativeLength.BinarySearch(d);
            if (i < 0) i = ~i;

            return i;
        }

        private double progressToDistance(double progress)
        {
            return Math.Clamp(progress, 0, 1) * Distance;
        }

        private Vector2 interpolateVertices(int i, double d)
        {
            if (calculatedPath.Count == 0)
                return Vector2.Zero;

            if (i <= 0)
                return calculatedPath.First();
            if (i >= calculatedPath.Count)
                return calculatedPath.Last();

            Vector2 p0 = calculatedPath[i - 1];
            Vector2 p1 = calculatedPath[i];

            double d0 = cumulativeLength[i - 1];
            double d1 = cumulativeLength[i];

            // Avoid division by and almost-zero number in case two points are extremely close to each other.
            if (Precision.AlmostEquals(d0, d1))
                return p0;

            double w = (d - d0) / (d1 - d0);
            return p0 + (p1 - p0) * (float)w;
        }
    }
}
