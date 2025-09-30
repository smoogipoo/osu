// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Graphics.Containers;
using osu.Framework.Testing;
using osu.Game.Overlays;
using osu.Game.Screens;
using osu.Game.Screens.Footer;

namespace osu.Game.Tests.Visual.Navigation
{
    public partial class TestSceneScreenFooterNavigation : OsuGameTestScene
    {
        private ScreenFooter screenFooter => this.ChildrenOfType<ScreenFooter>().Single();

        [Test]
        public void TestFooterButtonsOnScreenTransitions()
        {
            PushAndConfirm(() => new TestScreenOne());
            AddUntilStep("button one shown", () => screenFooter.ChildrenOfType<ScreenFooterButton>().First().Text.ToString(), () => Is.EqualTo("Button One"));

            PushAndConfirm(() => new TestScreenTwo());
            AddUntilStep("button two shown", () => screenFooter.ChildrenOfType<ScreenFooterButton>().First().Text.ToString(), () => Is.EqualTo("Button Two"));

            AddStep("exit screen", () => Game.ScreenStack.Exit());
            AddUntilStep("button one shown", () => screenFooter.ChildrenOfType<ScreenFooterButton>().First().Text.ToString(), () => Is.EqualTo("Button One"));
        }

        [Test]
        public void TestFooterHidesOldBackButton()
        {
            PushAndConfirm(() => new TestScreen(false));
            AddAssert("footer hidden", () => screenFooter.State.Value, () => Is.EqualTo(Visibility.Hidden));
            AddAssert("old back button shown", () => screenFooter.LegacyBackButton.State.Value, () => Is.EqualTo(Visibility.Visible));

            PushAndConfirm(() => new TestScreen(true));
            AddAssert("footer shown", () => screenFooter.State.Value, () => Is.EqualTo(Visibility.Visible));
            AddAssert("old back button hidden", () => screenFooter.LegacyBackButton.State.Value, () => Is.EqualTo(Visibility.Hidden));

            PushAndConfirm(() => new TestScreen(false));
            AddAssert("footer hidden", () => screenFooter.State.Value, () => Is.EqualTo(Visibility.Hidden));
            AddAssert("back button shown", () => screenFooter.LegacyBackButton.State.Value, () => Is.EqualTo(Visibility.Visible));

            AddStep("exit screen", () => Game.ScreenStack.Exit());
            AddAssert("footer shown", () => screenFooter.State.Value, () => Is.EqualTo(Visibility.Visible));
            AddAssert("old back button hidden", () => screenFooter.LegacyBackButton.State.Value, () => Is.EqualTo(Visibility.Hidden));

            AddStep("exit screen", () => Game.ScreenStack.Exit());
            AddAssert("footer hidden", () => screenFooter.State.Value, () => Is.EqualTo(Visibility.Hidden));
            AddAssert("old back button shown", () => screenFooter.LegacyBackButton.State.Value, () => Is.EqualTo(Visibility.Visible));
        }

        private partial class TestScreenOne : OsuScreen
        {
            [Cached]
            private readonly OverlayColourProvider colourProvider = new OverlayColourProvider(OverlayColourScheme.Blue);

            protected override ScreenFooterButton[] CreateFooterButtons() => new[]
            {
                new ScreenFooterButton { Text = "Button One" },
            };
        }

        private partial class TestScreenTwo : OsuScreen
        {
            [Cached]
            private readonly OverlayColourProvider colourProvider = new OverlayColourProvider(OverlayColourScheme.Blue);

            protected override ScreenFooterButton[] CreateFooterButtons() => new[]
            {
                new ScreenFooterButton { Text = "Button Two" },
            };
        }

        private partial class TestScreen : OsuScreen
        {
            private readonly bool footer;

            public TestScreen(bool footer)
            {
                this.footer = footer;
            }

            protected override ScreenFooterButton[]? CreateFooterButtons() => footer ? [] : null;
        }
    }
}
