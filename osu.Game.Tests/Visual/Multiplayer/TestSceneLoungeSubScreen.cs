// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Screens;
using osu.Game.Online.Multiplayer;
using osu.Game.Rulesets.Osu;
using osu.Game.Screens.Multi;
using osu.Game.Screens.Multi.Components;
using osu.Game.Screens.Multi.Lounge;
using osu.Game.Screens.Multi.Lounge.Components;
using osu.Game.Tests.Beatmaps;
using osu.Game.Users;

namespace osu.Game.Tests.Visual.Multiplayer
{
    public class TestSceneLoungeSubScreen : MultiplayerTestScene
    {
        public override IReadOnlyList<Type> RequiredTypes => new[]
        {
            typeof(LoungeSubScreen),
            typeof(RoomInspector),
            typeof(ParticipantInfo),
            typeof(RoomStatusInfo),
            typeof(ModeTypeInfo),
            typeof(RoomInfo)
        };

        protected override bool UseOnlineAPI => true;

        [Cached(typeof(IRoomManager))]
        private readonly TestRoomManager roomManager = new TestRoomManager();

        private LoungeSubScreen lounge;

        [SetUp]
        public void Setup() => Schedule(() =>
        {
            roomManager.Rooms.Clear();

            addTestRooms(10);
        });

        public override void SetUpSteps()
        {
            // Todo: Temp

            AddStep("load lounge", () => LoadScreen(lounge = new LoungeSubScreen()));
            AddUntilStep("wait for load", () => lounge.IsCurrentScreen());
        }

        private void addTestRooms(int count)
        {
            for (int i = 0; i < count; i++)
            {
                int id = roomManager.Rooms.Count == 0 ? 0 : roomManager.Rooms.Max(r => r.RoomID.Value) + 1 ?? 0;

                var room = new Room
                {
                    RoomID = { Value = id },
                    Name = { Value = $"Room {id}" },
                    Host = { Value = new User { Username = "peppy", Id = 2 } },
                };

                for (int p = 0; p < 10; p++)
                {
                    room.Playlist.Add(new PlaylistItem
                    {
                        ID = i * 10 + p,
                        Beatmap = { Value = new TestBeatmap(new OsuRuleset().RulesetInfo).BeatmapInfo },
                        Ruleset = { Value = new OsuRuleset().RulesetInfo }
                    });
                }

                roomManager.Rooms.Add(room);
            }
        }

        private class TestRoomManager : IRoomManager
        {
            public event Action RoomsUpdated;

            public readonly BindableList<Room> Rooms = new BindableList<Room>();

            IBindableList<Room> IRoomManager.Rooms => Rooms;

            public TestRoomManager()
            {
                Rooms.CollectionChanged += (_, __) => RoomsUpdated?.Invoke();
            }

            public void CreateRoom(Room room, Action<Room> onSuccess = null, Action<string> onError = null) => throw new NotImplementedException();

            public void JoinRoom(Room room, Action<Room> onSuccess = null, Action<string> onError = null) => throw new NotImplementedException();

            public void PartRoom()
            {
            }
        }
    }
}
