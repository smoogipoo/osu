// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Diagnostics;
using osu.Framework.Bindables;
using osu.Game.Beatmaps;
using osu.Game.Rulesets.Mania.Objects;
using osu.Game.Rulesets.Mania.Scoring;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Scoring;

namespace osu.Game.Rulesets.Mania.Mods
{
    /// <summary>
    /// May be attached to rate-adjustment mods to adjust hit windows adjust relative to gameplay rate.
    /// </summary>
    /// <remarks>
    /// Historically, in osu!mania, hit windows are expected to adjust relative to the gameplay rate such that the real-world hit window remains the same.
    /// </remarks>
    public interface IManiaRateAdjustmentMod : IApplicableToDifficulty, IApplicableToBeatmap, IApplicableHitWindows
    {
        /// <summary>
        /// The rate that should be applied to the hit windows.
        /// </summary>
        BindableNumber<double> Rate { get; }

        /// <summary>
        /// The beatmap difficulty. Internal use only.
        /// </summary>
        BeatmapDifficulty? Difficulty { get; set; }

        /// <summary>
        /// The beatmap. Internal use only.
        /// </summary>
        IBeatmap? Beatmap { get; set; }

        void UpdateHitWindows(ValueChangedEvent<double> rate)
        {
            Stopwatch sw = Stopwatch.StartNew();

            HitWindows hitWindows = new ManiaHitWindows(rate.NewValue);

            if (Difficulty != null)
                hitWindows.SetDifficulty(Difficulty.OverallDifficulty);

            if (Beatmap != null)
            {
                foreach (var hitObject in Beatmap.HitObjects)
                {
                    switch (hitObject)
                    {
                        case Note:
                            hitObject.HitWindows = hitWindows;
                            break;

                        case HoldNote hold:
                            hold.Head.HitWindows = hitWindows;
                            hold.Tail.HitWindows = hitWindows;
                            break;
                    }
                }
            }

            HitWindows.Value = hitWindows;

            Console.WriteLine($"Updating hit windows took {sw.Elapsed.TotalMilliseconds}ms");
        }

        void UpdateHitWindowsFromDifficulty(BeatmapDifficulty difficulty)
        {
            Difficulty = difficulty;
            UpdateHitWindows(new ValueChangedEvent<double>(Rate.Value, Rate.Value));
        }

        void UpdateHitWindowsFromBeatmap(IBeatmap beatmap)
        {
            Beatmap = beatmap;
            UpdateHitWindows(new ValueChangedEvent<double>(Rate.Value, Rate.Value));
        }

        void IApplicableToDifficulty.ApplyToDifficulty(BeatmapDifficulty difficulty) => UpdateHitWindowsFromDifficulty(difficulty);

        void IApplicableToBeatmap.ApplyToBeatmap(IBeatmap beatmap) => UpdateHitWindowsFromBeatmap(beatmap);
    }
}
