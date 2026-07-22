// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Linq;
using NUnit.Framework;
using osu.Framework.Extensions;
using osu.Framework.Screens;
using osu.Framework.Testing;
using osu.Game.Arcade;
using osu.Game.Arcade.Screens.RankedPlay;
using osu.Game.Graphics.UserInterfaceV2;
using osu.Game.Rulesets.Osu;
using osu.Game.Screens.Play;
using osu.Game.Tests.Visual.RankedPlay;
using osuTK.Input;

namespace osu.Game.Tests.Arcade
{
    public class TestSceneRankedPlayArcadeQueueScreen : RankedPlayTestScene
    {
        private RankedPlayArcadeQueueScreen screen = null!;

        [SetUpSteps]
        public void SetupSteps()
        {
            AddStep("prepare beatmap", () => Beatmap.Value = CreateWorkingBeatmap(new OsuRuleset().RulesetInfo));
            AddStep("load screen", () => LoadScreen(screen = new RankedPlayArcadeQueueScreen(peppy_user)));
            AddUntilStep("wait for load", () => screen.IsLoaded);
        }

        [Test]
        public void TestIntro()
        {
            AddStep("dummy", () => { });
        }

        [Test]
        public void TestWaitingForOpponent()
        {
            AddStep("finish animations", () => screen.FinishTransforms(true));
        }

        [Test]
        public void TestReady()
        {
            AddStep("connect users", () =>
            {
                ArcadeClient.Connect(peppy_user).WaitSafely();
                ArcadeClient.Connect(2, peppy_user).WaitSafely();
            });
        }

        [Test]
        public void TestWaitingForStart()
        {
            AddStep("connect users", () =>
            {
                ArcadeClient.Connect(peppy_user).WaitSafely();
                ArcadeClient.Connect(2, peppy_user).WaitSafely();
            });

            AddStep("finish animations", () => screen.FinishTransforms(true));
            AddStep("press ready button", () =>
            {
                InputManager.MoveMouseTo(screen.ChildrenOfType<RoundedButton>().Last());
                InputManager.Click(MouseButton.Left);
            });
        }

        [Test]
        public void TestPracticeWhileWaiting()
        {
            AddStep("finish animations", () => screen.FinishTransforms(true));
            AddStep("press practice button", () =>
            {
                InputManager.MoveMouseTo(screen.ChildrenOfType<RoundedButton>().First());
                InputManager.Click(MouseButton.Left);
            });

            AddUntilStep("practice started", () => Stack.CurrentScreen is Player p && p.IsLoaded);

            AddStep("end practice", () => screen.EndPracticeAfter(TimeSpan.FromSeconds(2)));
            AddUntilStep("practice ended", () => screen.IsCurrentScreen());

            AddAssert("button disabled", () => screen.ChildrenOfType<RoundedButton>().First().Enabled.Value, () => Is.False);
        }

        private static readonly ArcadeIdentity peppy_user = new ArcadeIdentity
        {
            User = new ArcadeUser
            {
                UserId = 2,
                Username = "peppy",
                AvatarUrl = "https://a.ppy.sh/2",
                Cover = new ArcadeUser.UserCover
                {
                    Url = "https://assets.ppy.sh/user-profile-covers/8195163/4a8e2ad5a02a2642b631438cfa6c6bd7e2f9db289be881cb27df18331f64144c.jpeg"
                }
            },
            MatchmakingStats =
            [
                new ArcadeUserMatchmakingStats
                {
                    PoolId = 1,
                    Rating = 1234,
                    RulesetId = 0,
                    VariantId = 0
                }
            ]
        };
    }
}
