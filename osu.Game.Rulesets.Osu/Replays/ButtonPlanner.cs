// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using osu.Game.Rulesets.Osu.Objects;
using osu.Game.Rulesets.Replays;

namespace osu.Game.Rulesets.Osu.Replays
{
    // Handles alternating buttons and 2B style playing
    public class ButtonPlanner
    {
        private ButtonPlan curr = new ButtonPlan();

        // Parameters
        //private const bool cycle_when_both_held = false;
        private const double alternate_threshold = 150; // 150ms is threshold for 120bpm streams

        // Extra metadata to manage state changes (when to alternate after Press, etc)
        private double lastUsedLeft = double.NegativeInfinity;
        private double lastUsedRight = double.NegativeInfinity;

        private int numHeld; // Buttons currently held

        private void setLastUsed(Button b, double time)
        {
            if (b.HasFlag(Button.Left))
                lastUsedLeft = Math.Max(lastUsedLeft, time);
            else
                lastUsedRight = Math.Max(lastUsedRight, time);
        }

        public ButtonPlan Press(double time)
        {
            if (numHeld == 0)
            {
                // Decide whether to alternate or not
                if (time - lastUsedLeft + AutoGenerator<OsuHitObject>.KEY_UP_DELAY > alternate_threshold)
                {
                    // The time since last used is big enough so we singletap
                    curr = new ButtonPlan { Primary = Button.Left };
                }
                else if (lastUsedLeft < lastUsedRight)
                {
                    // We're alternating, use the less recently used button
                    curr = new ButtonPlan { Primary = Button.Left };
                }
                else
                {
                    curr = new ButtonPlan { Primary = Button.Right };
                }

                setLastUsed(curr.Primary, time);
            }
            else if (numHeld == 1)
            {
                // Uncomment these if you want to use this option,
                // inspectcode doesn't like either public fields that are never accessed or unreachable code.
                // if (cycle_when_both_held) {
                //     curr = new ButtonPlan{
                //         Primary   = curr.Primary ^ (Button.Left | Button.Right),
                //         Secondary = curr.Primary
                //     };
                //     setLastUsed(curr.Primary, time);
                // }
                // else
                // {
                curr = new ButtonPlan
                {
                    Primary = curr.Primary,
                    Secondary = curr.Primary ^ (Button.Left | Button.Right)
                };
                setLastUsed(curr.Secondary, time);
                // }
            }
            else
            {
                // what
                numHeld--;
                throw new InvalidOperationException("Trying to click when both buttons are already pressed is likely a mistake. (at " + time + ")");
            }

            numHeld++;
            return curr;
        }

        public ButtonPlan Release(double time)
        {
            if (numHeld == 1)
            {
                setLastUsed(curr.Primary, time);
                curr = new ButtonPlan();
            }
            else if (numHeld == 2)
            {
                setLastUsed(curr.Secondary, time);
                curr = new ButtonPlan
                {
                    Primary = curr.Primary
                };
            }
            else
            {
                // do nothing
                numHeld++;
            }

            numHeld--;
            return curr;
        }
    }
}
