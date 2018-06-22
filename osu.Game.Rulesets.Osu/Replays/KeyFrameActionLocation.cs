// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

namespace osu.Game.Rulesets.Osu.Replays
{
    /// <summary>
    /// Represents the location of a <see cref="KeyFrameAction"/> within the interval of
    /// <see cref="KeyFrameAction"/>'s that share the same <see cref="KeyFrameActionType"/>.
    /// </summary>
    public enum KeyFrameActionLocation
    {
        /// <summary>
        /// The action occurs at the start of the interval of similar <see cref="KeyFrameActionType"/>.
        /// </summary>
        Start,
        /// <summary>
        /// The action is in progress.
        /// </summary>
        Mid,
        /// <summary>
        /// The action has completed.
        /// </summary>
        End
    }
}
