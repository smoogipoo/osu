// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using NUnit.Framework;
using osu.Game.Beatmaps;
using osu.Game.Rulesets;
using osu.Game.Rulesets.Difficulty;
using osu.Game.Rulesets.Difficulty.Preprocessing;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.UI;
using osu.Game.Tests.Beatmaps;

namespace osu.Game.Tests.NonVisual
{
    [TestFixture]
    public class TestSceneTimedDifficultyCalculation
    {
        [Test]
        public void TestAttributesGeneratedForEachObjectOnce()
        {
            var beatmap = new Beatmap<TestHitObject>
            {
                HitObjects =
                {
                    new TestHitObject { StartTime = 1 },
                    new TestHitObject
                    {
                        StartTime = 2,
                        Nested = 1
                    },
                    new TestHitObject { StartTime = 3 },
                }
            };

            List<TimedDifficultyAttributes> attribs = new TestDifficultyCalculator(new TestWorkingBeatmap(beatmap)).Calculate().ToList();

            Assert.That(attribs.Count, Is.EqualTo(3));
            assertEquals(attribs[0], beatmap.HitObjects[0]);
            assertEquals(attribs[1], beatmap.HitObjects[0], beatmap.HitObjects[1]);
            assertEquals(attribs[2], beatmap.HitObjects[0], beatmap.HitObjects[1], beatmap.HitObjects[2]);
        }

        [Test]
        public void TestAttributesGeneratedForSkippedObjects()
        {
            var beatmap = new Beatmap<TestHitObject>
            {
                HitObjects =
                {
                    // The first object is usually skipped in all implementations
                    new TestHitObject
                    {
                        StartTime = 1,
                        Skip = true
                    },
                    // An intermediate skipped object.
                    new TestHitObject
                    {
                        StartTime = 2,
                        Skip = true
                    },
                    new TestHitObject { StartTime = 3 },
                }
            };

            List<TimedDifficultyAttributes> attribs = new TestDifficultyCalculator(new TestWorkingBeatmap(beatmap)).Calculate().ToList();

            Assert.That(attribs.Count, Is.EqualTo(3));
            assertEquals(attribs[0], beatmap.HitObjects[0]);
            assertEquals(attribs[1], beatmap.HitObjects[0], beatmap.HitObjects[1]);
            assertEquals(attribs[2], beatmap.HitObjects[0], beatmap.HitObjects[1], beatmap.HitObjects[2]);
        }

        [Test]
        public void TestAttributesGeneratedOnceForSkippedObjects()
        {
            var beatmap = new Beatmap<TestHitObject>
            {
                HitObjects =
                {
                    new TestHitObject { StartTime = 1 },
                    new TestHitObject
                    {
                        StartTime = 2,
                        Nested = 5,
                        Skip = true
                    },
                    new TestHitObject
                    {
                        StartTime = 3,
                        Skip = true
                    },
                }
            };

            List<TimedDifficultyAttributes> attribs = new TestDifficultyCalculator(new TestWorkingBeatmap(beatmap)).Calculate().ToList();

            Assert.That(attribs.Count, Is.EqualTo(3));
            assertEquals(attribs[0], beatmap.HitObjects[0]);
            assertEquals(attribs[1], beatmap.HitObjects[0], beatmap.HitObjects[1]);
            assertEquals(attribs[2], beatmap.HitObjects[0], beatmap.HitObjects[1], beatmap.HitObjects[2]);
        }

        private void assertEquals(TimedDifficultyAttributes attribs, params HitObject[] expected)
        {
            Assert.That(((TestDifficultyAttributes)attribs.Attributes).Objects, Is.EquivalentTo(expected));
        }

        private class TestHitObject : HitObject
        {
            /// <summary>
            /// Whether to skip generating a difficulty representation for this object.
            /// </summary>
            public bool Skip { get; set; }

