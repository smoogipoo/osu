// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Threading.Tasks;
using osu.Framework.Allocation;
using osu.Framework.Extensions;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Colour;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osu.Game.Database;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osu.Game.Online.API.Requests.Responses;
using osu.Game.Online.Matchmaking;
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

        [Resolved]
        private UserLookupCache userLookupCache { get; set; } = null!;

        private readonly MatchmakingMatchResult result;

        public MatchResultPanel(MatchmakingMatchResult result)
        {
            this.result = result;

            Width = 300;
            AutoSizeAxes = Axes.Y;

            Masking = true;
            CornerRadius = 10;
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            Task<APIUser?> user1 = userLookupCache.GetUserAsync(result.Scores[0].UserID);
            Task<APIUser?> user2 = userLookupCache.GetUserAsync(result.Scores[1].UserID);
            Task.WhenAll(user1, user2).WaitSafely();

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
                                        User = user1.GetResultSafely()
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
                                        User = user2.GetResultSafely()
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
                                                    Child = new UpdateableAvatar(user1.GetResultSafely())
                                                    {
                                                        RelativeSizeAxes = Axes.Both
                                                    }
                                                },
                                                new OsuSpriteText
                                                {
                                                    Anchor = Anchor.CentreLeft,
                                                    Origin = Anchor.CentreLeft,
                                                    Text = user1.GetResultSafely()?.Username ?? "Unknown",
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
                                                    Child = new UpdateableAvatar(user2.GetResultSafely())
                                                    {
                                                        RelativeSizeAxes = Axes.Both
                                                    }
                                                },
                                                new OsuSpriteText
                                                {
                                                    Anchor = Anchor.CentreRight,
                                                    Origin = Anchor.CentreRight,
                                                    Text = user2.GetResultSafely()?.Username ?? "Unknown",
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
                                    Child = new FillFlowContainer
                                    {
                                        RelativeSizeAxes = Axes.X,
                                        AutoSizeAxes = Axes.Y,
                                        Direction = FillDirection.Vertical,
                                        Spacing = new Vector2(5),
                                        Children = new Drawable[]
                                        {
                                            new Container
                                            {
                                                RelativeSizeAxes = Axes.X,
                                                AutoSizeAxes = Axes.Y,
                                                Children = new Drawable[]
                                                {
                                                    new SpriteIcon
                                                    {
                                                        Anchor = Anchor.Centre,
                                                        Origin = Anchor.Centre,
                                                        Size = new Vector2(12),
                                                        Icon = FontAwesome.Solid.Heart,
                                                        Colour = Color4.Red
                                                    },
                                                    new OsuSpriteText
                                                    {
                                                        Anchor = Anchor.Centre,
                                                        Origin = Anchor.CentreRight,
                                                        X = -20,
                                                        Text = result.Scores[0].Life.ToString("N0"),
                                                        UseFullGlyphHeight = false,
                                                        Colour = result.Scores[0].Life > result.Scores[1].Life ? Color4.White : colourProvider.Foreground1,
                                                        Font = OsuFont.GetFont(weight: result.Scores[0].Life > result.Scores[1].Life ? FontWeight.SemiBold : FontWeight.Regular)
                                                    },
                                                    new OsuSpriteText
                                                    {
                                                        Anchor = Anchor.Centre,
                                                        Origin = Anchor.CentreLeft,
                                                        X = 20,
                                                        Text = result.Scores[1].Life.ToString("N0"),
                                                        UseFullGlyphHeight = false,
                                                        Colour = result.Scores[1].Life > result.Scores[0].Life ? Color4.White : colourProvider.Foreground1,
                                                        Font = OsuFont.GetFont(weight: result.Scores[1].Life > result.Scores[0].Life ? FontWeight.SemiBold : FontWeight.Regular)
                                                    }
                                                },
                                            },
                                            new Container
                                            {
                                                RelativeSizeAxes = Axes.X,
                                                AutoSizeAxes = Axes.Y,
                                                Colour = colourProvider.Foreground1,
                                                Children = new Drawable[]
                                                {
                                                    new SpriteIcon
                                                    {
                                                        Anchor = Anchor.Centre,
                                                        Origin = Anchor.Centre,
                                                        Size = new Vector2(10),
                                                        Icon = FontAwesome.Solid.Skull
                                                    },
                                                    new OsuSpriteText
                                                    {
                                                        Anchor = Anchor.Centre,
                                                        Origin = Anchor.CentreRight,
                                                        X = -20,
                                                        Text = result.Scores[0].Score.ToString(),
                                                        UseFullGlyphHeight = false,
                                                    },
                                                    new OsuSpriteText
                                                    {
                                                        Anchor = Anchor.Centre,
                                                        Origin = Anchor.CentreLeft,
                                                        X = 20,
                                                        Text = result.Scores[1].Score.ToString(),
                                                        UseFullGlyphHeight = false,
                                                    }
                                                },
                                            }
                                        }
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
