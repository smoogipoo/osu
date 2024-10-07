// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using osu.Game.Rulesets.Mods;

namespace osu.Game.Rulesets.Difficulty
{
    public class GameplayClock
    {
        private readonly IApplicableToRate[] mods;

        public GameplayClock(Mod[] mods)
        {
            this.mods = mods.OfType<IApplicableToRate>().ToArray();
        }

        public double RateAt(double time)
        {
            double rate = 1;

            foreach (var m in mods)
                rate = m.ApplyToRate(time, rate);

            return rate;
        }

        public double RateAdjustedTimeAt(double time)
            => time / RateAt(time);
    }
}
