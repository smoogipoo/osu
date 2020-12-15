// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
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
using osu.Game.Rulesets.Scoring;
using osu.Game.Scoring;
using osu.Game.Screens.Multi.Lounge.Components;
using osu.Game.Screens.Multi.Realtime;
using osu.Game.Users;

namespace osu.Game.Tests.Visual.Multiplayer
{
    public abstract class RealtimeMultiplayerTestScene : MultiplayerTestScene
    {
        [Cached(typeof(StatefulMultiplayerClient))]
        protected readonly TestMultiplayerClient Client = new TestMultiplayerClient();

        [Cached(typeof(RealtimeRoomManager))]
        private readonly TestRoomManager roomManager = new TestRoomManager();

        [Cached]
        private readonly Bindable<FilterCriteria> filter = new Bindable<FilterCriteria>();

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

                int currentScoreId = 0;

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

                        case CreateRoomScoreRequest createRoomScoreRequest:
                            createRoomScoreRequest.TriggerSuccess(new APIScoreToken { ID = 1 });
                            break;

                        case SubmitRoomScoreRequest submitRoomScoreRequest:
                            submitRoomScoreRequest.TriggerSuccess(new MultiplayerScore
                            {
                                ID = currentScoreId++,
                                Accuracy = 1,
                                EndedAt = DateTimeOffset.Now,
                                Passed = true,
                                Rank = ScoreRank.S,
                                MaxCombo = 1000,
                                TotalScore = 1000000,
                                User = api.LocalUser.Value,
                                Statistics = new Dictionary<HitResult, int>()
                            });
                            break;
                    }
                };
            }
        }

#nullable enable

        public class TestMultiplayerClient : StatefulMultiplayerClient
        {
            public override MultiplayerRoom? Room => joinedRoom;

            public override IBindable<bool> IsConnected { get; } = new Bindable<bool>(true);

            private MultiplayerRoom? joinedRoom;

            [Resolved]
            private IAPIProvider api { get; set; } = null!;

            public void SetRoom(MultiplayerRoom room) => joinedRoom = room;

            public void AddUser(User user) => ((IMultiplayerClient)this).UserJoined(new MultiplayerRoomUser(user.Id) { User = user });

            public void RemoveUser(User user)
            {
                Debug.Assert(joinedRoom != null);
                ((IMultiplayerClient)this).UserLeft(joinedRoom.Users.Single(u => u.User == user));
            }

            public void ChangeUserState(User user, MultiplayerUserState newState)
            {
                Debug.Assert(joinedRoom != null);

                ((IMultiplayerClient)this).UserStateChanged(user.Id, newState);

                Schedule(() =>
                {
                    switch (newState)
                    {
                        case MultiplayerUserState.Loaded:
                            if (joinedRoom.Users.All(u => u.State != MultiplayerUserState.WaitingForLoad))
                            {
                                foreach (var u in joinedRoom.Users.Where(u => u.State == MultiplayerUserState.Loaded))
                                {
                                    Debug.Assert(u.User != null);
                                    ChangeUserState(u.User, MultiplayerUserState.Playing);
                                }

                                ((IMultiplayerClient)this).MatchStarted();
                            }

                            break;

                        case MultiplayerUserState.FinishedPlay:
                            if (joinedRoom.Users.All(u => u.State != MultiplayerUserState.Playing))
                            {
                                foreach (var u in joinedRoom.Users.Where(u => u.State == MultiplayerUserState.FinishedPlay))
                                {
                                    Debug.Assert(u.User != null);
                                    ChangeUserState(u.User, MultiplayerUserState.Results);
                                }

                                ((IMultiplayerClient)this).ResultsReady();
                            }

                            break;
                    }
                });
            }

            public override Task JoinRoom(Room room)
            {
                Debug.Assert(room.RoomID.Value != null);

                base.JoinRoom(room);

                var user = new MultiplayerRoomUser(api.LocalUser.Value.Id) { User = api.LocalUser.Value };

                joinedRoom ??= new MultiplayerRoom(room.RoomID.Value.Value);
                joinedRoom.Users.Add(user);

                if (joinedRoom.Users.Count == 1)
                    joinedRoom.Host = user;

                InvokeRoomChanged();

                return Task.FromResult(room);
            }

            public override Task LeaveRoom()
            {
                base.LeaveRoom();

                joinedRoom = null;
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
