// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Online.API;
using osu.Game.Online.API.Requests;
using osu.Game.Online.Multiplayer;
using osu.Game.Online.RealtimeMultiplayer;
using osu.Game.Screens.Multi.Lounge.Components;
using osu.Game.Screens.Multi.Realtime;
using osu.Game.Users;

namespace osu.Game.Tests.Visual
{
    public abstract class RealtimeMultiplayerTestScene : MultiplayerTestScene
    {
        [Cached(typeof(RealtimeRoomManager))]
        private readonly TestRoomManager roomManager = new TestRoomManager();

        [Cached]
        private readonly Bindable<FilterCriteria> filter = new Bindable<FilterCriteria>();

        protected TestMultiplayerClient Client => (TestMultiplayerClient)roomManager.Client;

        protected virtual bool CreateRoom => true;

        protected override Container<Drawable> Content => content;
        private readonly Container content;

        protected RealtimeMultiplayerTestScene()
        {
            base.Content.AddRange(new Drawable[]
            {
                roomManager,
                content = new Container { RelativeSizeAxes = Axes.Both }
            });
        }

        [SetUp]
        public void Setup() => Schedule(() =>
        {
            if (CreateRoom)
                Client.SetRoom(new MultiplayerRoom(0));
        });

        public class TestRoomManager : RealtimeRoomManager
        {
            [Resolved]
            private IAPIProvider api { get; set; }

            [Resolved]
            private OsuGameBase game { get; set; }

            private readonly List<Room> rooms = new List<Room>();

            protected override void LoadComplete()
            {
                base.LoadComplete();

                ((DummyAPIAccess)api).HandleRequest = req =>
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

                        case GetBeatmapSetRequest getBeatmapSetRequest:
                            var onlineReq = new GetBeatmapSetRequest(getBeatmapSetRequest.ID, getBeatmapSetRequest.Type);
                            onlineReq.Success += res => getBeatmapSetRequest.TriggerSuccess(res);
                            onlineReq.Failure += e => getBeatmapSetRequest.TriggerFailure(e);

                            // Get the online API from the game's dependencies.
                            game.Dependencies.Get<IAPIProvider>().Queue(onlineReq);
                            break;
                    }
                };
            }

            protected override Task Connect() => Task.CompletedTask;

            protected override IStatefulMultiplayerClient CreateClient() => new TestMultiplayerClient();
        }

#nullable enable

        public class TestMultiplayerClient : StatefulMultiplayerClient
        {
            public override MultiplayerRoom? Room => room;
            private MultiplayerRoom? room;

            [Resolved]
            private IAPIProvider api { get; set; } = null!;

            public void SetRoom(MultiplayerRoom room) => this.room = room;

            public void AddUser(User user) => ((IMultiplayerClient)this).UserJoined(new MultiplayerRoomUser(user.Id) { User = user });

            public void RemoveUser(User user)
            {
                Debug.Assert(room != null);
                ((IMultiplayerClient)this).UserLeft(room.Users.Single(u => u.User == user));
            }

            public void ChangeUserState(User user, MultiplayerUserState newState)
            {
                Debug.Assert(room != null);

                ((IMultiplayerClient)this).UserStateChanged(user.Id, newState);

                switch (newState)
                {
                    case MultiplayerUserState.Loaded:
                        if (room.Users.All(u => u.State != MultiplayerUserState.WaitingForLoad))
                        {
                            foreach (var u in room.Users.Where(u => u.State == MultiplayerUserState.Loaded))
                            {
                                Debug.Assert(u.User != null);
                                ChangeUserState(u.User, MultiplayerUserState.Playing);
                            }

                            ((IMultiplayerClient)this).MatchStarted();
                        }

                        break;

                    case MultiplayerUserState.FinishedPlay:
                        if (room.Users.All(u => u.State != MultiplayerUserState.Playing))
                        {
                            foreach (var u in room.Users.Where(u => u.State == MultiplayerUserState.FinishedPlay))
                            {
                                Debug.Assert(u.User != null);
                                ChangeUserState(u.User, MultiplayerUserState.Results);
                            }

                            ((IMultiplayerClient)this).ResultsReady();
                        }

                        break;
                }
            }

            public override Task<MultiplayerRoom> JoinRoom(long roomId)
            {
                var user = new MultiplayerRoomUser(api.LocalUser.Value.Id) { User = api.LocalUser.Value };

                room ??= new MultiplayerRoom(roomId);
                room.Users.Add(user);

                if (room.Users.Count == 1)
                    room.Host = user;

                InvokeRoomChanged();

                return Task.FromResult(room);
            }

            public override Task LeaveRoom()
            {
                room = null;

                InvokeRoomChanged();

                return Task.CompletedTask;
            }

            public override Task TransferHost(long userId) => ((IMultiplayerClient)this).HostChanged(userId);

            public override Task ChangeSettings(MultiplayerRoomSettings settings) => ((IMultiplayerClient)this).SettingsChanged(settings);

            public override Task ChangeState(MultiplayerUserState newState)
            {
                ChangeUserState(api.LocalUser.Value, newState);
                return Task.CompletedTask;
            }

            public override Task StartMatch()
            {
                Debug.Assert(Room != null);

                foreach (var user in Room.Users)
                {
                    Debug.Assert(user.User != null);
                    ChangeUserState(user.User, MultiplayerUserState.WaitingForLoad);
                }

                ((IMultiplayerClient)this).LoadRequested();

                return Task.CompletedTask;
            }
        }

#nullable disable
    }
}
