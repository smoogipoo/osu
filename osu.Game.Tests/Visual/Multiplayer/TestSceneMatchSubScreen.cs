// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Game.Online.Multiplayer;
using osu.Game.Screens.Multi.Match;

namespace osu.Game.Tests.Visual.Multiplayer
{
    public class TestSceneMatchSubScreen : ScreenTestScene
    {
        protected override bool UseOnlineAPI => true;

        public override IReadOnlyList<Type> RequiredTypes => new[]
        {
            typeof(Screens.Multi.Multiplayer),
            typeof(MatchSubScreen),
        };

        [Cached]
        private readonly Bindable<Room> currentRoom = new Bindable<Room>();

        public TestSceneMatchSubScreen()
        {
            currentRoom.Value = new Room();
        }

        [Test]
        public void TestShowSettings()
        {
            AddStep(@"show", () =>
            {
                currentRoom.Value.RoomID.Value = null;
                LoadScreen(new MatchSubScreen(currentRoom.Value));
            });
        }

        [Test]
        public void TestShowRoom()
        {
            AddStep(@"show", () =>
            {
                currentRoom.Value.RoomID.Value = 1;
                LoadScreen(new MatchSubScreen(currentRoom.Value));
            });
        }

        protected override IReadOnlyDependencyContainer CreateChildDependencies(IReadOnlyDependencyContainer parent)
        {
            var dependencies = new CachedModelDependencyContainer<Room>(base.CreateChildDependencies(parent));
            dependencies.Model.BindTo(currentRoom);
            return dependencies;
        }
    }
}
