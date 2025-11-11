// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using osu.Framework.Allocation;
using osu.Framework.Audio;
using osu.Framework.Audio.Sample;
using osu.Framework.Bindables;
using osu.Framework.Extensions;
using osu.Framework.Extensions.ObjectExtensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Input.Bindings;
using osu.Framework.Input.Events;
using osu.Framework.Screens;
using osu.Framework.Threading;
using osu.Game.Database;
using osu.Game.Graphics;
using osu.Game.Graphics.Containers;
using osu.Game.Graphics.Sprites;
using osu.Game.Graphics.UserInterface;
using osu.Game.Input.Bindings;
using osu.Game.Online.API;
using osu.Game.Online.API.Requests.Responses;
using osu.Game.Online.Matchmaking;
using osu.Game.Online.Multiplayer;
using osu.Game.Overlays;
using osu.Game.Overlays.Dialog;
using osu.Game.Rulesets;
using osu.Game.Screens.OnlinePlay.Matchmaking.Match;
using osuTK;

namespace osu.Game.Screens.OnlinePlay.Matchmaking.Queue
{
    public partial class ChallengeList : CompositeDrawable
    {
        [Resolved]
        private QueueController controller { get; set; } = null!;

        [Resolved]
        private OverlayColourProvider colourProvider { get; set; } = null!;

        private readonly BindableList<int> activeChallenges = new BindableList<int>();

        private FillFlowContainer<ChallengeRow> rows = null!;

        [BackgroundDependencyLoader]
        private void load()
        {
            InternalChildren = new Drawable[]
            {
                new Container
                {
                    RelativeSizeAxes = Axes.Both,
                    Masking = true,
                    CornerRadius = 10,
                    Child = new Box
                    {
                        Colour = colourProvider.Background3,
                        RelativeSizeAxes = Axes.Both,
                    },
                },
                new OsuSpriteText
                {
                    Anchor = Anchor.TopCentre,
                    Origin = Anchor.TopCentre,
                    Text = "Challenges",
                    Margin = new MarginPadding { Top = 5 }
                },
                new GridContainer
                {
                    RelativeSizeAxes = Axes.Both,
                    Padding = new MarginPadding
                    {
                        Top = 30,
                        Bottom = 10,
                        Horizontal = 10,
                    },
                    RowDimensions =
                    [
                        new Dimension(GridSizeMode.AutoSize),
                        new Dimension(GridSizeMode.AutoSize),
                        new Dimension()
                    ],
                    Content = new[]
                    {
                        new Drawable[]
                        {
                            new Container
                            {
                                RelativeSizeAxes = Axes.X,
                                AutoSizeAxes = Axes.Y,
                                Children = new Drawable[]
                                {
                                    new OsuSpriteText
                                    {
                                        Anchor = Anchor.CentreLeft,
                                        Origin = Anchor.CentreLeft,
                                        Text = "Name",
                                        Font = OsuFont.Default.With(size: 14f),
                                        Colour = colourProvider.Content2
                                    },
                                    new OsuSpriteText
                                    {
                                        Anchor = Anchor.TopCentre,
                                        Origin = Anchor.TopCentre,
                                        Text = "Rating",
                                        Font = OsuFont.Default.With(size: 14f),
                                        Colour = colourProvider.Content2
                                    }
                                }
                            }
                        },
                        new Drawable[]
                        {
                            new Container
                            {
                                RelativeSizeAxes = Axes.X,
                                AutoSizeAxes = Axes.Y,
                                Padding = new MarginPadding { Vertical = 5 },
                                Child = new Box
                                {
                                    RelativeSizeAxes = Axes.X,
                                    Height = 2
                                }
                            }
                        },
                        new Drawable[]
                        {
                            new OsuScrollContainer
                            {
                                RelativeSizeAxes = Axes.Both,
                                Child = rows = new FillFlowContainer<ChallengeRow>
                                {
                                    RelativeSizeAxes = Axes.X,
                                    AutoSizeAxes = Axes.Y
                                }
                            }
                        }
                    }
                }
            };
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            activeChallenges.BindTo(controller.ActiveChallenges);
            activeChallenges.BindCollectionChanged(onActiveChallengesChanged, true);
        }

