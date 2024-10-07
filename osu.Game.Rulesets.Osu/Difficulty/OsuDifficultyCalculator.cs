// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using osu.Game.Beatmaps;
using osu.Game.Rulesets.Difficulty;
using osu.Game.Rulesets.Difficulty.Preprocessing;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Osu.Difficulty.Preprocessing;
using osu.Game.Rulesets.Osu.Difficulty.Skills;
using osu.Game.Rulesets.Osu.Mods;
using osu.Game.Rulesets.Osu.Objects;
using osu.Game.Rulesets.Osu.Scoring;
using osu.Game.Rulesets.Scoring;

namespace osu.Game.Rulesets.Osu.Difficulty
{
    public class OsuDifficultyCalculator : DifficultyCalculator
    {
        private const double difficulty_multiplier = 0.0675;

        public override int Version => 20220902;

        private readonly HitWindows hitWindows = new OsuHitWindows();

        private Aim? aimSkill;
        private Aim? aimSkillNoSliders;
        private Speed? speedSkill;
        private Flashlight? flashlightSkill;

        public OsuDifficultyCalculator(IRulesetInfo ruleset, IWorkingBeatmap beatmap)
            : base(ruleset, beatmap)
        {
        }

        [MemberNotNull(nameof(aimSkill), nameof(aimSkillNoSliders), nameof(speedSkill))]
        protected override void Prepare(DifficultyCalculationContext context)
        {
            hitWindows.SetDifficulty(context.Beatmap.Difficulty.OverallDifficulty);

            aimSkill = new Aim(context.Mods, true);
            aimSkillNoSliders = new Aim(context.Mods, false);
            speedSkill = new Speed(context.Mods);
            flashlightSkill = context.Mods.OfType<OsuModFlashlight>().Any() ? new Flashlight(context.Mods) : null;
        }

        protected override IEnumerable<DifficultyHitObject> EnumerateObjects(DifficultyCalculationContext context)
        {
            List<DifficultyHitObject> objects = new List<DifficultyHitObject>();

            int maxCombo = 0;
            int circleCount = 0;
            int sliderCount = 0;
            int spinnerCount = 0;

            for (int i = 0; i < context.Beatmap.HitObjects.Count; i++)
            {
                maxCombo += GetComboIncrease(context.Beatmap.HitObjects[i]);

                switch (context.Beatmap.HitObjects[i])
                {
                    case HitCircle:
                        circleCount++;
                        break;

                    case Slider:
                        sliderCount++;
                        break;

                    case Spinner:
                        spinnerCount++;
                        break;
                }

                // The first jump is formed by the first two hitobjects of the map.
                // If the map has less than two OsuHitObjects, the enumerator will not return anything.
                if (i < 1)
                    continue;

                var lastLast = i > 1 ? context.Beatmap.HitObjects[i - 2] : null;

                objects.Add(new OsuDifficultyHitObject(context.Beatmap.HitObjects[i], context.Beatmap.HitObjects[i - 1], lastLast, context.RateAt(0), objects, objects.Count, maxCombo)
                {
                    CircleCount = circleCount,
                    SliderCount = sliderCount,
                    SpinnerCount = spinnerCount
                });
            }

            return objects;
        }

        protected override void ProcessSingle(DifficultyCalculationContext context, DifficultyHitObject hitObject)
        {
            aimSkill!.Process(hitObject);
            aimSkillNoSliders!.Process(hitObject);
            speedSkill!.Process(hitObject);
            flashlightSkill?.Process(hitObject);
        }

        protected override DifficultyAttributes GenerateAttributes(DifficultyCalculationContext context, DifficultyHitObject? hitObject)
        {
            if (hitObject is not OsuDifficultyHitObject osuObject)
                return new OsuDifficultyAttributes { Mods = context.Mods };

            double aimRating = Math.Sqrt(aimSkill!.DifficultyValue()) * difficulty_multiplier;
            double aimRatingNoSliders = Math.Sqrt(aimSkillNoSliders!.DifficultyValue()) * difficulty_multiplier;
            double speedRating = Math.Sqrt(speedSkill!.DifficultyValue()) * difficulty_multiplier;
            double flashlightRating = flashlightSkill?.DifficultyValue() * difficulty_multiplier ?? 0;

            if (context.Mods.Any(m => m is OsuModTouchDevice))
            {
                aimRating = Math.Pow(aimRating, 0.8);
                flashlightRating = Math.Pow(flashlightRating, 0.8);
            }

            if (context.Mods.Any(h => h is OsuModRelax))
            {
                aimRating *= 0.9;
                speedRating = 0.0;
                flashlightRating *= 0.7;
            }

            double baseAimPerformance = OsuStrainSkill.DifficultyToPerformance(aimRating);
            double baseSpeedPerformance = OsuStrainSkill.DifficultyToPerformance(speedRating);
            double baseFlashlightPerformance = Flashlight.DifficultyToPerformance(flashlightRating);

            double basePerformance =
                Math.Pow(
                    Math.Pow(baseAimPerformance, 1.1) +
                    Math.Pow(baseSpeedPerformance, 1.1) +
                    Math.Pow(baseFlashlightPerformance, 1.1), 1.0 / 1.1
                );

            double starRating = basePerformance > 0.00001
                ? Math.Cbrt(OsuPerformanceCalculator.PERFORMANCE_BASE_MULTIPLIER) * 0.027 * (Math.Cbrt(100000 / Math.Pow(2, 1 / 1.1) * basePerformance) + 4)
                : 0;

            double preempt = IBeatmapDifficultyInfo.DifficultyRange(context.Beatmap.Difficulty.ApproachRate, 1800, 1200, 450) / context.RateAt(0);
            double hitWindowGreat = hitWindows.WindowFor(HitResult.Great) / context.RateAt(0);

            OsuDifficultyAttributes attributes = new OsuDifficultyAttributes
            {
                StarRating = starRating,
                Mods = context.Mods,
                AimDifficulty = aimRating,
                SpeedDifficulty = speedRating,
                SpeedNoteCount = speedSkill.RelevantNoteCount(),
                FlashlightDifficulty = flashlightRating,
                SliderFactor = aimRating > 0 ? aimRatingNoSliders / aimRating : 1,
                AimDifficultStrainCount = aimSkill.CountDifficultStrains(),
                SpeedDifficultStrainCount = speedSkill.CountDifficultStrains(),
                ApproachRate = preempt > 1200 ? (1800 - preempt) / 120 : (1200 - preempt) / 150 + 5,
                OverallDifficulty = (80 - hitWindowGreat) / 6,
                DrainRate = context.Beatmap.Difficulty.DrainRate,
                MaxCombo = hitObject.MaxCombo,
                HitCircleCount = osuObject.CircleCount,
                SliderCount = osuObject.SliderCount,
                SpinnerCount = osuObject.SpinnerCount,
            };

            return attributes;
        }

        protected override Mod[] DifficultyAdjustmentMods => new Mod[]
        {
            new OsuModTouchDevice(),
            new OsuModDoubleTime(),
            new OsuModHalfTime(),
            new OsuModEasy(),
            new OsuModHardRock(),
            new OsuModFlashlight(),
            new MultiMod(new OsuModFlashlight(), new OsuModHidden())
        };
    }
}
