// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using NUnit.Framework;
using osu.Game.Audio;
using osu.Game.Beatmaps;
using osu.Game.IO.Serialization;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Objects.Types;
using osu.Game.Rulesets.Osu;
using osu.Game.Rulesets.Osu.Beatmaps;
using osu.Game.Rulesets.Osu.Objects;
using osu.Game.Screens.Edit;
using osuTK;

namespace osu.Game.Tests.Editing
{
    [TestFixture]
    public class JsonBeatmapPatcherTest
    {
        private JsonBeatmapPatcher patcher;
        private EditorBeatmap current;

        [SetUp]
        public void Setup()
        {
            patcher = new JsonBeatmapPatcher(current = new EditorBeatmap(new OsuBeatmap
            {
                BeatmapInfo =
                {
                    Ruleset = new OsuRuleset().RulesetInfo
                }
            }));
        }

        [Test]
        public void TestPatchNoObjectChanges()
        {
            runTest(new OsuBeatmap());
        }

        [Test]
        public void TestAddHitObject()
        {
            var patch = new OsuBeatmap
            {
                HitObjects =
                {
                    new HitCircle { StartTime = 1000, NewCombo = true }
                }
            };

            runTest(patch);
        }

        [Test]
        public void TestInsertHitObject()
        {
            current.AddRange(new[]
            {
                new HitCircle { StartTime = 1000, NewCombo = true },
                new HitCircle { StartTime = 3000 },
            });

            var patch = new OsuBeatmap
            {
                HitObjects =
                {
                    (OsuHitObject)current.HitObjects[0],
                    new HitCircle { StartTime = 2000 },
                    (OsuHitObject)current.HitObjects[1],
                }
            };

            runTest(patch);
        }

        [Test]
        public void TestDeleteHitObject()
        {
            current.AddRange(new[]
            {
                new HitCircle { StartTime = 1000, NewCombo = true },
                new HitCircle { StartTime = 2000 },
                new HitCircle { StartTime = 3000 },
            });

            var patch = new OsuBeatmap
            {
                HitObjects =
                {
                    (OsuHitObject)current.HitObjects[0],
                    (OsuHitObject)current.HitObjects[2],
                }
            };

            runTest(patch);
        }

        [Test]
        public void TestChangeStartTime()
        {
            current.AddRange(new[]
            {
                new HitCircle { StartTime = 1000, NewCombo = true },
                new HitCircle { StartTime = 2000 },
                new HitCircle { StartTime = 3000 },
            });

            var patch = new OsuBeatmap
            {
                HitObjects =
                {
                    new HitCircle { StartTime = 500, NewCombo = true },
                    (OsuHitObject)current.HitObjects[1],
                    (OsuHitObject)current.HitObjects[2],
                }
            };

            runTest(patch);
        }

        [Test]
        public void TestChangeSample()
        {
            current.AddRange(new[]
            {
                new HitCircle { StartTime = 1000, NewCombo = true },
                new HitCircle { StartTime = 2000 },
                new HitCircle { StartTime = 3000 },
            });

            var patch = new OsuBeatmap
            {
                HitObjects =
                {
                    (OsuHitObject)current.HitObjects[0],
                    new HitCircle { StartTime = 2000, Samples = { new HitSampleInfo(HitSampleInfo.HIT_FINISH) } },
                    (OsuHitObject)current.HitObjects[2],
                }
            };

            runTest(patch);
        }

        [Test]
        public void TestChangeSliderPath()
        {
            current.AddRange(new OsuHitObject[]
            {
                new HitCircle { StartTime = 1000, NewCombo = true },
                new Slider
                {
                    StartTime = 2000,
                    Path = new SliderPath(new[]
                    {
                        new PathControlPoint(Vector2.Zero),
                        new PathControlPoint(Vector2.One),
                        new PathControlPoint(new Vector2(2), PathType.Bezier),
                        new PathControlPoint(new Vector2(3)),
                    }, 50)
                },
                new HitCircle { StartTime = 3000 },
            });

            var patch = new OsuBeatmap
            {
                HitObjects =
                {
                    (OsuHitObject)current.HitObjects[0],
                    new Slider
                    {
                        StartTime = 2000,
                        Path = new SliderPath(new[]
                        {
                            new PathControlPoint(Vector2.Zero, PathType.Bezier),
                            new PathControlPoint(new Vector2(4)),
                            new PathControlPoint(new Vector2(5)),
                        }, 100)
                    },
                    (OsuHitObject)current.HitObjects[2],
                }
            };

            runTest(patch);
        }

        [Test]
        public void TestAddMultipleHitObjects()
        {
            current.AddRange(new[]
            {
                new HitCircle { StartTime = 1000, NewCombo = true },
                new HitCircle { StartTime = 2000 },
                new HitCircle { StartTime = 3000 },
            });

            var patch = new OsuBeatmap
            {
                HitObjects =
                {
                    new HitCircle { StartTime = 500, NewCombo = true },
                    (OsuHitObject)current.HitObjects[0],
                    new HitCircle { StartTime = 1500 },
                    (OsuHitObject)current.HitObjects[1],
                    new HitCircle { StartTime = 2250 },
                    new HitCircle { StartTime = 2500 },
                    (OsuHitObject)current.HitObjects[2],
                    new HitCircle { StartTime = 3500 },
                }
            };

            runTest(patch);
        }

