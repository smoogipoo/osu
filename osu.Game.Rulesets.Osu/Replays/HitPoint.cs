// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Game.Rulesets.Osu.Objects;
using OpenTK;

namespace osu.Game.Rulesets.Osu.Replays
{
    /// <summary>
    /// Basically a timestamp and a position, used to generate positions.
    /// Keeps a reference to the corresponding <see cref="OsuHitObject"/> in so we can see whether we should follow sliders or not afterwards.
    /// </summary>
    public class HitPoint
    {
        public double Time;

        // The circle/slider/spinner associated with this hitpoint
        public OsuHitObject HitObject;

        public Vector2 Position
        {
            get
            {
                Slider s = HitObject as Slider;
                if (s != null)
                {
                    double progress = (Time - s.StartTime) / s.Duration;
                    return s.StackedPositionAt(progress) + s.StackOffset;
                }
                else
                {
                    return HitObject.StackedPosition;
                }
            }
        }
    }
}
