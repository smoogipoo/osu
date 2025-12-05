// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using NUnit.Framework;
using osu.Framework.Extensions;
using osu.Framework.Testing;
using osu.Game.Online.API.Requests.Responses;
using osu.Game.Online.Multiplayer.MatchTypes.RankedPlay;
using osu.Game.Online.Rooms;
using osu.Game.Screens.OnlinePlay.Matchmaking.RankedPlay;
using osu.Game.Tests.Visual.Multiplayer;
using osuTK.Input;

namespace osu.Game.Tests.Visual.RankedPlay
{
    public partial class TestSceneRankedPlayScreen2 : MultiplayerTestScene
    {
        private RankedPlayScreen2 screen = null!;

        public override void SetUpSteps()
        {
            base.SetUpSteps();

            AddStep("join room", () => JoinRoom(CreateDefaultRoom(MatchType.RankedPlay)));
            WaitForJoined();

            AddStep("load screen", () => LoadScreen(screen = new RankedPlayScreen2(MultiplayerClient.ClientRoom!)));
        }

        [Test]
        public void TestAddRemoveCards()
        {
            for (int i = 0; i < 3; i++)
                AddStep("add card", () => MultiplayerClient.RankedPlayAddCard(new RankedPlayCardItem()).WaitSafely());

            for (int i = 0; i < 3; i++)
                AddStep("remove card", () => MultiplayerClient.RankedPlayRemoveCard(((RankedPlayUserState)MultiplayerClient.LocalUser!.MatchState!).Hand[0]).WaitSafely());
        }

        [Test]
        public void TestRevealCards()
        {
            for (int i = 0; i < 3; i++)
            {
                int i2 = i;
                AddStep("reveal card", () => MultiplayerClient.RankedPlayRevealCard(((RankedPlayUserState)MultiplayerClient.LocalUser!.MatchState!).Hand[i2], new MultiplayerPlaylistItem
                {
                    ID = i2,
                    BeatmapID = i2
                }).WaitSafely());
            }
        }

        [Test]
        public void TestPlayCardDirect()
        {
            AddStep("play card", () => MultiplayerClient.PlayCard(((RankedPlayUserState)MultiplayerClient.LocalUser!.MatchState!).Hand[0]).WaitSafely());
        }

        [Test]
        public void TestDiscardCardsDirect()
        {
            AddStep("discard cards", () => MultiplayerClient.DiscardCards(((RankedPlayUserState)MultiplayerClient.LocalUser!.MatchState!).Hand.Take(3).ToArray()).WaitSafely());
        }

        [Test]
        public void TestDiscardCardsStage()
        {
            AddStep("set discard phase", () => MultiplayerClient.RankedPlayChangeStage(RankedPlayStage.CardDiscard).WaitSafely());

            for (int i = 0; i < 3; i++)
            {
                int i2 = i;
                AddStep($"click card {i2}", () =>
                {
                    InputManager.MoveMouseTo(this.ChildrenOfType<RankedPlayScreen2.Card>().ElementAt(i2));
                    InputManager.Click(MouseButton.Left);
                });
            }

            AddStep("click discard button", () =>
            {
                InputManager.MoveMouseTo(screen.ActionButton);
                InputManager.Click(MouseButton.Left);
            });
        }

        [Test]
        public void TestPlayStage()
        {
            AddStep("set play phase", () => MultiplayerClient.RankedPlayChangeStage(RankedPlayStage.CardPlay).WaitSafely());

            for (int i = 0; i < 3; i++)
            {
                int i2 = i;
                AddStep($"click card {i2}", () =>
                {
                    InputManager.MoveMouseTo(this.ChildrenOfType<RankedPlayScreen2.Card>().ElementAt(i2));
                    InputManager.Click(MouseButton.Left);
                });
            }

            AddStep("click play button", () =>
            {
                InputManager.MoveMouseTo(screen.ActionButton);
                InputManager.Click(MouseButton.Left);
            });
        }

        [Test]
        public void TestOtherPlaysCard()
        {
            AddStep("join other user", () => MultiplayerClient.AddUser(new APIUser { Id = 2 }));

            AddStep("set play phase", () => MultiplayerClient.RankedPlayChangeStage(RankedPlayStage.CardPlay, state => state.ActivePlayerIndex = 1).WaitSafely());

            AddStep("play beatmap", () => MultiplayerClient.PlayCard(((RankedPlayUserState)MultiplayerClient.ServerRoom!.Users[1].MatchState!).Hand[0]).WaitSafely());
        }
    }
}
