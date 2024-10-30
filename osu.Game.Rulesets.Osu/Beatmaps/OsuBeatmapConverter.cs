// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osuTK;
using osu.Game.Beatmaps;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Osu.Objects;
using System.Collections.Generic;
using osu.Game.Rulesets.Objects.Types;
using System.Linq;
using System.Threading;
using osu.Game.Rulesets.Osu.UI;
using osu.Framework.Extensions.IEnumerableExtensions;
using osu.Game.Beatmaps.Legacy;

namespace osu.Game.Rulesets.Osu.Beatmaps
{
    public class OsuBeatmapConverter : BeatmapConverter<OsuHitObject>
    {
        public OsuBeatmapConverter(IBeatmap beatmap, Ruleset ruleset)
            : base(beatmap, ruleset)
        {
        }

        public override bool CanConvert() => Beatmap.HitObjects.All(h => h is IHasPosition);

        protected override IEnumerable<OsuHitObject> ConvertHitObject(HitObject original, IBeatmap beatmap, CancellationToken cancellationToken)
        {
            var positionData = original as IHasPosition;
            var comboData = original as IHasCombo;
            var sliderVelocityData = original as IHasSliderVelocity;
            var generateTicksData = original as IHasGenerateTicks;

            switch (original)
            {
                case IHasPathWithRepeats curveData:
                    Slider slider = BeatmapLoadContext.Current.Rent<Slider>();
                    slider.StartTime = original.StartTime;
                    slider.Samples = original.Samples;
                    slider.Path = curveData.Path;
                    slider.NodeSamples = curveData.NodeSamples;
                    slider.RepeatCount = curveData.RepeatCount;
                    slider.Position = positionData?.Position ?? Vector2.Zero;
                    slider.NewCombo = comboData?.NewCombo ?? false;
                    slider.ComboOffset = comboData?.ComboOffset ?? 0;
                    // prior to v8, speed multipliers don't adjust for how many ticks are generated over the same distance.
                    // this results in more (or less) ticks being generated in <v8 maps for the same time duration.
                    slider.TickDistanceMultiplier = beatmap.BeatmapInfo.BeatmapVersion < 8
                        ? 1f / ((LegacyControlPointInfo)beatmap.ControlPointInfo).DifficultyPointAt(original.StartTime).SliderVelocity
                        : 1;
                    slider.GenerateTicks = generateTicksData?.GenerateTicks ?? true;
                    slider.SliderVelocityMultiplier = sliderVelocityData?.SliderVelocityMultiplier ?? 1;
                    return slider.Yield();

                case IHasDuration endTimeData:
                    Spinner spinner = BeatmapLoadContext.Current.Rent<Spinner>();
                    spinner.StartTime = original.StartTime;
                    spinner.Samples = original.Samples;
                    spinner.EndTime = endTimeData.EndTime;
                    spinner.Position = positionData?.Position ?? OsuPlayfield.BASE_SIZE / 2;
                    spinner.NewCombo = comboData?.NewCombo ?? false;
                    spinner.ComboOffset = comboData?.ComboOffset ?? 0;
                    return spinner.Yield();

                default:
                    HitCircle circle = BeatmapLoadContext.Current.Rent<HitCircle>();
                    circle.StartTime = original.StartTime;
                    circle.Samples = original.Samples;
                    circle.Position = positionData?.Position ?? Vector2.Zero;
                    circle.NewCombo = comboData?.NewCombo ?? false;
                    circle.ComboOffset = comboData?.ComboOffset ?? 0;
                    return circle.Yield();
            }
        }

        protected override Beatmap<OsuHitObject> CreateBeatmap() => new OsuBeatmap();
    }
}
