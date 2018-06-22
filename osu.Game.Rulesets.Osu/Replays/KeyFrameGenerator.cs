// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System.Collections.Generic;
using System.Linq;
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
                // All hitobjects require a click
                holdIntervals.AddInterval(addClickAction(obj));

                switch (obj)
                {
                    case Slider slider:
                        foreach (var n in slider.NestedHitObjects)
                        {
                            if (n != slider.HeadCircle)
                                addMovementAction((OsuHitObject)n);
                        }
                        break;
                    case Spinner spinner:
                        SpinIntervals.AddInterval(spinner.StartTime, spinner.EndTime);
                        SpinnerVisibleIntervals.AddInterval(spinner.StartTime - spinner.TimePreempt, spinner.EndTime);
                        break;
                }
            }

            foreach (var interval in holdIntervals)
                addReleaseAction(interval.End);

            foreach (var kvp in KeyFrames)
            {
                foreach (var action in kvp.Value.Actions)
                {
                    switch (action)
                    {
                        case KeyFrameClickAction click:
                            var interval = holdIntervals.IntervalAt(kvp.Key);
                            click.Primary = kvp.Key == interval.Start;
                            break;
                    }
                }
            }
        }

        /// <summary>
        /// Adds a movement <see cref="KeyFrame"/> at a point in time.
        /// </summary>
        /// <param name="obj">The <see cref="OsuHitObject"/> that is targeted by this movement.</param>
        private void addMovementAction(OsuHitObject obj)
        {
            addKeyFrame(obj.StartTime);

            if (KeyFrames[obj.StartTime].Actions.Any(a => a is KeyFrameMovementAction))
                return;

            KeyFrames[obj.StartTime].Actions.Add(new KeyFrameMovementAction
            {
                TargetPoint = new HitPoint
                {
                    Time = obj.StartTime,
                    HitObject = obj
                }
            });
        }

        /// <summary>
        /// Adds a <see cref="KeyFrameClickAction"/> for a <see cref="OsuHitObject"/>.
        /// </summary>
        /// <param name="obj">The <see cref="OsuHitObject"/> that will be clicked.</param>
        /// <returns>The interval of the button being held down for <paramref name="obj"/>.</returns>
        private Interval addClickAction(OsuHitObject obj)
        {
            var interval = new Interval
            {
                Start = obj.StartTime,
                End = ((obj as IHasEndTime)?.EndTime ?? obj.StartTime) + AutoGenerator<OsuHitObject>.KEY_UP_DELAY
            };

            addKeyFrame(interval.Start);
            addKeyFrame(interval.End);

            if (KeyFrames[obj.StartTime].Actions.Any(a => a is KeyFrameClickAction))
                return interval;

            KeyFrames[obj.StartTime].Actions.Add(new KeyFrameClickAction
            {
                TargetPoint = new HitPoint
                {
                    Time = obj.StartTime,
                    HitObject = obj
                }
            });

            return interval;
        }

        /// <summary>
        /// Adds a <see cref="KeyFrameReleaseAction"/> at a point in time.
        /// This will cause a button to be released.
        /// </summary>
        /// <param name="time">The time of the <see cref="KeyFrameReleaseAction"/>.</param>
        private void addReleaseAction(double time)
        {
            addKeyFrame(time);

            KeyFrames[time].Actions.Add(new KeyFrameReleaseAction
            {
                TargetPoint = new HitPoint { Time = time }
            });
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
