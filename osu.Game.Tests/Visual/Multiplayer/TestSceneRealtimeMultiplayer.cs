// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using osu.Framework.Allocation;
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
            protected override Task Connect() => Task.CompletedTask;

            protected override IStatefulMultiplayerClient CreateClient() => new TestRealtimeMultiplayerClient();
        }

#nullable enable

        private class TestRealtimeMultiplayerClient : StatefulMultiplayerClient
        {
            public override MultiplayerRoom? Room => room;
            private MultiplayerRoom? room;

            [Resolved]
            private IAPIProvider api { get; set; } = null!;

            public override Task<MultiplayerRoom> JoinRoom(long roomId)
            {
                room = new MultiplayerRoom(roomId)
                {
                    Users = { new MultiplayerRoomUser(api.LocalUser.Value.Id) { User = api.LocalUser.Value } }
                };

                InvokeRoomChanged();

                return Task.FromResult(room);
            }

            public override Task LeaveRoom()
            {
                room = null;

                InvokeRoomChanged();

                return Task.CompletedTask;
            }

            public override Task TransferHost(long userId) => throw new NotImplementedException();

            public override Task ChangeSettings(MultiplayerRoomSettings settings) => throw new NotImplementedException();

            public override Task ChangeState(MultiplayerUserState newState) => throw new NotImplementedException();

            public override Task StartMatch() => throw new NotImplementedException();
        }

#nullable disable
    }
}
