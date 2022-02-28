// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Linq;
using Humanizer;
using JetBrains.Annotations;
using osu.Framework.Allocation;
using osu.Framework.Audio;
using osu.Framework.Audio.Sample;
using osu.Framework.Bindables;
using osu.Framework.Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Cursor;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.UserInterface;
using osu.Framework.Threading;
using osu.Game.Graphics;
using osu.Game.Graphics.Backgrounds;
using osu.Game.Graphics.UserInterface;
using osu.Game.Graphics.UserInterfaceV2;
using osu.Game.Online.Multiplayer;
using osuTK;

namespace osu.Game.Screens.OnlinePlay.Multiplayer.Match
{
    public class MultiplayerReadyButton : MultiplayerRoomComposite
    {
        public Action<TimeSpan?> OnReadyClick;

        private Sample sampleReady;
        private Sample sampleReadyAll;
        private Sample sampleUnready;

        private readonly CountdownButton countdownButton;

        private int countReady;

        private ScheduledDelegate readySampleDelegate;

        public MultiplayerReadyButton()
        {
            InternalChild = new GridContainer
            {
                RelativeSizeAxes = Axes.Both,
                ColumnDimensions = new[]
                {
                    new Dimension(),
                    new Dimension(GridSizeMode.AutoSize)
                },
                Content = new[]
                {
                    new Drawable[]
                    {
                        new ReadyButton
                        {
                            RelativeSizeAxes = Axes.Both,
                            Size = Vector2.One,
                            Enabled = { Value = true },
                            Action = () => OnReadyClick?.Invoke(null)
                        },
                        countdownButton = new CountdownButton
                        {
                            RelativeSizeAxes = Axes.Y,
                            Size = new Vector2(40, 1),
                            Icon = FontAwesome.Solid.CaretDown,
                            IconScale = new Vector2(0.6f),
                            Alpha = 0,
                            Action = t => OnReadyClick?.Invoke(t)
                        }
                    }
                }
            };
        }

        [BackgroundDependencyLoader]
        private void load(AudioManager audio)
        {
            sampleReady = audio.Samples.Get(@"Multiplayer/player-ready");
            sampleReadyAll = audio.Samples.Get(@"Multiplayer/player-ready-all");
            sampleUnready = audio.Samples.Get(@"Multiplayer/player-unready");
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            CurrentPlaylistItem.BindValueChanged(_ => updateState());
        }

        protected override void OnRoomUpdated()
        {
            base.OnRoomUpdated();

            updateState();
        }

        private void updateState()
        {
            var localUser = Client.LocalUser;

            int newCountReady = Room?.Users.Count(u => u.State == MultiplayerUserState.Ready) ?? 0;
            int newCountTotal = Room?.Users.Count(u => u.State != MultiplayerUserState.Spectating) ?? 0;

            switch (localUser?.State)
            {
                default:
                    countdownButton.Alpha = 0;
                    break;

                case MultiplayerUserState.Spectating:
                case MultiplayerUserState.Ready:
                    countdownButton.Alpha = Room?.Host?.Equals(localUser) == true ? 1 : 0;
                    break;
            }

            if (newCountReady == countReady)
                return;

            readySampleDelegate?.Cancel();
            readySampleDelegate = Schedule(() =>
            {
                if (newCountReady > countReady)
                {
                    if (newCountReady == newCountTotal)
                        sampleReadyAll?.Play();
                    else
                        sampleReady?.Play();
                }
                else if (newCountReady < countReady)
                {
                    sampleUnready?.Play();
                }

                countReady = newCountReady;
            });
        }

        private class ReadyButton : Components.ReadyButton
        {
            public new Triangles Triangles => base.Triangles;

            [Resolved]
            private MultiplayerClient multiplayerClient { get; set; }

            [Resolved]
            private OsuColour colours { get; set; }

            [Resolved]
            private OngoingOperationTracker ongoingOperationTracker { get; set; }

            [CanBeNull]
            private MultiplayerRoom room => multiplayerClient.Room;

