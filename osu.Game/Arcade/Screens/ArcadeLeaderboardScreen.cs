// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Colour;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Effects;
using osu.Framework.Graphics.Shapes;
using osu.Game.Database;
using osu.Game.Extensions;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osu.Game.Online.API.Requests.Responses;
using osu.Game.Screens;
using osu.Game.Screens.OnlinePlay.Matchmaking.RankedPlay;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Arcade.Screens
{
    public class ArcadeLeaderboardScreen : OsuScreen
    {
        [Resolved]
        private ArcadeClient arcadeClient { get; set; } = null!;

        [Resolved]
        private UserLookupCache userLookupCache { get; set; } = null!;

        [Resolved]
        private BeatmapLookupCache beatmapLookupCache { get; set; } = null!;

        private readonly CancellationTokenSource cancellationSource = new CancellationTokenSource();

        protected override BackgroundScreen CreateBackground() => new RankedPlayBackgroundScreen();

        private OsuSpriteText player1Name = null!;
        private OsuSpriteText player2Name = null!;
        private OsuSpriteText beatmapText = null!;

        [BackgroundDependencyLoader]
        private void load()
        {
            InternalChild = new Container
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                RelativeSizeAxes = Axes.Both,
                Height = 0.5f,
                Children = new Drawable[]
                {
                    new Box
                    {
                        RelativeSizeAxes = Axes.Both,
                        Colour = Color4.Black,
                        Alpha = 0.5f
                    },
                    new BufferedContainer
                    {
                        RelativeSizeAxes = Axes.Both,
                        Children = new Drawable[]
                        {
                            new ArcadeLeaderboardCloud
                            {
                                RelativeSizeAxes = Axes.Both,
                            },
                            new FillFlowContainer
                            {
                                Anchor = Anchor.Centre,
                                Origin = Anchor.Centre,
                                AutoSizeAxes = Axes.X,
                                RelativeSizeAxes = Axes.Y,
                                Direction = FillDirection.Horizontal,
                                Blending = new BlendingParameters
                                {
                                    Source = BlendingType.Zero,
                                    Destination = BlendingType.One,
                                    SourceAlpha = BlendingType.Zero,
                                    DestinationAlpha = BlendingType.OneMinusSrcAlpha,
                                    RGBEquation = BlendingEquation.Add,
                                    AlphaEquation = BlendingEquation.ReverseSubtract,
                                },
                                Children = new Drawable[]
                                {
                                    new Box
                                    {
                                        RelativeSizeAxes = Axes.Y,
                                        Width = 100,
                                        Colour = ColourInfo.GradientHorizontal(Color4.Transparent, Color4.White)
                                    },
                                    new Box
                                    {
                                        RelativeSizeAxes = Axes.Y,
                                        Width = 300
                                    },
                                    new Box
                                    {
                                        RelativeSizeAxes = Axes.Y,
                                        Width = 100,
                                        Colour = ColourInfo.GradientHorizontal(Color4.White, Color4.Transparent)
                                    },
                                }
                            },
                            new Container
                            {
                                Size = new Vector2(200, 100),
                                Masking = true,
                                EdgeEffect = new EdgeEffectParameters
                                {
                                    Colour = Color4.Black,
                                    Radius = 50,
                                },
                                Blending = new BlendingParameters
                                {
                                    Source = BlendingType.Zero,
                                    Destination = BlendingType.One,
                                    SourceAlpha = BlendingType.Zero,
                                    DestinationAlpha = BlendingType.OneMinusSrcAlpha,
                                    RGBEquation = BlendingEquation.Add,
                                    AlphaEquation = BlendingEquation.ReverseSubtract,
                                },
                                // Child = new Box
                                // {
                                //     RelativeSizeAxes = Axes.Both,
                                //     Colour = Color4.Black,
                                //     Alpha = 1
                                // }
                            },
                            new Container
                            {
                                RelativeSizeAxes = Axes.Both,
                                Padding = new MarginPadding
                                {
                                    Top = 10,
                                    Horizontal = 10
                                },
                                Children = new Drawable[]
                                {
                                    player1Name = new OsuSpriteText
                                    {
                                        Font = OsuFont.GetFont(size: 32)
                                    },
                                    player2Name = new OsuSpriteText
                                    {
                                        Anchor = Anchor.TopRight,
                                        Origin = Anchor.TopRight,
                                        Font = OsuFont.GetFont(size: 32)
                                    },
                                    beatmapText = new OsuSpriteText
                                    {
                                        Anchor = Anchor.TopCentre,
                                        Origin = Anchor.TopCentre,
                                        Font = OsuFont.GetFont(size: 24)
                                    },
                                }
                            },
                            new Container
                            {
                                Anchor = Anchor.Centre,
                                Origin = Anchor.Centre,
                                RelativeSizeAxes = Axes.Y,
                                Width = 300,
                                Padding = new MarginPadding { Top = 50 },
                                Child = new ArcadeLeaderboard
                                {
                                    RelativeSizeAxes = Axes.X,
                                    MaxPanels = 5
                                }
                            },
                        }
                    }
                }
            };
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            Task.Run(fetchActiveRoom);
        }

        private async Task fetchActiveRoom()
        {
            while (!cancellationSource.IsCancellationRequested)
            {
                var room = (await arcadeClient.GetActiveRooms().ConfigureAwait(false)).FirstOrDefault();

                if (room == null)
                    return;

                if (room.Users.Count != 2)
                    return;

                APIUser? player1User = await userLookupCache.GetUserAsync(room.Users[0].UserID).ConfigureAwait(false);
                APIUser? player2User = await userLookupCache.GetUserAsync(room.Users[1].UserID).ConfigureAwait(false);
                APIBeatmap? beatmap = await beatmapLookupCache.GetBeatmapAsync(room.CurrentPlaylistItem.BeatmapID).ConfigureAwait(false);

                Schedule(() =>
                {
                    player1Name.Text = player1User?.Username ?? string.Empty;
                    player2Name.Text = player2User?.Username ?? string.Empty;
                    beatmapText.Text = beatmap == null ? string.Empty : beatmap.GetDisplayString();
                });

                await Task.Delay(1000).ConfigureAwait(false);
            }
        }

        protected override void Dispose(bool isDisposing)
        {
            base.Dispose(isDisposing);

            cancellationSource.Cancel();
        }
    }
}
