// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using NUnit.Framework;
using osu.Framework.Extensions;
using osu.Framework.Graphics;
using osu.Framework.Utils;
using osu.Game.Arcade;
using osu.Game.Arcade.Screens;
using osu.Game.Tests.Visual.RankedPlay;
using osuTK;

namespace osu.Game.Tests.Arcade
{
    public class TestSceneArcadeLeaderboard : RankedPlayTestScene
    {
        private ArcadeLeaderboard leaderboard = null!;

        public override void SetUpSteps()
        {
            base.SetUpSteps();

            AddStep("add leaderboard", () => Child = leaderboard = new ArcadeLeaderboard
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                Size = new Vector2(300, 500)
            });
        }

        [Test]
        public void TestLeaderboard()
        {
            AddStep("fetch with one user", () =>
            {
                ArcadeClient.FetchLeaderboardFunc = () =>
                [
                    TestArcadeClient.CreateUserStats(1, 1)
                ];

                leaderboard.Fetch().WaitSafely();
            });

            AddStep("fetch with two users", () =>
            {
                ArcadeClient.FetchLeaderboardFunc = () =>
                [
                    TestArcadeClient.CreateUserStats(1, 1),
                    TestArcadeClient.CreateUserStats(2, 2)
                ];

                leaderboard.Fetch().WaitSafely();
            });

            AddStep("reorder users", () =>
            {
                ArcadeClient.FetchLeaderboardFunc = () =>
                [
                    TestArcadeClient.CreateUserStats(1, RNG.Next(10)),
                    TestArcadeClient.CreateUserStats(2, RNG.Next(10)),
                    TestArcadeClient.CreateUserStats(3, RNG.Next(10)),
                    TestArcadeClient.CreateUserStats(4, RNG.Next(10)),
                    TestArcadeClient.CreateUserStats(5, RNG.Next(10)),
                    TestArcadeClient.CreateUserStats(6, RNG.Next(10)),
                    TestArcadeClient.CreateUserStats(7, RNG.Next(10)),
                ];

                leaderboard.Fetch().WaitSafely();
            });
        }
    }
}
