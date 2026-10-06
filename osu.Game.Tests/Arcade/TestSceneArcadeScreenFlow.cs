// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Screens;
using osu.Framework.Testing;
using osu.Game.Arcade;
using osu.Game.Arcade.Screens;
using osu.Game.Arcade.Screens.RankedPlay;
using osu.Game.Graphics.UserInterface;
using osu.Game.Graphics.UserInterfaceV2;
using osu.Game.Online.API;
using osu.Game.Tests.Visual.RankedPlay;

namespace osu.Game.Tests.Arcade
{
    public partial class TestSceneArcadeScreenFlow : RankedPlayTestScene
    {
        [Cached(typeof(ArcadeClient))]
        private readonly TestArcadeClient arcadeClient = new TestArcadeClient();

        private ArcadeScreen arcadeScreen = null!;
        private RankedPlayArcadeQueueScreen queueScreen = null!;

        public override void SetUpSteps()
        {
            base.SetUpSteps();

            AddStep("logout", () => API.Logout());
            AddStep("load arcade screen", () => LoadScreen(arcadeScreen = new ArcadeScreen(identity => queueScreen = new RankedPlayArcadeQueueScreen(identity))));
            AddUntilStep("wait for load", () => arcadeScreen.IsLoaded);
            AddStep("set state -> online", () => ((DummyAPIAccess)API).SetState(APIState.Online));
        }

        [Test]
        public void TestBasic()
        {
            AddStep("dummy", () => { });
        }

        [Test]
        public void TestButtonsAvailableOnExit()
        {
            AddStep("continue as guest", () => arcadeScreen.ChildrenOfType<RoundedButton>().Single().TriggerClick());
            AddStep("exit queue screen", () => queueScreen.Exit());
            AddAssert("guest button accessible", () => arcadeScreen.ChildrenOfType<RoundedButton>().Single().Enabled.Value);
            AddAssert("code box accessible", () => !arcadeScreen.ChildrenOfType<OsuNumberBox>().Single().Current.Disabled);
        }
    }
}
