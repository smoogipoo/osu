﻿// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Game.Rulesets.Judgements;
using osu.Game.Rulesets.Scoring;

namespace osu.Game.Rulesets.Osu.Judgements
{
    public class OsuJudgement : Judgement
    {
        public const int SMALL_TICK_RESULT = 10;
        public const int SMALL_BONUS_RESULT = 10;
        public const int LARGE_BONUS_RESULT = 50;

        public override HitResult MaxResult => HitResult.Great;

        protected override int NumericResultFor(HitResult result)
        {
            switch (result)
            {
                default:
                    return 0;

                case HitResult.Meh:
                    return 50;

                case HitResult.Good:
                    return 100;

                case HitResult.Great:
                    return 300;
            }
        }
    }
}
