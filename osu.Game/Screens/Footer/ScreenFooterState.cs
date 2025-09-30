// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;

namespace osu.Game.Screens.Footer
{
    public class ScreenFooterState
    {
        public event Action<ScreenFooterState>? StateChanged;

        private ScreenFooterButton[]? buttons;

        public ScreenFooterButton[]? Buttons
        {
            get => buttons;
            set
            {
                buttons = value;
                StateChanged?.Invoke(this);
            }
        }

        private bool allowBackButton;

        public bool AllowBackButton
        {
            get => allowBackButton;
            set
            {
                allowBackButton = value;
                StateChanged?.Invoke(this);
            }
        }
    }
}
