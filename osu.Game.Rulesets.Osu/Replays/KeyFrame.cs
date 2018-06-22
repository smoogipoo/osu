// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System.Collections.Generic;

namespace osu.Game.Rulesets.Osu.Replays
{
    /// <summary>
    /// Aggregates all the hitpoints/zones at a certain time into one data object.
    /// </summary>
    public class KeyFrame
    {
        /// <summary>
        /// The time of this <see cref="KeyFrame"/>.
        /// </summary>
        public readonly double Time;

        /// <summary>
        /// The current gameplay hold state.
        /// </summary>
        public IntervalState Hold = IntervalState.None;

        /// <summary>
        /// List of <see cref="HitPoint"/>s where the cursor should be near to.
        /// </summary>
        public readonly List<HitPoint> Moves = new List<HitPoint>();

        /// <summary>
        /// Whether the cursor should be near any points.
        /// </summary>
        public bool HasMove => Moves.Count > 0;

        /// <summary>
        /// List of <see cref="HitPoint"/>s that need to be clicked.
        /// </summary>
        public readonly List<HitPoint> Clicks = new List<HitPoint>();

        /// <summary>
        /// Whether any <see cref="HitPoint"/>s need to be clicked.
        /// </summary>
        public bool HasClick => Clicks.Count > 0;

        public KeyFrame(double time)
        {
            Time = time;
        }
    }
}
