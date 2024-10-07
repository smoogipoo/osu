// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using osu.Game.Beatmaps;
using osu.Game.Rulesets.Mods;

namespace osu.Game.Rulesets.Difficulty
{
    /// <summary>
    /// The context for the current difficulty calculation.
    /// </summary>
    /// <param name="Beatmap">The gameplay beatmap.</param>
    /// <param name="Mods">The gameplay mods.</param>
    public readonly record struct DifficultyCalculationContext(IBeatmap Beatmap, Mod[] Mods)
    {
        private readonly IApplicableToRate[] audioMods = Mods.OfType<IApplicableToRate>().ToArray();

        public double RateAt(double time)
        {
            double rate = 1;

            foreach (var m in audioMods)
                rate = m.ApplyToRate(0, rate); // Todo: Time should not be 0 :)

            return rate;
        }

        public double RateAdjustedTimeAt(double time)
            => time / RateAt(time);
    }
}
