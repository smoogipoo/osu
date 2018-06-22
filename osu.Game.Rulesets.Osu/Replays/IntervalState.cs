// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

namespace osu.Game.Rulesets.Osu.Replays
{
    /// <summary>
    /// Represents the gameplay state during an interval.
    /// </summary>
    public enum IntervalState
    {
        /// <summary>
        /// Nothing is happening.
        /// </summary>
        None,
        /// <summary>
        /// Gameplay is at the start of an interval.
        /// </summary>
        Start,
        /// <summary>
        /// Gameplay is within an interval.
        /// </summary>
        Mid,
        /// <summary>
        /// Gameplay is at the end of an interval.
        /// </summary>
        End
    }
}
