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
            bool actionInvoked = false;

            AddStep("setup", () => button.Start = () => actionInvoked = true);

            addClickButtonStep();
            AddAssert("user is ready", () => Client.Room?.Users[0].State == MultiplayerUserState.Ready);

            addClickButtonStep();
            AddAssert("user is idle", () => Client.Room?.Users[0].State == MultiplayerUserState.Idle);
            AddAssert("start not invoked", () => !actionInvoked);
        }

        [TestCase(true)]
        [TestCase(false)]
        public void TestToggleStateWhenHost(bool allReady)
        {
            bool actionInvoked = false;

            AddStep("setup", () =>
            {
                Client.TransferHost(Client.Room?.Users[0].UserID ?? 0);

                if (!allReady)
                    Client.AddUser(new User { Id = 2, Username = "Another user" });

                button.Start = () => actionInvoked = true;
            });

            addClickButtonStep();
            AddAssert("user is ready", () => Client.Room?.Users[0].State == MultiplayerUserState.Ready);

            addClickButtonStep();
            AddAssert("user is still ready", () => Client.Room?.Users[0].State == MultiplayerUserState.Ready);
            AddAssert("start invoked", () => actionInvoked);
        }

        [Test]
        public void TestBecomeHostWhileReady()
        {
            bool actionInvoked = false;

            AddStep("setup", () => button.Start = () => actionInvoked = true);

            addClickButtonStep();
            AddStep("make user host", () => Client.TransferHost(Client.Room?.Users[0].UserID ?? 0));

            addClickButtonStep();
            AddAssert("start invoked", () => actionInvoked);
        }

        [Test]
        public void TestLoseHostWhileReady()
        {
            bool actionInvoked = false;

            AddStep("setup", () =>
            {
                Client.TransferHost(Client.Room?.Users[0].UserID ?? 0);
                Client.AddUser(new User { Id = 2, Username = "Another user" });
                button.Start = () => actionInvoked = true;
            });

            addClickButtonStep();
            AddStep("transfer host", () => Client.TransferHost(Client.Room?.Users[1].UserID ?? 0));

            addClickButtonStep();
            AddAssert("start invoked", () => !actionInvoked);
        }

        private void addClickButtonStep() => AddStep("click button", () =>
        {
            InputManager.MoveMouseTo(button);
            InputManager.Click(MouseButton.Left);
        });
    }
}
