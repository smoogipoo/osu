// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Bindables;
using osu.Game.Beatmaps;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Scoring;

namespace osu.Game.Rulesets.Mania.Mods
{
    public class ManiaModWindDown : ModWindDown, IManiaRateAdjustmentMod
    {
        public BindableNumber<double> Rate => SpeedChange;
        public Bindable<HitWindows> HitWindows { get; } = new Bindable<HitWindows>();

        BeatmapDifficulty? IManiaRateAdjustmentMod.Difficulty { get; set; }
        IBeatmap? IManiaRateAdjustmentMod.Beatmap { get; set; }

        public ManiaModWindDown()
        {
            Rate.BindValueChanged(((IManiaRateAdjustmentMod)this).UpdateHitWindows);
        }

        public override void ApplyToBeatmap(IBeatmap beatmap)
        {
            base.ApplyToBeatmap(beatmap);
            ((IManiaRateAdjustmentMod)this).UpdateHitWindowsFromBeatmap(beatmap);
        }
    }
}
