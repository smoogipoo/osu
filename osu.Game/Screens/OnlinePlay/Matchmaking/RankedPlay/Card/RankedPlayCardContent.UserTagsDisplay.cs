// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;

namespace osu.Game.Screens.OnlinePlay.Matchmaking.RankedPlay.Card
{
    public partial class RankedPlayCardContent
    {
        public partial class UserTagsDisplay : CompositeDrawable
        {
            public UserTagsDisplay()
            {
                AutoSizeAxes = Axes.Y;
            }

            [BackgroundDependencyLoader]
            private void load(CardColours colours)
            {
                InternalChildren = new Drawable[]
                {
                    new Box
                    {
                        RelativeSizeAxes = Axes.Both,
                        Colour = colours.BackgroundDarker,
                    },
                    new OsuSpriteText
                    {
                        Anchor = Anchor.BottomCentre,
                        Origin = Anchor.BottomCentre,
                        Margin = new MarginPadding { Bottom = 4, Top = 4 },
                        Text = "aim / aim control (+2)",
                        Font = OsuFont.GetFont(size: 9, weight: FontWeight.SemiBold),
                        UseFullGlyphHeight = false,
                        Colour = colours.OnBackground,
                    }
                };
            }
        }
    }
}
