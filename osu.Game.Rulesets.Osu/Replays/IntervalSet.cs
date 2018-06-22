// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using System.Collections.Generic;

namespace osu.Game.Rulesets.Osu.Replays
{
    /// <summary>
    /// Stores an interal, usually an interval between two timestamps.
    /// </summary>
    public class IntervalSet : List<Interval>
    {
        /// <summary>
        /// Add a new interval to the interval set, merging intervals if they overlap.
        /// Returns the interval that was ultimately added (after merging)
        /// </summary>
        public Interval AddInterval(double start, double end)
        {
            if (end < start)
                return null;

            // Smallest and largest overlapping intervals
            int lowest = FindIndex(s => s.End >= start);
            int highest = FindLastIndex(s => s.Start <= end);

            // This means that the interval being inserted is larger than all existing intervals.
            if (lowest == -1)
            {
                lowest = Count;
            }

            // The case where the interval is smaller than everything is "automatic"
            //if (highest == -1) {
            //    highest = -1;
            //}

            if (lowest == highest + 1)
            {
                Interval interval = new Interval
                {
                    Start = start,
                    End = end
                };
                // There are no intervals to merge
                Insert(lowest, interval);
                return interval;
            }
            else
            {
                // Create a new interval that merges the overlapping intervals
                Interval interval = new Interval
                {
                    Start = Math.Min(start, this[lowest].Start),
                    End = Math.Max(end, this[highest].End)
                };

                RemoveRange(lowest, highest - lowest + 1);
                Insert(lowest, interval);
                return interval;
            }
        }

        public Interval AddInterval(Interval interval)
        {
            return AddInterval(interval.Start, interval.End);
        }

        public void RemoveInterval(double start, double end)
        {
            // Smallest and largest overlapping intervals
            int lowest = BinarySearch(new Interval { Start = start, End = start});
            int highest = BinarySearch(new Interval { Start = end, End = end });

            // Special case where both lowest and highest are on the same interval
            if (lowest >= 0 && lowest == highest)
            {
                double origend = this[lowest].End;
                this[lowest].End = start;
                AddInterval(end, origend);
                return;
            }

            // Trim the edge overlapping intervals
            // also set lowest and highest to the boundaries of all the intervals fully contained in (start, end)
            if (lowest >= 0)
                this[lowest++].End = start;
            else
                lowest = ~lowest;
            if (highest >= 0)
                this[highest].Start = end;
            else
                highest = ~highest;

            // Remove all the intervals that were fully contained
            RemoveRange(lowest, highest - lowest);
        }

        public void RemoveInterval(Interval interval)
        {
            RemoveInterval(interval.Start, interval.End);
        }

        public bool Contains(double value)
        {
            return BinarySearch(new Interval { Start = value, End = value }) >= 0;
        }

        public Interval GetIntervalContaining(double value)
        {
            int index = BinarySearch(new Interval { Start = value, End = value });
            if (index >= 0)
                return this[index];
            else
                return null;
        }

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
                {
                    break;
                }

                result.AddInterval(Math.Max(start, this[index].Start), Math.Min(end, this[index].End));
            }

            return result;
        }

        public IntervalSet Intersect(Interval interval)
        {
            return Intersect(interval.Start, interval.End);
        }
    }
}
