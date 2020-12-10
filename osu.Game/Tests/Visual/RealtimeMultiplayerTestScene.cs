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
using osu.Game.Online.Multiplayer;
using osu.Game.Online.RealtimeMultiplayer;
using osu.Game.Screens.Multi.Lounge.Components;
using osu.Game.Screens.Multi.Realtime;
using osu.Game.Users;

namespace osu.Game.Tests.Visual
{
    public class RealtimeMultiplayerTestScene : OsuTestScene
    {
        [Cached(typeof(RealtimeRoomManager))]
        private readonly TestRoomManager roomManager = new TestRoomManager();

        [Cached]
        private readonly Bindable<Room> room = new Bindable<Room>();

        [Cached]
        private readonly Bindable<FilterCriteria> filter = new Bindable<FilterCriteria>();

        protected TestMultiplayerClient Client => (TestMultiplayerClient)roomManager.Client;

        protected override Container<Drawable> Content => content;
        private readonly Container content;

        public RealtimeMultiplayerTestScene()
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
            Client.SetRoom(new MultiplayerRoom(1));
        });

        private class TestRoomManager : RealtimeRoomManager
        {
            protected override Task Connect() => Task.CompletedTask;

            protected override IStatefulMultiplayerClient CreateClient() => new TestMultiplayerClient();
        }

#nullable enable

        protected class TestMultiplayerClient : StatefulMultiplayerClient
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

            public void ChangeUserState(User user, MultiplayerUserState newState) => ((IMultiplayerClient)this).UserStateChanged(user.Id, newState);

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
