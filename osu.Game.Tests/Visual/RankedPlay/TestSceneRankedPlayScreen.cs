// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Linq;
using NUnit.Framework;
using osu.Framework.Extensions;
using osu.Framework.Screens;
using osu.Framework.Testing;
using osu.Game.Graphics.UserInterfaceV2;
using osu.Game.Online.API.Requests.Responses;
using osu.Game.Online.Multiplayer;
using osu.Game.Online.Multiplayer.MatchTypes.RankedPlay;
using osu.Game.Online.Rooms;
using osu.Game.Screens.OnlinePlay.Matchmaking.RankedPlay;
using osu.Game.Tests.Visual.Multiplayer;

namespace osu.Game.Tests.Visual.RankedPlay
{
    public partial class TestSceneRankedPlayScreen : MultiplayerTestScene
    {
        private RankedPlayScreen screen = null!;

        public override void SetUpSteps()
        {
            base.SetUpSteps();

            AddStep("join room", () =>
            {
                var room = CreateDefaultRoom(MatchType.RankedPlay);

                JoinRoom(room);
            });

            WaitForJoined();

            AddStep("join users", () =>
            {
                MultiplayerClient.AddUser(new MultiplayerRoomUser(0)
                {
                    User = new APIUser
                    {
                        Username = $"Other User"
                    }
                });
            });

            AddStep("load match", () =>
            {
                LoadScreen(screen = new RankedPlayScreen(new MultiplayerRoom(0)
                {
                    Users =
                    [
                        new MultiplayerRoomUser(0)
                        {
                            User = new APIUser { Username = "Other Player" }
                        }
                    ],
                }));
            });
            AddUntilStep("wait for load", () => screen.IsCurrentScreen());
        }

        [Test]
        public void TestRankedPlayScreen()
        {
            changeStage(RankedPlayStage.CardDiscard);

            AddStep("select card", () => screen.ChildrenOfType<DiscardScreen.Card>().First().TriggerClick());
            AddStep("select another card", () => screen.ChildrenOfType<DiscardScreen.Card>().Skip(2).First().TriggerClick());
            AddWaitStep("wait", 3);
            AddStep("discard", () => screen.ChildrenOfType<RoundedButton>().First(it => it.Name == "Discard").TriggerClick());
        }

        private void changeStage(RankedPlayStage stage, Action<RankedPlayRoomState>? prepare = null, int waitTime = 5)
        {
            AddStep($"stage: {stage}", () => MultiplayerClient.RankedPlayChangeStage(stage, prepare).WaitSafely());
            AddWaitStep("wait", waitTime);
        }
    }
}
