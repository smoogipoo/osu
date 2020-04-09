// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Globalization;
using osu.Game.Rulesets.Judgements;
using osu.Game.Rulesets.Osu.Judgements;

namespace osu.Game.Rulesets.Osu.Objects
{
    public class HitCircle : OsuHitObject
    {
        public override Judgement CreateJudgement() => new OsuJudgement();

        public override string ToString() => StartTime.ToString(CultureInfo.InvariantCulture);
    }
}
