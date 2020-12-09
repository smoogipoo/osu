// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Screens;
using osu.Game.Online.Multiplayer;
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
                                        new Drawable[] { new Match.Components.Header() },
                                        new Drawable[]
                                        {
                                            new GridContainer
                                            {
                                                RelativeSizeAxes = Axes.Both,
                                                Content = new[]
                                                {
                                                    new Drawable[]
                                                    {
                                                        null,
                                                        new BeatmapSelectionControl
                                                        {
                                                            Anchor = Anchor.Centre,
                                                            Origin = Anchor.Centre,
                                                            RelativeSizeAxes = Axes.X
                                                        }
                                                    }
                                                }
                                            }
                                        }
                                    },
                                }
                            }
                        },
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
                    OpenSongSelect = () => this.Push(new RealtimeMatchSongSelect()),
                    State = { Value = roomId.Value == null ? Visibility.Visible : Visibility.Hidden }
                }
            };
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            roomId.BindValueChanged(id =>
            {
                if (id.NewValue == null)
                    settingsOverlay.Show();
                else
                    settingsOverlay.Hide();
            }, true);
        }
    }
}
