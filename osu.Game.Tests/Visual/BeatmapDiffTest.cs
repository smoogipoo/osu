// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using DiffPlex;
using Microsoft.EntityFrameworkCore.Internal;
using NUnit.Framework;
using osu.Framework.Audio.Track;
using osu.Framework.Graphics.Textures;
using osu.Game.Audio;
using osu.Game.Beatmaps;
using osu.Game.Beatmaps.Formats;
using osu.Game.IO;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Objects.Types;
using osu.Game.Rulesets.Osu;
using osu.Game.Rulesets.Osu.Beatmaps;
using osu.Game.Rulesets.Osu.Objects;
using osu.Game.Screens.Edit;
using osuTK;
using Decoder = osu.Game.Beatmaps.Formats.Decoder;

namespace osu.Game.Tests.Visual
{
    [TestFixture]
    public class BeatmapDiffTest
    {
        private LegacyEditorBeatmapDiffer differ;
        private EditorBeatmap current;

        [SetUp]
        public void Setup()
        {
            differ = new LegacyEditorBeatmapDiffer(current = new EditorBeatmap(new OsuBeatmap
            {
                BeatmapInfo =
                {
                    Ruleset = new OsuRuleset().RulesetInfo
                }
            }));
        }

        [Test]
        public void TestAddHitObject()
        {
            var patch = new OsuBeatmap
            {
                HitObjects =
                {
                    new HitCircle { StartTime = 1000 }
                }
            };

            runTest(patch);
        }

