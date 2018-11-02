// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System.Collections.Generic;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Lines;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Input.Events;
using osu.Framework.Threading;
using osu.Game.Graphics;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Osu.Objects;
using OpenTK;

namespace osu.Game.Rulesets.Osu.Edit.Masks.SliderMasks.Components
{
    public class PathControlPointPiece : CompositeDrawable
    {
        private readonly Slider slider;
        private readonly int index;
        private readonly List<PathControlPointDescriptor> descriptors;

        private readonly Path path;
        private readonly CircularContainer marker;

        [Resolved]
        private OsuColour colours { get; set; }

        public PathControlPointPiece(Slider slider, int index, List<PathControlPointDescriptor> descriptors)
        {
            this.slider = slider;
            this.index = index;
            this.descriptors = descriptors;

            Origin = Anchor.Centre;
            AutoSizeAxes = Axes.Both;

            InternalChildren = new Drawable[]
            {
                path = new SmoothPath
                {
                    Anchor = Anchor.Centre,
                    PathWidth = 1
                },
                marker = new CircularContainer
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    Size = new Vector2(10),
                    Masking = true,
                    Child = new Box { RelativeSizeAxes = Axes.Both }
                }
            };
        }

        protected override void Update()
        {
            base.Update();

            Position = slider.StackedPosition + descriptors[index].ControlPoint;

            marker.Colour = isSegmentSeparator ? colours.Red : colours.Yellow;

            path.ClearVertices();

            if (index != descriptors.Count - 1)
            {
                path.AddVertex(Vector2.Zero);
                path.AddVertex(descriptors[index + 1].ControlPoint - descriptors[index].ControlPoint);
            }

            path.OriginPosition = path.PositionInBoundingBox(Vector2.Zero);
        }

        public override bool ReceivePositionalInputAt(Vector2 screenSpacePos) => marker.ReceivePositionalInputAt(screenSpacePos);

        protected override bool OnDragStart(DragStartEvent e) => true;

        private ScheduledDelegate scheduledUpdate;

        protected override bool OnDrag(DragEvent e)
        {
            scheduledUpdate?.Cancel();
            scheduledUpdate = Schedule(() =>
            {
                var newSegments = new SliderSegment[slider.Path.Segments.Length];
                for (int i = 0; i < newSegments.Length; i++)
                    newSegments[i] = new SliderSegment(slider.Path.Segments[i].Type, slider.Path.Segments[i].ControlPoints.ToArray());

                var current = descriptors[index];

                if (index == 0)
                {
                    // Special handling for the head - only the position of the slider changes
                    slider.Position += e.Delta;

                    // Since control points are relative to the position of the slider, they all need to be offset backwards by the delta
                    // The first point is positioned at the head of the slider, so it must not change

                    int s = 0;
                    int p = 1;

                    while (true)
                    {
                        if (p >= newSegments[s].ControlPoints.Length)
                        {
                            s++;
                            p = 0;
                        }

                        if (s >= newSegments.Length)
                            break;
                        newSegments[s].ControlPoints[p++] -= e.Delta;
                    }
                }
                else
                    newSegments[current.SegmentIndex].ControlPoints[current.IndexInSegment] += e.Delta;

                // Copy to the first point in the next segment
                if (isSegmentSeparatorWithNext)
                {
                    var next = descriptors[index + 1];
                    newSegments[next.SegmentIndex].ControlPoints[next.IndexInSegment] = newSegments[current.SegmentIndex].ControlPoints[current.IndexInSegment];
                }

                // Copy to the last point in the previous segment
                if (isSegmentSeparatorWithPrevious)
                {
                    var prev = descriptors[index - 1];
                    newSegments[prev.SegmentIndex].ControlPoints[prev.IndexInSegment] = newSegments[current.SegmentIndex].ControlPoints[current.IndexInSegment];
                }

                slider.Path = new SliderPath(newSegments);
            });

            return true;
        }

        protected override bool OnDragEnd(DragEndEvent e) => true;

        private bool isSegmentSeparator => isSegmentSeparatorWithNext || isSegmentSeparatorWithPrevious;

        private bool isSegmentSeparatorWithNext => index < descriptors.Count - 1 && descriptors[index + 1].ControlPoint == descriptors[index].ControlPoint;

        private bool isSegmentSeparatorWithPrevious => index > 0 && descriptors[index - 1].ControlPoint == descriptors[index].ControlPoint;
    }
}
