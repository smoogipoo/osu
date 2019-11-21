// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Diagnostics;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Primitives;
using osu.Framework.Graphics.UserInterface;
using osu.Framework.Input.Events;
using osu.Game.Graphics.UserInterface;
using osu.Game.Rulesets.Edit;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Objects.Types;
using osu.Game.Rulesets.Osu.Edit.Blueprints.Sliders.Components;
using osu.Game.Rulesets.Osu.Objects;
using osu.Game.Rulesets.Osu.Objects.Drawables;
using osuTK;
using osuTK.Input;

namespace osu.Game.Rulesets.Osu.Edit.Blueprints.Sliders
{
    public class SliderSelectionBlueprint : OsuSelectionBlueprint<Slider>
    {
        protected readonly SliderBodyPiece BodyPiece;
        protected readonly SliderCircleSelectionBlueprint HeadBlueprint;
        protected readonly SliderCircleSelectionBlueprint TailBlueprint;
        protected readonly PathControlPointVisualiser ControlPointVisualiser;

        [Resolved(CanBeNull = true)]
        private HitObjectComposer composer { get; set; }

        public SliderSelectionBlueprint(DrawableSlider slider)
            : base(slider)
        {
            var sliderObject = (Slider)slider.HitObject;

            InternalChildren = new Drawable[]
            {
                BodyPiece = new SliderBodyPiece(),
                HeadBlueprint = CreateCircleSelectionBlueprint(slider, SliderPosition.Start),
                TailBlueprint = CreateCircleSelectionBlueprint(slider, SliderPosition.End),
                ControlPointVisualiser = new PathControlPointVisualiser(sliderObject, true)
                {
                    ControlPointsChanged = onNewControlPoints,
                    SegmentsChanged = onNewSegments
                },
            };
        }

        protected override void Update()
        {
            base.Update();

            BodyPiece.UpdateFrom(HitObject);
        }

        private Vector2 rightClickPosition;

        protected override bool OnMouseDown(MouseDownEvent e)
        {
            switch (e.Button)
            {
                case MouseButton.Right:
                    rightClickPosition = e.MouseDownPosition;
                    return false; // Allow right click to be handled by context menu

                case MouseButton.Left when e.ControlPressed && IsSelected:
                    (placementSegmentIndex, placementControlPointIndex) = addControlPoint(e.MousePosition);
                    return true; // Stop input from being handled and modifying the selection
            }

            return false;
        }

        private int? placementSegmentIndex;
        private int? placementControlPointIndex;

        protected override bool OnDragStart(DragStartEvent e) => placementControlPointIndex != null;

        protected override bool OnDrag(DragEvent e)
        {
            Debug.Assert(placementSegmentIndex != null);
            Debug.Assert(placementControlPointIndex != null);

            Vector2 position = e.MousePosition - HitObject.Position;

            var newControlPoints = HitObject.Path.Segments[placementSegmentIndex.Value].ControlPoints.ToArray();
            newControlPoints[placementControlPointIndex.Value] = position;

            onNewControlPoints(placementSegmentIndex.Value, newControlPoints);

            return true;
        }

        protected override bool OnDragEnd(DragEndEvent e)
        {
            placementControlPointIndex = null;
            return true;
        }

        private (int segmentIndex, int controlPointIndex) addControlPoint(Vector2 position)
        {
            position -= HitObject.Position;

            int segmentIndex = 0;
            int insertionIndex = 0;
            float minDistance = float.MaxValue;

            for (int s = 0; s < HitObject.Path.Segments.Length; s++)
            {
                PathSegment segment = HitObject.Path.Segments[s];

                for (int c = 0; c < segment.ControlPoints.Length - 1; c++)
                {
                    float dist = new Line(segment.ControlPoints[c], segment.ControlPoints[c + 1]).DistanceToPoint(position);

                    if (dist < minDistance)
                    {
                        segmentIndex = s;
                        insertionIndex = c + 1;
                        minDistance = dist;
                    }
                }
            }

            var newControlPoints = new Vector2[HitObject.Path.Segments[segmentIndex].ControlPoints.Length + 1];
            HitObject.Path.Segments[segmentIndex].ControlPoints.CopyTo(newControlPoints);

            // Move the control points from the insertion index onwards to make room for the insertion
            Array.Copy(newControlPoints, insertionIndex, newControlPoints, insertionIndex + 1, newControlPoints.Length - insertionIndex - 1);
            newControlPoints[insertionIndex] = position;

            onNewControlPoints(segmentIndex, newControlPoints);

            return (segmentIndex, insertionIndex);
        }

        private void onNewControlPoints(int segmentIndex, Vector2[] controlPoints)
        {
            var newSegments = HitObject.Path.Segments.ToArray();
            newSegments[segmentIndex] = new PathSegment(controlPoints.Length > 1 ? PathType.Bezier : PathType.Linear, controlPoints);

            onNewSegments(newSegments);
        }

        private void onNewSegments(PathSegment[] segments)
        {
            var unsnappedPath = new SliderPath(segments);
            var snappedDistance = composer?.GetSnappedDistanceFromDistance(HitObject.StartTime, (float)unsnappedPath.Distance) ?? (float)unsnappedPath.Distance;

            HitObject.Path = new SliderPath(segments, snappedDistance);

            UpdateHitObject();
        }

        public override MenuItem[] ContextMenuItems => new MenuItem[]
        {
            new OsuMenuItem("Add control point", MenuItemType.Standard, () => addControlPoint(rightClickPosition)),
        };

        public override Vector2 SelectionPoint => HeadBlueprint.SelectionPoint;

        public override bool ReceivePositionalInputAt(Vector2 screenSpacePos) => BodyPiece.ReceivePositionalInputAt(screenSpacePos);

        protected virtual SliderCircleSelectionBlueprint CreateCircleSelectionBlueprint(DrawableSlider slider, SliderPosition position) => new SliderCircleSelectionBlueprint(slider, position);
    }
}
