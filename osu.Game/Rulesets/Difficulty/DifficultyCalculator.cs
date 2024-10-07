// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using osu.Framework.Extensions.IEnumerableExtensions;
using osu.Game.Beatmaps;
using osu.Game.Rulesets.Difficulty.Preprocessing;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Scoring;

namespace osu.Game.Rulesets.Difficulty
{
    public abstract class DifficultyCalculator
    {
        /// <summary>
        /// A yymmdd version which is used to discern when reprocessing is required.
        /// </summary>
        public virtual int Version => 0;

        private readonly IRulesetInfo ruleset;
        private readonly IWorkingBeatmap beatmap;

        protected DifficultyCalculator(IRulesetInfo ruleset, IWorkingBeatmap beatmap)
        {
            this.ruleset = ruleset;
            this.beatmap = beatmap;
        }

        public IEnumerable<TimedDifficultyAttributes> Calculate(CancellationToken cancellationToken = default)
            => Calculate(Array.Empty<Mod>(), cancellationToken);

        public IEnumerable<TimedDifficultyAttributes> Calculate(Mod[] mods, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();

            // Isolate mods for this process.
            Mod[] playableMods = mods.Select(m => m.DeepClone()).ToArray();

            // Only pass through the cancellation token if it's non-default.
            // This allows for the default timeout to be applied for playable beatmap construction.
            IBeatmap playableBeatmap = cancellationToken == default
                ? beatmap.GetPlayableBeatmap(ruleset, playableMods)
                : beatmap.GetPlayableBeatmap(ruleset, playableMods, cancellationToken);

            DifficultyCalculationContext context = new DifficultyCalculationContext(playableBeatmap, playableMods);

            Prepare(context);

            bool anyAttributes = false;

            foreach (var obj in EnumerateObjects(context))
            {
                ProcessSingle(context, obj);
                yield return new TimedDifficultyAttributes(obj.BaseObject.GetEndTime(), GenerateAttributes(context, obj));
                anyAttributes = true;
            }

            if (!anyAttributes)
                yield return new TimedDifficultyAttributes(0, GenerateAttributes(context, null));
        }

        /// <summary>
        /// Prepares for a new processing context.
        /// </summary>
        /// <param name="context">The processing context.</param>
        protected abstract void Prepare(DifficultyCalculationContext context);

        /// <summary>
        /// Enumerates the hitobjects that contribute to difficulty from a beatmap.
        /// </summary>
        /// <param name="context">The processing context.</param>
        protected abstract IEnumerable<DifficultyHitObject> EnumerateObjects(DifficultyCalculationContext context);

        /// <summary>
        /// Processes a single hitobject.
        /// </summary>
        /// <param name="context">The processing context.</param>
        /// <param name="hitObject">The hitobject to process.</param>
        protected abstract void ProcessSingle(DifficultyCalculationContext context, DifficultyHitObject hitObject);

        /// <summary>
        /// Generates difficulty attributes at the current time.
        /// </summary>
        /// <param name="context">The processing context.</param>
        /// <param name="hitObject">The last processed hitobject.</param>
        protected abstract DifficultyAttributes GenerateAttributes(DifficultyCalculationContext context, DifficultyHitObject? hitObject);

        /// <summary>
        /// Calculates the difficulty of the beatmap using all mod combinations applicable to the beatmap.
        /// </summary>
        /// <remarks>
        /// This can only be used to compute difficulties for legacy mod combinations.
        /// </remarks>
        /// <returns>A collection of structures describing the difficulty of the beatmap for each mod combination.</returns>
        public IEnumerable<TimedDifficultyAttributes> CalculateAllLegacyCombinations(CancellationToken cancellationToken = default)
        {
            yield break;

            // var rulesetInstance = ruleset.CreateInstance();
            //
            // foreach (var combination in CreateDifficultyAdjustmentModCombinations())
            // {
            //     Mod? classicMod = rulesetInstance.CreateMod<ModClassic>();
            //
            //     var finalCombination = ModUtils.FlattenMod(combination);
            //     if (classicMod != null)
            //         finalCombination = finalCombination.Append(classicMod);
            //
            //     yield return Calculate(finalCombination.ToArray(), cancellationToken);
            // }
        }

        /// <summary>
        /// Creates all <see cref="Mod"/> combinations which adjust the <see cref="Beatmaps.Beatmap"/> difficulty.
        /// </summary>
        public Mod[] CreateDifficultyAdjustmentModCombinations()
        {
            return createDifficultyAdjustmentModCombinations(DifficultyAdjustmentMods, Array.Empty<Mod>()).ToArray();

            static IEnumerable<Mod> createDifficultyAdjustmentModCombinations(ReadOnlyMemory<Mod> remainingMods, IEnumerable<Mod> currentSet, int currentSetCount = 0)
            {
                // Return the current set.
                switch (currentSetCount)
                {
                    case 0:
                        // Initial-case: Empty current set
                        yield return new ModNoMod();

                        break;

                    case 1:
                        yield return currentSet.Single();

                        break;

                    default:
                        yield return new MultiMod(currentSet.ToArray());

                        break;
                }

                // Apply the rest of the remaining mods recursively.
                for (int i = 0; i < remainingMods.Length; i++)
                {
                    (var nextSet, int nextCount) = flatten(remainingMods.Span[i]);

                    // Check if any mods in the next set are incompatible with any of the current set.
                    if (currentSet.SelectMany(m => m.IncompatibleMods).Any(c => nextSet.Any(c.IsInstanceOfType)))
                        continue;

                    // Check if any mods in the next set are the same type as the current set. Mods of the exact same type are not incompatible with themselves.
                    if (currentSet.Any(c => nextSet.Any(n => c.GetType() == n.GetType())))
                        continue;

                    // If all's good, attach the next set to the current set and recurse further.
                    foreach (var combo in createDifficultyAdjustmentModCombinations(remainingMods.Slice(i + 1), currentSet.Concat(nextSet), currentSetCount + nextCount))
                        yield return combo;
                }
            }

            // Flattens a mod hierarchy (through MultiMod) as an IEnumerable<Mod>
            static (IEnumerable<Mod> set, int count) flatten(Mod mod)
            {
                if (!(mod is MultiMod multi))
                    return (mod.Yield(), 1);

                IEnumerable<Mod> set = Enumerable.Empty<Mod>();
                int count = 0;

                foreach (var nested in multi.Mods)
                {
                    (var nestedSet, int nestedCount) = flatten(nested);
                    set = set.Concat(nestedSet);
                    count += nestedCount;
                }

                return (set, count);
            }
        }

        /// <summary>
        /// Retrieves all <see cref="Mod"/>s which adjust the <see cref="Beatmaps.Beatmap"/> difficulty.
        /// </summary>
        protected virtual Mod[] DifficultyAdjustmentMods => Array.Empty<Mod>();

        protected static int GetComboIncrease(HitObject hitObject)
        {
            int increase = 0;

            if (hitObject.Judgement.MaxResult.AffectsCombo())
                increase++;

            foreach (var nested in hitObject.NestedHitObjects)
                increase += GetComboIncrease(nested);

            return increase;
        }
    }
}
