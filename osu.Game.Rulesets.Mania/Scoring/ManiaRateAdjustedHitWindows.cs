// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using osu.Game.Rulesets.Scoring;

namespace osu.Game.Rulesets.Mania.Scoring
{
    /// <summary>
    /// A type of <see cref="ManiaHitWindows"/> which is used in the context of rate-adjustment mods.
    /// Changing the audio rate lengthens or shortens the hit windows, but osu!mania and other VSRGs historically don't want this side effect.
    /// </summary>
    public class ManiaRateAdjustedHitWindows : ManiaHitWindows
    {
        private readonly double speedChange;

        public ManiaRateAdjustedHitWindows(double speedChange)
        {
            this.speedChange = speedChange;
        }

        protected override DifficultyRange[] GetRanges() => base.GetRanges().Select(r => r * speedChange).ToArray();
    }
}
