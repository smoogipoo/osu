// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Input.Events;
using osu.Game.Graphics;
using osu.Game.Rulesets.Osu.Objects;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Rulesets.Osu.Edit.Blueprints.Sliders.Components
{
    public class SegmentControlPointPiece : PathControlPointPiece
    {
        public int SegmentIndex;
        public int ControlPointIndex;

        [Resolved]
        private OsuColour colours { get; set; }

        public SegmentControlPointPiece(Slider slider, bool allowSelection)
            : base(slider, allowSelection)
        {
        }

        protected override void Update()
        {
            base.Update();

            updateConnectingPath();
        }

        protected override Vector2 GetPosition() => Slider.StackedPosition + GetControlPoints(SegmentIndex)[ControlPointIndex];

        protected override Color4 GetColour() => isSegmentSeparator ? colours.Red : colours.Yellow;

        protected override bool OnDrag(DragEvent e)
        {
            var controlPoints = GetControlPoints(SegmentIndex).ToArray();
            controlPoints[ControlPointIndex] += e.Delta;

            SetControlPoints(SegmentIndex, controlPoints);
            return true;
        }

        /// <summary>
        /// Updates the path connecting this control point to the previous one.
        /// </summary>
        private void updateConnectingPath()
        {
            Path.ClearVertices();

            Vector2 lastPoint = Vector2.Zero;

            if (ControlPointIndex == 0 && SegmentIndex > 0)
                lastPoint = GetControlPoints(SegmentIndex - 1)[GetControlPoints(SegmentIndex - 1).Length - 1];
            else if (ControlPointIndex > 0)
                lastPoint = GetControlPoints(SegmentIndex)[ControlPointIndex - 1];

            Path.AddVertex(Vector2.Zero);
            Path.AddVertex(lastPoint - Slider.Path.Segments[SegmentIndex].ControlPoints[ControlPointIndex]);

            Path.OriginPosition = Path.PositionInBoundingBox(Vector2.Zero);
        }

        private bool isSegmentSeparator => SegmentIndex < Slider.Path.Segments.Length - 1 && ControlPointIndex == GetControlPoints(SegmentIndex).Length - 1;
    }
}
