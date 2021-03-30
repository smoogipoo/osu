// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using JetBrains.Annotations;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Screens;
using osu.Game.Audio;
using osu.Game.Beatmaps;
using osu.Game.Configuration;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osu.Game.Graphics.UserInterface;
using osu.Game.Online.API;
using osu.Game.Online.API.Requests;
using osu.Game.Online.Spectator;
using osu.Game.Overlays.BeatmapListing.Panels;
using osu.Game.Overlays.Settings;
using osu.Game.Replays;
using osu.Game.Rulesets;
using osu.Game.Rulesets.Replays;
using osu.Game.Rulesets.Replays.Types;
using osu.Game.Scoring;
using osu.Game.Screens.OnlinePlay.Match.Components;
using osu.Game.Users;
using osuTK;

namespace osu.Game.Screens.Play
{
    public abstract class AbstractSpectator : OsuScreen
    {
        [Resolved]
        protected BeatmapManager Beatmaps { get; private set; }

        [Resolved]
        protected RulesetStore Rulesets { get; private set; }

        [Resolved]
        private SpectatorStreamingClient spectatorClient { get; set; }

        private readonly object usersLock = new object();
        private readonly Dictionary<int, UserState> userStates = new Dictionary<int, UserState>();

        private IBindable<WeakReference<BeatmapSetInfo>> managerUpdated;

        protected AbstractSpectator(params User[] users)
        {
            foreach (var user in users)
                userStates[user.Id] = new UserState(user);
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            spectatorClient.OnUserBeganPlaying += userBeganPlaying;
            spectatorClient.OnUserFinishedPlaying += userFinishedPlaying;
            spectatorClient.OnNewFrames += userSentFrames;

            lock (usersLock)
            {
                foreach (var (_, state) in userStates)
                    spectatorClient.WatchUser(state.User.Id);
            }

            managerUpdated = Beatmaps.ItemUpdated.GetBoundCopy();
            managerUpdated.BindValueChanged(beatmapUpdated);
        }

        private void beatmapUpdated(ValueChangedEvent<WeakReference<BeatmapSetInfo>> beatmap)
        {
            if (!beatmap.NewValue.TryGetTarget(out var beatmapSet))
                return;

            var usersToStart = new List<int>();

            lock (usersLock)
            {
                foreach (var (_, state) in userStates)
                {
                    lock (state.StateLock)
                    {
                        if (beatmapSet.Beatmaps.Any(b => b.OnlineBeatmapID == state.SpectatorState.BeatmapID))
                            usersToStart.Add(state.User.Id);
                    }
                }
            }

            foreach (var user in usersToStart)
                updateGameplayState(user);
        }

        private void userBeganPlaying(int userId, SpectatorState state)
        {
            if (state.RulesetID == null || state.BeatmapID == null)
                return;

            if (!tryGetUserState(userId, out var userState))
                return;

            lock (userState.StateLock)
                userState.SpectatorState = state;

            OnUserStateChanged(userState);

            updateGameplayState(userId);
        }

        private void userSentFrames(int userId, FrameDataBundle bundle)
        {
            if (!tryGetUserState(userId, out var userState))
                return;

            lock (userState.StateLock)
            {
                var gameplayState = userState.GameplayState;
                if (gameplayState == null)
                    return;

                // The ruleset instance should be guaranteed to be in sync with the score via ScoreLock.
                Debug.Assert(gameplayState.Ruleset != null && gameplayState.Ruleset.RulesetInfo.Equals(gameplayState.Score.ScoreInfo.Ruleset));

                foreach (var frame in bundle.Frames)
                {
                    IConvertibleReplayFrame convertibleFrame = gameplayState.Ruleset.CreateConvertibleReplayFrame();
                    convertibleFrame.FromLegacy(frame, gameplayState.Beatmap);

                    var convertedFrame = (ReplayFrame)convertibleFrame;
                    convertedFrame.Time = frame.Time;

                    gameplayState.Score.Replay.Frames.Add(convertedFrame);
                }
            }
        }

        private void userFinishedPlaying(int userId, SpectatorState state)
        {
            if (!tryGetUserState(userId, out var userState))
                return;

            lock (userState.StateLock)
            {
                var gameplayState = userState.GameplayState;
                if (gameplayState == null)
                    return;

                gameplayState.Score.Replay.HasReceivedAllFrames = true;
                userState.GameplayState = null;

                OnGameplayStateChanged(userState.User, userState.GameplayState);
            }
        }

