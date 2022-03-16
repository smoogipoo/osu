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
using osu.Framework.Localisation;
using osu.Framework.Threading;
using osu.Game.Graphics;
using osu.Game.Graphics.Backgrounds;
using osu.Game.Graphics.UserInterface;
using osu.Game.Graphics.UserInterfaceV2;
using osu.Game.Online.Multiplayer;
using osu.Game.Online.Multiplayer.Countdown;
using osuTK;

namespace osu.Game.Screens.OnlinePlay.Multiplayer.Match
{
    public class MultiplayerReadyButton : MultiplayerRoomComposite, IHasTooltip
    {
        public Action<TimeSpan?> OnReadyClick;
        public Action OnCancelCountdown;

        private Sample sampleReady;
        private Sample sampleReadyAll;
        private Sample sampleUnready;

        private readonly BindableBool buttonsEnabled = new BindableBool();
        private readonly CountdownButton countdownButton;

        [Resolved]
        private OngoingOperationTracker ongoingOperationTracker { get; set; }

        private int countReady;
        private ScheduledDelegate readySampleDelegate;
        private IBindable<bool> operationInProgress;

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
                            Action = () => OnReadyClick?.Invoke(null),
                            CancelCountdown = () => OnCancelCountdown?.Invoke(),
                            Enabled = { BindTarget = buttonsEnabled },
                        },
                        countdownButton = new CountdownButton
                        {
                            RelativeSizeAxes = Axes.Y,
                            Size = new Vector2(40, 1),
                            Alpha = 0,
                            Action = t => OnReadyClick?.Invoke(t),
                            Enabled = { BindTarget = buttonsEnabled }
                        }
                    }
                }
            };
        }

        [BackgroundDependencyLoader]
        private void load(AudioManager audio)
        {
            operationInProgress = ongoingOperationTracker.InProgress.GetBoundCopy();
            operationInProgress.BindValueChanged(_ => updateState());

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

        protected override void NewEvent(MatchServerEvent e)
        {
            base.NewEvent(e);
            updateState();
        }

        private void updateState()
        {
            if (Room == null)
                return;

            var localUser = Client.LocalUser;

            int newCountReady = Room.Users.Count(u => u.State == MultiplayerUserState.Ready);
            int newCountTotal = Room.Users.Count(u => u.State != MultiplayerUserState.Spectating);

            if (Room.Countdown != null)
                countdownButton.Alpha = 0;
            else
            {
                switch (localUser?.State)
                {
                    default:
                        countdownButton.Alpha = 0;
                        break;

                    case MultiplayerUserState.Spectating:
                    case MultiplayerUserState.Ready:
                        countdownButton.Alpha = Room.Host?.Equals(localUser) == true ? 1 : 0;
                        break;
                }
            }

            buttonsEnabled.Value =
                Room.State == MultiplayerRoomState.Open
                // TODO: && CurrentPlaylistItem.Value?.ID == Room.Settings.PlaylistItemId
                && !Room.Playlist.Single(i => i.ID == Room.Settings.PlaylistItemId).Expired
                && !operationInProgress.Value;

            // When the local user is the host and spectating the match, the "start match" state should be enabled if any users are ready.
            if (localUser?.State == MultiplayerUserState.Spectating)
                buttonsEnabled.Value &= Room.Host?.Equals(localUser) == true && newCountReady > 0;

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

        public LocalisableString TooltipText
        {
            get
            {
                if (Room?.Countdown != null)
                    return "Cancel countdown";

                return default;
            }
        }

        public class ReadyButton : Components.ReadyButton
        {
            public new Triangles Triangles => base.Triangles;

            public new Action Action;
            public Action CancelCountdown;

            [Resolved]
            private MultiplayerClient multiplayerClient { get; set; }

            [Resolved]
            private OsuColour colours { get; set; }

            [CanBeNull]
            private MultiplayerRoom room => multiplayerClient.Room;

            public ReadyButton()
            {
                base.Action = () =>
                {
                    if (room?.Countdown != null && room.Host?.Equals(multiplayerClient.LocalUser) == true)
                        CancelCountdown?.Invoke();
                    else
                        Action?.Invoke();
                };
            }

            [BackgroundDependencyLoader]
            private void load()
            {
            }

            protected override void LoadComplete()
            {
                base.LoadComplete();

                multiplayerClient.RoomUpdated += () => Scheduler.Add(onRoomUpdated);
                multiplayerClient.NewEvent += e => Scheduler.Add(onNewEvent, e);
                onRoomUpdated();
            }

            protected override void Update()
            {
                base.Update();

                if (room?.Countdown != null)
                    onRoomUpdated();
            }

            private void onNewEvent(MatchServerEvent e)
            {
                if (e is CountdownChangedEvent)
                    onRoomUpdated();
            }

            private void onRoomUpdated()
            {
                updateButtonText();
                updateButtonColour();
            }

            private void updateButtonText()
            {
                if (room == null)
                    return;

                var localUser = multiplayerClient.LocalUser;

                int countReady = room.Users.Count(u => u.State == MultiplayerUserState.Ready);
                int countTotal = room.Users.Count(u => u.State != MultiplayerUserState.Spectating);

                string countdownText = room.Countdown == null ? string.Empty : $"Starting in {room.Countdown.EndTime - DateTimeOffset.Now:mm\\:ss}";
                string countText = $"({countReady} / {countTotal} ready)";

                if (room.Countdown != null)
                {
                    switch (localUser?.State)
                    {
                        default:
                            Text = $"Ready ({countdownText.ToLowerInvariant()})";
                            break;

                        case MultiplayerUserState.Spectating:
                        case MultiplayerUserState.Ready:
                            Text = $"{countdownText} {countText}";
                            break;
                    }

                    return;
                }

                switch (localUser?.State)
                {
                    default:
                        Text = "Ready";
                        break;

                    case MultiplayerUserState.Spectating:
                    case MultiplayerUserState.Ready:
                        Text = room.Host?.Equals(localUser) == true
                            ? $"Start match {countText}"
                            : $"Waiting for host... {countText}";

                        break;
                }
            }

            private void updateButtonColour()
            {
                if (room == null)
                    return;

                var localUser = multiplayerClient.LocalUser;

                if (room.Countdown != null)
                {
                    switch (localUser?.State)
                    {
                        default:
                            setGreen();
                            break;

                        case MultiplayerUserState.Spectating:
                        case MultiplayerUserState.Ready:
                            setYellow();
                            break;
                    }

                    return;
                }

                switch (localUser?.State)
                {
                    default:
                        setGreen();
                        break;

                    case MultiplayerUserState.Spectating:
                    case MultiplayerUserState.Ready:
                        if (room?.Host?.Equals(localUser) == true)
                            setGreen();
                        else
                            setYellow();

                        break;
                }

                void setYellow()
                {
                    BackgroundColour = colours.YellowDark;
                    Triangles.ColourDark = colours.YellowDark;
                    Triangles.ColourLight = colours.Yellow;
                }

                void setGreen()
                {
                    BackgroundColour = colours.Green;
                    Triangles.ColourDark = colours.Green;
                    Triangles.ColourLight = colours.GreenLight;
                }
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
            private static readonly TimeSpan[] available_delays =
            {
                TimeSpan.FromSeconds(10),
                TimeSpan.FromSeconds(30),
                TimeSpan.FromMinutes(1),
                TimeSpan.FromMinutes(2)
            };

            public new Action<TimeSpan> Action;

            private readonly Drawable background;

            public CountdownButton()
            {
                Icon = FontAwesome.Solid.CaretDown;
                IconScale = new Vector2(0.6f);

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
