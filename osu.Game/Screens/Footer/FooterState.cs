// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

namespace osu.Game.Screens.Footer
{
    public partial class ScreenFooter
    {
        private class FooterState
        {
            public ScreenFooterButton[]? Buttons { get; set; }
            public bool ShowBackButton { get; set; }
        }
    }
}
