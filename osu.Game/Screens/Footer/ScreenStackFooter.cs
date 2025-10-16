// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Screens;

namespace osu.Game.Screens.Footer
{
    public class ScreenStackFooter : CompositeDrawable
    {
        public required ScreenFooter Footer
        {
            get => (ScreenFooter)InternalChild;
            init => InternalChild = value;
        }

        private readonly IBindable<bool> backButtonVisibility = new BindableBool();
        private readonly ScreenStackEntry stackEntry;

        public ScreenStackFooter(ScreenStack screenStack)
        {
            RelativeSizeAxes = Axes.Both;

            stackEntry = new ScreenStackEntry(screenStack);
            stackEntry.ScreenChanged += onScreenChanged;

            backButtonVisibility.ValueChanged += onBackButtonVisibilityChanged;
        }

        private void onScreenChanged(IScreen lastScreen, IScreen newScreen)
        {
            unbindScreen(lastScreen);
            bindScreen(newScreen);
        }

        private void onBackButtonVisibilityChanged(ValueChangedEvent<bool> visible)
        {
            if (visible.NewValue)
                Footer.LegacyBackButton.Show();
            else
                Footer.LegacyBackButton.Hide();
        }

        private void unbindScreen(IScreen screen)
        {
            if (screen is not OsuScreen osuScreen)
                return;

            backButtonVisibility.UnbindFrom(osuScreen.BackButtonVisibility);
        }

        private void bindScreen(IScreen screen)
        {
            if (screen is not OsuScreen osuScreen)
            {
                ((BindableBool)backButtonVisibility).Value = true;

                Footer.SetButtons([]);
                Footer.Hide();
                return;
            }

            if (osuScreen.ShowFooter)
            {
                // the legacy back button should never display while the new footer is in use, as it
                // contains its own local back button.
                ((BindableBool)backButtonVisibility).Value = false;

                Footer.Show();

                if (osuScreen.IsLoaded)
                    updateFooterButtons();
                else
                {
                    // ensure the current buttons are immediately disabled on screen change (so they can't be pressed).
                    Footer.SetButtons([]);

                    osuScreen.OnLoadComplete += _ => updateFooterButtons();
                }

                void updateFooterButtons()
                {
                    Footer.SetButtons(osuScreen.CreateFooterButtons());
                    Footer.Show();
                }
            }
            else
            {
                backButtonVisibility.BindTo(osuScreen.BackButtonVisibility);

                Footer.SetButtons([]);
                Footer.Hide();
            }
        }

        protected override void Dispose(bool isDisposing)
        {
            base.Dispose(isDisposing);

            stackEntry.Dispose();
        }

        /// <summary>
        /// Recursively represents a single screen stack and any nested subscreen stack.
        /// </summary>
        private class ScreenStackEntry : IDisposable
        {
            /// <summary>
            /// Invoked when the leading screen changes.
            /// </summary>
            /// <remarks>
            /// This differs from <see cref="ScreenStack.ScreenPushed"/> and <see cref="ScreenStack.ScreenExited"/>
            /// because <c>lastScreen</c> and <c>newScreen</c> may be subscreens of the current screen stack.
            /// <br />
            /// As such, no assumptions may be made as to the relation of screens to this entry's <see cref="ScreenStack"/>.
            /// </remarks>
            public event ScreenChangedDelegate? ScreenChanged;

            /// <summary>
            /// The screen stack tracked by this entry.
            /// </summary>
            private readonly ScreenStack stack;

            /// <summary>
            /// An entry corresponding to the subscreen stack of the current screen, if any.
            /// </summary>
            private ScreenStackEntry? subEntry;

            /// <summary>
            /// The screen which should be bound to the screen footer - the most nested subscreen.
            /// </summary>
            private IScreen leadingScreen => subEntry?.leadingScreen ?? stack.CurrentScreen;

            public ScreenStackEntry(ScreenStack stack)
            {
                this.stack = stack;

                stack.ScreenPushed += onParentScreenChanged;
                stack.ScreenExited += onParentScreenChanged;
            }

            private void onParentScreenChanged(IScreen lastScreen, IScreen newScreen)
            {
                // The screen which we will be UNBINDING from the screen footer later on.
                IScreen lastLeadingScreen = subEntry?.leadingScreen ?? lastScreen;

                // Subscreens are attached to a parent screen, so when the parent changes the subscreen must also.
                subEntry?.Dispose();
                subEntry = null;

                // Check if we've switched to a screen that has a subscreen.
                if (newScreen is IHasSubScreenStack newStack)
                {
                    subEntry = new ScreenStackEntry(newStack.SubScreenStack);
                    subEntry.ScreenChanged += onSubScreenChanged;
                }

                ScreenChanged?.Invoke(lastLeadingScreen, leadingScreen);
            }

            private void onSubScreenChanged(IScreen lastScreen, IScreen newScreen)
            {
                ScreenChanged?.Invoke(lastScreen, newScreen);
            }

            public void Dispose()
            {
                stack.ScreenPushed -= onParentScreenChanged;
                stack.ScreenExited -= onParentScreenChanged;

                if (subEntry != null)
                {
                    subEntry.ScreenChanged -= onSubScreenChanged;
                    subEntry.Dispose();
                }
            }
        }
    }
}