        [Test]
        public void TestDeleteMultipleHitObjects()
        {
            current.AddRange(new[]
            {
                new HitCircle { StartTime = 500, NewCombo = true },
                new HitCircle { StartTime = 1000 },
                new HitCircle { StartTime = 1500 },
                new HitCircle { StartTime = 2000 },
                new HitCircle { StartTime = 2250 },
                new HitCircle { StartTime = 2500 },
                new HitCircle { StartTime = 3000 },
                new HitCircle { StartTime = 3500 },
            });

            var patchedFirst = (HitCircle)current.HitObjects[1];
            patchedFirst.NewCombo = true;

            var patch = new OsuBeatmap
            {
                HitObjects =
                {
                    (OsuHitObject)current.HitObjects[1],
                    (OsuHitObject)current.HitObjects[3],
                    (OsuHitObject)current.HitObjects[6],
                }
            };

            runTest(patch);
        }

        [Test]
        public void TestChangeSamplesOfMultipleHitObjects()
        {
            current.AddRange(new[]
            {
                new HitCircle { StartTime = 500, NewCombo = true },
                new HitCircle { StartTime = 1000 },
                new HitCircle { StartTime = 1500 },
                new HitCircle { StartTime = 2000 },
                new HitCircle { StartTime = 2250 },
                new HitCircle { StartTime = 2500 },
                new HitCircle { StartTime = 3000 },
                new HitCircle { StartTime = 3500 },
            });

            var patch = new OsuBeatmap
            {
                HitObjects =
                {
                    (OsuHitObject)current.HitObjects[0],
                    new HitCircle { StartTime = 1000, Samples = { new HitSampleInfo(HitSampleInfo.HIT_FINISH) } },
                    (OsuHitObject)current.HitObjects[2],
                    (OsuHitObject)current.HitObjects[3],
                    new HitCircle { StartTime = 2250, Samples = { new HitSampleInfo(HitSampleInfo.HIT_WHISTLE) } },
                    (OsuHitObject)current.HitObjects[5],
                    new HitCircle { StartTime = 3000, Samples = { new HitSampleInfo(HitSampleInfo.HIT_CLAP) } },
                    (OsuHitObject)current.HitObjects[7],
                }
            };

            runTest(patch);
        }

        [Test]
        public void TestAddAndDeleteHitObjects()
        {
            current.AddRange(new[]
            {
                new HitCircle { StartTime = 500, NewCombo = true },
                new HitCircle { StartTime = 1000 },
                new HitCircle { StartTime = 1500 },
                new HitCircle { StartTime = 2000 },
                new HitCircle { StartTime = 2250 },
                new HitCircle { StartTime = 2500 },
                new HitCircle { StartTime = 3000 },
                new HitCircle { StartTime = 3500 },
            });

            var patch = new OsuBeatmap
            {
                HitObjects =
                {
                    new HitCircle { StartTime = 750, NewCombo = true },
                    (OsuHitObject)current.HitObjects[1],
                    (OsuHitObject)current.HitObjects[4],
                    (OsuHitObject)current.HitObjects[5],
                    new HitCircle { StartTime = 2650 },
                    new HitCircle { StartTime = 2750 },
                    new HitCircle { StartTime = 4000 },
                }
            };

            runTest(patch);
        }

        [Test]
        public void TestChangeHitObjectAtSameTime()
        {
            current.AddRange(new[]
            {
                new HitCircle { StartTime = 500, Position = new Vector2(50), NewCombo = true },
                new HitCircle { StartTime = 500, Position = new Vector2(100), NewCombo = true },
                new HitCircle { StartTime = 500, Position = new Vector2(150), NewCombo = true },
                new HitCircle { StartTime = 500, Position = new Vector2(200), NewCombo = true },
            });

            var patch = new OsuBeatmap
            {
                HitObjects =
                {
                    new HitCircle { StartTime = 500, Position = new Vector2(150), NewCombo = true },
                    new HitCircle { StartTime = 500, Position = new Vector2(100), NewCombo = true },
                    new HitCircle { StartTime = 500, Position = new Vector2(50), NewCombo = true },
                    new HitCircle { StartTime = 500, Position = new Vector2(200), NewCombo = true },
                }
            };

            runTest(patch);
        }

        private void runTest(IBeatmap patch)
        {
            // Due to the method of testing, "patch" comes in without having been decoded via a beatmap decoder.
            // This causes issues because the decoder adds various default properties (e.g. new combo on first object, default samples).
            // To resolve "patch" into a sane state it is encoded and then re-decoded.
            var ruleset = current.BeatmapInfo.Ruleset;
            var beatmapProcessor = ruleset.CreateInstance().CreateBeatmapProcessor(patch);
            patch.BeatmapInfo.Ruleset = ruleset;
            beatmapProcessor.PreProcess();
            foreach (var h in patch.HitObjects)
                h.ApplyDefaults(patch.ControlPointInfo, patch.Difficulty);
            beatmapProcessor.PostProcess();

            // Apply the patch.
            patcher.Patch(encode(current.PlayableBeatmap), encode(patch));

            // Convert beatmaps to strings for assertion purposes.
            string currentStr = encode(current.PlayableBeatmap);
            string patchStr = encode(patch);

            Assert.That(currentStr, Is.EqualTo(patchStr));
        }

        private string encode(IBeatmap beatmap) => beatmap.Serialize();
    }
}
