// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Game.Graphics;
using osu.Game.Rulesets.Osu.Objects;
using osu.Game.Rulesets.Osu.Objects.Drawables;
using osu.Game.Rulesets.Osu.Objects.Drawables.Pieces;

namespace osu.Game.Rulesets.Osu.Edit.Layers.Selection.Overlays
{
    public class SliderCircleOverlay : OsuHitObjectOverlay
    {
        /// <summary>
        /// Invoked when this <see cref="SliderCircleOverlay"/> wants the parent <see cref="SliderOverlay"/>
        /// to update its position.
        /// </summary>
        public Action RequestPositionUpdate;

        public SliderCircleOverlay(DrawableHitCircle sliderHead, DrawableSlider slider)
            : this(sliderHead, slider, true)
        {
        }

        public SliderCircleOverlay(DrawableSliderTail sliderTail, DrawableSlider slider)
            : this(sliderTail, slider, false)
        {
        }

        private readonly Slider sliderObject;
        private readonly bool isHead;

        private SliderCircleOverlay(DrawableOsuHitObject hitObject, DrawableSlider slider, bool isHead)
            : base(hitObject)
        {
            this.isHead = isHead;

            sliderObject = (Slider)slider.HitObject;

            Origin = Anchor.Centre;

            Size = slider.HeadCircle.Size;
            Scale = slider.HeadCircle.Scale;

            AddInternal(new RingPiece());
        }

        protected override void UpdatePosition()
        {
            RequestPositionUpdate?.Invoke();

            if (isHead)
                Position = sliderObject.HeadCircle.StackedPosition;
            else
                Position = sliderObject.TailCircle.StackedPosition;
        }

        [BackgroundDependencyLoader]
        private void load(OsuColour colours)
        {
            Colour = colours.Yellow;
        }
    }
}
