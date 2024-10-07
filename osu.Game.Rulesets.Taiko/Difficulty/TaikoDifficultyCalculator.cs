// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using osu.Game.Beatmaps;
using osu.Game.Rulesets.Difficulty;
using osu.Game.Rulesets.Difficulty.Preprocessing;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Scoring;
using osu.Game.Rulesets.Taiko.Difficulty.Preprocessing;
using osu.Game.Rulesets.Taiko.Difficulty.Preprocessing.Colour;
using osu.Game.Rulesets.Taiko.Difficulty.Skills;
using osu.Game.Rulesets.Taiko.Mods;
using osu.Game.Rulesets.Taiko.Scoring;

namespace osu.Game.Rulesets.Taiko.Difficulty
{
    public class TaikoDifficultyCalculator : DifficultyCalculator
    {
        private const double difficulty_multiplier = 0.084375;
        private const double rhythm_skill_multiplier = 0.2 * difficulty_multiplier;
        private const double colour_skill_multiplier = 0.375 * difficulty_multiplier;
        private const double stamina_skill_multiplier = 0.375 * difficulty_multiplier;

        public override int Version => 20221107;

        private readonly HitWindows hitWindows = new TaikoHitWindows();

        private Rhythm? rhythmSkill;
        private Colour? colourSkill;
        private Stamina? staminaSkill;

        public TaikoDifficultyCalculator(IRulesetInfo ruleset, IWorkingBeatmap beatmap)
            : base(ruleset, beatmap)
        {
        }

        protected override void Prepare(DifficultyCalculationContext context)
        {
            hitWindows.SetDifficulty(context.Beatmap.Difficulty.OverallDifficulty);

            rhythmSkill = new Rhythm(context.Mods);
            colourSkill = new Colour(context.Mods);
            staminaSkill = new Stamina(context.Mods);
        }

        protected override IEnumerable<DifficultyHitObject> EnumerateObjects(DifficultyCalculationContext context)
        {
            List<DifficultyHitObject> objects = new List<DifficultyHitObject>();
            List<TaikoDifficultyHitObject> centreObjects = new List<TaikoDifficultyHitObject>();
            List<TaikoDifficultyHitObject> rimObjects = new List<TaikoDifficultyHitObject>();
            List<TaikoDifficultyHitObject> noteObjects = new List<TaikoDifficultyHitObject>();

            int maxCombo = 0;

            for (int i = 0; i < context.Beatmap.HitObjects.Count; i++)
            {
                maxCombo += GetComboIncrease(context.Beatmap.HitObjects[i]);

                if (i < 2)
                    continue;

                objects.Add(
                    new TaikoDifficultyHitObject(
                        context.Beatmap.HitObjects[i], context.Beatmap.HitObjects[i - 1], context.Beatmap.HitObjects[i - 2], context.RateAt(0), objects,
                        centreObjects, rimObjects, noteObjects, objects.Count, maxCombo)
                );
            }

            TaikoColourDifficultyPreprocessor.ProcessAndAssign(objects);

            return objects;
        }

        protected override void ProcessSingle(DifficultyCalculationContext context, DifficultyHitObject hitObject)
        {
            rhythmSkill!.Process(hitObject);
            colourSkill!.Process(hitObject);
            staminaSkill!.Process(hitObject);
        }

        protected override DifficultyAttributes GenerateAttributes(DifficultyCalculationContext context, DifficultyHitObject? hitObject)
        {
            if (hitObject is not TaikoDifficultyHitObject)
                return new TaikoDifficultyAttributes { Mods = context.Mods };

            double colourRating = colourSkill!.DifficultyValue() * colour_skill_multiplier;
            double rhythmRating = rhythmSkill!.DifficultyValue() * rhythm_skill_multiplier;
            double staminaRating = staminaSkill!.DifficultyValue() * stamina_skill_multiplier;

            double combinedRating = combinedDifficultyValue(rhythmSkill, colourSkill, staminaSkill);
            double starRating = rescale(combinedRating * 1.4);

            TaikoDifficultyAttributes attributes = new TaikoDifficultyAttributes
            {
                StarRating = starRating,
                Mods = context.Mods,
                StaminaDifficulty = staminaRating,
                RhythmDifficulty = rhythmRating,
                ColourDifficulty = colourRating,
                PeakDifficulty = combinedRating,
                GreatHitWindow = hitWindows.WindowFor(HitResult.Great) / context.RateAt(0),
                MaxCombo = hitObject.MaxCombo
            };

            return attributes;
        }

        protected override Mod[] DifficultyAdjustmentMods => new Mod[]
        {
            new TaikoModDoubleTime(),
            new TaikoModHalfTime(),
            new TaikoModEasy(),
            new TaikoModHardRock(),
        };

        /// <summary>
        /// Applies a final re-scaling of the star rating.
        /// </summary>
        /// <param name="sr">The raw star rating value before re-scaling.</param>
        private double rescale(double sr)
        {
            if (sr < 0) return sr;

            return 10.43 * Math.Log(sr / 8 + 1);
        }

        /// <summary>
        /// Returns the combined star rating of the beatmap, calculated using peak strains from all sections of the map.
        /// </summary>
        /// <remarks>
        /// For each section, the peak strains of all separate skills are combined into a single peak strain for the section.
        /// The resulting partial rating of the beatmap is a weighted sum of the combined peaks (higher peaks are weighted more).
        /// </remarks>
        private double combinedDifficultyValue(Rhythm rhythm, Colour colour, Stamina stamina)
        {
            List<double> peaks = new List<double>();

            var colourPeaks = colour.GetCurrentStrainPeaks().ToList();
            var rhythmPeaks = rhythm.GetCurrentStrainPeaks().ToList();
            var staminaPeaks = stamina.GetCurrentStrainPeaks().ToList();

            for (int i = 0; i < colourPeaks.Count; i++)
            {
                double colourPeak = colourPeaks[i] * colour_skill_multiplier;
                double rhythmPeak = rhythmPeaks[i] * rhythm_skill_multiplier;
                double staminaPeak = staminaPeaks[i] * stamina_skill_multiplier;

                double peak = norm(1.5, colourPeak, staminaPeak);
                peak = norm(2, peak, rhythmPeak);

                // Sections with 0 strain are excluded to avoid worst-case time complexity of the following sort (e.g. /b/2351871).
                // These sections will not contribute to the difficulty.
                if (peak > 0)
                    peaks.Add(peak);
            }

            double difficulty = 0;
            double weight = 1;

            foreach (double strain in peaks.OrderDescending())
            {
                difficulty += strain * weight;
                weight *= 0.9;
            }

            return difficulty;
        }

        /// <summary>
        /// Returns the <i>p</i>-norm of an <i>n</i>-dimensional vector.
        /// </summary>
        /// <param name="p">The value of <i>p</i> to calculate the norm for.</param>
        /// <param name="values">The coefficients of the vector.</param>
        private double norm(double p, params double[] values) => Math.Pow(values.Sum(x => Math.Pow(x, p)), 1 / p);
    }
}
