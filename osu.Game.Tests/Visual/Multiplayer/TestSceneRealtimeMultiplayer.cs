// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using System.Threading.Tasks;
using osu.Game.Online.API;
using osu.Game.Online.Multiplayer;
using osu.Game.Online.RealtimeMultiplayer;
using osu.Game.Screens.Multi;
using osu.Game.Screens.Multi.Realtime;

namespace osu.Game.Tests.Visual.Multiplayer
{
    public class TestSceneRealtimeMultiplayer : ScreenTestScene
    {
        private readonly List<Room> rooms = new List<Room>();

        public TestSceneRealtimeMultiplayer()
        {
            Screens.Multi.Multiplayer multi = new TestRealtimeMultiplayer();

            AddStep("show", () => LoadScreen(multi));
            AddUntilStep("wait for loaded", () => multi.IsLoaded);
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            ((DummyAPIAccess)API).HandleRequest = req =>
            {
                switch (req)
                {
                    case CreateRoomRequest createRoomRequest:
                        var createdRoom = new APICreatedRoom();

                        createdRoom.CopyFrom(createRoomRequest.Room);
                        createdRoom.RoomID.Value = 1;

                        rooms.Add(createdRoom);
                        createRoomRequest.TriggerSuccess(createdRoom);
                        break;

                    case JoinRoomRequest joinRoomRequest:
                        joinRoomRequest.TriggerSuccess();
                        break;

                    case PartRoomRequest partRoomRequest:
                        partRoomRequest.TriggerSuccess();
                        break;

                    case GetRoomsRequest getRoomsRequest:
                        getRoomsRequest.TriggerSuccess(rooms);
                        break;
                }
            };
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
                Room = new MultiplayerRoom(roomId);
                return Task.FromResult(Room);
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

            public MultiplayerUserState State { get; private set; }

            public MultiplayerRoom? Room { get; private set; }
        }

#nullable disable
    }
}
