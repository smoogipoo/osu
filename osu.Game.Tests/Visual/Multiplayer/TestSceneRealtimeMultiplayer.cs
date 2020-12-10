// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using osu.Framework.Allocation;
using osu.Game.Database;
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
            [Resolved]
            private UserLookupCache userLookupCache { get; set; }

            protected override Task<IStatefulMultiplayerClient> Connect() => Task.FromResult<IStatefulMultiplayerClient>(new TestRealtimeMultiplayerClient(0, userLookupCache));
        }

#nullable enable

        private class TestRealtimeMultiplayerClient : StatefulMultiplayerClient
        {
            public override MultiplayerRoom? Room => room;
            private MultiplayerRoom? room;

            public TestRealtimeMultiplayerClient(int userId, UserLookupCache userLookupCache)
                : base(userId, userLookupCache)
            {
            }

            public override Task<MultiplayerRoom> JoinRoom(long roomId)
                => Task.FromResult(room = new MultiplayerRoom(roomId));

            public override Task LeaveRoom()
                => Task.CompletedTask;

            public override Task TransferHost(long userId) => throw new NotImplementedException();

            public override Task ChangeSettings(MultiplayerRoomSettings settings) => throw new NotImplementedException();

            public override Task ChangeState(MultiplayerUserState newState) => throw new NotImplementedException();

            public override Task StartMatch() => throw new NotImplementedException();
        }

#nullable disable
    }
}
