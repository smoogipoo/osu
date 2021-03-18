// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Diagnostics;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Beatmaps;
using osu.Game.Database;
using osu.Game.Online.Rooms;
using osu.Game.Online.Spectator;
using osu.Game.Replays;
using osu.Game.Rulesets;
using osu.Game.Rulesets.Replays;
using osu.Game.Rulesets.Replays.Types;
using osu.Game.Scoring;
using osuTK;

namespace osu.Game.Screens.OnlinePlay.Multiplayer.Spectate
{
    public class MultiplayerSpectator : OsuScreen
    {
        private const float player_spacing = 5;
        private const int max_instances = 16;

        // Isolates beatmap/ruleset to this screen.
        public override bool DisallowExternalBeatmapRulesetChanges => true;

        public bool AllPlayersLoaded => instances.All(p => p?.PlayerLoaded == true);

        private readonly object scoreLock = new object();

        private readonly int[] spectatingIds;
        private readonly PlayerInstance[] instances;
        private readonly PlaylistItem playlistItem;

        [Resolved]
        private BeatmapManager beatmapManager { get; set; }

        [Resolved]
        private RulesetStore rulesetStore { get; set; }

        [Resolved]
        private UserLookupCache userLookupCache { get; set; }

        [Resolved]
        private SpectatorStreamingClient spectatorClient { get; set; }

        private Container<PlayerInstance> instanceContainer;
        private Container paddingContainer;
        private FillFlowContainer<PlayerFacade> facades;
        private PlayerFacade maximisedFacade;

        // A depth value that gets decremented every time a new instance is maximised in order to reduce underlaps.
        private float maximisedInstanceDepth = 1;

        public MultiplayerSpectator(PlaylistItem playlistItem, int[] userIds)
        {
            this.playlistItem = playlistItem;

            spectatingIds = new int[Math.Min(max_instances, userIds.Length)];
            instances = new PlayerInstance[spectatingIds.Length];

            userIds.AsSpan().Slice(spectatingIds.Length).CopyTo(spectatingIds);
        }

        [BackgroundDependencyLoader]
        private void load(UserLookupCache userLookupCache)
        {
            InternalChildren = new Drawable[]
            {
                paddingContainer = new Container
                {
                    RelativeSizeAxes = Axes.Both,
                    Padding = new MarginPadding(player_spacing),
                    Children = new Drawable[]
                    {
                        new Container
                        {
                            RelativeSizeAxes = Axes.Both,
                            Child = facades = new FillFlowContainer<PlayerFacade>
                            {
                                RelativeSizeAxes = Axes.X,
                                AutoSizeAxes = Axes.Y,
                                Spacing = new Vector2(player_spacing),
                            }
                        },
                        maximisedFacade = new PlayerFacade { RelativeSizeAxes = Axes.Both }
                    }
                },
                instanceContainer = new Container<PlayerInstance> { RelativeSizeAxes = Axes.Both }
            };

            for (int i = 0; i < spectatingIds.Length; i++)
            {
                var facade = new PlayerFacade();

                facades.Add(facade);
                facades.SetLayoutPosition(facade, i);
            }
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            spectatorClient.OnUserBeganPlaying += userBeganPlaying;
            spectatorClient.OnUserFinishedPlaying += userFinishedPlaying;
            spectatorClient.OnNewFrames += userSentFrames;

            // Add up to 16 player instances.
            foreach (var id in spectatingIds)
                spectatorClient.WatchUser(id);
        }

        protected override void Update()
        {
            base.Update();

            Vector2 cellsPerDimension;

            switch (facades.Count)
            {
                case 1:
                    cellsPerDimension = Vector2.One;
                    break;

                case 2:
                    cellsPerDimension = new Vector2(2, 1);
                    break;

                case 3:
                case 4:
                    cellsPerDimension = new Vector2(2);
                    break;

                case 5:
                case 6:
                    cellsPerDimension = new Vector2(3, 2);
                    break;

                case 7:
                case 8:
                case 9:
                    // 3 rows / 3 cols.
                    cellsPerDimension = new Vector2(3);
                    break;

                case 10:
                case 11:
                case 12:
                    // 3 rows / 4 cols.
                    cellsPerDimension = new Vector2(4, 3);
                    break;

                default:
                    // 4 rows / 4 cols.
                    cellsPerDimension = new Vector2(4);
                    break;
            }

            // Total spacing between cells
            Vector2 totalCellSpacing = player_spacing * (cellsPerDimension - Vector2.One);

            Vector2 fullSize = paddingContainer.ChildSize - totalCellSpacing;
            Vector2 cellSize = Vector2.Divide(fullSize, new Vector2(cellsPerDimension.X, cellsPerDimension.Y));

            foreach (var facade in facades)
            {
                facade.FullSize = fullSize;
                facade.Size = cellSize;
            }
        }

