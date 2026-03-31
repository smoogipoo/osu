// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Colour;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osu.Game.Online.API.Requests.Responses;
using osu.Game.Overlays;
using osu.Game.Screens.OnlinePlay.Lounge.Components;
using osu.Game.Users;
using osu.Game.Users.Drawables;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Screens.OnlinePlay.Matchmaking.Queue
{
    public class MatchResultPanel : CompositeDrawable
    {
        [Resolved]
        private OverlayColourProvider colourProvider { get; set; } = null!;

        [Resolved]
        private OsuColour colours { get; set; } = null!;

        public MatchResultPanel()
        {
            Width = 300;
            AutoSizeAxes = Axes.Y;

            Masking = true;
            CornerRadius = 10;
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            APIUser user1 = new APIUser
            {
                Id = 2,
                Username = "peppy",
                CoverUrl = "https://assets.ppy.sh/user-cover-presets/1/df28696b58541a9e67f6755918951d542d93bdf1da41720fcca2fd2c1ea8cf51.jpeg",
            };

            APIUser user2 = new APIUser
            {
                Id = 1040328,
                Username = "smoogipoo",
                CoverUrl = "https://assets.ppy.sh/user-cover-presets/7/4a0ccb7b7fdd5c4238b11f0e7c686760fe2c99c6472b19400e82d1a8ff503e31.jpeg",
            };

            InternalChildren = new Drawable[]
            {
                new Box
                {
                    RelativeSizeAxes = Axes.Both,
                    Colour = colourProvider.Background3
                },
                new FillFlowContainer
                {
                    RelativeSizeAxes = Axes.X,
                    AutoSizeAxes = Axes.Y,
                    Children = new Drawable[]
                    {
                        new Container
                        {
                            Name = "Top part",
                            RelativeSizeAxes = Axes.X,
                            AutoSizeAxes = Axes.Y,
                            Children = new Drawable[]
                            {
                                new Box
                                {
                                    RelativeSizeAxes = Axes.Both,
                                    Colour = colourProvider.Background4
                                },
                                new PillContainer
                                {
                                    Margin = new MarginPadding(5),
                                    Background =
                                    {
                                        Colour = colours.YellowDarker,
                                        Alpha = 1
                                    },
                                    Child = new OsuSpriteText
                                    {
                                        Text = "Completed",
                                        Font = OsuFont.GetFont(size: 12, weight: FontWeight.SemiBold),
                                        Colour = Color4.Black
                                    }
                                }
                            }
                        },
                        new Container
                        {
                            Name = "Middle part",
                            RelativeSizeAxes = Axes.X,
                            AutoSizeAxes = Axes.Y,
                            Children = new Drawable[]
                            {
                                new Container
                                {
                                    RelativeSizeAxes = Axes.Both,
                                    Height = 0.5f,
                                    Masking = true,
                                    Colour = ColourInfo.GradientHorizontal(Color4.White.Opacity(0.3f), colourProvider.Background3.Opacity(0)),
                                    Child = new UserCoverBackground
                                    {
                                        RelativeSizeAxes = Axes.Both,
                                        User = user1
                                    }
                                },
                                new Container
                                {
                                    Anchor = Anchor.BottomCentre,
                                    Origin = Anchor.BottomCentre,
                                    RelativeSizeAxes = Axes.Both,
                                    Height = 0.5f,
                                    Masking = true,
                                    Colour = ColourInfo.GradientHorizontal(colourProvider.Background3.Opacity(0), Color4.White.Opacity(0.3f)),
                                    Child = new UserCoverBackground
                                    {
                                        RelativeSizeAxes = Axes.Both,
                                        User = user2
                                    }
                                },
                                new OsuSpriteText
                                {
                                    Anchor = Anchor.Centre,
                                    Origin = Anchor.Centre,
                                    Text = "vs",
                                    Font = OsuFont.GetFont(size: 50, weight: FontWeight.Bold),
                                    UseFullGlyphHeight = false,
                                    Colour = colourProvider.Foreground1,
                                },
                                new FillFlowContainer
                                {
                                    RelativeSizeAxes = Axes.X,
                                    AutoSizeAxes = Axes.Y,
                                    Direction = FillDirection.Vertical,
                                    Children = new Drawable[]
                                    {
                                        new FillFlowContainer
                                        {
                                            RelativeSizeAxes = Axes.X,
                                            AutoSizeAxes = Axes.Y,
                                            Direction = FillDirection.Horizontal,
                                            Padding = new MarginPadding(5),
                                            Spacing = new Vector2(5),
                                            Children = new Drawable[]
                                            {
                                                new CircularContainer
                                                {
                                                    Anchor = Anchor.CentreLeft,
                                                    Origin = Anchor.CentreLeft,
                                                    Size = new Vector2(25),
                                                    Masking = true,
                                                    Child = new UpdateableAvatar(user1)
                                                    {
                                                        RelativeSizeAxes = Axes.Both
                                                    }
                                                },
                                                new OsuSpriteText
                                                {
                                                    Anchor = Anchor.CentreLeft,
                                                    Origin = Anchor.CentreLeft,
                                                    Text = user1.Username,
                                                    Font = OsuFont.GetFont(weight: FontWeight.SemiBold),
                                                    UseFullGlyphHeight = false,
                                                },
                                            }
                                        },
                                        new FillFlowContainer
                                        {
                                            RelativeSizeAxes = Axes.X,
                                            AutoSizeAxes = Axes.Y,
                                            Direction = FillDirection.Horizontal,
                                            Padding = new MarginPadding(5),
                                            Spacing = new Vector2(5),
                                            Children = new Drawable[]
                                            {
                                                new CircularContainer
                                                {
                                                    Anchor = Anchor.CentreRight,
                                                    Origin = Anchor.CentreRight,
                                                    Size = new Vector2(25),
                                                    Masking = true,
                                                    Child = new UpdateableAvatar(user2)
                                                    {
                                                        RelativeSizeAxes = Axes.Both
                                                    }
                                                },
                                                new OsuSpriteText
                                                {
                                                    Anchor = Anchor.CentreRight,
                                                    Origin = Anchor.CentreRight,
                                                    Text = user2.Username,
                                                    Font = OsuFont.GetFont(weight: FontWeight.SemiBold),
                                                    UseFullGlyphHeight = false,
                                                },
                                            }
                                        }
                                    }
                                }
                            }
                        },
                        new Container
                        {
                            Name = "Bottom part",
                            RelativeSizeAxes = Axes.X,
                            AutoSizeAxes = Axes.Y,
                            Children = new Drawable[]
                            {
                                new Box
                                {
                                    RelativeSizeAxes = Axes.Both,
                                    Colour = colourProvider.Background4
                                },
                                new Container
                                {
                                    RelativeSizeAxes = Axes.X,
                                    AutoSizeAxes = Axes.Y,
                                    Padding = new MarginPadding(5),
                                    Child = new OsuSpriteText
                                    {
                                        Anchor = Anchor.Centre,
                                        Origin = Anchor.Centre,
                                        Text = "1 - 1"
                                    }
                                }
                            }
                        }
                    }
                }
            };
        }
    }
}
