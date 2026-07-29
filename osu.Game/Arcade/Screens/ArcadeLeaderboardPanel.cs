// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.Textures;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osu.Game.Utils;
using osuTK;

namespace osu.Game.Arcade.Screens
{
    public class ArcadeLeaderboardPanel : CompositeDrawable
    {
        public const float HEIGHT = 50;

        public readonly int UserId;
        private readonly string username;

        [Resolved]
        private OsuColour colours { get; set; } = null!;

        private OsuSpriteText? rankText;
        private OsuSpriteText? countText;

        public ArcadeLeaderboardPanel(int userId, string username)
        {
            this.UserId = userId;
            this.username = username;

            RelativeSizeAxes = Axes.X;
            Height = HEIGHT;
            Padding = new MarginPadding { Right = 5 };
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            InternalChild = new Container
            {
                RelativeSizeAxes = Axes.Both,
                Masking = true,
                CornerRadius = 5,
                Children = new Drawable[]
                {
                    new Box
                    {
                        RelativeSizeAxes = Axes.Both,
                        Colour = colours.Blue4
                    },
                    new Container
                    {
                        RelativeSizeAxes = Axes.Both,
                        Padding = new MarginPadding(5),
                        Children = new Drawable[]
                        {
                            new FillFlowContainer
                            {
                                Anchor = Anchor.CentreLeft,
                                Origin = Anchor.CentreLeft,
                                AutoSizeAxes = Axes.Both,
                                Direction = FillDirection.Horizontal,
                                Children = new Drawable[]
                                {
                                    rankText = new OsuSpriteText
                                    {
                                        Anchor = Anchor.CentreLeft,
                                        Origin = Anchor.CentreLeft,
                                        Width = 40,
                                        Text = Rank.FormatRank().Insert(0, "#"),
                                        Margin = new MarginPadding { Left = 5 }
                                    },
                                    new DelayedLoadWrapper(new DrawableAvatar(UserId)
                                    {
                                        Size = new Vector2(40)
                                    })
                                    {
                                        Anchor = Anchor.CentreLeft,
                                        Origin = Anchor.CentreLeft,
                                        Size = new Vector2(40)
                                    },
                                    new OsuSpriteText
                                    {
                                        Anchor = Anchor.CentreLeft,
                                        Origin = Anchor.CentreLeft,
                                        Margin = new MarginPadding { Left = 10 },
                                        Text = username
                                    }
                                }
                            },
                            new FillFlowContainer
                            {
                                Anchor = Anchor.CentreRight,
                                Origin = Anchor.CentreRight,
                                AutoSizeAxes = Axes.Both,
                                Direction = FillDirection.Vertical,
                                Margin = new MarginPadding { Right = 10 },
                                Children = new Drawable[]
                                {
                                    countText = new OsuSpriteText
                                    {
                                        Anchor = Anchor.TopCentre,
                                        Origin = Anchor.TopCentre,
                                        Text = Count.ToString(),
                                        Font = OsuFont.GetFont(size: 20, weight: FontWeight.SemiBold)
                                    },
                                    new OsuSpriteText
                                    {
                                        Anchor = Anchor.TopCentre,
                                        Origin = Anchor.TopCentre,
                                        Text = "wins",
                                        Font = OsuFont.GetFont(size: 12)
                                    }
                                }
                            }
                        }
                    }
                }
            };
        }

        private int count;

        public int Count
        {
            get => count;
            set
            {
                count = value;

                if (countText != null)
                    countText.Text = value.ToString();
            }
        }

        private int rank;

        public int Rank
        {
            get => rank;
            set
            {
                rank = value;

                if (rankText != null)
                    rankText.Text = value.FormatRank().Insert(0, "#");
            }
        }

        [LongRunningLoad]
        private partial class DrawableAvatar : Sprite
        {
            private readonly int userId;

            public DrawableAvatar(int userId)
            {
                this.userId = userId;
            }

            [BackgroundDependencyLoader]
            private void load(TextureStore textures)
            {
                Texture = textures.Get($@"https://a.ppy.sh/{userId}")
                          ?? textures.Get(@"Online/avatar-guest");
            }
        }
    }
}
