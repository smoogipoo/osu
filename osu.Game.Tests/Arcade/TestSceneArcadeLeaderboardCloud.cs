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
    public class TestSceneArcadeLeaderboardCloud : RankedPlayTestScene
    {
        private ArcadeLeaderboardCloud leaderboard = null!;

        public override void SetUpSteps()
        {
            base.SetUpSteps();

            AddStep("add leaderboard", () => Child = leaderboard = new ArcadeLeaderboardCloud
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                RelativeSizeAxes = Axes.X,
                Height = 500
            });
        }

        [Test]
        public void TestManyPlayers()
        {
            List<ArcadeUserStats> stats = [];

            for (int i = 0; i < 1000; i++)
                stats.Add(TestArcadeClient.CreateUserStats(i, RNG.Next(100)));

            AddStep("fetch", () =>
            {
                ArcadeClient.FetchLeaderboardFunc = () =>
                {
                    return stats.ToArray();
                };

                leaderboard.Fetch().WaitSafely();
            });
        }
    }
}
