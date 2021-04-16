// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Bindables;
using osu.Framework.Timing;

namespace osu.Game.Screens.OnlinePlay.Multiplayer.Spectate.Sync
{
    /// <summary>
    /// A clock which is used by spectator player and managed by an <see cref="ISyncManager"/>.
    /// </summary>
    public interface ISyncSlaveClock : IFrameBasedClock, IAdjustableClock
    {
        /// <summary>
        /// Whether this clock is waiting on frames to continue playback.
        /// </summary>
        IBindable<bool> WaitingOnFrames { get; }

        /// <summary>
        /// Whether this clock is resynchronising to the master clock.
        /// </summary>
        bool IsCatchingUp { get; set; }
    }
}
