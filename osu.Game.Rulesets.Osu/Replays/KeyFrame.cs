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
        /// The actions that need to be performed during this <see cref="KeyFrame"/>.
        /// </summary>
        public readonly List<KeyFrameAction> Actions = new List<KeyFrameAction>();

        public KeyFrame(double time)
        {
            Time = time;
        }
    }
}
