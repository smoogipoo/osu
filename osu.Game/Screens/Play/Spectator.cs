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

        private readonly object stateLock = new object();

        private readonly Dictionary<int, User> userMap = new Dictionary<int, User>();
        private readonly Dictionary<int, SpectatorState> spectatorStates = new Dictionary<int, SpectatorState>();
        private readonly Dictionary<int, GameplayState> gameplayStates = new Dictionary<int, GameplayState>();

        private IBindable<WeakReference<BeatmapSetInfo>> managerUpdated;

        protected AbstractSpectator(params User[] users)
        {
            foreach (var user in users)
                userMap[user.Id] = user;
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            spectatorClient.OnUserBeganPlaying += userBeganPlaying;
            spectatorClient.OnUserFinishedPlaying += userFinishedPlaying;
            spectatorClient.OnNewFrames += userSentFrames;

            lock (stateLock)
            {
                foreach (var (userId, _) in userMap)
                    spectatorClient.WatchUser(userId);
            }

            managerUpdated = Beatmaps.ItemUpdated.GetBoundCopy();
            managerUpdated.BindValueChanged(beatmapUpdated);
        }

        private void beatmapUpdated(ValueChangedEvent<WeakReference<BeatmapSetInfo>> beatmap)
        {
            if (!beatmap.NewValue.TryGetTarget(out var beatmapSet))
                return;

            lock (stateLock)
            {
                foreach (var (userId, state) in spectatorStates)
                {
                    if (beatmapSet.Beatmaps.Any(b => b.OnlineBeatmapID == state.BeatmapID))
                        updateGameplayState(userId);
                }
            }
        }

        private void userBeganPlaying(int userId, SpectatorState state)
        {
            if (state.RulesetID == null || state.BeatmapID == null)
                return;

            lock (stateLock)
            {
                if (!userMap.TryGetValue(userId, out var user))
                    return;

                spectatorStates[userId] = state;
                OnUserStateChanged(user, state);

                updateGameplayState(userId);
            }
        }

        private void updateGameplayState(int userId)
        {
            lock (stateLock)
            {
                var spectatorState = spectatorStates[userId];
                var user = userMap[userId];

                var resolvedRuleset = Rulesets.AvailableRulesets.FirstOrDefault(r => r.ID == spectatorState.RulesetID)?.CreateInstance();
                if (resolvedRuleset == null)
                    return;

                var resolvedBeatmap = Beatmaps.QueryBeatmap(b => b.OnlineBeatmapID == spectatorState.BeatmapID);
                if (resolvedBeatmap == null)
                    return;

                var score = new Score
                {
                    ScoreInfo = new ScoreInfo
                    {
                        Beatmap = resolvedBeatmap,
                        User = user,
                        Mods = spectatorState.Mods.Select(m => m.ToMod(resolvedRuleset)).ToArray(),
                        Ruleset = resolvedRuleset.RulesetInfo,
                    },
                    Replay = new Replay { HasReceivedAllFrames = false },
                };

                var gameplayState = new GameplayState(score, resolvedRuleset, Beatmaps.GetWorkingBeatmap(resolvedBeatmap));

                gameplayStates[userId] = gameplayState;
                OnGameplayStateChanged(user, gameplayState);
            }
        }

        private void userSentFrames(int userId, FrameDataBundle bundle)
        {
            lock (stateLock)
            {
                if (!userMap.ContainsKey(userId))
                    return;

                if (!gameplayStates.TryGetValue(userId, out var gameplayState))
                    return;

                // The ruleset instance should be guaranteed to be in sync with the score via ScoreLock.
                Debug.Assert(gameplayState.Ruleset != null && gameplayState.Ruleset.RulesetInfo.Equals(gameplayState.Score.ScoreInfo.Ruleset));

                foreach (var frame in bundle.Frames)
                {
                    IConvertibleReplayFrame convertibleFrame = gameplayState.Ruleset.CreateConvertibleReplayFrame();
                    convertibleFrame.FromLegacy(frame, gameplayState.Beatmap.Beatmap);

                    var convertedFrame = (ReplayFrame)convertibleFrame;
                    convertedFrame.Time = frame.Time;

                    gameplayState.Score.Replay.Frames.Add(convertedFrame);
                }
            }
        }

        private void userFinishedPlaying(int userId, SpectatorState state)
        {
            lock (stateLock)
            {
                if (!userMap.TryGetValue(userId, out var user))
                    return;

                if (!gameplayStates.TryGetValue(userId, out var gameplayState))
                    return;

                gameplayState.Score.Replay.HasReceivedAllFrames = true;

                gameplayStates.Remove(userId);
                OnGameplayStateChanged(user, null);
            }
        }

        protected abstract void OnUserStateChanged(User user, SpectatorState spectatorState);

        protected abstract void OnGameplayStateChanged([NotNull] User user, [CanBeNull] GameplayState gameplayState);

        protected override void Dispose(bool isDisposing)
        {
            base.Dispose(isDisposing);

            if (spectatorClient != null)
            {
                spectatorClient.OnUserBeganPlaying -= userBeganPlaying;
                spectatorClient.OnUserFinishedPlaying -= userFinishedPlaying;
                spectatorClient.OnNewFrames -= userSentFrames;

                lock (stateLock)
                {
                    foreach (var (userId, _) in userMap)
                        spectatorClient.StopWatchingUser(userId);
                }
            }

            managerUpdated?.UnbindAll();
        }
    }

    public class GameplayState
    {
        public readonly Score Score;
        public readonly Ruleset Ruleset;
        public readonly WorkingBeatmap Beatmap;

        public GameplayState(Score score, Ruleset ruleset, WorkingBeatmap beatmap)
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

        /// <summary>
        /// The player's immediate online gameplay state.
        /// This doesn't reflect the gameplay state being watched by the user if <see cref="pendingGameplayState"/> is non-null.
        /// </summary>
        private GameplayState immediateGameplayState;

        /// <summary>
        /// The gameplay state that is pending to be watched, upon this screen becoming current.
        /// </summary>
        private GameplayState pendingGameplayState;

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
                                Action = () => attemptStart(immediateGameplayState),
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
            pendingGameplayState = null;
            immediateGameplayState = gameplayState;

            if (gameplayState == null)
                Schedule(clearDisplay);
            else if (this.IsCurrentScreen())
                Schedule(() => attemptStart(gameplayState));
            else
                pendingGameplayState = gameplayState;

            watchButton.Enabled.Value = true;
        });

        protected override void OnUserStateChanged(User user, SpectatorState spectatorState) => Schedule(() =>
        {
            clearDisplay();
            showBeatmapPanel(spectatorState);
        });

        public override void OnResuming(IScreen last)
        {
            base.OnResuming(last);

            if (pendingGameplayState != null)
            {
                attemptStart(pendingGameplayState);
                pendingGameplayState = null;
            }
        }

        private void clearDisplay()
        {
            watchButton.Enabled.Value = false;
            onlineBeatmapRequest?.Cancel();
            beatmapPanelContainer.Clear();
            previewTrackManager.StopAnyPlaying(this);
        }

        private void attemptStart(GameplayState gameplayState)
        {
            if (gameplayState == null)
                return;

            Beatmap.Value = gameplayState.Beatmap;
            Ruleset.Value = gameplayState.Ruleset.RulesetInfo;

            this.Push(new SpectatorPlayerLoader(gameplayState.Score));
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
