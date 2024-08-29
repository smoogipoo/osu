// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

namespace osu.Game.Rulesets.Mods
{
    /// <summary>
    /// Interface for mods that block the player from failing.
    /// </summary>
    public interface IBlockFail
    {
        /// <summary>
        /// Check if a fail should be allowed. This is only triggered once a failing condition has been met.
        /// </summary>
        /// <returns>Whether the fail should be allowed.</returns>
        bool AllowFail();
    }
}
