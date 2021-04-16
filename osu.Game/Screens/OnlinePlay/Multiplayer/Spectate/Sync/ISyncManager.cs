// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Timing;

namespace osu.Game.Screens.OnlinePlay.Multiplayer.Spectate.Sync
{
    /// <summary>
    /// Manages the synchronisation between one or more <see cref="ISyncSlaveClock"/>s in relation to a master clock.
    /// </summary>
    public interface ISyncManager
    {
        /// <summary>
        /// The master clock which slaves should synchronise to.
        /// </summary>
        IAdjustableClock Master { get; }

        /// <summary>
        /// Adds an <see cref="ISyncSlaveClock"/> to manage.
        /// </summary>
        /// <param name="clock">The <see cref="ISyncSlaveClock"/> to add.</param>
        void AddSlave(ISyncSlaveClock clock);

        /// <summary>
        /// Removes an <see cref="ISyncSlaveClock"/>, stopping it from being managed by this <see cref="ISyncManager"/>.
        /// </summary>
        /// <param name="clock">The <see cref="ISyncSlaveClock"/> to remove.</param>
        void RemoveSlave(ISyncSlaveClock clock);
    }
}
