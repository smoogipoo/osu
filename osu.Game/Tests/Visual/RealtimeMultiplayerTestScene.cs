// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Online.API;
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

            public void ChangeUserState(User user, MultiplayerUserState newState) => ((IMultiplayerClient)this).UserStateChanged(user.Id, newState);

            public override Task<MultiplayerRoom> JoinRoom(long roomId)
            {
                room ??= new MultiplayerRoom(roomId);
                room.Users.Add(new MultiplayerRoomUser(api.LocalUser.Value.Id) { User = api.LocalUser.Value });

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

                return Task.CompletedTask;
            }
        }

#nullable disable
    }
}
