// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Linq;
using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Testing;
using osu.Game.Arcade;
using osu.Game.Arcade.Screens;
using osu.Game.Graphics.UserInterface;
using osu.Game.Online.API;
using osu.Game.Screens;
using osu.Game.Tests.Visual;

namespace osu.Game.Tests.Arcade
{
    public class TestSceneArcadeScreen : ScreenTestScene
    {
        [Cached(typeof(ArcadeClient))]
        private readonly TestArcadeClient arcadeClient;

        public TestSceneArcadeScreen()
        {
            Child = arcadeClient = new TestArcadeClient();
        }

        [Test]
        public void TestLogin()
        {
            AddStep("logout", () => API.Logout());

            ArcadeScreen screen = null!;
            AddStep("load arcade screen", () => LoadScreen(screen = new ArcadeScreen(_ => new DummyScreen())));
            AddUntilStep("wait for load", () => screen.IsLoaded);

            AddStep("set state -> connecting", () => ((DummyAPIAccess)API).SetState(APIState.Connecting));
            AddStep("set state -> second factor", () => ((DummyAPIAccess)API).SetState(APIState.RequiresSecondFactorAuth));
            AddStep("set state -> online", () => ((DummyAPIAccess)API).SetState(APIState.Online));
        }

        [Test]
        public void TestSuccessfulAuth()
        {
            AddStep("logout", () => API.Logout());
            AddStep("bind handler", () => arcadeClient.GetUserWithCodeFunc = _ => peppy_user);

            ArcadeScreen screen = null!;
            AddStep("load arcade screen", () => LoadScreen(screen = new ArcadeScreen(_ => new DummyScreen())));
            AddUntilStep("wait for load", () => screen.IsLoaded);
            AddStep("set state -> online", () => ((DummyAPIAccess)API).SetState(APIState.Online));

            AddStep("attempt login", () => screen.ChildrenOfType<OsuNumberBox>().Single().Text = "111111");
        }

        [Test]
        public void TestFailedAuth()
        {
            AddStep("logout", () => API.Logout());
            AddStep("bind handler", () => arcadeClient.GetUserWithCodeFunc = _ => throw new InvalidOperationException("Invalid code."));

            ArcadeScreen screen = null!;
            AddStep("load arcade screen", () => LoadScreen(screen = new ArcadeScreen(_ => new DummyScreen())));
            AddUntilStep("wait for load", () => screen.IsLoaded);
            AddStep("set state -> online", () => ((DummyAPIAccess)API).SetState(APIState.Online));

            AddStep("attempt login", () => screen.ChildrenOfType<OsuNumberBox>().Single().Text = "111111");
        }

        private class DummyScreen : OsuScreen;

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
