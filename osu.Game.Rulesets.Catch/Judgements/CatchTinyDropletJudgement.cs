// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Game.Rulesets.Scoring;

namespace osu.Game.Rulesets.Catch.Judgements
{
    public class CatchTinyDropletJudgement : CatchJudgement
    {
        public override bool AffectsCombo => false;

        public override HitResult MaxResult => HitResult.SmallTickHit;

        public override HitResult MinResult => HitResult.SmallTickMiss;

        protected override double HealthIncreaseFor(HitResult result)
        {
            switch (result)
            {
                default:
                    return 0;

                case HitResult.Perfect:
                    return 0.02;
            }
        }
    }
}
