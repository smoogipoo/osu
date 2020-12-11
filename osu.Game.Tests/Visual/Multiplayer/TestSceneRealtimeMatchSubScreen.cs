// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Screens;
using osu.Framework.Testing;
using osu.Game.Online.API;
using osu.Game.Online.API.Requests;
using osu.Game.Online.Multiplayer;
using osu.Game.Screens.Multi.Match.Components;
using osu.Game.Screens.Multi.Realtime;
using osuTK.Input;

namespace osu.Game.Tests.Visual.Multiplayer
{
    public class TestSceneRealtimeMatchSubScreen : RealtimeMultiplayerTestScene
    {
        protected override bool CreateRoom => false;

        private RealtimeMatchSubScreen screen;

        [Resolved]
        private IAPIProvider onlineApi { get; set; }

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
                        createRoomRequest.TriggerSuccess(createdRoom);
                        break;

                    case JoinRoomRequest joinRoomRequest:
                        joinRoomRequest.TriggerSuccess();
                        break;

                    case PartRoomRequest partRoomRequest:
                        partRoomRequest.TriggerSuccess();
                        break;

                    case GetBeatmapSetRequest getBeatmapSetRequest:
                        var onlineReq = new GetBeatmapSetRequest(getBeatmapSetRequest.ID, getBeatmapSetRequest.Type);
                        onlineReq.Success += res => getBeatmapSetRequest.TriggerSuccess(res);
                        onlineReq.Failure += e => getBeatmapSetRequest.TriggerFailure(e);
                        onlineApi.Queue(onlineReq);
                        break;
                }
            };
        }

        [SetUp]
        public new void Setup() => Schedule(() =>
        {
            Room = new Room { Name = { Value = "Test Room" } };
        });

        [SetUpSteps]
        public void SetupSteps()
        {
            AddStep("load match", () => LoadScreen(screen = new RealtimeMatchSubScreen(Room)));
            AddUntilStep("wait for load", () => screen.IsCurrentScreen());
        }

        [Test]
        public void TestSettingValidity()
        {
            AddAssert("create button enabled", () => this.ChildrenOfType<MatchSettingsOverlay.CreateRoomButton>().Single().Enabled.Value);

            AddStep("make room name empty", () => Room.Name.Value = string.Empty);
            AddAssert("create button not enabled", () => !this.ChildrenOfType<MatchSettingsOverlay.CreateRoomButton>().Single().Enabled.Value);
        }

        [Test]
        public void TestCreatedRoom()
        {
            AddStep("click create button", () =>
            {
                InputManager.MoveMouseTo(this.ChildrenOfType<RealtimeMatchSettingsOverlay.CreateOrUpdateButton>().Single());
                InputManager.Click(MouseButton.Left);
            });

            AddWaitStep("wait", 500);
        }
    }
}
