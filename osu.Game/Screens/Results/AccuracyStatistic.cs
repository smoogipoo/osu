// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Graphics;
using osu.Game.Graphics;
using osu.Game.Graphics.UserInterface;
using osu.Game.Utils;

namespace osu.Game.Screens.Results
{
    public class AccuracyStatistic : StatisticDisplay
    {
        private readonly double accuracy;

        private RollingCounter<double> counter;

        public AccuracyStatistic(double accuracy)
            : base("accuracy")
        {
            this.accuracy = accuracy;
        }

        public override void Appear()
        {
            base.Appear();
            counter.Current.Value = accuracy;
        }

        protected override Drawable CreateContent() => counter = new Counter();

        private class Counter : RollingCounter<double>
        {
            protected override double RollingDuration => 3000;

            protected override Easing RollingEasing => Easing.OutPow10;

            public Counter()
            {
                DisplayedCountSpriteText.Font = OsuFont.Torus.With(size: 20);
            }

            protected override string FormatCount(double count) => count.FormatAccuracy();

            public override void Increment(double amount)
                => Current.Value += amount;
        }
    }
}