        private void updateGameplayState(int userId)
        {
            if (!tryGetUserState(userId, out var userState))
                return;

            lock (userState.StateLock)
            {
                var resolvedRuleset = Rulesets.AvailableRulesets.FirstOrDefault(r => r.ID == userState.SpectatorState.RulesetID)?.CreateInstance();
                if (resolvedRuleset == null)
                    return;

                var resolvedBeatmap = Beatmaps.QueryBeatmap(b => b.OnlineBeatmapID == userState.SpectatorState.BeatmapID);
                if (resolvedBeatmap == null)
                    return;

                var score = new Score
                {
                    ScoreInfo = new ScoreInfo
                    {
                        Beatmap = resolvedBeatmap,
                        User = userState.User,
                        Mods = userState.SpectatorState.Mods.Select(m => m.ToMod(resolvedRuleset)).ToArray(),
                        Ruleset = resolvedRuleset.RulesetInfo,
                    },
                    Replay = new Replay { HasReceivedAllFrames = false },
                };

                // Todo: This is completely wrong.
                var playableBeatmap = Beatmaps.GetWorkingBeatmap(resolvedBeatmap).Beatmap;

                userState.GameplayState = new GameplayState(score, resolvedRuleset, playableBeatmap);
                OnGameplayStateChanged(userState.User, userState.GameplayState);
            }
        }

        private bool tryGetUserState(int userId, out UserState state)
        {
            lock (usersLock)
                return userStates.TryGetValue(userId, out state);
        }

        protected abstract void OnUserStateChanged(UserState userState);

        protected abstract void OnGameplayStateChanged([NotNull] User user, [CanBeNull] GameplayState gameplayState);

        protected override void Dispose(bool isDisposing)
        {
            base.Dispose(isDisposing);

            if (spectatorClient != null)
            {
                spectatorClient.OnUserBeganPlaying -= userBeganPlaying;
                spectatorClient.OnUserFinishedPlaying -= userFinishedPlaying;
                spectatorClient.OnNewFrames -= userSentFrames;

                lock (usersLock)
                {
                    foreach (var (_, state) in userStates)
                        spectatorClient.StopWatchingUser(state.User.Id);
                }
            }

            managerUpdated?.UnbindAll();
        }
    }

    public class UserState
    {
        public readonly object StateLock = new object();

        public readonly User User;

        public SpectatorState SpectatorState;
        public GameplayState GameplayState;

        public UserState(User user)
        {
            User = user;
        }
    }

    public class GameplayState
    {
        public readonly Score Score;
        public readonly Ruleset Ruleset;
        public readonly IBeatmap Beatmap;

        public GameplayState(Score score, Ruleset ruleset, IBeatmap beatmap)
        {
            Score = score;
            Ruleset = ruleset;
            Beatmap = beatmap;
        }
    }

    [Cached(typeof(IPreviewTrackOwner))]
    public class Spectator : AbstractSpectator, IPreviewTrackOwner
    {
        [NotNull]
        private readonly User targetUser;

        [Resolved]
        private IAPIProvider api { get; set; }

        [Resolved]
        private PreviewTrackManager previewTrackManager { get; set; }

        private Container beatmapPanelContainer;
        private TriangleButton watchButton;
        private SettingsCheckbox automaticDownload;
        private BeatmapSetInfo onlineBeatmap;

        private GameplayState currentGameplayState;
        private GetBeatmapSetRequest onlineBeatmapRequest;

        public Spectator([NotNull] User targetUser)
            : base(targetUser)
        {
            this.targetUser = targetUser;
        }

