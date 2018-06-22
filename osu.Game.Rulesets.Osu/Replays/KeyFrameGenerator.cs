// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System.Collections.Generic;
using osu.Game.Beatmaps;
using osu.Game.Rulesets.Objects.Types;
using osu.Game.Rulesets.Osu.Objects;
using osu.Game.Rulesets.Replays;

namespace osu.Game.Rulesets.Osu.Replays
{
    /// <summary>
    /// A generator that extracts the most important information from a <see cref="Beatmap"/> for further processing to take place.
    /// </summary>
    public class KeyFrameGenerator
    {
        /// <summary>
        /// A dictionary of { Time, <see cref="KeyFrame"/> } for the important timestamps of the <see cref="Beatmap"/>.
        /// </summary>
        /// <remarks>
        /// Each <see cref="KeyFrame"/> contains information such as where clicks should occur or which points should be moved to.
        /// </remarks>
        public readonly SortedDictionary<double, KeyFrame> KeyFrames = new SortedDictionary<double, KeyFrame>();

        /// <summary>
        /// The set of intervals where spinning a spinner is required.
        /// </summary>
        public readonly IntervalSet SpinIntervals = new IntervalSet();

        /// <summary>
        /// The set of intervals where at least 1 spinner is visible.
        /// </summary>
        public readonly IntervalSet SpinnerVisibleIntervals = new IntervalSet();

        public KeyFrameGenerator(Beatmap<OsuHitObject> beatmap)
        {
            IntervalSet holdIntervals = new IntervalSet();

            foreach (OsuHitObject obj in beatmap.HitObjects)
            {
                // All hitobjects require the button to be held from their start time to their end time
                Interval baseInterval = holdIntervals.AddInterval(obj.StartTime, ((obj as IHasEndTime)?.EndTime ?? obj.StartTime) + AutoGenerator<OsuHitObject>.KEY_UP_DELAY);
                addKeyFrame(baseInterval.Start);
                addKeyFrame(baseInterval.End);

                switch (obj)
                {
                    // Now we add hitpoints of interest (clicks and follows or spins)
                    case HitCircle _:
                        addClickPoint(obj, obj.StartTime);
                        break;
                    case Slider slider:
                        foreach (var n in slider.NestedHitObjects)
                        {
                            if (n == slider.HeadCircle)
                                addClickPoint((OsuHitObject)n, n.StartTime);
                            else
                                addMovePoint((OsuHitObject)n, n.StartTime);
                        }
                        break;
                    case Spinner spinner:
                        Interval interval = SpinIntervals.AddInterval(spinner.StartTime, spinner.EndTime);
                        SpinnerVisibleIntervals.AddInterval(spinner.StartTime - spinner.TimePreempt, spinner.EndTime);

                        // Create keyframes for the spinner, but don't explicitly require movement
                        addKeyFrame(interval.Start);
                        addKeyFrame(interval.End);
                        break;
                }
            }

            // Set Hold and Spin IntervalStates
            var keyFrameIter = KeyFrames.GetEnumerator();
            keyFrameIter.MoveNext();
            foreach (var hold in holdIntervals)
            {
                while (keyFrameIter.Current.Key < hold.Start)
                {
                    keyFrameIter.MoveNext();
                }

                keyFrameIter.Current.Value.Hold = IntervalState.Start;
                keyFrameIter.MoveNext();
                while (keyFrameIter.Current.Key < hold.End)
                {
                    keyFrameIter.Current.Value.Hold = IntervalState.Mid;
                    keyFrameIter.MoveNext();
                }

                keyFrameIter.Current.Value.Hold = IntervalState.End;
                keyFrameIter.MoveNext();
            }

            keyFrameIter.Dispose();
        }

        /// <summary>
        /// Adds a movement <see cref="KeyFrame"/> at a point in time.
        /// </summary>
        /// <param name="obj">The <see cref="OsuHitObject"/> that is targeted by this movement.</param>
        /// <param name="time">The time to add the <see cref="KeyFrame"/> at.</param>
        private void addMovePoint(OsuHitObject obj, double time)
        {
            HitPoint movePoint = new HitPoint
            {
                Time = time,
                HitObject = obj
            };

            addKeyFrame(time);
            KeyFrames[time].Moves.Add(movePoint);
        }

        /// <summary>
        /// Adds a click <see cref="KeyFrame"/> at a point in time.
        /// </summary>
        /// <param name="obj">The <see cref="OsuHitObject"/> that will be clicked.</param>
        /// <param name="time">The time to add the <see cref="KeyFrame"/> at.</param>
        private void addClickPoint(OsuHitObject obj, double time)
        {
            HitPoint clickPoint = new HitPoint
            {
                Time = time,
                HitObject = obj
            };

            addKeyFrame(time);
            KeyFrames[time].Clicks.Add(clickPoint);
        }

        /// <summary>
        /// Create a new <see cref="KeyFrame"/> at a specified time.
        /// </summary>
        /// <param name="time">The time to generate the <see cref="KeyFrame"/> at.</param>
        private void addKeyFrame(double time)
        {
            if (!KeyFrames.ContainsKey(time))
                KeyFrames[time] = new KeyFrame(time);
        }
    }
}
