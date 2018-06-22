// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

namespace osu.Game.Rulesets.Osu.Replays
{
    /// <summary>
    /// The types of action performed during the a <see cref="KeyFrame"/>.
    /// </summary>
    public enum KeyFrameActionType
    {
        /// <summary>
        /// The cursor should be moved.
        /// </summary>
        Move,
        /// <summary>
        /// A button should be clicked.
        /// </summary>
        Click,
        /// <summary>
        /// A button should be released.
        /// </summary>
        Release,
        /// <summary>
        /// The cursor should be spun.
        /// </summary>
        Spin
    }
}
