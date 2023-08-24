// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Bindables;
using osu.Game.Beatmaps;
using osu.Game.Rulesets.Mania.Scoring;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Scoring;

namespace osu.Game.Rulesets.Mania.Mods
{
    public class ManiaModHalfTime : ModHalfTime, IManiaRateAdjustmentMod
    {
        public BindableNumber<double> Rate => SpeedChange;
        public Bindable<HitWindows> HitWindows { get; } = new Bindable<HitWindows>(new ManiaHitWindows());

        BeatmapDifficulty? IManiaRateAdjustmentMod.Difficulty { get; set; }
        IBeatmap? IManiaRateAdjustmentMod.Beatmap { get; set; }
    }
}
