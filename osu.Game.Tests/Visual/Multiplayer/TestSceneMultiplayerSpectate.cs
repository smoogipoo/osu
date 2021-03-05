// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Testing;
using osu.Framework.Utils;
using osu.Game.Beatmaps;
using osu.Game.Online;
using osu.Game.Online.Rooms;
using osu.Game.Online.Spectator;
using osu.Game.Replays.Legacy;
using osu.Game.Rulesets.UI;
using osu.Game.Scoring;
using osu.Game.Screens.OnlinePlay.Multiplayer.Spectate;
using osu.Game.Screens.Play;
using osu.Game.Tests.Beatmaps.IO;
using osuTK.Input;

namespace osu.Game.Tests.Visual.Multiplayer
{
    public class TestSceneMultiplayerSpectate : ScreenTestScene
    {
        [Cached(typeof(SpectatorStreamingClient))]
        private TestSpectatorStreamingClient testSpectatorStreamingClient = new TestSpectatorStreamingClient();

        [Resolved]
        private OsuGameBase game { get; set; }

        [Resolved]
        private BeatmapManager beatmapManager { get; set; }

        private MultiplayerSpectateScreen spectateScreen;

        private readonly List<int> playingUserIds = new List<int>();
        private readonly Dictionary<int, int> nextFrame = new Dictionary<int, int>();

        private BeatmapSetInfo importedSet;
        private BeatmapInfo importedBeatmap;
        private int importedBeatmapId;

        [BackgroundDependencyLoader]
        private void load()
        {
            importedSet = ImportBeatmapTest.LoadOszIntoOsu(game, virtualTrack: true).Result;
            importedBeatmap = importedSet.Beatmaps.First(b => b.RulesetID == 0);
            importedBeatmapId = importedBeatmap.OnlineBeatmapID ?? -1;
        }

        public override void SetUpSteps()
        {
            base.SetUpSteps();

            AddStep("reset sent frames", () => nextFrame.Clear());

            AddStep("add streaming client", () =>
            {
                Remove(testSpectatorStreamingClient);
                Add(testSpectatorStreamingClient);
            });

            AddStep("finish previous gameplay", () =>
            {
                foreach (var id in playingUserIds)
                    testSpectatorStreamingClient.EndPlay(id, importedBeatmapId);
                playingUserIds.Clear();
            });
        }

        [Test]
        public void TestGeneral()
        {
            int[] userIds = Enumerable.Range(0, 4).Select(i => 55 + i).ToArray();

            start(userIds);
            loadSpectateScreen();

            sendFrames(userIds, 1000);
            AddWaitStep("wait a bit", 20);
        }

        [Test]
        public void TestPlayersStartAtDifferentTimes()
        {
            start(new[] { 55, 56 });
            loadSpectateScreen();

            sendFrames(55, 20);

            AddWaitStep("wait a bit", 5);
            checkPaused(55, true);
        }

        [Test]
        public void TestPlayerStopsSendingFrames()
        {
            start(new[] { 55, 56 });
            loadSpectateScreen();

            // Send initial frames for both players.
            sendFrames(new[] { 55, 56 });
            checkPaused(55, false);
            checkPaused(56, false);

            // Eventually they will pause due to running out of frames.
            checkPaused(55, true);
            checkPaused(56, true);

            // Send more frames for the first user only. Both should remain paused.
            sendFrames(55);
            checkPaused(55, true);
            checkPaused(56, true);

            // Send more frames for the second user. Both should unpause.
            sendFrames(56);
            checkPaused(55, false);
            checkPaused(56, false);
        }

        [Test]
        public void TestMaximiseAndMinimise()
        {
            start(new[] { 55, 56 });
            loadSpectateScreen();

            AddStep("maximise user 55", () =>
            {
                InputManager.MoveMouseTo(spectateScreen.ChildrenOfType<PlayerInstance>().Single(p => p.User.Id == 55));
                InputManager.Click(MouseButton.Left);
            });

            AddUntilStep("user 55 maximised", () => isMaximised(55));
            AddAssert("user 56 minimised", () => !isMaximised(56));

            AddStep("minimise user 55", () =>
            {
                InputManager.MoveMouseTo(spectateScreen.ChildrenOfType<PlayerInstance>().Single(p => p.User.Id == 55));
                InputManager.Click(MouseButton.Left);
            });

            AddUntilStep("user 55 minimised", () => !isMaximised(55));
            AddAssert("user 56 minimised", () => !isMaximised(56));
        }

        [Test]
        public void TestMaximiseTwoInstancesSimultaneously()
        {
            start(new[] { 55, 56 });
            loadSpectateScreen();

            AddStep("maximise user 55 then 56", () =>
            {
                InputManager.MoveMouseTo(spectateScreen.ChildrenOfType<PlayerInstance>().Single(p => p.User.Id == 55));
                InputManager.Click(MouseButton.Left);

                InputManager.MoveMouseTo(spectateScreen.ChildrenOfType<PlayerInstance>().Single(p => p.User.Id == 56));
                InputManager.Click(MouseButton.Left);
            });

            AddUntilStep("user 56 maximised", () => isMaximised(56));
            AddAssert("user 55 minimised", () => !isMaximised(55));
        }

