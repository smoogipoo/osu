// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Graphics;
using osuTK;

namespace osu.Game.Screens.OnlinePlay.Multiplayer.Spectate
{
    public class PlayerFacade : Drawable
    {
        /// <summary>
        /// The size of the entire screen area.
        /// </summary>
        public Vector2 FullSize;

        public PlayerFacade()
        {
            Anchor = Anchor.Centre;
            Origin = Anchor.Centre;
        }
    }
}
