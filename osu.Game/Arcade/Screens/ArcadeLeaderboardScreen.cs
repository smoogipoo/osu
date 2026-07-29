// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Colour;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Game.Database;
using osu.Game.Extensions;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osu.Game.Online.API.Requests.Responses;
using osu.Game.Screens;
using osu.Game.Screens.OnlinePlay.Matchmaking.RankedPlay;
using osu.Game.Screens.OnlinePlay.Matchmaking.RankedPlay.Components;
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

        private RankedPlayCornerPiece leftCornerPiece = null!;
        private RankedPlayCornerPiece rightCornerPiece = null!;
        private OsuSpriteText beatmapText = null!;

        private long? lastRoomId;
        private int? lastBeatmapId;

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
                                RelativeSizeAxes = Axes.Both,
                                Children = new Drawable[]
                                {
                                    leftCornerPiece = new RankedPlayCornerPiece(RankedPlayColourScheme.BLUE, Anchor.TopLeft)
                                    {
                                        Scale = new Vector2(0.75f),
                                        State = { Value = Visibility.Visible },
                                    },
                                    rightCornerPiece = new RankedPlayCornerPiece(RankedPlayColourScheme.BLUE, Anchor.TopRight)
                                    {
                                        Anchor = Anchor.TopRight,
                                        Origin = Anchor.TopRight,
                                        Scale = new Vector2(0.75f),
                                        State = { Value = Visibility.Visible },
                                    },
                                    beatmapText = new OsuSpriteText
                                    {
                                        Anchor = Anchor.TopCentre,
                                        Origin = Anchor.TopCentre,
                                        Font = OsuFont.GetFont(size: 24),
                                        Margin = new MarginPadding { Top = 10 }
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
                                    Anchor = Anchor.Centre,
                                    Origin = Anchor.Centre,
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

                APIUser player1User = await userLookupCache.GetUserAsync(room.Users[0].UserID).ConfigureAwait(false) ?? APIUser.UnknownUser(room.Users[0].UserID);
                APIUser player2User = await userLookupCache.GetUserAsync(room.Users[1].UserID).ConfigureAwait(false) ?? APIUser.UnknownUser(room.Users[1].UserID);
                APIBeatmap? beatmap = await beatmapLookupCache.GetBeatmapAsync(room.CurrentPlaylistItem.BeatmapID).ConfigureAwait(false);

                Schedule(() =>
                {
                    if (room.RoomID != lastRoomId)
                    {
                        leftCornerPiece.Child = new ArcadeLeaderboardScreenUserDisplay(player1User, Anchor.TopLeft, RankedPlayColourScheme.BLUE)
                        {
                            RelativeSizeAxes = Axes.Both,
                        };

                        rightCornerPiece.Child = new ArcadeLeaderboardScreenUserDisplay(player2User, Anchor.TopRight, RankedPlayColourScheme.BLUE)
                        {
                            RelativeSizeAxes = Axes.Both,
                        };
                    }

                    if (beatmap?.OnlineID != lastBeatmapId)
                        beatmapText.Text = beatmap == null ? string.Empty : beatmap.GetDisplayString();

                    lastRoomId = room.RoomID;
                    lastBeatmapId = beatmap?.OnlineID;
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
