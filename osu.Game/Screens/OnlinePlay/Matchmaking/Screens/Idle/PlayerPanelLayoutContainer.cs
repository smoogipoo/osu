// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Graphics.Containers;

namespace osu.Game.Screens.OnlinePlay.Matchmaking.Screens.Idle
{
    /// <summary>
    /// Abstract class for a container that defines the layout for a <see cref="PlayerPanelList"/>.
    /// </summary>
    public abstract partial class PlayerPanelLayoutContainer : CompositeDrawable
    {
        /// <summary>
        /// Retrieves a panel facade to be used for positioning a single <see cref="PlayerPanel"/>.
        /// </summary>
        /// <param name="index">The panel index, in order of placement.</param>
        /// <param name="count">The total number of panels that will be displayed.</param>
        /// <returns>The facade corresponding to the panel.</returns>
        public abstract PlayerPanelFacade GetFacade(int index, int count);
    }
}
