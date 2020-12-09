// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Threading.Tasks;
using osu.Framework.Allocation;
using osu.Game.Online.Multiplayer;
using osu.Game.Online.RealtimeMultiplayer;
using osu.Game.Overlays;
using osu.Game.Screens.Multi;
using osu.Game.Screens.Multi.Realtime;

namespace osu.Game.Tests.Visual.Multiplayer
{
    public class TestSceneRealtimeMultiplayer : ScreenTestScene
    {
        protected override bool UseOnlineAPI => true;

        [Cached]
        private MusicController musicController { get; set; } = new MusicController();

        public TestSceneRealtimeMultiplayer()
        {
            Screens.Multi.Multiplayer multi = new TestRealtimeMultiplayer();

            AddStep("show", () => LoadScreen(multi));
            AddUntilStep("wait for loaded", () => multi.IsLoaded);
        }

        private class TestRealtimeMultiplayer : RealtimeMultiplayer
        {
            protected override IRoomManager CreateRoomManager() => new TestRealtimeRoomManager();
        }

        private class TestRealtimeRoomManager : RealtimeRoomManager
        {
            protected override Task Connect() => Task.CompletedTask;

            protected override void JoinMultiplayerRoom(Room room, Action<Room, MultiplayerRoom> onSuccess, Action<string> onError)
            {
                base.JoinMultiplayerRoom(room, onSuccess, onError);
            }

            protected override void PartMultiplayerRoom()
            {
                base.PartMultiplayerRoom();
            }
        }
    }
}
