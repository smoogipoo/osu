// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

namespace osu.Game.Rulesets.Osu.Replays
{
    public class KeyFrameClickAction : KeyFrameAction
    {
        /// <summary>
        /// Whether this click action is the first in the sequence.
        /// </summary>
        public bool Primary = true;
    }
}
