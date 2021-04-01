// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using JetBrains.Annotations;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Game.Beatmaps;
using osu.Game.Database;
using osu.Game.Online.Spectator;
using osu.Game.Replays;
using osu.Game.Rulesets;
using osu.Game.Rulesets.Replays;
using osu.Game.Rulesets.Replays.Types;
using osu.Game.Scoring;
using osu.Game.Users;

namespace osu.Game.Screens.Spectate
{
    public abstract class SpectatorScreen : OsuScreen
    {
        protected readonly int[] UserIds;

        [Resolved]
        protected BeatmapManager Beatmaps { get; private set; }

        [Resolved]
        protected RulesetStore Rulesets { get; private set; }

        [Resolved]
        protected SpectatorStreamingClient SpectatorClient { get; private set; }

        [Resolved]
        private UserLookupCache userLookupCache { get; set; }

        private readonly object stateLock = new object();

        private readonly Dictionary<int, User> userMap = new Dictionary<int, User>();
        private readonly Dictionary<int, SpectatorState> spectatorStates = new Dictionary<int, SpectatorState>();
        private readonly Dictionary<int, GameplayState> gameplayStates = new Dictionary<int, GameplayState>();

        private IBindable<WeakReference<BeatmapSetInfo>> managerUpdated;

        protected SpectatorScreen(params int[] userIds)
        {
            UserIds = userIds;
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            SpectatorClient.OnUserBeganPlaying += userBeganPlaying;
            SpectatorClient.OnUserFinishedPlaying += userFinishedPlaying;
            SpectatorClient.OnNewFrames += userSentFrames;

            foreach (var id in UserIds)
            {
                userLookupCache.GetUserAsync(id).ContinueWith(u => Schedule(() =>
                {
                    if (u.Result == null)
                        return;

                    lock (stateLock)
                        userMap[id] = u.Result;

                    SpectatorClient.WatchUser(id);
                }), TaskContinuationOptions.OnlyOnRanToCompletion);
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
                if (!userMap.ContainsKey(userId))
                    return;

                spectatorStates[userId] = state;
                OnUserStateChanged(userId, state);

                updateGameplayState(userId);
            }
        }

        private void updateGameplayState(int userId)
        {
            lock (stateLock)
            {
                Debug.Assert(userMap.ContainsKey(userId));

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
                OnGameplayStateChanged(userId, gameplayState);
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
                if (!userMap.ContainsKey(userId))
                    return;

                if (!gameplayStates.TryGetValue(userId, out var gameplayState))
                    return;

                gameplayState.Score.Replay.HasReceivedAllFrames = true;

                gameplayStates.Remove(userId);
                OnGameplayStateChanged(userId, null);
            }
        }

        protected abstract void OnUserStateChanged(int userId, SpectatorState spectatorState);

        protected abstract void OnGameplayStateChanged(int userId, [CanBeNull] GameplayState gameplayState);

        protected override void Dispose(bool isDisposing)
        {
            base.Dispose(isDisposing);

            if (SpectatorClient != null)
            {
                SpectatorClient.OnUserBeganPlaying -= userBeganPlaying;
                SpectatorClient.OnUserFinishedPlaying -= userFinishedPlaying;
                SpectatorClient.OnNewFrames -= userSentFrames;

                lock (stateLock)
                {
                    foreach (var (userId, _) in userMap)
                        SpectatorClient.StopWatchingUser(userId);
                }
            }

            managerUpdated?.UnbindAll();
        }
    }
}
