// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System.Collections.Generic;
using osu.Game.Rulesets.Objects.Types;
using OpenTK;

namespace osu.Game.Rulesets.Objects
{
    public readonly struct SliderSegment
    {
        public readonly PathType Type;
        public readonly Vector2[] ControlPoints;

        public SliderSegment(PathType type, params Vector2[] controlPoints)
        {
            Type = type;
            ControlPoints = controlPoints;
        }

        public List<Vector2> CalculatePath()
        {
            switch (Type)
            {
                case PathType.Linear:
                    var result = new List<Vector2>(ControlPoints.Length);
                    foreach (var c in ControlPoints)
                        result.Add(c);

                    return result;
                case PathType.PerfectCurve:
                    //we can only use CircularArc iff we have exactly three control points and no dissection.
                    if (ControlPoints.Length != 3)
                        break;

                    // Here we have exactly 3 control points. Attempt to fit a circular arc.
                    List<Vector2> subpath = new CircularArcApproximator(ControlPoints).CreateArc();

                    // If for some reason a circular arc could not be fit to the 3 given points, fall back to a numerically stable bezier approximation.
                    if (subpath.Count == 0)
                        break;

                    return subpath;
                case PathType.Catmull:
                    return new CatmullApproximator(ControlPoints).CreateCatmull();
            }

            return new BezierApproximator(ControlPoints).CreateBezier();
        }
    }
}
