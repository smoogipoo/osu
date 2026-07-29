// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using osu.Framework.Utils;
using osu.Game.Arcade;
using osu.Game.Arcade.Screens;
using osu.Game.Tests.Visual.RankedPlay;

namespace osu.Game.Tests.Arcade
{
    public class TestSceneArcadeLeaderboardScreen : RankedPlayTestScene
    {
        private ArcadeLeaderboardScreen screen = null!;

        public override void SetUpSteps()
        {
            base.SetUpSteps();

            List<ArcadeUserStats> stats = [];

            for (int i = 0; i < 1000; i++)
                stats.Add(TestArcadeClient.CreateUserStats(i, RNG.Next(100)));

            AddStep("load screen", () =>
            {
                ArcadeClient.FetchLeaderboardFunc = stats.ToArray;
                LoadScreen(screen = new ArcadeLeaderboardScreen());
            });

            AddUntilStep("wait for load", () => screen.IsLoaded);
        }
    }
}
