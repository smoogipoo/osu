// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using BenchmarkDotNet.Attributes;
using osu.Framework.Bindables;
using osu.Game.Utils;

namespace osu.Game.Benchmarks
{
    [MemoryDiagnoser]
    public class BenchmarkModUtils
    {
        [Benchmark]
        public float? GetValueNonNullable() => (float?)ModUtils.GetSettingUnderlyingValue(new Bindable<float>(1.5f));

        [Benchmark]
        public float? GetValueNullable() => (float?)ModUtils.GetSettingUnderlyingValue(new Bindable<float?>(1.5f));
    }
}
