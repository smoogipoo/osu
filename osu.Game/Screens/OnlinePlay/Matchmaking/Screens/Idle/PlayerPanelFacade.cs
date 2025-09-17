// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Graphics;

namespace osu.Game.Screens.OnlinePlay.Matchmaking.Screens.Idle
{
    /// <summary>
    /// Defines a single area where a <see cref="PlayerPanel"/> is to be displayed.
    /// </summary>
    public sealed partial class PlayerPanelFacade : Drawable
    {
        /// <summary>
        /// Whether the panel is to be displayed horizontally.
        /// </summary>
        public readonly bool Horizontal;

        /// <summary>
        /// Creates a new <see cref="PlayerPanelFacade"/>.
        /// </summary>
        /// <param name="horizontal">Whether the panel is to be displayed horizontally.</param>
        public PlayerPanelFacade(bool horizontal)
        {
            Horizontal = horizontal;

            Anchor = Anchor.Centre;
            Origin = Anchor.Centre;
            Size = horizontal ? PlayerPanel.SIZE_HORIZONTAL : PlayerPanel.SIZE_VERTICAL;
        }
    }
}
