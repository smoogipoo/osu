// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Game.Rulesets.Scoring;

namespace osu.Game.Rulesets.Osu.Objects.Drawables
{
    public class DrawableSpinnerBonusTick : DrawableSpinnerTick
    {
        public DrawableSpinnerBonusTick(SpinnerBonusTick spinnerTick)
            : base(spinnerTick)
        {
        }

        internal override void TriggerResult(bool hit) => ApplyResult(r => r.Type = hit ? r.Judgement.MaxResult : HitResult.LargeBonusMiss);
    }
}
