// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using osu.Game.Beatmaps;
using osu.Game.Rulesets.Catch.Beatmaps;
using osu.Game.Rulesets.Catch.Difficulty.Preprocessing;
using osu.Game.Rulesets.Catch.Difficulty.Skills;
using osu.Game.Rulesets.Catch.Mods;
using osu.Game.Rulesets.Catch.Objects;
using osu.Game.Rulesets.Catch.UI;
using osu.Game.Rulesets.Difficulty;
using osu.Game.Rulesets.Difficulty.Preprocessing;
using osu.Game.Rulesets.Mods;

namespace osu.Game.Rulesets.Catch.Difficulty
{
    public class CatchDifficultyCalculator : DifficultyCalculator
    {
        private const double difficulty_multiplier = 4.59;

        private float halfCatcherWidth;

        public override int Version => 20220701;

        private Movement? movementSkill;

        public CatchDifficultyCalculator(IRulesetInfo ruleset, IWorkingBeatmap beatmap)
            : base(ruleset, beatmap)
        {
        }

        protected override void Prepare(DifficultyCalculationContext context)
        {
            halfCatcherWidth = Catcher.CalculateCatchWidth(context.Beatmap.Difficulty) * 0.5f;

            // For circle sizes above 5.5, reduce the catcher width further to simulate imperfect gameplay.
            halfCatcherWidth *= 1 - Math.Max(0, context.Beatmap.Difficulty.CircleSize - 5.5f) * 0.0625f;

            movementSkill = new Movement(context.Mods, halfCatcherWidth, context.RateAt(0));
        }

        protected override IEnumerable<DifficultyHitObject> EnumerateObjects(DifficultyCalculationContext context)
        {
            List<DifficultyHitObject> objects = new List<DifficultyHitObject>();

            CatchHitObject? lastObject = null;
            int maxCombo = 0;

            // In 2B beatmaps, it is possible that a normal Fruit is placed in the middle of a JuiceStream.
            foreach (var hitObject in CatchBeatmap.GetPalpableObjects(context.Beatmap.HitObjects))
            {
                // We want to only consider fruits that contribute to the combo.
                if (hitObject is Banana || hitObject is TinyDroplet)
                    continue;

                maxCombo += GetComboIncrease(hitObject);

                if (lastObject != null)
                    objects.Add(new CatchDifficultyHitObject(hitObject, lastObject, context.RateAt(0), halfCatcherWidth, objects, objects.Count, maxCombo));

                lastObject = hitObject;
            }

            return objects;
        }

        protected override void ProcessSingle(DifficultyCalculationContext context, DifficultyHitObject hitObject)
        {
            movementSkill!.Process(hitObject);
        }

        protected override DifficultyAttributes GenerateAttributes(DifficultyCalculationContext context, DifficultyHitObject? hitObject)
        {
            if (hitObject is not CatchDifficultyHitObject)
                return new CatchDifficultyAttributes { Mods = context.Mods };

            // this is the same as osu!, so there's potential to share the implementation... maybe
            double preempt = IBeatmapDifficultyInfo.DifficultyRange(context.Beatmap.Difficulty.ApproachRate, 1800, 1200, 450) / context.RateAt(0);

            CatchDifficultyAttributes attributes = new CatchDifficultyAttributes
            {
                StarRating = Math.Sqrt(movementSkill!.DifficultyValue()) * difficulty_multiplier,
                Mods = context.Mods,
                ApproachRate = preempt > 1200.0 ? -(preempt - 1800.0) / 120.0 : -(preempt - 1200.0) / 150.0 + 5.0,
                MaxCombo = hitObject.MaxCombo,
            };

            return attributes;
        }

        protected override Mod[] DifficultyAdjustmentMods => new Mod[]
        {
            new CatchModDoubleTime(),
            new CatchModHalfTime(),
            new CatchModHardRock(),
            new CatchModEasy(),
        };
    }
}
