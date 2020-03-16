// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Graphics;
using osu.Game.Graphics;
using osu.Game.Graphics.UserInterface;
using osuTK;

namespace osu.Game.Screens.Results
{
    public class TotalScoreCounter : RollingCounter<long>
    {
        protected override double RollingDuration => 3000;

        protected override Easing RollingEasing => Easing.OutPow10;

        public TotalScoreCounter()
        {
            // Todo: AutoSize X removed here due to https://github.com/ppy/osu-framework/issues/3369
            AutoSizeAxes = Axes.Y;
            RelativeSizeAxes = Axes.X;
            DisplayedCountSpriteText.Anchor = Anchor.TopCentre;
            DisplayedCountSpriteText.Origin = Anchor.TopCentre;

            DisplayedCountSpriteText.Font = OsuFont.Torus.With(size: 60, weight: FontWeight.Light, fixedWidth: true);
            DisplayedCountSpriteText.Spacing = new Vector2(-5, 0);
        }

        protected override string FormatCount(long count) => count.ToString("N0");

        public override void Increment(long amount)
            => Current.Value += amount;
    }
}
