// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System.Collections.Generic;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Rulesets.Osu.Objects;

namespace osu.Game.Rulesets.Osu.Edit.Masks.SliderMasks.Components
{
    public class PathControlPointVisualiser : CompositeDrawable
    {
        private readonly Slider slider;

        private readonly Container<PathControlPointPiece> pieces;

        public PathControlPointVisualiser(Slider slider)
        {
            this.slider = slider;

            InternalChild = pieces = new Container<PathControlPointPiece> { RelativeSizeAxes = Axes.Both };

            slider.PathChanged += _ => updatePathControlPoints();
            updatePathControlPoints();
        }

        private readonly List<PathControlPointDescriptor> pathDescriptors = new List<PathControlPointDescriptor>();

        private void updatePathControlPoints()
        {
            // Convert the segments/control points from jagged arrays into a contiguous list
            int currentDescriptor = 0;
            for (int s = 0; s < slider.Path.Segments.Length; s++)
            {
                var segment = slider.Path.Segments[s];
                for (int c = 0; c < segment.ControlPoints.Length; c++)
                {
                    if (currentDescriptor >= pathDescriptors.Count)
                        pathDescriptors.Add(new PathControlPointDescriptor());

                    pathDescriptors[currentDescriptor].SegmentIndex = s;
                    pathDescriptors[currentDescriptor].IndexInSegment = c;
                    pathDescriptors[currentDescriptor].ControlPoint = segment.ControlPoints[c];

                    currentDescriptor++;
                }
            }

            while (pathDescriptors.Count > pieces.Count)
                pieces.Add(new PathControlPointPiece(slider, pieces.Count, pathDescriptors));
            while (pathDescriptors.Count < pieces.Count)
                pieces.Remove(pieces[pieces.Count - 1]);
        }
    }
}
