// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osu.Game.Online.API.Requests.Responses;
using osu.Game.Screens.OnlinePlay.Matchmaking.RankedPlay;
using osu.Game.Screens.OnlinePlay.Matchmaking.RankedPlay.Components;
using osu.Game.Users.Drawables;
using osuTK;

namespace osu.Game.Arcade.Screens
{
    public class ArcadeLeaderboardScreenUserDisplay : CompositeDrawable
    {
        private readonly APIUser user;
        private readonly Anchor contentAnchor;
        private readonly RankedPlayColourScheme colourScheme;

        [Resolved]
        private RankedPlayCornerPiece? cornerPiece { get; set; }

        public ArcadeLeaderboardScreenUserDisplay(APIUser user, Anchor contentAnchor, RankedPlayColourScheme colourScheme)
        {
            this.user = user;
            this.contentAnchor = contentAnchor;
            this.colourScheme = colourScheme;
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            InternalChildren =
            [
                new CircularContainer
                {
                    Name = "Avatar",
                    Size = new Vector2(72),
                    Masking = true,
                    Anchor = contentAnchor,
                    Origin = contentAnchor,
                    Children =
                    [
                        new Box
                        {
                            RelativeSizeAxes = Axes.Both,
                            Colour = colourScheme.Surface,
                            Alpha = 0.5f,
                        },
                        new UpdateableAvatar(user, isInteractive: !OsuGame.ARCADE)
                        {
                            RelativeSizeAxes = Axes.Both,
                        }
                    ]
                },
                new FillFlowContainer
                {
                    RelativeSizeAxes = Axes.Both,
                    Padding = (contentAnchor & Anchor.x0) != 0
                        ? new MarginPadding
                        {
                            Left = 82,
                            Top = 15
                        }
                        : new MarginPadding
                        {
                            Right = 82,
                            Top = 15
                        },
                    Direction = FillDirection.Vertical,
                    Children =
                    [
                        new OsuSpriteText
                        {
                            Name = "Username",
                            Text = user.Username,
                            Anchor = contentAnchor,
                            Origin = contentAnchor,
                            Font = OsuFont.GetFont(size: 24, weight: FontWeight.SemiBold),
                            UseFullGlyphHeight = false,
                        }
                    ]
                }
            ];
        }
    }
}
