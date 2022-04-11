// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using NUnit.Framework;
using osu.Framework.Graphics;
using osu.Game.Screens.OnlinePlay.Lounge.Components;

namespace osu.Game.Tests.Visual.Multiplayer
{
    public class TestSceneRankRangePill : MultiplayerTestScene
    {
        [SetUp]
        public new void Setup() => Schedule(() =>
        {
            Child = new RankRangePill
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre
            };
        });

        [Test]
        public void TestSingleUser()
        {
            AddStep("add user", () =>
            {
                MultiplayerServer.AddUser(2);

                // Remove the local user so only the one above is displayed.
                MultiplayerServer.RemoveUser(API.LocalUser.Value.Id);
            });
        }

        [Test]
        public void TestMultipleUsers()
        {
            AddStep("add users", () =>
            {
                MultiplayerServer.AddUser(2);

                MultiplayerServer.AddUser(3);

                MultiplayerServer.AddUser(4);

                // Remove the local user so only the ones above are displayed.
                MultiplayerServer.RemoveUser(API.LocalUser.Value.Id);
            });
        }

        [TestCase(1, 10)]
        [TestCase(10, 100)]
        [TestCase(100, 1000)]
        [TestCase(1000, 10000)]
        [TestCase(10000, 100000)]
        [TestCase(100000, 1000000)]
        [TestCase(1000000, 10000000)]
        public void TestRange(int min, int max)
        {
            AddStep("add users", () =>
            {
                MultiplayerServer.AddUser(2);

                MultiplayerServer.AddUser(3);

                // Remove the local user so only the ones above are displayed.
                MultiplayerServer.RemoveUser(API.LocalUser.Value.Id);
            });
        }
    }
}
