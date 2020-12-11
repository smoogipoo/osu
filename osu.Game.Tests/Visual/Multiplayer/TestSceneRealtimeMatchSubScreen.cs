// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using NUnit.Framework;
using osu.Framework.Screens;
using osu.Framework.Testing;
using osu.Game.Online.API;
using osu.Game.Online.Multiplayer;
using osu.Game.Rulesets.Osu;
using osu.Game.Screens.Multi.Realtime;
using osu.Game.Tests.Beatmaps;
using osuTK.Input;

namespace osu.Game.Tests.Visual.Multiplayer
{
    public class TestSceneRealtimeMatchSubScreen : RealtimeMultiplayerTestScene
    {
        protected override bool CreateRoom => false;

        private RealtimeMatchSubScreen screen;

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
        //
        // [Test]
        // public void TestCreateButtonDisabledWithNoBeatmap()
        // {
        //     AddAssert("create button not enabled", () => !this.ChildrenOfType<RealtimeMatchSettingsOverlay.CreateRoomButton>().Single().Enabled.Value);
        // }
        //
        // [Test]
        // public void TestCreateButtonEnabledWithBeatmap()
        // {
        //     AddStep("create room", () =>
        //     {
        //         Room.Playlist.Add(new PlaylistItem
        //         {
        //             Beatmap = { Value = new TestBeatmap(new OsuRuleset().RulesetInfo).BeatmapInfo },
        //             Ruleset = { Value = new OsuRuleset().RulesetInfo }
        //         });
        //     });
        //
        //     AddAssert("create button enabled", () => this.ChildrenOfType<RealtimeMatchSettingsOverlay.CreateRoomButton>().Single().Enabled.Value);
        // }

        [Test]
        public void TestCreatedRoom()
        {
            AddStep("set valid properties", () =>
            {
                Room.Playlist.Add(new PlaylistItem
                {
                    Beatmap = { Value = new TestBeatmap(new OsuRuleset().RulesetInfo).BeatmapInfo },
                    Ruleset = { Value = new OsuRuleset().RulesetInfo }
                });
            });

            AddStep("click create button", () =>
            {
                InputManager.MoveMouseTo(this.ChildrenOfType<RealtimeMatchSettingsOverlay.CreateRoomButton>().Single());
                InputManager.Click(MouseButton.Left);
            });

            AddWaitStep("wait", 500);
        }
    }
}
