// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
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
using osuTK;

namespace osu.Game.Tests.Visual.Multiplayer
{
    public class TestSceneRealtimeMatchParticipants : OsuTestScene
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

        private MatchParticipantsList list;

        public TestSceneRealtimeMatchParticipants()
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

            Child = list = new MatchParticipantsList
            {
                RelativeSizeAxes = Axes.Both,
                Size = new Vector2(0.5f, 0.7f)
            };
        });

        private class MatchParticipantsList : CompositeDrawable
        {
            [Resolved]
            private RealtimeRoomManager roomManager { get; set; }
        }

        private class TestRoomManager : RealtimeRoomManager
        {
            protected override Task<IStatefulMultiplayerClient> Connect() => Task.FromResult<IStatefulMultiplayerClient>(new TestMultiplayerClient(0));
        }

#nullable enable

        private class TestMultiplayerClient : StatefulMultiplayerClient
        {
            public TestMultiplayerClient(int userId)
                : base(userId)
            {
            }

            public override MultiplayerRoom? Room => room;
            private MultiplayerRoom? room;

            public void SetRoom(MultiplayerRoom room) => this.room = room;

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