        private void toggleMaximisationState(PlayerInstance target)
        {
            // Iterate through all instances to ensure only one is maximised at any time.
            foreach (var i in instanceContainer)
            {
                if (i == target)
                    i.IsMaximised = !i.IsMaximised;
                else
                    i.IsMaximised = false;

                if (i.IsMaximised)
                {
                    i.SetFacade(maximisedFacade);
                    ChangeInternalChildDepth(i, maximisedInstanceDepth -= 0.001f);
                }
                else
                    i.SetFacade(facades[getIndexForUser(i.Score.ScoreInfo.User.Id)]);
            }
        }

        private void userBeganPlaying(int userId, SpectatorState state)
        {
            if (state.BeatmapID == null) return;
            if (state.RulesetID == null) return;

            lock (scoreLock)
            {
                int userIndex = getIndexForUser(userId);
                var userBeatmap = beatmapManager.QueryBeatmap(b => b.OnlineBeatmapID == state.BeatmapID);
                var userRuleset = rulesetStore.GetRuleset(state.RulesetID.Value).CreateInstance();
                var userMods = state.Mods.Select(m => m.ToMod(userRuleset)).ToArray();

                var score = new Score
                {
                    ScoreInfo = new ScoreInfo
                    {
                        User = userLookupCache.GetUserAsync(userId).Result,
                        Beatmap = userBeatmap,
                        Ruleset = userRuleset.RulesetInfo,
                        Mods = userMods,
                    },
                    Replay = new Replay { HasReceivedAllFrames = false },
                };

                instances[userIndex] = new PlayerInstance(score, facades[userIndex])
                {
                    Depth = 1,
                    ToggleMaximisationState = toggleMaximisationState
                };

                LoadComponentAsync(instances[userIndex], instanceContainer.Add);
            }
        }

        private void userFinishedPlaying(int userId, SpectatorState state)
        {
            lock (scoreLock)
            {
                var instance = instances[getIndexForUser(userId)];
                Debug.Assert(instance != null);

                var score = instance.Score;
                if (score == null)
                    return;

                score.Replay.HasReceivedAllFrames = true;
            }
        }

        private void userSentFrames(int userId, FrameDataBundle bundle)
        {
            lock (scoreLock)
            {
                var instance = instances[getIndexForUser(userId)];
                Debug.Assert(instance != null);

                var score = instance.Score;

                // this should never happen as the server sends the user's state on watching,
                // but is here as a safety measure.
                if (score == null)
                    return;

                var ruleset = instance.Ruleset.Value;
                var beatmap = instance.Beatmap.Value;

                // rulesetInstance should be guaranteed to be in sync with the score via scoreLock.
                Debug.Assert(ruleset != null && ruleset.RulesetInfo.Equals(score.ScoreInfo.Ruleset));

                foreach (var frame in bundle.Frames)
                {
                    IConvertibleReplayFrame convertibleFrame = ruleset.CreateConvertibleReplayFrame();
                    convertibleFrame.FromLegacy(frame, beatmap);

                    var convertedFrame = (ReplayFrame)convertibleFrame;
                    convertedFrame.Time = frame.Time;

                    score.Replay.Frames.Add(convertedFrame);
                }
            }
        }

        private int getIndexForUser(int userId) => Array.IndexOf(spectatingIds, userId);

        protected override void Dispose(bool isDisposing)
        {
            base.Dispose(isDisposing);

            if (spectatorClient != null)
            {
                spectatorClient.OnUserBeganPlaying -= userBeganPlaying;
                spectatorClient.OnUserFinishedPlaying -= userFinishedPlaying;
                spectatorClient.OnNewFrames -= userSentFrames;

                foreach (var id in spectatingIds)
                    spectatorClient.StopWatchingUser(id);
            }
        }
    }
}
