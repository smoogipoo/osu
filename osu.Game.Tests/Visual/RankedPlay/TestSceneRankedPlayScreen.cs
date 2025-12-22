// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Extensions;
using osu.Framework.Testing;
using osu.Game.Graphics.UserInterface;
using osu.Game.Online.API.Requests.Responses;
using osu.Game.Online.Multiplayer.MatchTypes.RankedPlay;
using osu.Game.Online.Rooms;
using osu.Game.Screens.OnlinePlay.Matchmaking.RankedPlay;
using osu.Game.Tests.Visual.Multiplayer;
using osuTK.Input;

namespace osu.Game.Tests.Visual.RankedPlay
{
    public partial class TestSceneRankedPlayScreen : MultiplayerTestScene
    {
        public override bool AutomaticallyRunFirstStep => false;

        [Cached(name: "debugEnabled")]
        private readonly Bindable<bool> debugEnabled = new Bindable<bool>();

        public TestSceneRankedPlayScreen()
        {
            AddToggleStep("debug overlay", enabled => debugEnabled.Value = enabled);
        }

        private RankedPlayScreen screen = null!;

        public override void SetUpSteps()
        {
            base.SetUpSteps();

            AddStep("join room", () => JoinRoom(CreateDefaultRoom(MatchType.RankedPlay)));
            WaitForJoined();

            AddStep("join other user", () => MultiplayerClient.AddUser(new APIUser { Id = 2 }));

            AddStep("load screen", () => LoadScreen(screen = new RankedPlayScreen(MultiplayerClient.ClientRoom!)));
        }

        [Test]
        public void TestAddRemoveCards()
        {
            AddStep("set discard phase", () => MultiplayerClient.RankedPlayChangeStage(RankedPlayStage.CardDiscard).WaitSafely());

            for (int i = 0; i < 3; i++)
                AddStep("add card", () => MultiplayerClient.RankedPlayAddCards([new RankedPlayCardItem()]).WaitSafely());

            for (int i = 0; i < 3; i++)
                AddStep("remove card", () => MultiplayerClient.RankedPlayRemoveCards(hand => [hand[0]]).WaitSafely());
        }

        [Test]
        public void TestRevealCards()
        {
            AddStep("set discard phase", () => MultiplayerClient.RankedPlayChangeStage(RankedPlayStage.CardDiscard).WaitSafely());

            for (int i = 0; i < 3; i++)
            {
                int i2 = i;
                AddStep("reveal card", () => MultiplayerClient.RankedPlayRevealCard(hand => hand[i2], new MultiplayerPlaylistItem
                {
                    ID = i2,
                    BeatmapID = i2
                }).WaitSafely());
            }
        }

        [Test]
        public void TestPlayCardDirect()
        {
            AddStep("set play phase", () => MultiplayerClient.RankedPlayChangeStage(RankedPlayStage.CardPlay).WaitSafely());
            AddWaitStep("wait", 3);
            AddStep("play card", () => MultiplayerClient.PlayCard(hand => hand[0]).WaitSafely());
        }

        [Test]
        public void TestDiscardCardsDirect()
        {
            AddStep("set discard phase", () => MultiplayerClient.RankedPlayChangeStage(RankedPlayStage.CardDiscard).WaitSafely());
            AddWaitStep("wait", 3);
            AddStep("discard cards", () => MultiplayerClient.DiscardCards(hand => hand.Take(3)).WaitSafely());
            AddWaitStep("wait", 13);
            AddStep("set finish discard phase", () => MultiplayerClient.RankedPlayChangeStage(RankedPlayStage.FinishCardDiscard).WaitSafely());
        }

        [Test]
        public void TestDiscardCardsStage()
        {
            AddStep("set discard phase", () => MultiplayerClient.RankedPlayChangeStage(RankedPlayStage.CardDiscard).WaitSafely());

            AddWaitStep("wait", 3);

            for (int i = 0; i < 3; i++)
            {
                int i2 = i;
                AddStep($"click card {i2}", () =>
                {
                    InputManager.MoveMouseTo(this.ChildrenOfType<RankedPlayScreen.Card>().ElementAt(i2));
                    InputManager.Click(MouseButton.Left);
                });
            }

            AddWaitStep("wait", 3);

            AddStep("click discard button", () =>
            {
                var button = screen.ChildrenOfType<ShearedButton>().Single(it => it.Text == "Discard");

                InputManager.MoveMouseTo(button);
                InputManager.Click(MouseButton.Left);
            });

            AddWaitStep("wait", 13);
            AddStep("set finish discard phase", () => MultiplayerClient.RankedPlayChangeStage(RankedPlayStage.FinishCardDiscard).WaitSafely());
        }

        [Test]
        public void TestPlayStage()
        {
            AddStep("set play phase", () => MultiplayerClient.RankedPlayChangeStage(RankedPlayStage.CardPlay, state => state.ActiveUserId = API.LocalUser.Value.OnlineID).WaitSafely());

            for (int i = 0; i < 3; i++)
            {
                int i2 = i;
                AddStep($"click card {i2}", () =>
                {
                    InputManager.MoveMouseTo(this.ChildrenOfType<RankedPlayScreen.Card>().ElementAt(i2));
                    InputManager.Click(MouseButton.Left);
                });
            }

            AddWaitStep("wait", 3);

            AddStep("click play button", () =>
            {
                var button = screen.ChildrenOfType<ShearedButton>().Single(it => it.Text == "Play");

                InputManager.MoveMouseTo(button);
                InputManager.Click(MouseButton.Left);
            });
        }

        [Test]
        public void TestOtherPlaysCard()
        {
            AddStep("set play phase", () => MultiplayerClient.RankedPlayChangeStage(RankedPlayStage.CardPlay, state => state.ActiveUserId = 2).WaitSafely());
            AddWaitStep("wait", 5);
            AddStep("play beatmap", () => MultiplayerClient.PlayUserCard(2, hand => hand[0]).WaitSafely());
        }

        [Test]
        public void TestHealthChange()
        {
            AddStep("change player 1 health", () => MultiplayerClient.RankedPlayChangeUserState(MultiplayerClient.LocalUser!.UserID, state => state.Life = 250_000).WaitSafely());
            AddStep("change player 2 health", () => MultiplayerClient.RankedPlayChangeUserState(2, state => state.Life = 250_000).WaitSafely());
        }
    }
}