        private void onActiveChallengesChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    foreach (int userId in e.NewItems!.OfType<int>())
                        rows.Add(new ChallengeRow(userId));
                    break;

                case NotifyCollectionChangedAction.Remove:
                    foreach (int userId in e.NewItems!.OfType<int>())
                        rows.RemoveAll(u => u.UserId == userId, true);
                    break;
            }
        }

        private class ChallengeRow : CompositeDrawable
        {
            public readonly int UserId;

            [Resolved]
            private UserLookupCache userLookupCache { get; set; } = null!;

            [Resolved]
            private OsuColour colours { get; set; } = null!;

            [Resolved]
            private MultiplayerClient client { get; set; } = null!;

            public ChallengeRow(int userId)
            {
                UserId = userId;

                RelativeSizeAxes = Axes.X;
                AutoSizeAxes = Axes.Y;
            }

            [BackgroundDependencyLoader]
            private void load()
            {
                APIUser user = userLookupCache.GetUserAsync(UserId).GetResultSafely()!;

                InternalChildren = new Drawable[]
                {
                    new OsuSpriteText
                    {
                        Anchor = Anchor.CentreLeft,
                        Origin = Anchor.CentreLeft,
                        Text = user.Username
                    },
                    new OsuSpriteText
                    {
                        Anchor = Anchor.Centre,
                        Origin = Anchor.Centre,
                        Text = "1234"
                    },
                    new ShearedButton(height: 16f)
                    {
                        Anchor = Anchor.CentreRight,
                        Origin = Anchor.CentreRight,
                        Text = "Accept",
                        TextSize = 14f,
                        DarkerColour = colours.Green4,
                        LighterColour = colours.Green2,
                        Action = () =>
                        {
                            client.MatchmakingAcceptChallenge(UserId);
                            Expire();
                        }
                    },
                };
            }
        }
    }

    /// <summary>
    /// The initial screen that users arrive at when preparing for a quick play session.
    /// </summary>
    public partial class ScreenQueue : OsuScreen
    {
        public override bool ShowFooter => true;

        private Container mainContent = null!;

        private MatchmakingScreenState state;
        private CloudVisualisation cloud = null!;

        [Resolved]
        private IAPIProvider api { get; set; } = null!;

        [Resolved]
        private OverlayColourProvider colourProvider { get; set; } = null!;

        [Resolved]
        private OsuColour colours { get; set; } = null!;

        [Resolved]
        private MultiplayerClient client { get; set; } = null!;

        [Resolved]
        private IDialogOverlay dialogOverlay { get; set; } = null!;

        [Resolved]
        private QueueController controller { get; set; } = null!;

        [Resolved]
        private UserLookupCache userLookupCache { get; set; } = null!;

        [Resolved]
        private IBindable<RulesetInfo> ruleset { get; set; } = null!;

        [Resolved]
        private MusicController musicController { get; set; } = null!;

        private readonly IBindable<MatchmakingScreenState> currentState = new Bindable<MatchmakingScreenState>();

        private readonly Bindable<MatchmakingPool[]> availablePools = new Bindable<MatchmakingPool[]>();
        private readonly Bindable<MatchmakingPool?> selectedPool = new Bindable<MatchmakingPool?>();

        private CancellationTokenSource userLookupCancellation = new CancellationTokenSource();

        private Sample? enqueueSample;
        private Sample? waitingLoopSample;
        private Sample? matchFoundSample;

        private SampleChannel? waitingLoopChannel;
        private ScheduledDelegate? startLoopPlaybackDelegate;

        protected override void LoadComplete()
        {
            base.LoadComplete();

            InternalChildren = new Drawable[]
            {
                cloud = new CloudVisualisation
                {
                    Y = -100,
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    RelativeSizeAxes = Axes.Both,
                    Size = new Vector2(0.6f)
                },
                new MatchmakingAvatar(api.LocalUser.Value, true)
                {
                    Y = -100,
                    Scale = new Vector2(3),
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                },
                new ChallengeList
                {
                    RelativePositionAxes = Axes.Y,
                    Y = 0.25f,
                    Anchor = Anchor.CentreLeft,
                    Origin = Anchor.CentreLeft,
                    Size = new Vector2(250, 150)
                },
                new Container
                {
                    RelativePositionAxes = Axes.Y,
                    Y = 0.25f,
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    AutoSizeAxes = Axes.Both,
                    CornerRadius = 10f,
                    Masking = true,
                    Children = new Drawable[]
                    {
                        new Box
                        {
                            Colour = colourProvider.Background3,
                            RelativeSizeAxes = Axes.Both,
                        },
                        mainContent = new Container
                        {
                            Anchor = Anchor.Centre,
                            Origin = Anchor.Centre,
                            Alpha = 0,
                            AutoSizeAxes = Axes.Both,
                            AutoSizeDuration = 300,
                            AutoSizeEasing = Easing.OutQuint,
                            Padding = new MarginPadding(20),
                        },
                    }
                },
            };

            currentState.BindTo(controller.CurrentState);
            currentState.BindValueChanged(s => SetState(s.NewValue));

            client.MatchmakingLobbyStatusChanged += onMatchmakingLobbyStatusChanged;

            populateAvailablePools().FireAndForget();
        }

        private async Task populateAvailablePools()
        {
            MatchmakingPool[] pools = await client.GetMatchmakingPools().ConfigureAwait(false);

            Schedule(() =>
            {
                availablePools.Value = pools;

                // Default to the user's ruleset for the initial pool selection.
                selectedPool.Value = pools.FirstOrDefault(p => p.RulesetId == ruleset.Value.OnlineID) ?? pools.FirstOrDefault();
            });
        }

        [BackgroundDependencyLoader]
        private void load(AudioManager audio)
        {
            enqueueSample = audio.Samples.Get(@"Multiplayer/Matchmaking/enqueue");
            waitingLoopSample = audio.Samples.Get(@"Multiplayer/Matchmaking/waiting-loop");
            matchFoundSample = audio.Samples.Get(@"Multiplayer/Matchmaking/match-found");
        }

        private void onMatchmakingLobbyStatusChanged(MatchmakingLobbyStatus status) => Scheduler.Add(() =>
        {
            userLookupCancellation.Cancel();
            var cancellation = userLookupCancellation = new CancellationTokenSource();

            userLookupCache.GetUsersAsync(status.UsersInQueue, cancellation.Token)
                           .ContinueWith(result => Schedule(() =>
                           {
                               APIUser?[] users = result.GetResultSafely();
                               if (!cancellation.IsCancellationRequested)
                                   Users = users.OfType<APIUser>().ToArray();
                           }), cancellation.Token);
        });

        public override void OnEntering(ScreenTransitionEvent e)
        {
            base.OnEntering(e);

            controller.SearchInForeground();

            client.MatchmakingJoinLobby().FireAndForget();

            using (BeginDelayedSequence(800))
                Schedule(() => SetState(currentState.Value));
        }

        public override void OnResuming(ScreenTransitionEvent e)
        {
            base.OnResuming(e);

            client.MatchmakingJoinLobby().FireAndForget();
        }

        public override void OnSuspending(ScreenTransitionEvent e)
        {
            base.OnSuspending(e);

            client.MatchmakingLeaveLobby().FireAndForget();
        }

        private bool exitConfirmed;
        private bool isBackgrounded;

        public override bool OnExiting(ScreenExitEvent e)
        {
            if (base.OnExiting(e))
                return true;

            client.MatchmakingLeaveLobby().FireAndForget();

            if (isBackgrounded)
                return false;

            if (exitConfirmed)
            {
                client.MatchmakingLeaveQueue().FireAndForget();
                return false;
            }

            if (currentState.Value == MatchmakingScreenState.Idle)
                return false;

            if (dialogOverlay.CurrentDialog is ConfirmDialog confirmDialog)
                confirmDialog.PerformOkAction();
            else
            {
                dialogOverlay.Push(new ConfirmDialog("Are you sure you want to leave the matchmaking queue?", () =>
                {
                    exitConfirmed = true;
                    if (this.IsCurrentScreen())
                        this.Exit();
                }));
            }

            return true;
        }

        public APIUser[] Users
        {
            set => cloud.Users = value;
        }

        public void SetState(MatchmakingScreenState newState)
        {
            state = newState;

            mainContent.FadeInFromZero(500, Easing.OutQuint);
            mainContent.Clear();

            startLoopPlaybackDelegate?.Cancel();
            stopWaitingLoopPlayback();

            switch (newState)
            {
                case MatchmakingScreenState.Idle:
                    mainContent.Child = new FillFlowContainer
                    {
                        Anchor = Anchor.Centre,
                        Origin = Anchor.Centre,
                        AutoSizeAxes = Axes.Both,
                        Direction = FillDirection.Vertical,
                        Spacing = new Vector2(10),
                        Children = new Drawable[]
                        {
                            new PoolSelector
                            {
                                Anchor = Anchor.TopCentre,
                                Origin = Anchor.TopCentre,
                                AvailablePools = { BindTarget = availablePools },
                                SelectedPool = { BindTarget = selectedPool }
                            },
                            new BeginQueueingButton(200)
                            {
                                DarkerColour = colours.Blue2,
                                LighterColour = colours.Blue1,
                                Anchor = Anchor.TopCentre,
                                Origin = Anchor.TopCentre,
                                SelectedPool = { BindTarget = selectedPool },
                                Action = () =>
                                {
                                    Debug.Assert(selectedPool.Value != null);
                                    client.MatchmakingJoinQueue(selectedPool.Value.Id).FireAndForget();
                                },
                                Text = "Begin queueing",
                            }
                        }
                    };
                    break;

                case MatchmakingScreenState.Queueing:
                    ShearedButton sendToBackgroundButton;

                    mainContent.Child = new FillFlowContainer
                    {
                        Anchor = Anchor.Centre,
                        Origin = Anchor.Centre,
                        AutoSizeAxes = Axes.Both,
                        Direction = FillDirection.Vertical,
                        Spacing = new Vector2(20),
                        Children = new Drawable[]
                        {
                            new OsuSpriteText
                            {
                                Anchor = Anchor.Centre,
                                Origin = Anchor.Centre,
                                Text = "Waiting for a game...",
                                Font = OsuFont.GetFont(size: 32, weight: FontWeight.Light, typeface: Typeface.TorusAlternate),
                            },
                            new LoadingSpinner
                            {
                                State = { Value = Visibility.Visible },
                            },
                            sendToBackgroundButton = new ShearedButton(200)
                            {
                                DarkerColour = colours.Orange3,
                                LighterColour = colours.Orange4,
                                Anchor = Anchor.Centre,
                                Origin = Anchor.Centre,
                                Text = "Queue in background",
                                Action = () =>
                                {
                                    controller.SearchInBackground();
                                    isBackgrounded = true;
                                    this.Exit();
                                },
                                Enabled = { Value = false },
                                TooltipText = "Wait 5 seconds for this option to become available."
                            }
                        }
                    };

                    Scheduler.AddDelayed(() =>
                    {
                        if (state != newState)
                            return;

                        sendToBackgroundButton.Enabled.Value = true;
                        sendToBackgroundButton.TooltipText = "You will receive a notification when your game is ready. Make sure to watch out for it!";
                    }, 5000);

                    enqueueSample?.Play();
                    startLoopPlaybackDelegate = Scheduler.AddDelayed(startWaitingLoopPlayback, 2000);
                    break;

                case MatchmakingScreenState.PendingAccept:
                    mainContent.Child = new FillFlowContainer
                    {
                        Anchor = Anchor.Centre,
                        Origin = Anchor.Centre,
                        AutoSizeAxes = Axes.Both,
                        Direction = FillDirection.Vertical,
                        Spacing = new Vector2(20),
                        Children = new Drawable[]
                        {
                            new OsuSpriteText
                            {
                                Anchor = Anchor.Centre,
                                Origin = Anchor.Centre,
                                Text = "Found a match!",
                                Font = OsuFont.GetFont(size: 32, weight: FontWeight.Regular, typeface: Typeface.TorusAlternate),
                            },
                            new SelectionButton(200)
                            {
                                DarkerColour = colours.YellowDark,
                                LighterColour = colours.YellowLight,
                                Anchor = Anchor.Centre,
                                Origin = Anchor.Centre,
                                Action = () =>
                                {
                                    client.MatchmakingAcceptInvitation().FireAndForget();
                                    SetState(MatchmakingScreenState.AcceptedWaitingForRoom);
                                },
                                Text = "Join match!",
                            }
                        }
                    };
                    matchFoundSample?.Play();
                    musicController.DuckMomentarily(1250);
                    break;

                case MatchmakingScreenState.AcceptedWaitingForRoom:
                    mainContent.Child = new FillFlowContainer
                    {
                        Anchor = Anchor.Centre,
                        Origin = Anchor.Centre,
                        AutoSizeAxes = Axes.Both,
                        Direction = FillDirection.Vertical,
                        Spacing = new Vector2(20),
                        Children = new Drawable[]
                        {
                            new OsuSpriteText
                            {
                                Anchor = Anchor.Centre,
                                Origin = Anchor.Centre,
                                Text = "Waiting for all players...",
                                Font = OsuFont.GetFont(size: 32, weight: FontWeight.Light, typeface: Typeface.TorusAlternate),
                            },
                            new LoadingSpinner
                            {
                                State = { Value = Visibility.Visible },
                            },
                        }
                    };

                    startWaitingLoopPlayback();
                    break;

                case MatchmakingScreenState.InRoom:
                    // room received, show users and transition to next screen.
                    mainContent.Child = new FillFlowContainer
                    {
                        Anchor = Anchor.Centre,
                        Origin = Anchor.Centre,
                        AutoSizeAxes = Axes.Both,
                        Direction = FillDirection.Vertical,
                        Spacing = new Vector2(20),
                        Children = new Drawable[]
                        {
                            new OsuSpriteText
                            {
                                Anchor = Anchor.Centre,
                                Origin = Anchor.Centre,
                                Text = "Good luck!",
                                Font = OsuFont.GetFont(size: 32, weight: FontWeight.Light, typeface: Typeface.TorusAlternate),
                            },
                        }
                    };

                    using (BeginDelayedSequence(2000))
                        Schedule(() => this.Push(new ScreenMatchmaking(client.Room!)));
                    break;

                default:
                    throw new ArgumentOutOfRangeException(nameof(newState), newState, null);
            }
        }

        protected override void Dispose(bool isDisposing)
        {
            base.Dispose(isDisposing);

            stopWaitingLoopPlayback();

            if (client.IsNotNull())
                client.MatchmakingLobbyStatusChanged -= onMatchmakingLobbyStatusChanged;
        }

        public enum MatchmakingScreenState
        {
            Idle,
            Queueing,
            PendingAccept,
            AcceptedWaitingForRoom,
            InRoom
        }

        private void startWaitingLoopPlayback()
        {
            stopWaitingLoopPlayback();

            waitingLoopChannel = waitingLoopSample?.GetChannel();
            if (waitingLoopChannel == null)
                return;

            waitingLoopChannel.Looping = true;
            waitingLoopChannel?.Play();
        }

        private void stopWaitingLoopPlayback()
        {
            waitingLoopChannel?.Stop();
            waitingLoopChannel?.Dispose();
        }

        private partial class BeginQueueingButton : SelectionButton
        {
            public readonly IBindable<MatchmakingPool?> SelectedPool = new Bindable<MatchmakingPool?>();

            public BeginQueueingButton(float? width = null)
                : base(width)
            {
            }

            protected override void LoadComplete()
            {
                base.LoadComplete();

                SelectedPool.BindValueChanged(p => Enabled.Value = p.NewValue != null, true);
            }
        }

        private partial class SelectionButton : ShearedButton, IKeyBindingHandler<GlobalAction>
        {
            public SelectionButton(float? width = null, float height = DEFAULT_HEIGHT)
                : base(width, height)
            {
            }

            public bool OnPressed(KeyBindingPressEvent<GlobalAction> e)
            {
                if (e.Action == GlobalAction.Select && !e.Repeat)
                {
                    TriggerClickWithSound();
                    return true;
                }

                return false;
            }

            public void OnReleased(KeyBindingReleaseEvent<GlobalAction> e)
            {
            }
        }
    }
}
