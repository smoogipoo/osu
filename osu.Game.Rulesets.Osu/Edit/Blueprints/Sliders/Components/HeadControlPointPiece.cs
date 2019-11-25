// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Input.Events;
using osu.Game.Graphics;
using osu.Game.Rulesets.Edit;
using osu.Game.Rulesets.Osu.Objects;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Rulesets.Osu.Edit.Blueprints.Sliders.Components
{
    public class HeadControlPointPiece : PathControlPointPiece
    {
        [Resolved(CanBeNull = true)]
        private IDistanceSnapProvider snapProvider { get; set; }

        [Resolved]
        private OsuColour colours { get; set; }

        public HeadControlPointPiece(Slider slider, bool allowSelection)
            : base(slider, allowSelection)
        {
        }

        protected override Vector2 GetPosition() => Slider.Position;

        protected override Color4 GetColour() => colours.Yellow;

        protected override bool OnDrag(DragEvent e)
        {
            // Special handling for the head control point - the position of the slider changes which means the snapped position and time have to be taken into account
            (Vector2 snappedPosition, double snappedTime) = snapProvider?.GetSnappedPosition(e.MousePosition, Slider.StartTime) ?? (e.MousePosition, Slider.StartTime);
            Vector2 movementDelta = snappedPosition - Slider.Position;

            Slider.Position += movementDelta;
            Slider.StartTime = snappedTime;

            for (int s = 0; s < Slider.Path.Segments.Length; s++)
            {
                var controlPoints = GetControlPoints(s).ToArray();

                // Since control points are relative to the position of the slider, they all need to be offset backwards by the delta
                for (int i = 0; i < controlPoints.Length; i++)
                    controlPoints[i] -= movementDelta;

                SetControlPoints(s, controlPoints);
            }

            return true;
        }
    }
}
