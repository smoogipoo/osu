// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using osu.Framework.Bindables;
using osu.Game.Beatmaps;
using osu.Game.Configuration;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Objects.Drawables;

namespace osu.Game.Rulesets.Mods
{
    public abstract class ModWithFirstObjectVisibilityIncrease : Mod, IReadFromConfig, IApplicableToBeatmap, IApplicableToDrawableHitObjects
    {
        /// <summary>
        /// The first hideable object.
        /// </summary>
        protected HitObject FirstObject { get; private set; }

        /// <summary>
        /// Whether the visibility of <see cref="FirstObject"/> should be increased.
        /// </summary>
        protected readonly Bindable<bool> IncreaseFirstObjectVisibility = new Bindable<bool>();

        /// <summary>
        /// Check whether the provided hitobject should be considered the "first" hideable object.
        /// Can be used to skip spinners, for instance.
        /// </summary>
        /// <param name="hitObject">The hitobject to check.</param>
        protected virtual bool IsFirstAdjustableObject(HitObject hitObject) => true;

        /// <summary>
        /// Apply a special visibility state to the first object in a beatmap, if the user chooses to turn on the "increase first object visibility" setting.
        /// </summary>
        /// <param name="hitObject">The hit object to apply the state change to.</param>
        /// <param name="state">The state of the hit object.</param>
        protected virtual void ApplyFirstObjectIncreaseVisibilityState(DrawableHitObject hitObject, ArmedState state)
        {
        }

        /// <summary>
        /// Apply a hidden state to the provided object.
        /// </summary>
        /// <param name="hitObject">The hit object to apply the state change to.</param>
        /// <param name="state">The state of the hit object.</param>
        protected virtual void ApplyVisibilityState(DrawableHitObject hitObject, ArmedState state)
        {
        }

        public virtual void ReadFromConfig(OsuConfigManager config)
        {
            config.BindWith(OsuSetting.IncreaseFirstObjectVisibility, IncreaseFirstObjectVisibility);
        }

        public virtual void ApplyToBeatmap(IBeatmap beatmap)
        {
            FirstObject = getFirstHideableObjectRecursive(beatmap.HitObjects);

            HitObject getFirstHideableObjectRecursive(IReadOnlyList<HitObject> hitObjects)
            {
                foreach (var h in hitObjects)
                {
                    if (IsFirstAdjustableObject(h))
                        return h;

                    var nestedResult = getFirstHideableObjectRecursive(h.NestedHitObjects);
                    if (nestedResult != null)
                        return nestedResult;
                }

                return null;
            }
        }

        public virtual void ApplyToDrawableHitObjects(IEnumerable<DrawableHitObject> drawables)
        {
            foreach (var dho in drawables)
            {
                dho.ApplyCustomUpdateState += (o, state) =>
                {
                    if (IncreaseFirstObjectVisibility.Value && isObjectObjectOrNested(o.HitObject, FirstObject))
                        ApplyFirstObjectIncreaseVisibilityState(o, state);
                    else
                        ApplyVisibilityState(o, state);
                };
            }
        }

        private bool isObjectObjectOrNested(HitObject toCheck, HitObject target)
        {
            if (target == null)
                return false;

            if (toCheck == target)
                return true;

            foreach (var h in target.NestedHitObjects)
            {
                if (isObjectObjectOrNested(toCheck, h))
                    return true;
            }

            return false;
        }
    }
}