            private IBindable<bool> operationInProgress;

            [BackgroundDependencyLoader]
            private void load()
            {
                operationInProgress = ongoingOperationTracker.InProgress.GetBoundCopy();
                operationInProgress.BindValueChanged(_ => onRoomUpdated());
            }

            protected override void LoadComplete()
            {
                base.LoadComplete();

                multiplayerClient.RoomUpdated += onRoomUpdated;
                onRoomUpdated();
            }

            private void onRoomUpdated()
            {
                var localUser = multiplayerClient.LocalUser;

                int newCountReady = room?.Users.Count(u => u.State == MultiplayerUserState.Ready) ?? 0;
                int newCountTotal = room?.Users.Count(u => u.State != MultiplayerUserState.Spectating) ?? 0;

                switch (localUser?.State)
                {
                    default:
                        Text = "Ready";
                        BackgroundColour = colours.Green;
                        Triangles.ColourDark = colours.Green;
                        Triangles.ColourLight = colours.GreenLight;
                        break;

                    case MultiplayerUserState.Spectating:
                    case MultiplayerUserState.Ready:

                        string countText = $"({newCountReady} / {newCountTotal} ready)";

                        if (room?.Host?.Equals(localUser) == true)
                        {
                            Text = $"Start match {countText}";
                            BackgroundColour = colours.Green;
                            Triangles.ColourDark = colours.Green;
                            Triangles.ColourLight = colours.GreenLight;
                        }
                        else
                        {
                            Text = $"Waiting for host... {countText}";
                            BackgroundColour = colours.YellowDark;
                            Triangles.ColourDark = colours.YellowDark;
                            Triangles.ColourLight = colours.Yellow;
                        }

                        break;
                }

                bool enableButton =
                    room?.State == MultiplayerRoomState.Open
                    // TODO: && CurrentPlaylistItem.Value?.ID == Room.Settings.PlaylistItemId
                    && !room.Playlist.Single(i => i.ID == room.Settings.PlaylistItemId).Expired
                    && !operationInProgress.Value;

                // When the local user is the host and spectating the match, the "start match" state should be enabled if any users are ready.
                if (localUser?.State == MultiplayerUserState.Spectating)
                    enableButton &= room?.Host?.Equals(localUser) == true && newCountReady > 0;

                Enabled.Value = enableButton;
            }

            protected override void Dispose(bool isDisposing)
            {
                base.Dispose(isDisposing);

                if (multiplayerClient != null)
                    multiplayerClient.RoomUpdated -= onRoomUpdated;
            }
        }

        public class CountdownButton : IconButton, IHasPopover
        {
            public new Action<TimeSpan> Action;

            private readonly Drawable background;

            public CountdownButton()
            {
                Add(background = new Box
                {
                    RelativeSizeAxes = Axes.Both,
                    Depth = float.MaxValue
                });

                base.Action = this.ShowPopover;
            }

            [BackgroundDependencyLoader]
            private void load(OsuColour colours)
            {
                background.Colour = colours.Green;
            }

            private static readonly TimeSpan[] available_delays =
            {
                TimeSpan.FromSeconds(10),
                TimeSpan.FromSeconds(30),
                TimeSpan.FromMinutes(1),
                TimeSpan.FromMinutes(2)
            };

            public Popover GetPopover()
            {
                var flow = new FillFlowContainer
                {
                    Width = 200,
                    AutoSizeAxes = Axes.Y,
                    Direction = FillDirection.Vertical,
                    Spacing = new Vector2(2),
                };

                foreach (var duration in available_delays)
                {
                    flow.Add(new PopoverButton
                    {
                        RelativeSizeAxes = Axes.X,
                        Text = $"Start match in {duration.Humanize()}",
                        BackgroundColour = background.Colour,
                        Action = () =>
                        {
                            Action(duration);
                            this.HidePopover();
                        }
                    });
                }

                return new OsuPopover { Child = flow };
            }

            public class PopoverButton : OsuButton
            {
            }
        }
    }
}
