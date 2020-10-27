// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Game.Configuration;
using osu.Game.Graphics;
using osu.Game.Rulesets.Objects.Drawables;
using System.Collections.Generic;
using osu.Framework.Bindables;
using osu.Framework.Graphics.Sprites;
using osu.Game.Beatmaps;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Scoring;
using osu.Game.Scoring;

namespace osu.Game.Rulesets.Mods
{
    public abstract class ModHidden : Mod, IReadFromConfig, IApplicableToDrawableHitObjects, IApplicableToScoreProcessor, IApplicableToBeatmap
    {
        public override string Name => "Hidden";
        public override string Acronym => "HD";
        public override IconUsage? Icon => OsuIcon.ModHidden;
        public override ModType Type => ModType.DifficultyIncrease;
        public override bool Ranked => true;

        protected Bindable<bool> IncreaseFirstObjectVisibility = new Bindable<bool>();

        /// <summary>
        /// Check whether the provided hitobject should be considered the "first" hideable object.
        /// Can be used to skip spinners, for instance.
        /// </summary>
        /// <param name="hitObject">The hitobject to check.</param>
        protected virtual bool IsFirstHideableObject(HitObject hitObject) => true;

        private HitObject firstHideableObject;

        public void ReadFromConfig(OsuConfigManager config)
        {
            IncreaseFirstObjectVisibility = config.GetBindable<bool>(OsuSetting.IncreaseFirstObjectVisibility);
        }

        public virtual void ApplyToDrawableHitObjects(IEnumerable<DrawableHitObject> drawables)
        {
            foreach (var dho in drawables)
                dho.ApplyCustomUpdateState += applyCustomState;
        }

        public void ApplyToScoreProcessor(ScoreProcessor scoreProcessor)
        {
            // Default value of ScoreProcessor's Rank in Hidden Mod should be SS+
            scoreProcessor.Rank.Value = ScoreRank.XH;
        }

        public void ApplyToBeatmap(IBeatmap beatmap)
        {
            firstHideableObject = getFirstHideableObjectRecursive(beatmap.HitObjects);

            HitObject getFirstHideableObjectRecursive(IReadOnlyList<HitObject> hitObjects)
            {
                foreach (var h in hitObjects)
                {
                    if (IsFirstHideableObject(h))
                        return h;

                    var nestedResult = getFirstHideableObjectRecursive(h.NestedHitObjects);
                    if (nestedResult != null)
                        return nestedResult;
                }

                return null;
            }
        }

        public ScoreRank AdjustRank(ScoreRank rank, double accuracy)
        {
            switch (rank)
            {
                case ScoreRank.X:
                    return ScoreRank.XH;

                case ScoreRank.S:
                    return ScoreRank.SH;

                default:
                    return rank;
            }
        }

        private void applyCustomState(DrawableHitObject hitObject, ArmedState state)
        {
            if (hitObject.HitObject == firstHideableObject && IncreaseFirstObjectVisibility.Value)
                ApplyFirstObjectIncreaseVisibilityState(hitObject, state);
            else
                ApplyHiddenState(hitObject, state);
        }

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
        protected virtual void ApplyHiddenState(DrawableHitObject hitObject, ArmedState state)
        {
        }
    }
}
