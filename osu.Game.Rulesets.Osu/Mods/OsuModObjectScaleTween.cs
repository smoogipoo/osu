// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Objects.Drawables;
using osu.Game.Rulesets.Osu.Objects;
using osu.Game.Rulesets.Osu.Objects.Drawables;

namespace osu.Game.Rulesets.Osu.Mods
{
    /// <summary>
    /// Adjusts the size of hit objects during their fade in animation.
    /// </summary>
    public abstract class OsuModObjectScaleTween : ModWithVisibilityAdjustment
    {
        public override ModType Type => ModType.Fun;

        public override double ScoreMultiplier => 1;

        public abstract BindableNumber<float> StartScale { get; }

        protected virtual float EndScale => 1;

        public override Type[] IncompatibleMods => new[] { typeof(OsuModSpinIn), typeof(OsuModTraceable) };

        protected override void ApplyNormalVisibilityState(DrawableHitObject hitObject, ArmedState state)
        {
            base.ApplyNormalVisibilityState(hitObject, state);

            if (hitObject is DrawableSpinner)
                return;

            var h = (OsuHitObject)hitObject.HitObject;

            // apply grow effect
            switch (hitObject)
            {
                case DrawableSliderHead _:
                case DrawableSliderTail _:
                    // special cases we should *not* be scaling.
                    break;

                case DrawableSlider _:
                case DrawableHitCircle _:
                {
                    using (hitObject.BeginAbsoluteSequence(h.StartTime - h.TimePreempt))
                        hitObject.ScaleTo(StartScale.Value).Then().ScaleTo(EndScale, h.TimePreempt, Easing.OutSine);
                    break;
                }
            }

            // remove approach circles
            switch (hitObject)
            {
                case DrawableHitCircle circle:
                    // we don't want to see the approach circle
                    using (circle.BeginAbsoluteSequence(h.StartTime - h.TimePreempt))
                        circle.ApproachCircle.Hide();
                    break;
            }
        }
    }
}