            /// <summary>
            /// Whether to generate nested difficulty representations for this object, and if so, how many.
            /// </summary>
            public int Nested { get; set; }

            protected override void CreateNestedHitObjects(CancellationToken cancellationToken)
            {
                for (int i = 0; i < Nested; i++)
                    AddNested(new TestHitObject { StartTime = StartTime + 0.1 * i });
            }
        }

        private class TestRuleset : Ruleset
        {
            public override IEnumerable<Mod> GetModsFor(ModType type) => Enumerable.Empty<Mod>();

            public override DrawableRuleset CreateDrawableRulesetWith(IBeatmap beatmap, IReadOnlyList<Mod>? mods = null) => throw new NotImplementedException();

            public override IBeatmapConverter CreateBeatmapConverter(IBeatmap beatmap) => new PassThroughBeatmapConverter(beatmap);

            public override DifficultyCalculator CreateDifficultyCalculator(IWorkingBeatmap beatmap) => new TestDifficultyCalculator(beatmap);

            public override string Description => string.Empty;
            public override string ShortName => string.Empty;

            private class PassThroughBeatmapConverter : IBeatmapConverter
            {
                public event Action<HitObject, IEnumerable<HitObject>>? ObjectConverted
                {
                    add { }
                    remove { }
                }

                public IBeatmap Beatmap { get; }

                public PassThroughBeatmapConverter(IBeatmap beatmap)
                {
                    Beatmap = beatmap;
                }

                public bool CanConvert() => true;

                public IBeatmap Convert(CancellationToken cancellationToken = default) => Beatmap;
            }
        }

        private class TestDifficultyCalculator : DifficultyCalculator
        {
            private readonly List<HitObject> processedObjects = new List<HitObject>();

            public TestDifficultyCalculator(IWorkingBeatmap beatmap)
                : base(new TestRuleset().RulesetInfo, beatmap)
            {
            }

            protected override void Prepare(DifficultyCalculationContext context)
            {
            }

            protected override IEnumerable<DifficultyHitObject> EnumerateObjects(DifficultyCalculationContext context)
            {
                List<DifficultyHitObject> objects = new List<DifficultyHitObject>();

                int maxCombo = 0;
                int countObjects = 0;

                foreach (var obj in context.Beatmap.HitObjects.OfType<TestHitObject>())
                {
                    maxCombo += GetComboIncrease(obj);
                    countObjects++;

                    if (!obj.Skip)
                        objects.Add(new TestDifficultyHitObject(obj, obj, context.RateAt(0), objects, objects.Count, maxCombo, countObjects));

                    foreach (var nested in obj.NestedHitObjects)
                        objects.Add(new TestDifficultyHitObject(nested, nested, context.RateAt(0), objects, objects.Count, maxCombo, countObjects));
                }

                return objects;
            }

            protected override void ProcessSingle(DifficultyCalculationContext context, DifficultyHitObject hitObject)
            {
            }

            protected override DifficultyAttributes GenerateAttributes(DifficultyCalculationContext context, DifficultyHitObject? hitObject)
            {
                if (hitObject is not TestDifficultyHitObject testObject)
                    return new TestDifficultyAttributes { Mods = context.Mods };

                return new TestDifficultyAttributes
                {
                    Mods = context.Mods,
                    Objects = context.Beatmap.HitObjects.Take(testObject.CountObjects).ToArray()
                };
            }

            private class TestDifficultyHitObject : DifficultyHitObject
            {
                public readonly int CountObjects;

                public TestDifficultyHitObject(HitObject hitObject, HitObject lastObject, double clockRate, List<DifficultyHitObject> objects, int index, int maxCombo, int countObjects)
                    : base(hitObject, lastObject, clockRate, objects, index, maxCombo)
                {
                    CountObjects = countObjects;
                }
            }
        }

        private class TestDifficultyAttributes : DifficultyAttributes
        {
            public HitObject[] Objects = Array.Empty<HitObject>();
        }
    }
}
