// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using NUnit.Framework;
using osu.Framework.Extensions;
using osu.Framework.Graphics;
using osu.Framework.Utils;
using osu.Game.Arcade;
using osu.Game.Arcade.Screens;
using osu.Game.Tests.Visual.RankedPlay;

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
                Width = 300
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

        [Test]
        public void TestManyPlayers()
        {
            AddStep("fetch", () =>
            {
                ArcadeClient.FetchLeaderboardFunc = () =>
                {
                    List<ArcadeUserStats> stats = [];

                    for (int i = 0; i < 1000; i++)
                        stats.Add(TestArcadeClient.CreateUserStats(i, 1));

                    return stats.ToArray();
                };

                leaderboard.Fetch().WaitSafely();
            });
        }

        [Test]
        public void TestMaxPanels()
        {
            AddStep("set max panels", () => leaderboard.MaxPanels = 3);

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
