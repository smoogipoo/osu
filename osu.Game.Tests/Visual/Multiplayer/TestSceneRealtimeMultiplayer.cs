// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Threading.Tasks;
using osu.Framework.Allocation;
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
            protected override Task<IStatefulMultiplayerClient> Connect() => Task.FromResult<IStatefulMultiplayerClient>(new TestRealtimeMultiplayerClient());
        }

#nullable enable

        private class TestRealtimeMultiplayerClient : IStatefulMultiplayerClient
        {
            public Task RoomStateChanged(MultiplayerRoomState state)
            {
                throw new System.NotImplementedException();
            }

            public Task UserJoined(MultiplayerRoomUser user)
            {
                throw new System.NotImplementedException();
            }

            public Task UserLeft(MultiplayerRoomUser user)
            {
                throw new System.NotImplementedException();
            }

            public Task HostChanged(long userId)
            {
                throw new System.NotImplementedException();
            }

            public Task SettingsChanged(MultiplayerRoomSettings newSettings)
            {
                throw new System.NotImplementedException();
            }

            public Task UserStateChanged(long userId, MultiplayerUserState state)
            {
                throw new System.NotImplementedException();
            }

            public Task LoadRequested()
            {
                throw new System.NotImplementedException();
            }

            public Task MatchStarted()
            {
                throw new System.NotImplementedException();
            }

            public Task ResultsReady()
            {
                throw new System.NotImplementedException();
            }

            public Task<MultiplayerRoom> JoinRoom(long roomId)
            {
                throw new System.NotImplementedException();
            }

            public Task LeaveRoom()
            {
                throw new System.NotImplementedException();
            }

            public Task TransferHost(long userId)
            {
                throw new System.NotImplementedException();
            }

            public Task ChangeSettings(MultiplayerRoomSettings settings)
            {
                throw new System.NotImplementedException();
            }

            public Task ChangeState(MultiplayerUserState newState)
            {
                throw new System.NotImplementedException();
            }

            public Task StartMatch()
            {
                throw new System.NotImplementedException();
            }

            public MultiplayerUserState State { get; }

            public MultiplayerRoom? Room { get; }
        }

#nullable disable
    }
}