        [TestCase(1)]
        [TestCase(2)]
        [TestCase(3)]
        [TestCase(4)]
        [TestCase(5)]
        [TestCase(9)]
        [TestCase(11)]
        [TestCase(12)]
        [TestCase(15)]
        [TestCase(16)]
        [TestCase(32)]
        public void TestPlayerCount(int playerCount)
        {
            start(Enumerable.Range(0, playerCount).Select(i => 55 + i).ToArray());
            loadSpectateScreen();
        }

        private void loadSpectateScreen()
        {
            AddStep("load screen", () =>
            {
                Beatmap.Value = beatmapManager.GetWorkingBeatmap(importedBeatmap);
                Ruleset.Value = importedBeatmap.Ruleset;

                LoadScreen(spectateScreen = new MultiplayerSpectateScreen(new PlaylistItem
                {
                    Beatmap = { Value = importedBeatmap },
                    Ruleset = { Value = importedBeatmap.Ruleset }
                }, playingUserIds.ToArray()));
            });

            AddUntilStep("wait for screen load", () => spectateScreen.LoadState == LoadState.Loaded && spectateScreen.AllPlayersLoaded);
        }

        private void start(int userId, int? beatmapId = null) => start(new[] { userId }, beatmapId);

        private void start(int[] userIds, int? beatmapId = null)
        {
            AddStep("start play", () =>
            {
                foreach (int id in userIds)
                {
                    testSpectatorStreamingClient.StartPlay(id, beatmapId ?? importedBeatmapId);
                    playingUserIds.Add(id);
                    nextFrame[id] = 0;
                }
            });
        }

        private void finish(int userId, int? beatmapId = null)
        {
            AddStep("end play", () =>
            {
                testSpectatorStreamingClient.EndPlay(userId, beatmapId ?? importedBeatmapId);
                playingUserIds.Remove(userId);
                nextFrame.Remove(userId);
            });
        }

        private void sendFrames(int userId, int count = 10) => sendFrames(new[] { userId }, count);

        private void sendFrames(int[] userIds, int count = 10)
        {
            AddStep("send frames", () =>
            {
                foreach (int id in userIds)
                {
                    testSpectatorStreamingClient.SendFrames(id, nextFrame[id], count);
                    nextFrame[id] += count;
                }
            });
        }

        private bool isMaximised(int userId)
            => Precision.AlmostEquals(spectateScreen.DrawSize, getPlayer(userId).DrawSize, 100);

        private void checkPaused(int userId, bool state) =>
            AddUntilStep($"game is {(state ? "paused" : "playing")}", () => getPlayer(userId).ChildrenOfType<DrawableRuleset>().First().IsPaused.Value == state);

        private Player getPlayer(int userId)
            => spectateScreen
               .ChildrenOfType<PlayerInstance>().Single(p => p.User.Id == userId)
               .ChildrenOfType<Player>().Single();

        public class TestSpectatorStreamingClient : SpectatorStreamingClient
        {
            private readonly Dictionary<int, int> userBeatmapDictionary = new Dictionary<int, int>();
            private readonly Dictionary<int, bool> userSentStateDictionary = new Dictionary<int, bool>();

            public TestSpectatorStreamingClient()
                : base(new DevelopmentEndpointConfiguration())
            {
            }

            public void StartPlay(int userId, int beatmapId)
            {
                userBeatmapDictionary[userId] = beatmapId;
                userSentStateDictionary[userId] = false;

                sendState(userId, beatmapId);
            }

            public void EndPlay(int userId, int beatmapId)
            {
                ((ISpectatorClient)this).UserFinishedPlaying(userId, new SpectatorState
                {
                    BeatmapID = beatmapId,
                    RulesetID = 0,
                });

                userSentStateDictionary[userId] = false;
            }

            public void SendFrames(int userId, int index, int count)
            {
                var frames = new List<LegacyReplayFrame>();

                for (int i = index; i < index + count; i++)
                {
                    var buttonState = i == index + count - 1 ? ReplayButtonState.None : ReplayButtonState.Left1;

                    frames.Add(new LegacyReplayFrame(i * 100, RNG.Next(0, 512), RNG.Next(0, 512), buttonState));
                }

                var bundle = new FrameDataBundle(new ScoreInfo(), frames);
                ((ISpectatorClient)this).UserSentFrames(userId, bundle);

                if (!userSentStateDictionary[userId])
                    sendState(userId, userBeatmapDictionary[userId]);
            }

            public override void WatchUser(int userId)
            {
                if (userSentStateDictionary[userId])
                {
                    // usually the server would do this.
                    sendState(userId, userBeatmapDictionary[userId]);
                }

                base.WatchUser(userId);
            }

            private void sendState(int userId, int beatmapId)
            {
                ((ISpectatorClient)this).UserBeganPlaying(userId, new SpectatorState
                {
                    BeatmapID = beatmapId,
                    RulesetID = 0,
                });

                userSentStateDictionary[userId] = true;
            }
        }
    }
}
