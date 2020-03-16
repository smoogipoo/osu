// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Graphics;
using osu.Game.Graphics;
using osu.Game.Graphics.UserInterface;

namespace osu.Game.Screens.Results
{
    public class CounterStatistic : StatisticDisplay
    {
        private readonly int count;

        private RollingCounter<int> counter;

        public CounterStatistic(string header, int count)
            : base(header)
        {
            this.count = count;
        }

        public override void Appear()
        {
            base.Appear();
            counter.Current.Value = count;
        }

        protected override Drawable CreateContent() => counter = new Counter();

        private class Counter : RollingCounter<int>
        {
            protected override double RollingDuration => 3000;

            protected override Easing RollingEasing => Easing.OutPow10;

            public Counter()
            {
                DisplayedCountSpriteText.Font = OsuFont.Torus.With(size: 20);
            }

            public override void Increment(int amount)
                => Current.Value += amount;
        }
    }
}
