// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Game.Rulesets.Judgements;
using osu.Game.Rulesets.Scoring;

namespace osu.Game.Rulesets.Osu.Judgements
{
    public class OsuJudgement : Judgement
    {
        public override HitResult MaxResult => HitResult.Great;

        protected override double HealthIncreaseFor(HitResult result)
        {
            switch (result)
            {
                case HitResult.Ok:
                    return DEFAULT_MAX_HEALTH_INCREASE * 0.5;

                default:
                    return base.HealthIncreaseFor(result);
            }
        }
    }
}
