// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using NUnit.Framework;
using osu.Framework.Graphics;
using osu.Framework.Utils;
using osu.Game.Online.API.Requests.Responses;
using osu.Game.Online.Multiplayer;
using osu.Game.Screens.OnlinePlay.Multiplayer.Match;

namespace osu.Game.Tests.Visual.Multiplayer
{
    public class TestSceneAddOrEditPlaylistButtons : MultiplayerTestScene
    {
        private AddOrEditPlaylistButtons buttons;

        [SetUp]
        public new void Setup() => Schedule(() =>
        {
            Child = buttons = new AddOrEditPlaylistButtons
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                Width = 200
            };
        });

        [Test]
        public void TestHostOnlyAsHost()
        {
            setQueueMode(QueueMode.HostOnly);

            AddAssert("add item button enabled", () => buttons.AddButton.Enabled.Value);
            AddAssert("edit item button enabled", () => buttons.EditButton.Enabled.Value);
        }

        [Test]
        public void TestHostOnlyAsGuest()
        {
            setQueueMode(QueueMode.HostOnly);

            addUser(1234);
            setHost(1234);

            AddAssert("add item button disabled", () => !buttons.AddButton.Enabled.Value);
            AddAssert("edit item button disabled", () => !buttons.EditButton.Enabled.Value);
            AddAssert("component has size 0", () => Precision.AlmostEquals(0, buttons.DrawHeight));
        }

        [Test]
        public void TestAllPlayersAsHost()
        {
            setQueueMode(QueueMode.AllPlayers);

            AddAssert("add item button enabled", () => buttons.AddButton.Enabled.Value);
            AddAssert("edit item button enabled", () => buttons.EditButton.Enabled.Value);
        }

        [Test]
        public void TestAllPlayersAsGuest()
        {
            setQueueMode(QueueMode.AllPlayers);

            addUser(1234);
            setHost(1234);

            AddAssert("add item button enabled", () => buttons.AddButton.Enabled.Value);
            AddAssert("edit item button disabled", () => !buttons.EditButton.Enabled.Value);
        }

        private void setQueueMode(QueueMode mode)
        {
            AddStep($"set {mode}", () => Client.ChangeSettings(new MultiplayerRoomSettings { QueueMode = mode }));
            AddUntilStep("wait for queue mode change", () => Client.Room?.Settings.QueueMode == mode);
        }

        private void addUser(int userId)
        {
            AddStep($"add user {userId}", () => Client.AddUser(new APIUser { Id = userId }));
        }

        private void setHost(int userId)
        {
            AddStep($"set {userId} host", () => Client.TransferHost(userId));
            AddUntilStep("wait for user to become host", () => Client.Room?.Host?.UserID == userId);
        }
    }
}
