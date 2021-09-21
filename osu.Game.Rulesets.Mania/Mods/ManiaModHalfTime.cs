// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Game.Beatmaps;
using osu.Game.Rulesets.Mania.Objects;
using osu.Game.Rulesets.Mania.Scoring;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Scoring;

namespace osu.Game.Rulesets.Mania.Mods
{
    public class ManiaModHalfTime : ModHalfTime, IApplicableToDifficulty, IApplicableToBeatmap
    {
        public override double ScoreMultiplier => 0.5;

        private BeatmapDifficulty difficulty;

        public void ApplyToDifficulty(BeatmapDifficulty difficulty)
        {
            // This is only stored and used in ApplyToBeatmap, which is the last thing to run before the beatmap becomes usable for gameplay.
            this.difficulty = difficulty;
        }

        // This mod uses ApplyToBeatmap to enforce that it runs after all other mods (such as ModDifficultyAdjust) which could affect BeatmapDifficulty.
        public void ApplyToBeatmap(IBeatmap beatmap)
        {
            var hitWindows = new ManiaRateAdjustedHitWindows(SpeedChange.Value);
            hitWindows.SetDifficulty(difficulty.OverallDifficulty);

            foreach (var hitobject in beatmap.HitObjects)
                applyToObject(hitobject, hitWindows);

            static void applyToObject(HitObject obj, HitWindows hitWindows)
            {
                var maniaObj = (ManiaHitObject)obj;
                maniaObj.HitWindows = hitWindows;

                for (int i = 0; i < maniaObj.NestedHitObjects.Count; i++)
                    applyToObject(maniaObj.NestedHitObjects[i], hitWindows);
            }
        }
    }
}
