// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Game.Graphics;
using osu.Game.Rulesets.Osu.Objects;
using osu.Game.Rulesets.Osu.Objects.Drawables;
using osu.Game.Rulesets.Osu.Objects.Drawables.Pieces;
using OpenTK.Graphics;

namespace osu.Game.Rulesets.Osu.Edit.Layers.Selection.Overlays
{
    public class SliderOverlay : OsuHitObjectOverlay
    {
        private readonly Slider sliderObject;
        private readonly SliderBody body;

        public SliderOverlay(DrawableSlider slider)
            : base(slider)
        {
            sliderObject = (Slider)slider.HitObject;

            var obj = (Slider)slider.HitObject;

            InternalChildren = new Drawable[]
            {
                body = new SliderBody(obj)
                {
                    AccentColour = Color4.Transparent,
                    Position = obj.StackedPosition,
                    PathWidth = obj.Scale * 64
                },
                new SliderCircleOverlay(slider.HeadCircle, slider) { RequestPositionUpdate = () => positionUpdateRequested(true) },
                new SliderCircleOverlay(slider.TailCircle, slider) { RequestPositionUpdate = () => positionUpdateRequested(false) },
            };
        }

        [BackgroundDependencyLoader]
        private void load(OsuColour colours)
        {
            body.BorderColour = colours.Yellow;
        }

        private void positionUpdateRequested(bool fromHead)
        {
            if (fromHead)
                sliderObject.Position = sliderObject.HeadCircle.Position;
            else
            {
                var offset = sliderObject.Position - sliderObject.EndPosition;
                sliderObject.Position = sliderObject.TailCircle.Position + offset;
            }
        }

        protected override void UpdatePosition()
        {
            body.Position = sliderObject.StackedPosition;

            sliderObject.HeadCircle.Position = sliderObject.Position;
            sliderObject.TailCircle.Position = sliderObject.EndPosition;
        }

        protected override void Update()
        {
            base.Update();

            // Need to cause one update
            body.UpdateProgress(0);
        }
    }
}
