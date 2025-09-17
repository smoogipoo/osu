// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using NUnit.Framework;
using osu.Framework.Extensions;
using osu.Framework.Graphics;
using osu.Game.Online.API.Requests.Responses;
using osu.Game.Online.Multiplayer;
using osu.Game.Online.Multiplayer.MatchTypes.Matchmaking;
using osu.Game.Online.Rooms;
using osu.Game.Screens.OnlinePlay.Matchmaking.Screens.Idle;
using osu.Game.Tests.Visual.Multiplayer;

namespace osu.Game.Tests.Visual.Matchmaking
{
    public partial class TestScenePlayerPanelLayoutContainer : MultiplayerTestScene
    {
        public override void SetUpSteps()
        {
            base.SetUpSteps();

            AddStep("join room", () =>
            {
                var room = CreateDefaultRoom();
                room.Type = MatchType.Matchmaking;
                JoinRoom(room);
            });

            WaitForJoined();

            AddStep("join users", () =>
            {
                MultiplayerClient.ChangeMatchRoomState(new MatchmakingRoomState
                {
                    Stage = MatchmakingStage.RoundWarmupTime
                }).WaitSafely();

                for (int i = 0; i < 7; i++)
                {
                    MultiplayerClient.AddUser(new MultiplayerRoomUser(i)
                    {
                        User = new APIUser
                        {
                            Username = $"Player {i}"
                        }
                    });
                }
            });
        }

        [Test]
        public void TestSplitLayout()
        {
            PlayerPanelList list = null!;
            SplitPlayerPanelLayoutContainer splitLayout = null!;
            GridPlayerPanelLayoutContainer gridLayout = null!;

            AddStep("add panels", () =>
            {
                Children = new Drawable[]
                {
                    splitLayout = new SplitPlayerPanelLayoutContainer
                    {
                        Anchor = Anchor.Centre,
                        Origin = Anchor.Centre,
                        RelativeSizeAxes = Axes.Both
                    },
                    gridLayout = new GridPlayerPanelLayoutContainer
                    {
                        Anchor = Anchor.Centre,
                        Origin = Anchor.Centre,
                        Width = PlayerPanel.SIZE_VERTICAL.X * 3
                    },
                    list = new PlayerPanelList
                    {
                        RelativeSizeAxes = Axes.Both
                    }
                };
            });

            AddStep("move panels to split layout", () => list.SetLayout(splitLayout));
            AddWaitStep("wait", 5);
            AddStep("move panels to grid layout", () => list.SetLayout(gridLayout));
        }
    }
}
