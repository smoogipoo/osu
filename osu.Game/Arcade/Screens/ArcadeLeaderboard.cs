// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.Textures;
using osu.Game.Graphics;
using osu.Game.Graphics.Containers;
using osu.Game.Graphics.Sprites;
using osuTK;

namespace osu.Game.Arcade.Screens
{
    public class ArcadeLeaderboard : CompositeDrawable
    {
        [Resolved]
        private ArcadeClient arcadeClient { get; set; } = null!;

        private readonly CancellationTokenSource cancellationSource = new CancellationTokenSource();
        private readonly Dictionary<int, LeaderboardPanel> panelMap = [];

        private FillFlowContainer<LeaderboardPanel> panelFlow = [];

        [BackgroundDependencyLoader]
        private void load()
        {
            InternalChild = new OsuScrollContainer(Direction.Vertical)
            {
                RelativeSizeAxes = Axes.Both,
                ScrollbarOverlapsContent = false,
                Child = panelFlow = new FillFlowContainer<LeaderboardPanel>
                {
                    RelativeSizeAxes = Axes.X,
                    AutoSizeAxes = Axes.Y,
                    Spacing = new Vector2(5),
                    LayoutDuration = 200,
                    LayoutEasing = Easing.OutQuint
                }
            };
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            Task.Run(runLoop);
        }

        private async Task runLoop()
        {
            while (!cancellationSource.IsCancellationRequested)
            {
                try
                {
                    await Fetch().ConfigureAwait(false);
                }
                finally
                {
                    await Task.Delay(2000, cancellationSource.Token).ConfigureAwait(false);
                }
            }
        }

        public async Task Fetch()
        {
            ArcadeUserStats[] stats = await arcadeClient.FetchLeaderboard().ConfigureAwait(false);

            stats = stats
                    .OrderByDescending(s => s.Victories)
                    .ThenBy(s => s.Username)
                    .ToArray();

            Schedule(() =>
            {
                for (int i = 0; i < stats.Length; i++)
                {
                    ArcadeUserStats item = stats[i];

                    if (!panelMap.TryGetValue(item.UserId, out LeaderboardPanel? panel))
                        panelFlow.Add(panelMap[item.UserId] = panel = new LeaderboardPanel(item.UserId, item.Username));

                    panel.Count = item.Victories;
                    panel.Rank = i + 1;

                    panelFlow.SetLayoutPosition(panel, i);
                }
            });
        }

        protected override void Dispose(bool isDisposing)
        {
            base.Dispose(isDisposing);

            cancellationSource.Cancel();
        }

        private class LeaderboardPanel : CompositeDrawable
        {
            [Resolved]
            private OsuColour colours { get; set; } = null!;

            private readonly int userId;
            private readonly string username;

            private OsuSpriteText? rankText;
            private OsuSpriteText? countText;

            public LeaderboardPanel(int userId, string username)
            {
                this.userId = userId;
                this.username = username;

                RelativeSizeAxes = Axes.X;
                Height = 50;
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
                                    Spacing = new Vector2(15),
                                    Children = new Drawable[]
                                    {
                                        rankText = new OsuSpriteText
                                        {
                                            Anchor = Anchor.CentreLeft,
                                            Origin = Anchor.CentreLeft,
                                            Width = 20,
                                            Text = $"#{Rank}",
                                            Margin = new MarginPadding { Left = 5 }
                                        },
                                        new DrawableAvatar(userId)
                                        {
                                            Anchor = Anchor.CentreLeft,
                                            Origin = Anchor.CentreLeft,
                                            Size = new Vector2(40)
                                        },
                                        new OsuSpriteText
                                        {
                                            Anchor = Anchor.CentreLeft,
                                            Origin = Anchor.CentreLeft,
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
                        rankText.Text = $"#{value}";
                }
            }
        }

        public partial class DrawableAvatar : Sprite
        {
            private readonly int userId;

            public DrawableAvatar(int userId)
            {
                this.userId = userId;
            }

            [BackgroundDependencyLoader]
            private void load(LargeTextureStore textures)
            {
                Texture = textures.Get($@"https://a.ppy.sh/{userId}")
                          ?? textures.Get(@"Online/avatar-guest");
            }
        }
    }
}
