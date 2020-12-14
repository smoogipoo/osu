// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Diagnostics;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Screens;
using osu.Game.Online.API;
using osu.Game.Online.Multiplayer;
using osu.Game.Screens.Multi.Components;
using osu.Game.Screens.Multi.Match.Components;
using osu.Game.Screens.Multi.Play;
using osu.Game.Screens.Multi.Ranking;
using osu.Game.Screens.Play;
using osu.Game.Screens.Select;
using osu.Game.Users;

namespace osu.Game.Screens.Multi.Timeshift
{
    public class TimeshiftRoomSubScreen : RoomSubScreen
    {
        public override string Title { get; }

        public override string ShortTitle => "room";

        [Resolved(typeof(Room), nameof(Room.RoomID))]
        private Bindable<int?> roomId { get; set; }

        [Resolved(canBeNull: true)]
        private Multiplayer multiplayer { get; set; }

        private MatchSettingsOverlay settingsOverlay;
        private MatchLeaderboard leaderboard;

        private OverlinedHeader participantsHeader;

        public TimeshiftRoomSubScreen(Room room)
        {
            Title = room.RoomID.Value == null ? "New room" : room.Name.Value;
            Activity.Value = new UserActivity.InLobby(room);
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            InternalChildren = new Drawable[]
            {
                new GridContainer
                {
                    RelativeSizeAxes = Axes.Both,
                    Content = new[]
                    {
                        new Drawable[]
                        {
                            new Container
                            {
                                RelativeSizeAxes = Axes.Both,
                                Padding = new MarginPadding
                                {
                                    Horizontal = 105,
                                    Vertical = 20
                                },
                                Child = new GridContainer
                                {
                                    RelativeSizeAxes = Axes.Both,
                                    RowDimensions = new[]
                                    {
                                        new Dimension(GridSizeMode.AutoSize),
                                        new Dimension(GridSizeMode.AutoSize),
                                        new Dimension(GridSizeMode.AutoSize),
                                        new Dimension(),
                                    },
                                    Content = new[]
                                    {
                                        new Drawable[] { new Match.Components.Header() },
                                        new Drawable[]
                                        {
                                            participantsHeader = new OverlinedHeader("Participants")
                                            {
                                                ShowLine = false
                                            }
                                        },
                                        new Drawable[]
                                        {
                                            new Container
                                            {
                                                RelativeSizeAxes = Axes.X,
                                                AutoSizeAxes = Axes.Y,
                                                Margin = new MarginPadding { Top = 5 },
                                                Child = new ParticipantsDisplay(Direction.Horizontal)
                                                {
                                                    Details = { BindTarget = participantsHeader.Details }
                                                }
                                            }
                                        },
                                        new Drawable[]
                                        {
                                            new GridContainer
                                            {
                                                RelativeSizeAxes = Axes.Both,
                                                Content = new[]
                                                {
                                                    new Drawable[]
                                                    {
                                                        new Container
                                                        {
                                                            RelativeSizeAxes = Axes.Both,
                                                            Padding = new MarginPadding { Right = 5 },
                                                            Child = new GridContainer
                                                            {
                                                                RelativeSizeAxes = Axes.Both,
                                                                Content = new[]
                                                                {
                                                                    new Drawable[] { new OverlinedPlaylistHeader(), },
                                                                    new Drawable[]
                                                                    {
                                                                        new DrawableRoomPlaylistWithResults
                                                                        {
                                                                            RelativeSizeAxes = Axes.Both,
                                                                            Items = { BindTarget = Playlist },
                                                                            SelectedItem = { BindTarget = SelectedItem },
                                                                            RequestShowResults = item =>
                                                                            {
                                                                                Debug.Assert(roomId.Value != null);
                                                                                multiplayer?.Push(new TimeshiftResultsScreen(null, roomId.Value.Value, item, false));
                                                                            }
                                                                        }
                                                                    },
                                                                },
                                                                RowDimensions = new[]
                                                                {
                                                                    new Dimension(GridSizeMode.AutoSize),
                                                                    new Dimension(),
                                                                }
                                                            }
                                                        },
                                                        null,
                                                        new GridContainer
                                                        {
                                                            RelativeSizeAxes = Axes.Both,
                                                            Content = new[]
                                                            {
                                                                new Drawable[] { new OverlinedHeader("Leaderboard"), },
                                                                new Drawable[] { leaderboard = new MatchLeaderboard { RelativeSizeAxes = Axes.Both }, },
                                                                new Drawable[] { new OverlinedHeader("Chat"), },
                                                                new Drawable[] { new MatchChatDisplay { RelativeSizeAxes = Axes.Both } }
                                                            },
                                                            RowDimensions = new[]
                                                            {
                                                                new Dimension(GridSizeMode.AutoSize),
                                                                new Dimension(),
                                                                new Dimension(GridSizeMode.AutoSize),
                                                                new Dimension(GridSizeMode.Relative, size: 0.4f, minSize: 120),
                                                            }
                                                        },
                                                        null
                                                    },
                                                },
                                                ColumnDimensions = new[]
                                                {
                                                    new Dimension(GridSizeMode.Relative, size: 0.5f, maxSize: 400),
                                                    new Dimension(),
                                                    new Dimension(GridSizeMode.Relative, size: 0.5f, maxSize: 600),
                                                    new Dimension(),
                                                }
                                            }
                                        }
                                    },
                                }
                            }
                        },
                        new Drawable[]
                        {
                            new Match.Components.Footer
                            {
                                OnStart = onStart,
                                SelectedItem = { BindTarget = SelectedItem }
                            }
                        }
                    },
                    RowDimensions = new[]
                    {
                        new Dimension(),
                        new Dimension(GridSizeMode.AutoSize),
                    }
                },
                settingsOverlay = new MatchSettingsOverlay
                {
                    RelativeSizeAxes = Axes.Both,
                    EditPlaylist = () => this.Push(new MatchSongSelect()),
                    State = { Value = roomId.Value == null ? Visibility.Visible : Visibility.Hidden }
                }
            };
        }

        [Resolved]
        private IAPIProvider api { get; set; }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            roomId.BindValueChanged(id =>
            {
                if (id.NewValue == null)
                    settingsOverlay.Show();
                else
                {
                    settingsOverlay.Hide();

                    // Set the first playlist item.
                    // This is scheduled since updating the room and playlist may happen in an arbitrary order (via Room.CopyFrom()).
                    Schedule(() => SelectedItem.Value = Playlist.FirstOrDefault());
                }
            }, true);
        }

        private void onStart()
        {
            multiplayer?.Push(new PlayerLoader(() => new TimeshiftPlayer(SelectedItem.Value)
            {
                Exited = () => leaderboard.RefreshScores()
            }));
        }
    }
}
