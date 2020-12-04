// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using NUnit.Framework;
using osu.Framework.Screens;
using osu.Framework.Testing;
using osu.Game.Graphics.Containers;
using osu.Game.Screens.Multi.Lounge;
using osu.Game.Screens.Multi.Lounge.Components;
using osu.Game.Screens.Multi.Timeshift;

namespace osu.Game.Tests.Visual.Multiplayer
{
    public class TestSceneTimeshiftLoungeSubScreen : RoomManagerTestScene
    {
        private LoungeSubScreen loungeScreen;

        public override void SetUpSteps()
        {
            base.SetUpSteps();

            AddStep("push screen", () => LoadScreen(loungeScreen = new TimeShiftLoungeSubScreen()));

            AddUntilStep("wait for present", () => loungeScreen.IsCurrentScreen());
        }

        private RoomsContainer roomsContainer => loungeScreen.ChildrenOfType<RoomsContainer>().First();

        [Test]
        public void TestScrollSelectedIntoView()
        {
            AddRooms(30);

            AddUntilStep("first room is not masked", () => checkRoomVisible(roomsContainer.Rooms.First()));

            AddStep("select last room", () => roomsContainer.Rooms.Last().Action?.Invoke());

            AddUntilStep("first room is masked", () => !checkRoomVisible(roomsContainer.Rooms.First()));
            AddUntilStep("last room is not masked", () => checkRoomVisible(roomsContainer.Rooms.Last()));
        }

        private bool checkRoomVisible(DrawableRoom room) =>
            loungeScreen.ChildrenOfType<OsuScrollContainer>().First().ScreenSpaceDrawQuad
                        .Contains(room.ScreenSpaceDrawQuad.Centre);
    }
}
