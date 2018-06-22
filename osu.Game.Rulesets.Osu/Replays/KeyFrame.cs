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
        // The timestamp where all this is happening
        public readonly double Time;

        // Whether we're at the start of a holdZone, middle of one, or at the end of one.
        public IntervalState Hold = IntervalState.None;
        public bool WasHolding => Hold == IntervalState.Mid || Hold == IntervalState.End;
        public bool Holding => Hold == IntervalState.Start || Hold == IntervalState.Mid;

        // Ditto for spins
        public IntervalState Spin = IntervalState.None;
        public bool WasSpinning => Spin == IntervalState.Mid || Spin == IntervalState.End;
        public bool Spinning => Spin == IntervalState.Start || Hold == IntervalState.Mid;

        // List of hitpoints we want our cursor to be near to
        public readonly List<Hitpoint> Moves = new List<Hitpoint>();
        public bool HasMove => Moves.Count > 0;

        // List of hitpoints we need to click
        public readonly List<Hitpoint> Clicks = new List<Hitpoint>();
        public bool HasClick => Clicks.Count > 0;

        public KeyFrame(double time)
        {
            Time = time;
        }
    }
}
