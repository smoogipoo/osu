// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Linq;
using NUnit.Framework;
using osu.Framework.Testing;
using osu.Game.Beatmaps;
using osu.Game.Online.Multiplayer;
using osu.Game.Screens.OnlinePlay.Multiplayer;
using osuTK.Input;

namespace osu.Game.Tests.Visual.Multiplayer
{
    public class TestSceneAllPlayersQueueMode : QueueModeTestScene
    {
        protected override QueueMode Mode => QueueMode.AllPlayers;

        [Test]
        public void TestFirstItemSelectedByDefault()
        {
            AddAssert("first item selected", () => Client.CurrentMatchPlayingItem.Value?.ID == Client.APIRoom?.Playlist[0].ID);
        }

        [Test]
        public void TestItemAddedToTheEndOfQueue()
        {
            addItem(() => OtherBeatmap);
            AddAssert("playlist has 2 items", () => Client.APIRoom?.Playlist.Count == 2);
            AddAssert("last playlist item is different", () => Client.APIRoom?.Playlist[1].Beatmap.Value.OnlineID == OtherBeatmap.OnlineID);

            addItem(() => InitialBeatmap);
            AddAssert("playlist has 3 items", () => Client.APIRoom?.Playlist.Count == 3);
            AddAssert("last playlist item is different", () => Client.APIRoom?.Playlist[2].Beatmap.Value.OnlineID == InitialBeatmap.OnlineID);

            AddAssert("first item still selected", () => Client.CurrentMatchPlayingItem.Value?.ID == Client.APIRoom?.Playlist[0].ID);
        }

        [Test]
        public void TestSingleItemExpiredAfterGameplay()
        {
            RunGameplay();

            AddAssert("playlist has only one item", () => Client.APIRoom?.Playlist.Count == 1);
            AddAssert("playlist item is expired", () => Client.APIRoom?.Playlist[0].Expired == true);
            AddAssert("last item selected", () => Client.CurrentMatchPlayingItem.Value?.ID == Client.APIRoom?.Playlist[0].ID);
        }

        [Test]
        public void TestNextItemSelectedAfterGameplayFinish()
        {
            addItem(() => OtherBeatmap);
            addItem(() => InitialBeatmap);

            RunGameplay();

            AddAssert("first item expired", () => Client.APIRoom?.Playlist[0].Expired == true);
            AddAssert("next item selected", () => Client.CurrentMatchPlayingItem.Value?.ID == Client.APIRoom?.Playlist[1].ID);

            RunGameplay();

            AddAssert("second item expired", () => Client.APIRoom?.Playlist[1].Expired == true);
            AddAssert("next item selected", () => Client.CurrentMatchPlayingItem.Value?.ID == Client.APIRoom?.Playlist[2].ID);
        }

        [Test]
        public void TestItemsNotClearedWhenSwitchToHostOnlyMode()
        {
            addItem(() => OtherBeatmap);
            addItem(() => InitialBeatmap);

            // Move to the "other" beatmap.
            RunGameplay();

            AddStep("change queue mode", () => Client.ChangeSettings(queueMode: QueueMode.HostOnly));
            AddAssert("playlist has 3 items", () => Client.APIRoom?.Playlist.Count == 3);
            AddAssert("playlist item is the other beatmap", () => Client.CurrentMatchPlayingItem.Value?.BeatmapID == OtherBeatmap.OnlineID);
            AddAssert("playlist item is not expired", () => Client.APIRoom?.Playlist[1].Expired == false);
        }

        private void addItem(Func<BeatmapInfo> beatmap)
        {
            AddStep("click edit button", () =>
            {
                InputManager.MoveMouseTo(this.ChildrenOfType<MultiplayerMatchSubScreen>().Single().AddOrEditPlaylistButton);
                InputManager.Click(MouseButton.Left);
            });

            AddUntilStep("wait for song select", () => CurrentSubScreen is Screens.Select.SongSelect select && select.IsLoaded);
            AddStep("select other beatmap", () => ((Screens.Select.SongSelect)CurrentSubScreen).FinaliseSelection(beatmap()));
            AddUntilStep("wait for return to match", () => CurrentSubScreen is MultiplayerMatchSubScreen);
        }
    }
}
