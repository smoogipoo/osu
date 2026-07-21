// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Linq;
using System.Threading.Tasks;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Platform;
using osu.Framework.Screens;
using osu.Framework.Threading;
using osu.Game.Extensions;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osu.Game.Graphics.UserInterface;
using osu.Game.Graphics.UserInterfaceV2;
using osu.Game.Online.API;
using osu.Game.Online.API.Requests.Responses;
using osu.Game.Online.Matchmaking;
using osu.Game.Online.Multiplayer;
using osu.Game.Online.Rooms;
using osu.Game.Screens;
using osu.Game.Screens.OnlinePlay.Matchmaking.Queue;
using osu.Game.Screens.OnlinePlay.Matchmaking.RankedPlay;
using osu.Game.Screens.Play;
using osu.Game.Users.Drawables;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Arcade.Screens.RankedPlay
{
    public class RankedPlayArcadeQueueScreen : OsuScreen
    {
        protected override BackgroundScreen CreateBackground() => new RankedPlayBackgroundScreen();

        [Resolved]
        private MultiplayerClient multiplayerClient { get; set; } = null!;

        [Resolved]
        private ArcadeClient arcadeClient { get; set; } = null!;

        [Resolved]
        private IAPIProvider api { get; set; } = null!;

        [Resolved]
        private OsuColour colours { get; set; } = null!;

        [Resolved]
        private GameHost host { get; set; } = null!;

        private readonly Bindable<MatchmakingPool[]?> availablePools = new Bindable<MatchmakingPool[]?>();
        private readonly Bindable<MatchmakingPool?> selectedPool = new Bindable<MatchmakingPool?>();
        private readonly Bindable<bool> canPractice = new Bindable<bool>();
        private readonly Bindable<bool> canQueue = new Bindable<bool>();
        private readonly BindableDictionary<int, ArcadeIdentity> connectedClients = [];
        private readonly ArcadeIdentity identity;

        private OsuSpriteText welcomeText = null!;
        private Container mainContainer = null!;

        private DateTimeOffset practiceEndTime = DateTimeOffset.MaxValue;
        private ScheduledDelegate? scheduledReturnToFromPlayer;
        private bool isQueueing;

        public RankedPlayArcadeQueueScreen(ArcadeIdentity identity)
        {
            this.identity = identity;
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            InternalChildren = new Drawable[]
            {
                welcomeText = new OsuSpriteText
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    RelativePositionAxes = Axes.Y,
                    Text = $"Welcome, {identity.User.Username}",
                    Font = OsuFont.GetFont(size: 72)
                },
                new Container
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    AutoSizeAxes = Axes.Both,
                    Masking = true,
                    CornerRadius = 20,
                    Children = new Drawable[]
                    {
                        new Box
                        {
                            RelativeSizeAxes = Axes.Both,
                            Colour = Color4.Black,
                            Alpha = 0.8f
                        },
                        mainContainer = new Container
                        {
                            AutoSizeAxes = Axes.Both,
                            Padding = new MarginPadding(20)
                        }
                    }
                }
            };
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            welcomeText.FadeInFromZero(1000, Easing.OutQuint)
                       .Delay(1000)
                       .MoveToOffset(new Vector2(0, -0.25f), 500, Easing.OutQuint);

            mainContainer.FadeOut()
                         .Delay(1200)
                         .FadeInFromZero(1000, Easing.OutQuint);

            multiplayerClient.MatchmakingRoomInvited += onMatchmakingRoomInvited;
            multiplayerClient.MatchmakingRoomReady += onMatchmakingRoomReady;

            connectedClients.BindTo(arcadeClient.ConnectedClients);
            connectedClients.BindCollectionChanged(onConnectedClientsChanged, true);

            Task.Run(populateAvailablePools);
        }

        protected override void Update()
        {
            base.Update();

            canPractice.Value = selectedPool.Value != null && practiceEndTime >= DateTimeOffset.Now;
            canQueue.Value = selectedPool.Value != null;
        }

        public void EndPracticeAfter(TimeSpan duration)
        {
            if (duration == TimeSpan.MaxValue)
                practiceEndTime = DateTimeOffset.MaxValue;
            else
                practiceEndTime = DateTimeOffset.Now + duration;
        }

        private void onMatchmakingRoomInvited(MatchmakingRoomInvitationParams e)
            => Schedule(() => multiplayerClient.MatchmakingAcceptInvitation().FireAndForget());

        private void onMatchmakingRoomReady(long roomId, string password) => Schedule(() =>
        {
            multiplayerClient.JoinRoom(new Room { RoomID = roomId }, password)
                             .FireAndForget(() => Schedule(() => this.Push(new RankedPlayScreen(multiplayerClient.Room!))));
        });

        private void onConnectedClientsChanged(object? sender, NotifyDictionaryChangedEventArgs<int, ArcadeIdentity> e)
        {
            if (connectedClients.Count < 2)
            {
                EndPracticeAfter(TimeSpan.MaxValue);
                setState(ArcadeState.WaitingForOpponent);
            }
            else
            {
                EndPracticeAfter(TimeSpan.FromMinutes(2));
                setState(ArcadeState.ReadyForPlay);
            }
        }

        private async Task populateAvailablePools()
        {
            MatchmakingPool[] pools = await multiplayerClient.GetMatchmakingPoolsOfType(MatchmakingPoolType.RankedPlay).ConfigureAwait(false);

            Schedule(() =>
            {
                availablePools.Value = pools;
                selectedPool.Value = pools.FirstOrDefault();
            });
        }

        private void setState(ArcadeState state) => Schedule(() =>
        {
            switch (state)
            {
                case ArcadeState.WaitingForOpponent:
                    mainContainer.Child = new FillFlowContainer
                    {
                        AutoSizeAxes = Axes.Both,
                        Padding = new MarginPadding { Horizontal = 100 },
                        Direction = FillDirection.Vertical,
                        Spacing = new Vector2(10),
                        Children = new Drawable[]
                        {
                            new FillFlowContainer
                            {
                                Anchor = Anchor.TopCentre,
                                Origin = Anchor.TopCentre,
                                AutoSizeAxes = Axes.Both,
                                Direction = FillDirection.Horizontal,
                                Spacing = new Vector2(5),
                                Children = new Drawable[]
                                {
                                    new OsuSpriteText
                                    {
                                        Anchor = Anchor.CentreLeft,
                                        Origin = Anchor.CentreLeft,
                                        Text = "Waiting for your opponent...",
                                        Font = OsuFont.Style.Heading1,
                                    },
                                    new LoadingSpinner
                                    {
                                        Anchor = Anchor.CentreLeft,
                                        Origin = Anchor.CentreLeft,
                                        Size = new Vector2(16),
                                        State = { Value = Visibility.Visible }
                                    }
                                }
                            },
                            new PoolSelector
                            {
                                Anchor = Anchor.TopCentre,
                                Origin = Anchor.TopCentre,
                                AvailablePools = { BindTarget = availablePools },
                                SelectedPool = { BindTarget = selectedPool }
                            },
                            new PracticeButton(practiceEndTime)
                            {
                                Anchor = Anchor.TopCentre,
                                Origin = Anchor.TopCentre,
                                Width = 100,
                                Enabled = { BindTarget = canPractice },
                                Action = enterPractice
                            }
                        }
                    };
                    break;

                case ArcadeState.ReadyForPlay:
                    ArcadeIdentity otherUser = arcadeClient.ConnectedClients.Single(u => u.Key != api.LocalUser.Value.OnlineID).Value;

                    mainContainer.Child = new FillFlowContainer
                    {
                        AutoSizeAxes = Axes.Both,
                        Direction = FillDirection.Vertical,
                        Spacing = new Vector2(10),
                        Children = new Drawable[]
                        {
                            new FillFlowContainer
                            {
                                Anchor = Anchor.TopCentre,
                                Origin = Anchor.TopCentre,
                                AutoSizeAxes = Axes.Both,
                                Direction = FillDirection.Horizontal,
                                Spacing = new Vector2(10),
                                Children = new Drawable[]
                                {
                                    new UserRow(identity.User.ToAPIUser(), Anchor.CentreLeft)
                                    {
                                        Anchor = Anchor.CentreLeft,
                                        Origin = Anchor.CentreLeft,
                                    },
                                    new OsuSpriteText
                                    {
                                        Anchor = Anchor.CentreLeft,
                                        Origin = Anchor.CentreLeft,
                                        Text = "vs",
                                        Font = OsuFont.Style.Heading1,
                                        Margin = new MarginPadding { Horizontal = 20 }
                                    },
                                    new UserRow(otherUser.User.ToAPIUser(), Anchor.CentreRight)
                                    {
                                        Anchor = Anchor.CentreLeft,
                                        Origin = Anchor.CentreLeft,
                                    }
                                }
                            },
                            new PoolSelector
                            {
                                Anchor = Anchor.TopCentre,
                                Origin = Anchor.TopCentre,
                                AvailablePools = { BindTarget = availablePools },
                                SelectedPool = { BindTarget = selectedPool },
                            },
                            new FillFlowContainer
                            {
                                Anchor = Anchor.TopCentre,
                                Origin = Anchor.TopCentre,
                                AutoSizeAxes = Axes.Both,
                                Direction = FillDirection.Horizontal,
                                Spacing = new Vector2(10),
                                Children = new Drawable[]
                                {
                                    new PracticeButton(practiceEndTime)
                                    {
                                        Width = 150,
                                        Enabled = { BindTarget = canPractice },
                                        Action = enterPractice
                                    },
                                    new RoundedButton
                                    {
                                        Width = 150,
                                        Text = "Ready",
                                        BackgroundColour = colours.Green3,
                                        Enabled = { BindTarget = canQueue },
                                        Action = beginQueueing
                                    }
                                }
                            }
                        }
                    };
                    break;

                case ArcadeState.WaitingForStart:
                    mainContainer.Child = new FillFlowContainer
                    {
                        Anchor = Anchor.TopCentre,
                        Origin = Anchor.TopCentre,
                        AutoSizeAxes = Axes.Both,
                        Direction = FillDirection.Horizontal,
                        Spacing = new Vector2(5),
                        Padding = new MarginPadding { Horizontal = 100 },
                        Children = new Drawable[]
                        {
                            new OsuSpriteText
                            {
                                Anchor = Anchor.CentreLeft,
                                Origin = Anchor.CentreLeft,
                                Text = "Waiting for your opponent...",
                                Font = OsuFont.Style.Heading1,
                            },
                            new LoadingSpinner
                            {
                                Anchor = Anchor.CentreLeft,
                                Origin = Anchor.CentreLeft,
                                Size = new Vector2(16),
                                State = { Value = Visibility.Visible }
                            }
                        }
                    };
                    break;
            }
        });

        private void enterPractice()
        {
            scheduledReturnToFromPlayer = host.UpdateThread.Scheduler.AddDelayed(() =>
            {
                if (DateTimeOffset.Now >= practiceEndTime)
                    this.MakeCurrent();
            }, 1000, true);

            this.Push(new PlayerLoader(() => new RankedPlayPracticePlayer()));
        }

        private void beginQueueing()
        {
            if (selectedPool.Value == null)
                return;

            isQueueing = true;

            multiplayerClient.MatchmakingJoinQueue(selectedPool.Value.Id).FireAndForget();

            setState(ArcadeState.WaitingForStart);
        }

        public override void OnResuming(ScreenTransitionEvent e)
        {
            base.OnResuming(e);
            scheduledReturnToFromPlayer?.Cancel();
        }

        public override bool OnExiting(ScreenExitEvent e)
        {
            scheduledReturnToFromPlayer?.Cancel();
            return base.OnExiting(e);
        }

        protected override void Dispose(bool isDisposing)
        {
            base.Dispose(isDisposing);
            scheduledReturnToFromPlayer?.Cancel();
        }

        private enum ArcadeState
        {
            WaitingForOpponent,
            ReadyForPlay,
            WaitingForStart
        }

        private class UserRow : CompositeDrawable
        {
            public UserRow(APIUser user, Anchor contentAnchor)
            {
                AutoSizeAxes = Axes.Both;

                InternalChildren = new Drawable[]
                {
                    new FillFlowContainer
                    {
                        AutoSizeAxes = Axes.Both,
                        Direction = FillDirection.Horizontal,
                        Spacing = new Vector2(5),
                        Children = new Drawable[]
                        {
                            new UpdateableAvatar(user, false)
                            {
                                Anchor = contentAnchor,
                                Origin = contentAnchor,
                                Size = new Vector2(64)
                            },
                            new OsuSpriteText
                            {
                                Anchor = contentAnchor,
                                Origin = contentAnchor,
                                Text = user.Username,
                                Font = OsuFont.Style.Heading2
                            }
                        }
                    }
                };
            }
        }

        private class PracticeButton : RoundedButton
        {
            private readonly DateTimeOffset endTime;

            public PracticeButton(DateTimeOffset endTime)
            {
                this.endTime = endTime;
            }

            protected override void Update()
            {
                base.Update();

                if (endTime == DateTimeOffset.MaxValue)
                    Text = "Practice";
                else
                {
                    TimeSpan remaining = endTime - DateTimeOffset.Now;

                    if (remaining < TimeSpan.Zero)
                        remaining = TimeSpan.Zero;

                    Text = $"Practice ({remaining.ToFormattedDuration()})";
                }
            }
        }
    }
}
