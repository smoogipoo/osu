// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using System.Collections.Generic;

namespace osu.Game.Rulesets.Osu.Replays
{
    /// <summary>
    /// Stores a set of <see cref="Interval"/>s, each between two data points.
    /// </summary>
    public class IntervalSet : List<Interval>
    {
        /// <summary>
        /// Adds a new interval to the set, merging intervals if they overlap.
        /// Returns the interval that was ultimately added (after merging).
        /// </summary>
        public Interval AddInterval(double start, double end)
        {
            if (end < start)
                throw new ArgumentException($"{nameof(start)} must be <= {nameof(end)}.");

            // Any existing interval which overlaps at the start point of the new one
            int startOverlap = FindIndex(s => s.End >= start);

            // Any existing interval which overlaps at the end point of the new one
            int endOverlap = FindLastIndex(s => s.Start <= end);

            Interval newInterval;

            if (startOverlap == -1)
            {
                // Adding at end, no overlap possible
                startOverlap = Count;
                Insert(startOverlap, newInterval = new Interval
                {
                    Start = start,
                    End = end
                });
            }
            else if (endOverlap == -1)
            {
                // Adding at start, no overlap possible
                Insert(startOverlap, newInterval = new Interval
                {
                    Start = start,
                    End = end
                });
            }
            else
            {
                // Adding somewhere in the middle, merge any overlapped intervals
                double newStart = Math.Min(start, this[startOverlap].Start);
                double newEnd = Math.Max(end, this[endOverlap].End);

                RemoveRange(startOverlap, endOverlap - startOverlap + 1);
                Insert(startOverlap, newInterval = new Interval
                {
                    Start = newStart,
                    End = newEnd
                });
            }

            return newInterval;
        }

        public Interval IntervalAt(double value)
        {
            int index = BinarySearch(new Interval { Start = value, End = value });
            if (index < 0)
                throw new ArgumentOutOfRangeException(nameof(value), "The point is not contained by any interval.");

            return this[index];
        }

        public bool TryGetIntervalAt(double value, out Interval interval)
        {
            int index = BinarySearch(new Interval { Start = value, End = value });
            if (index < 0)
            {
                interval = default(Interval);
                return false;
            }

            interval = this[index];
            return true;
        }

        public bool IsInInterval(double value) => BinarySearch(new Interval { Start = value, End = value }) >= 0;

        public IntervalSet Intersect(double start, double end)
        {
            if (end < start)
                return new IntervalSet();

            int startindex = BinarySearch(new Interval { Start = start, End = start });
            if (startindex < 0)
                startindex = ~startindex;

            IntervalSet result = new IntervalSet();
            for (int index = startindex; index < Count; index++)
            {
                if (this[index].Start > end)
                    break;

                result.AddInterval(Math.Max(start, this[index].Start), Math.Min(end, this[index].End));
            }

            return result;
        }
    }
}
