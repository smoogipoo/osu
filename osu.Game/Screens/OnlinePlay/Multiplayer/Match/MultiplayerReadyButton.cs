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

        private readonly CountdownButton countdownButton;

        private int countReady;
        private bool isCountingDown;
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
                            Action = () => OnReadyClick?.Invoke(null),
                            CancelCountdown = () => OnCancelCountdown?.Invoke()
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

        protected override void NewEvent(MatchServerEvent e)
        {
            base.NewEvent(e);

            switch (e)
            {
                case MatchStartCountdownEvent _:
                    isCountingDown = true;
                    break;

                case EndCountdownEvent _:
                    isCountingDown = false;
                    break;
            }

            updateState();
        }

        private void updateState()
        {
            var localUser = Client.LocalUser;

            int newCountReady = Room?.Users.Count(u => u.State == MultiplayerUserState.Ready) ?? 0;
            int newCountTotal = Room?.Users.Count(u => u.State != MultiplayerUserState.Spectating) ?? 0;

            if (isCountingDown)
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
                        countdownButton.Alpha = Room?.Host?.Equals(localUser) == true ? 1 : 0;
                        break;
                }
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

        public LocalisableString TooltipText
        {
            get
            {
                if (isCountingDown)
                    return "Cancel countdown";

                return default;
            }
        }

        private class ReadyButton : Components.ReadyButton
        {
            public new Triangles Triangles => base.Triangles;

            public new Action Action;
            public Action CancelCountdown;

            [Resolved]
            private MultiplayerClient multiplayerClient { get; set; }

            [Resolved]
            private OsuColour colours { get; set; }

            [Resolved]
            private OngoingOperationTracker ongoingOperationTracker { get; set; }

            [CanBeNull]
            private MultiplayerRoom room => multiplayerClient.Room;

            private IBindable<bool> operationInProgress;
            private MatchStartCountdownEvent currentCountdown;

            public ReadyButton()
            {
                base.Action = () =>
                {
                    if (currentCountdown != null && room?.Host?.Equals(multiplayerClient.LocalUser) == true)
                        CancelCountdown?.Invoke();
                    else
                        Action?.Invoke();
                };
            }

            [BackgroundDependencyLoader]
            private void load()
            {
                operationInProgress = ongoingOperationTracker.InProgress.GetBoundCopy();
                operationInProgress.BindValueChanged(_ => onRoomUpdated());
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

                if (currentCountdown != null)
                    onRoomUpdated();
            }

            private void onNewEvent(MatchServerEvent e)
            {
                switch (e)
                {
                    case MatchStartCountdownEvent startCountdownEvent:
                        currentCountdown = startCountdownEvent;
                        break;

                    case EndCountdownEvent _:
                        currentCountdown = null;
                        break;
                }

                onRoomUpdated();
            }

            private void onRoomUpdated()
            {
                var localUser = multiplayerClient.LocalUser;
                int newCountReady = room?.Users.Count(u => u.State == MultiplayerUserState.Ready) ?? 0;

                updateButtonText();
                updateButtonColour();

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

            private void updateButtonText()
            {
                var localUser = multiplayerClient.LocalUser;

                int countReady = room?.Users.Count(u => u.State == MultiplayerUserState.Ready) ?? 0;
                int countTotal = room?.Users.Count(u => u.State != MultiplayerUserState.Spectating) ?? 0;

                string countdownText = currentCountdown == null ? string.Empty : $"Starting in {currentCountdown.EndTime - DateTimeOffset.Now:mm\\:ss}";
                string countText = $"({countReady} / {countTotal} ready)";

                if (currentCountdown != null)
                {
                    Text = localUser?.State == MultiplayerUserState.Ready
                        ? $"{countdownText} {countText}"
                        : $"Ready ({countdownText.ToLowerInvariant()})";

                    return;
                }

                switch (localUser?.State)
                {
                    default:
                        Text = "Ready";
                        break;

                    case MultiplayerUserState.Spectating:
                    case MultiplayerUserState.Ready:
                        Text = room?.Host?.Equals(localUser) == true
                            ? $"Start match {countText}"
                            : $"Waiting for host... {countText}";

                        break;
                }
            }

            private void updateButtonColour()
            {
                var localUser = multiplayerClient.LocalUser;

                if (currentCountdown != null)
                {
                    if (localUser?.State == MultiplayerUserState.Ready)
                        setYellow();
                    else
                        setGreen();

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
