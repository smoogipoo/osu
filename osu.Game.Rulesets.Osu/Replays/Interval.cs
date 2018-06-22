// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;

namespace osu.Game.Rulesets.Osu.Replays
{
    /// <summary>
    /// An interval. Stores a start point and an end point.
    /// </summary>
    public class Interval : IComparable<Interval>, IEquatable<Interval>
    {
        public double Start;
        public double End;

        public int CompareTo(Interval i)
        {
            if (End < i.Start)
                return -1;

            if (Start > i.End)
                return 1;

            // The two intervals overlap
            return 0;
        }

        public bool Equals(Interval other) => other != null && Start.Equals(other.Start) && End.Equals(other.End);

        public override string ToString() => $"{Start} -> {End}";
    }
}
