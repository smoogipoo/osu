// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Diagnostics;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Objects.Drawables;
using osu.Game.Rulesets.Osu.Objects.Drawables;
using osu.Game.Rulesets.UI;

namespace osu.Game.Rulesets.Osu.UI
{
    /// <summary>
    /// Ensures that <see cref="HitObject"/>s are hit in order of appearance. The classic note lock.
    /// <remarks>
    /// Hits will be blocked until the previous <see cref="HitObject"/>s have been judged.
    /// </remarks>
    /// </summary>
    public class ObjectOrderedHitPolicy : IHitPolicy
    {
        public IHitObjectContainer HitObjectContainer { get; set; }

        public bool IsHittable(DrawableHitObject hitObject, double time)
        {
            if (hitObject.HitObject.StartTime == 213200)
            {
            }

            foreach (var obj in HitObjectContainer.AliveObjects)
            {
                DrawableSlider slider = obj as DrawableSlider;
                DrawableHitObject target = slider?.HeadCircle ?? obj;

                if (target == hitObject)
                    return true;

                switch (target)
                {
                    case DrawableSpinner _:
                        // Spinners don't prevent future hitobjects from being hittable.
                        continue;

                    case DrawableSliderHead head:
                        Debug.Assert(slider != null);

                        // Sliders prevent the future hitobjects from being hittable until the hittable range for the head has passed, regardless of whether the head has been judged.
                        if ((time <= head.HitObject.StartTime || head.HitObject.HitWindows.CanBeHit(time - head.HitObject.StartTime))
                            // UNLESS the slider has been fully judged.
                            && !slider.AllJudged)
                        {
                            return false;
                        }

                        break;

                    default:
                        // All other objects prevent future hitobjects from being hittable unless they're judged.
                        if (!target.AllJudged)
                            return false;

                        break;
                }
            }

            return false;
        }

        public void HandleHit(DrawableHitObject hitObject)
        {
        }
    }
}
