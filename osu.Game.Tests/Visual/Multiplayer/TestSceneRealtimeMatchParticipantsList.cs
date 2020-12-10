// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Testing;
using osu.Game.Online.Multiplayer;
using osu.Game.Online.RealtimeMultiplayer;
using osu.Game.Screens.Multi.Lounge.Components;
using osu.Game.Screens.Multi.Realtime;
using osu.Game.Screens.Multi.Realtime.Participants;
using osu.Game.Users;
using osuTK;

namespace osu.Game.Tests.Visual.Multiplayer
{
    public class TestSceneRealtimeMatchParticipantsList : OsuTestScene
    {
        [Cached(typeof(RealtimeRoomManager))]
        private readonly TestRoomManager roomManager = new TestRoomManager();

        [Cached]
        private readonly Bindable<Room> room = new Bindable<Room>();

        [Cached]
        private readonly Bindable<FilterCriteria> filter = new Bindable<FilterCriteria>();

        private TestMultiplayerClient client => (TestMultiplayerClient)roomManager.Client;

        protected override Container<Drawable> Content => content;
        private readonly Container content;

        public TestSceneRealtimeMatchParticipantsList()
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
            client.SetRoom(new MultiplayerRoom(1));

            Child = new ParticipantList
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                RelativeSizeAxes = Axes.Y,
                Size = new Vector2(380, 0.7f)
            };
        });

        [Test]
        public void TestAddUser()
        {
            AddStep("add user", () => client.AddUser(new User
            {
                Id = 2,
                Username = "First",
                CoverUrl = @"https://osu.ppy.sh/images/headers/profile-covers/c6.jpg",
                CurrentModeRank = 1234
            }));

            AddAssert("one unique panel", () => this.ChildrenOfType<ParticipantPanel>().Select(p => p.User).Distinct().Count() == 1);

            AddStep("add user", () => client.AddUser(new User
            {
                Id = 3,
                Username = "Second",
                CoverUrl = @"https://osu.ppy.sh/images/headers/profile-covers/c3.jpg",
            }));

            AddAssert("two unique panels", () => this.ChildrenOfType<ParticipantPanel>().Select(p => p.User).Distinct().Count() == 2);
        }

        [Test]
        public void TestRemoveUser()
        {
            User firstUser = null;
            User secondUser = null;

            AddStep("add two users", () =>
            {
                client.AddUser(firstUser = new User
                {
                    Id = 2,
                    Username = "First",
                    CoverUrl = @"https://osu.ppy.sh/images/headers/profile-covers/c6.jpg",
                    CurrentModeRank = 1234
                });

                client.AddUser(secondUser = new User
                {
                    Id = 3,
                    Username = "Second",
                    CoverUrl = @"https://osu.ppy.sh/images/headers/profile-covers/c3.jpg",
                });
            });

            AddStep("remove first user", () => client.RemoveUser(firstUser));

            AddAssert("single panel is for second user", () => this.ChildrenOfType<ParticipantPanel>().Single().User.User == secondUser);
        }

        private class TestRoomManager : RealtimeRoomManager
        {
            protected override Task Connect() => Task.CompletedTask;

            protected override IStatefulMultiplayerClient CreateClient() => new TestMultiplayerClient();
        }

#nullable enable

        private class TestMultiplayerClient : StatefulMultiplayerClient
        {
            public override MultiplayerRoom? Room => room;
            private MultiplayerRoom? room;

            public void SetRoom(MultiplayerRoom room) => this.room = room;

            public void AddUser(User user) => ((IMultiplayerClient)this).UserJoined(new MultiplayerRoomUser(user.Id) { User = user });

            public void RemoveUser(User user)
            {
                Debug.Assert(room != null);
                ((IMultiplayerClient)this).UserLeft(room.Users.Single(u => u.User == user));
            }

            public override Task<MultiplayerRoom> JoinRoom(long roomId) => throw new NotImplementedException();

            public override Task LeaveRoom() => throw new NotImplementedException();

            public override Task TransferHost(long userId) => throw new NotImplementedException();

            public override Task ChangeSettings(MultiplayerRoomSettings settings) => throw new NotImplementedException();

            public override Task ChangeState(MultiplayerUserState newState) => throw new NotImplementedException();

            public override Task StartMatch() => throw new NotImplementedException();
        }

#nullable disable
    }
}