        [Test]
        public void TestInsertHitObject()
        {
            current.AddRange(new[]
            {
                new HitCircle { StartTime = 1000 },
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
                new HitCircle { StartTime = 1000 },
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
                new HitCircle { StartTime = 1000 },
                new HitCircle { StartTime = 2000 },
                new HitCircle { StartTime = 3000 },
            });

            var patch = new OsuBeatmap
            {
                HitObjects =
                {
                    new HitCircle { StartTime = 500 },
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
                new HitCircle { StartTime = 1000 },
                new HitCircle { StartTime = 2000 },
                new HitCircle { StartTime = 3000 },
            });

            var patch = new OsuBeatmap
            {
                HitObjects =
                {
                    (OsuHitObject)current.HitObjects[0],
                    new HitCircle { StartTime = 2000, Samples = { new HitSampleInfo { Name = HitSampleInfo.HIT_FINISH } } },
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
                new HitCircle { StartTime = 1000 },
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
                new HitCircle { StartTime = 1000 },
                new HitCircle { StartTime = 2000 },
                new HitCircle { StartTime = 3000 },
            });

            var patch = new OsuBeatmap
            {
                HitObjects =
                {
                    new HitCircle { StartTime = 500 },
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
                new HitCircle { StartTime = 500 },
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
                new HitCircle { StartTime = 500 },
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
                    new HitCircle { StartTime = 1000, Samples = { new HitSampleInfo { Name = HitSampleInfo.HIT_FINISH } } },
                    (OsuHitObject)current.HitObjects[2],
                    (OsuHitObject)current.HitObjects[3],
                    new HitCircle { StartTime = 2250, Samples = { new HitSampleInfo { Name = HitSampleInfo.HIT_WHISTLE } } },
                    (OsuHitObject)current.HitObjects[5],
                    new HitCircle { StartTime = 3000, Samples = { new HitSampleInfo { Name = HitSampleInfo.HIT_CLAP } } },
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
                new HitCircle { StartTime = 500 },
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
                    new HitCircle { StartTime = 750 },
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

        private void runTest(IBeatmap patch)
        {
            // Due to the method of testing, "patch" comes in without having been decoded via a beatmap decoder.
            // This causes issues because the decoder adds various default properties (e.g. new combo on first object, default samples).
            // To resolve "patch" into a sane state it is encoded and then re-decoded.
            patch = decode(encode(patch));

            // Apply the patch.
            differ.Patch(encode(current), encode(patch));

            // Convert beatmaps to strings for assertion purposes.
            string currentStr = Encoding.ASCII.GetString(encode(current).ToArray());
            string patchStr = Encoding.ASCII.GetString(encode(patch).ToArray());

            Assert.That(currentStr, Is.EqualTo(patchStr));
        }

        private MemoryStream encode(IBeatmap beatmap)
        {
            var encoded = new MemoryStream();

            using (var sw = new StreamWriter(encoded, leaveOpen: true))
                new LegacyBeatmapEncoder(beatmap).Encode(sw);

            return encoded;
        }

        private IBeatmap decode(Stream stream)
        {
            stream.Seek(0, SeekOrigin.Begin);

            using (var reader = new LineBufferedReader(stream, true))
                return Decoder.GetDecoder<Beatmap>(reader).Decode(reader);
        }

        private class LegacyEditorBeatmapDiffer
        {
            private readonly EditorBeatmap editorBeatmap;

            public LegacyEditorBeatmapDiffer(EditorBeatmap editorBeatmap)
            {
                this.editorBeatmap = editorBeatmap;
            }

            public void Patch(Stream currentState, Stream newState)
            {
                // Diff the beatmaps
                var result = new Differ().CreateLineDiffs(readString(currentState), readString(newState), true, false);

                int oldHitObjectsIndex = result.PiecesOld.IndexOf("[HitObjects]");
                int newHitObjectsIndex = result.PiecesNew.IndexOf("[HitObjects]");

                var toRemove = new List<int>();
                var toAdd = new List<int>();

                foreach (var block in result.DiffBlocks)
                {
                    // Removed hitobject
                    for (int i = 0; i < block.DeleteCountA; i++)
                    {
                        int hoIndex = block.DeleteStartA + i - oldHitObjectsIndex - 1;

                        if (hoIndex < 0)
                            continue;

                        toRemove.Add(hoIndex);
                    }

                    // Added hitobject
                    for (int i = 0; i < block.InsertCountB; i++)
                    {
                        int hoIndex = block.InsertStartB + i - newHitObjectsIndex - 1;

                        if (hoIndex < 0)
                            continue;

                        toAdd.Add(hoIndex);
                    }
                }

                // Make the removal indices are sorted so that iteration order doesn't get messed up post-removal.
                toRemove.Sort();

                // Apply the changes.
                for (int i = toRemove.Count - 1; i >= 0; i--)
                    editorBeatmap.RemoveAt(toRemove[i]);

                if (toAdd.Count > 0)
                {
                    IBeatmap newBeatmap = readBeatmap(newState);
                    foreach (var i in toAdd)
                        editorBeatmap.Add(newBeatmap.HitObjects[i]);
                }
            }

            private string readString(Stream stream)
            {
                stream.Seek(0, SeekOrigin.Begin);

                using (var sr = new StreamReader(stream, Encoding.UTF8, true, 1024, true))
                    return sr.ReadToEnd();
            }

            private IBeatmap readBeatmap(Stream stream)
            {
                stream.Seek(0, SeekOrigin.Begin);

                using (var reader = new LineBufferedReader(stream, true))
                    return new PassThroughWorkingBeatmap(Decoder.GetDecoder<Beatmap>(reader).Decode(reader)).GetPlayableBeatmap(editorBeatmap.BeatmapInfo.Ruleset);
            }

            private class PassThroughWorkingBeatmap : WorkingBeatmap
            {
                private readonly IBeatmap beatmap;

                public PassThroughWorkingBeatmap(IBeatmap beatmap)
                    : base(beatmap.BeatmapInfo, null)
                {
                    this.beatmap = beatmap;
                }

                protected override IBeatmap GetBeatmap() => beatmap;

                protected override Texture GetBackground() => throw new NotImplementedException();

                protected override Track GetTrack() => throw new NotImplementedException();
            }
        }
    }
}
