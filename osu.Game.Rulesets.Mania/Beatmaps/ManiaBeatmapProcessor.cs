// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Linq;
using osu.Framework.Utils;
using osu.Game.Beatmaps;
using osu.Game.Rulesets.Mania.Objects;
using osu.Game.Screens.Edit;

namespace osu.Game.Rulesets.Mania.Beatmaps
{
    public class ManiaBeatmapProcessor : BeatmapProcessor
    {
        public ManiaBeatmapProcessor(IBeatmap beatmap)
            : base(beatmap)
        {
        }

        public override void PostProcess()
        {
            base.PostProcess();

            foreach (var h in Beatmap.HitObjects.Cast<ManiaHitObject>())
            {
                var timingPoint = Beatmap.ControlPointInfo.TimingPointAt(h.StartTime);

                foreach (var d in BindableBeatDivisor.VALID_DIVISORS)
                {
                    double snapLength = timingPoint.BeatLength / d;
                    double delta = (h.StartTime - timingPoint.Time);

                    int beatIndex = (int)Math.Round(delta / snapLength);

                    if (!Precision.AlmostEquals(delta, beatIndex * snapLength, 1))
                        continue;

                    h.SnapDivisor = d;
                    break;
                }
            }
        }
    }
}
