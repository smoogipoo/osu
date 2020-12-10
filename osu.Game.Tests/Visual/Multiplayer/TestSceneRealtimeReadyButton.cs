// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using NUnit.Framework;
using osu.Framework.Graphics;
using osu.Game.Online.RealtimeMultiplayer;
using osu.Game.Screens.Multi.Realtime;
using osu.Game.Users;
using osuTK;
using osuTK.Input;

namespace osu.Game.Tests.Visual.Multiplayer
{
    public class TestSceneRealtimeReadyButton : RealtimeMultiplayerTestScene
    {
        private ReadyButton button;

        [SetUp]
        public new void Setup() => Schedule(() =>
        {
            Child = button = new ReadyButton
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                Size = new Vector2(200, 50)
            };

            Client.AddUser(API.LocalUser.Value);
        });

        [Test]
        public void TestToggleStateWhenNotHost()
        {
            addClickButtonStep();
            AddAssert("user is ready", () => Client.Room?.Users[0].State == MultiplayerUserState.Ready);

            addClickButtonStep();
            AddAssert("user is idle", () => Client.Room?.Users[0].State == MultiplayerUserState.Idle);
        }

        [TestCase(true)]
        [TestCase(false)]
        public void TestToggleStateWhenHost(bool allReady)
        {
            AddStep("setup", () =>
            {
                Client.TransferHost(Client.Room?.Users[0].UserID ?? 0);

                if (!allReady)
                    Client.AddUser(new User { Id = 2, Username = "Another user" });
            });

            addClickButtonStep();
            AddAssert("user is ready", () => Client.Room?.Users[0].State == MultiplayerUserState.Ready);

            addClickButtonStep();
            AddAssert("match started", () => Client.Room?.Users[0].State == MultiplayerUserState.WaitingForLoad);
        }

        [Test]
        public void TestBecomeHostWhileReady()
        {
            addClickButtonStep();
            AddStep("make user host", () => Client.TransferHost(Client.Room?.Users[0].UserID ?? 0));

            addClickButtonStep();
            AddAssert("match started", () => Client.Room?.Users[0].State == MultiplayerUserState.WaitingForLoad);
        }

        [Test]
        public void TestLoseHostWhileReady()
        {
            AddStep("setup", () =>
            {
                Client.TransferHost(Client.Room?.Users[0].UserID ?? 0);
                Client.AddUser(new User { Id = 2, Username = "Another user" });
            });

            addClickButtonStep();
            AddStep("transfer host", () => Client.TransferHost(Client.Room?.Users[1].UserID ?? 0));

            addClickButtonStep();
            AddAssert("match not started", () => Client.Room?.Users[0].State == MultiplayerUserState.Idle);
        }

        private void addClickButtonStep() => AddStep("click button", () =>
        {
            InputManager.MoveMouseTo(button);
            InputManager.Click(MouseButton.Left);
        });
    }
}
