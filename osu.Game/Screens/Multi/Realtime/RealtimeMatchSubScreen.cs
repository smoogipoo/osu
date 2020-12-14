// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Specialized;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Screens;
using osu.Game.Online.Multiplayer;
using osu.Game.Screens.Multi.Components;
using osu.Game.Screens.Multi.Match.Components;
using osu.Game.Screens.Multi.Realtime.Participants;
using osu.Game.Screens.Play;
using osu.Game.Users;

namespace osu.Game.Screens.Multi.Realtime
{
    [Cached]
    public class RealtimeMatchSubScreen : RoomSubScreen
    {
        public override string Title { get; }

        public override string ShortTitle => "match";

        [Resolved(typeof(Room), nameof(Room.RoomID))]
        private Bindable<int?> roomId { get; set; }

        [Resolved(canBeNull: true)]
        private Multiplayer multiplayer { get; set; }

        [Resolved]
        private RealtimeRoomManager roomManager { get; set; }

        private RealtimeMatchSettingsOverlay settingsOverlay;

        public RealtimeMatchSubScreen(Room room)
        {
            Title = room.RoomID.Value == null ? "New match" : room.Name.Value;
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
                                        new Dimension(),
                                    },
                                    Content = new[]
                                    {
                                        new Drawable[]
                                        {
                                            new Match.Components.Header
                                            {
                                                OpenSettings = () => settingsOverlay.Show()
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
                                                            Padding = new MarginPadding { Horizontal = 5, Vertical = 10 },
                                                            Child = new GridContainer
                                                            {
                                                                RelativeSizeAxes = Axes.Both,
                                                                RowDimensions = new[]
                                                                {
                                                                    new Dimension(GridSizeMode.AutoSize)
                                                                },
                                                                Content = new[]
                                                                {
                                                                    new Drawable[] { new ParticipantsHeader() },
                                                                    new Drawable[]
                                                                    {
                                                                        new Participants.ParticipantsList
                                                                        {
                                                                            RelativeSizeAxes = Axes.Both
                                                                        },
                                                                    }
                                                                }
                                                            }
                                                        },
                                                        new FillFlowContainer
                                                        {
                                                            Anchor = Anchor.Centre,
                                                            Origin = Anchor.Centre,
                                                            RelativeSizeAxes = Axes.X,
                                                            AutoSizeAxes = Axes.Y,
                                                            Padding = new MarginPadding { Horizontal = 5 },
                                                            Children = new Drawable[]
                                                            {
                                                                new OverlinedHeader("Beatmap"),
                                                                new BeatmapSelectionControl { RelativeSizeAxes = Axes.X }
                                                            }
                                                        }
                                                    }
                                                }
                                            }
                                        },
                                        new Drawable[]
                                        {
                                            new GridContainer
                                            {
                                                RelativeSizeAxes = Axes.Both,
                                                RowDimensions = new[]
                                                {
                                                    new Dimension(GridSizeMode.AutoSize)
                                                },
                                                Content = new[]
                                                {
                                                    new Drawable[] { new OverlinedHeader("Chat") },
                                                    new Drawable[] { new MatchChatDisplay { RelativeSizeAxes = Axes.Both } }
                                                }
                                            }
                                        }
                                    },
                                }
                            }
                        },
                        new Drawable[]
                        {
                            new Footer { SelectedItem = { BindTarget = SelectedItem } }
                        }
                    },
                    RowDimensions = new[]
                    {
                        new Dimension(),
                        new Dimension(GridSizeMode.AutoSize),
                    }
                },
                settingsOverlay = new RealtimeMatchSettingsOverlay
                {
                    RelativeSizeAxes = Axes.Both,
                    State = { Value = roomId.Value == null ? Visibility.Visible : Visibility.Hidden }
                }
            };
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            Playlist.BindCollectionChanged(onPlaylistChanged, true);

            roomManager.Client.LoadRequested += onLoadRequested;
        }

        private void onPlaylistChanged(object sender, NotifyCollectionChangedEventArgs e) => SelectedItem.Value = Playlist.FirstOrDefault();

        private void onLoadRequested() => multiplayer?.Push(new PlayerLoader(() => new RealtimePlayer(SelectedItem.Value)));

        protected override void Dispose(bool isDisposing)
        {
            base.Dispose(isDisposing);

            if (roomManager != null)
                roomManager.Client.LoadRequested -= onLoadRequested;
        }
    }
}
