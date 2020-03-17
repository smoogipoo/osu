// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Colour;
using osu.Framework.Graphics.Containers;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osu.Game.Screens.Results.Expanded.Accuracy;
using osuTK;

namespace osu.Game.Screens.Results.Expanded.Statistics
{
    public class ComboStatistic : CounterStatistic
    {
        private readonly bool isPerfect;

        private Drawable perfectText;

        public ComboStatistic(int combo, bool isPerfect)
            : base("combo", combo)
        {
            this.isPerfect = isPerfect;
        }

        public override void Appear()
        {
            base.Appear();

            if (isPerfect)
            {
                using (BeginDelayedSequence(AccuracyCircle.ACCURACY_TRANSFORM_DURATION / 2, true))
                    perfectText.FadeIn(50);
            }
        }

        protected override Drawable CreateContent()
        {
            return new FillFlowContainer
            {
                AutoSizeAxes = Axes.Both,
                Direction = FillDirection.Horizontal,
                Spacing = new Vector2(10, 0),
                Children = new[]
                {
                    base.CreateContent().With(d =>
                    {
                        Anchor = Anchor.CentreLeft;
                        Origin = Anchor.CentreLeft;
                    }),
                    perfectText = new OsuSpriteText
                    {
                        Anchor = Anchor.CentreLeft,
                        Origin = Anchor.CentreLeft,
                        Text = "PERFECT",
                        Font = OsuFont.Torus.With(size: 11, weight: FontWeight.SemiBold),
                        Colour = ColourInfo.GradientVertical(Color4Extensions.FromHex("#66FFCC"), Color4Extensions.FromHex("#FF9AD7")),
                        Alpha = 0,
                        UseFullGlyphHeight = false,
                    }
                }
            };
        }
    }
}