        [BackgroundDependencyLoader]
        private void load(OsuColour colours, OsuConfigManager config)
        {
            InternalChild = new Container
            {
                Masking = true,
                CornerRadius = 20,
                AutoSizeAxes = Axes.Both,
                AutoSizeDuration = 500,
                AutoSizeEasing = Easing.OutQuint,
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                Children = new Drawable[]
                {
                    new Box
                    {
                        Colour = colours.GreySeafoamDark,
                        RelativeSizeAxes = Axes.Both,
                    },
                    new FillFlowContainer
                    {
                        Margin = new MarginPadding(20),
                        AutoSizeAxes = Axes.Both,
                        Direction = FillDirection.Vertical,
                        Anchor = Anchor.Centre,
                        Origin = Anchor.Centre,
                        Spacing = new Vector2(15),
                        Children = new Drawable[]
                        {
                            new OsuSpriteText
                            {
                                Text = "Spectator Mode",
                                Font = OsuFont.Default.With(size: 30),
                                Anchor = Anchor.Centre,
                                Origin = Anchor.Centre,
                            },
                            new FillFlowContainer
                            {
                                AutoSizeAxes = Axes.Both,
                                Direction = FillDirection.Horizontal,
                                Anchor = Anchor.Centre,
                                Origin = Anchor.Centre,
                                Spacing = new Vector2(15),
                                Children = new Drawable[]
                                {
                                    new UserGridPanel(targetUser)
                                    {
                                        Anchor = Anchor.CentreLeft,
                                        Origin = Anchor.CentreLeft,
                                        Height = 145,
                                        Width = 290,
                                    },
                                    new SpriteIcon
                                    {
                                        Size = new Vector2(40),
                                        Icon = FontAwesome.Solid.ArrowRight,
                                        Anchor = Anchor.CentreLeft,
                                        Origin = Anchor.CentreLeft,
                                    },
                                    beatmapPanelContainer = new Container
                                    {
                                        AutoSizeAxes = Axes.Both,
                                        Anchor = Anchor.CentreLeft,
                                        Origin = Anchor.CentreLeft,
                                    },
                                }
                            },
                            automaticDownload = new SettingsCheckbox
                            {
                                LabelText = "Automatically download beatmaps",
                                Current = config.GetBindable<bool>(OsuSetting.AutomaticallyDownloadWhenSpectating),
                                Anchor = Anchor.Centre,
                                Origin = Anchor.Centre,
                            },
                            watchButton = new PurpleTriangleButton
                            {
                                Text = "Start Watching",
                                Width = 250,
                                Anchor = Anchor.Centre,
                                Origin = Anchor.Centre,
                                Action = attemptStart,
                                Enabled = { Value = false }
                            }
                        }
                    }
                }
            };
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();
            automaticDownload.Current.BindValueChanged(_ => checkForAutomaticDownload());
        }

        protected override void OnGameplayStateChanged(User user, GameplayState gameplayState) => Schedule(() =>
        {
            currentGameplayState = gameplayState;

            if (gameplayState == null)
                Schedule(clearDisplay);
            else if (this.IsCurrentScreen())
                Schedule(attemptStart);
        });

        protected override void OnUserStateChanged(UserState userState)
        {
            clearDisplay();
            showBeatmapPanel(userState.SpectatorState);
        }

        public override void OnResuming(IScreen last)
        {
            base.OnResuming(last);
            attemptStart();
        }

        private void clearDisplay()
        {
            watchButton.Enabled.Value = false;
            beatmapPanelContainer.Clear();
            previewTrackManager.StopAnyPlaying(this);
        }

        private void attemptStart()
        {
            if (currentGameplayState == null)
                return;

            this.Push(new SpectatorPlayerLoader(currentGameplayState.Score));
        }

        private void showBeatmapPanel(SpectatorState state)
        {
            Debug.Assert(state.BeatmapID != null);

            onlineBeatmapRequest = new GetBeatmapSetRequest(state.BeatmapID.Value, BeatmapSetLookupType.BeatmapId);
            onlineBeatmapRequest.Success += res => Schedule(() =>
            {
                onlineBeatmap = res.ToBeatmapSet(Rulesets);
                beatmapPanelContainer.Child = new GridBeatmapPanel(onlineBeatmap);
                checkForAutomaticDownload();
            });

            api.Queue(onlineBeatmapRequest);
        }

        private void checkForAutomaticDownload()
        {
            if (onlineBeatmap == null)
                return;

            if (!automaticDownload.Current.Value)
                return;

            if (Beatmaps.IsAvailableLocally(onlineBeatmap))
                return;

            Beatmaps.Download(onlineBeatmap);
        }

        public override bool OnExiting(IScreen next)
        {
            previewTrackManager.StopAnyPlaying(this);
            return base.OnExiting(next);
        }
    }
}
