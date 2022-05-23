// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using BenchmarkDotNet.Attributes;
using osu.Game.Rulesets.Osu.Objects;

namespace osu.Game.Benchmarks
{
    public class BenchmarkHitObject : BenchmarkTest
    {
        [Params(1, 100, 1000)]
        public int Count { get; set; }

        [Params(false, true)]
        public bool WithBindable { get; set; }

        [Benchmark]
        public HitCircle[] HitCircle()
        {
            var circles = new HitCircle[Count];

            for (int i = 0; i < Count; i++)
            {
                circles[i] = new HitCircle();

                if (WithBindable)
                {
                    _ = circles[i].PositionBindable;
                    _ = circles[i].ScaleBindable;
                    _ = circles[i].ComboIndexBindable;
                    _ = circles[i].ComboOffsetBindable;
                    _ = circles[i].StackHeightBindable;
                    _ = circles[i].LastInComboBindable;
                    _ = circles[i].ComboIndexWithOffsetsBindable;
                    _ = circles[i].IndexInCurrentComboBindable;
                    _ = circles[i].SamplesBindable;
                    _ = circles[i].StartTimeBindable;
                }
            }

            return circles;
        }
    }
}
