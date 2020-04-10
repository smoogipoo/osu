// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osuTK;
using osu.Game.Rulesets.Objects.Types;
using System;
using System.Collections.Generic;
using System.IO;
using osu.Game.Beatmaps.Formats;
using osu.Game.Audio;
using JetBrains.Annotations;
using osu.Framework.Utils;
using osu.Game.Beatmaps.Legacy;

namespace osu.Game.Rulesets.Objects.Legacy
{
    /// <summary>
    /// A HitObjectParser to parse legacy Beatmaps.
    /// </summary>
    public abstract class ConvertHitObjectParser : HitObjectParser
    {
        /// <summary>
        /// The offset to apply to all time values.
        /// </summary>
        protected readonly double Offset;

        /// <summary>
        /// The beatmap version.
        /// </summary>
        protected readonly int FormatVersion;

        protected bool FirstObject { get; private set; } = true;

        protected ConvertHitObjectParser(double offset, int formatVersion)
        {
            Offset = offset;
            FormatVersion = formatVersion;
        }

        [CanBeNull]
        public override HitObject Parse(ReadOnlySpan<char> line)
        {
            LegacyLineTokenizer lineTokenizer = new LegacyLineTokenizer(line);

            // [0], [1]
            Vector2 pos = new Vector2(
                (int)Parsing.ParseFloat(lineTokenizer.Read(), Parsing.MAX_COORDINATE_VALUE),
                (int)Parsing.ParseFloat(lineTokenizer.Read(), Parsing.MAX_COORDINATE_VALUE));

            // [2]
            double startTime = Parsing.ParseDouble(lineTokenizer.Read()) + Offset;

            // [3]
            LegacyHitObjectType type = (LegacyHitObjectType)Parsing.ParseInt(lineTokenizer.Read());

            int comboOffset = (int)(type & LegacyHitObjectType.ComboOffset) >> 4;
            type &= ~LegacyHitObjectType.ComboOffset;

            bool combo = type.HasFlag(LegacyHitObjectType.NewCombo);
            type &= ~LegacyHitObjectType.NewCombo;

            // [4]
            var soundType = (LegacyHitSoundType)Parsing.ParseInt(lineTokenizer.Read());
            var bankInfo = new SampleBankInfo();

            HitObject result = null;

            if (type.HasFlag(LegacyHitObjectType.Circle))
            {
                result = CreateHit(pos, combo, comboOffset);

                // [5]
                if (lineTokenizer.HasMore)
                    readCustomSampleBanks(lineTokenizer.Read(), bankInfo);
            }
            else if (type.HasFlag(LegacyHitObjectType.Slider))
            {
                var points = new List<Vector2> { Vector2.Zero };

                PathType pathType = PathType.Catmull;
                double? length = null;

                // [5]
                LegacyLineTokenizer pointTokenizer = new LegacyLineTokenizer(lineTokenizer.Read(), '|');

                while (pointTokenizer.HasMore)
                {
                    ReadOnlySpan<char> pt = pointTokenizer.Read();

                    if (pt.Length == 1)
                    {
                        switch (pt[0])
                        {
                            case 'C':
                                pathType = PathType.Catmull;
                                break;

                            case 'B':
                                pathType = PathType.Bezier;
                                break;

                            case 'L':
                                pathType = PathType.Linear;
                                break;

                            case 'P':
                                pathType = PathType.PerfectCurve;
                                break;
                        }

                        continue;
                    }

                    LegacyLineTokenizer coordinateTokenizer = new LegacyLineTokenizer(pt, ':');
                    points.Add(new Vector2(
                        (int)Parsing.ParseDouble(coordinateTokenizer.Read(), Parsing.MAX_COORDINATE_VALUE),
                        (int)Parsing.ParseDouble(coordinateTokenizer.Read(), Parsing.MAX_COORDINATE_VALUE)) - pos);
                }

                // [6]
                int repeatCount = Parsing.ParseInt(lineTokenizer.Read());

                if (repeatCount > 9000)
                    throw new FormatException(@"Repeat count is way too high");

                // osu-stable treated the first span of the slider as a repeat, but no repeats are happening
                repeatCount = Math.Max(0, repeatCount - 1);

                // [7]
                if (lineTokenizer.HasMore)
                {
                    length = Math.Max(0, Parsing.ParseDouble(lineTokenizer.Read(), Parsing.MAX_COORDINATE_VALUE));
                    if (length == 0)
                        length = null;
                }

                // One node for each repeat + the start and end nodes
                int nodes = repeatCount + 2;

                // Populate node sound types with the default hit object sound type
                var nodeSoundTypes = new List<LegacyHitSoundType>();
                for (int i = 0; i < nodes; i++)
                    nodeSoundTypes.Add(soundType);

                // Read any per-node sound types
                // [8]
                if (lineTokenizer.HasMore)
                {
                    LegacyLineTokenizer addsTokenizer = new LegacyLineTokenizer(lineTokenizer.Read(), '|');

                    if (addsTokenizer.HasMore)
                    {
                        for (int i = 0; i < nodes; i++)
                        {
                            if (!addsTokenizer.HasMore)
                                break;

                            int.TryParse(addsTokenizer.Read(), out var sound);
                            nodeSoundTypes[i] = (LegacyHitSoundType)sound;
                        }
                    }
                }

                // Populate node sample bank infos with the default hit object sample bank
                var nodeBankInfos = new List<SampleBankInfo>();
                for (int i = 0; i < nodes; i++)
                    nodeBankInfos.Add(bankInfo.Clone());

                // Read any per-node sample banks
                // [9]
                if (lineTokenizer.HasMore)
                {
                    LegacyLineTokenizer setsTokenizer = new LegacyLineTokenizer(lineTokenizer.Read(), '|');

                    if (setsTokenizer.HasMore)
                    {
                        for (int i = 0; i < nodes; i++)
                        {
                            if (!setsTokenizer.HasMore)
                                break;

                            SampleBankInfo info = nodeBankInfos[i];
                            readCustomSampleBanks(setsTokenizer.Read(), info);
                        }
                    }
                }

                // [10]
                if (lineTokenizer.HasMore)
                    readCustomSampleBanks(lineTokenizer.Read(), bankInfo);

                // Generate the final per-node samples
                var nodeSamples = new List<IList<HitSampleInfo>>(nodes);
                for (int i = 0; i < nodes; i++)
                    nodeSamples.Add(convertSoundType(nodeSoundTypes[i], nodeBankInfos[i]));

                result = CreateSlider(pos, combo, comboOffset, convertControlPoints(points, pathType), length, repeatCount, nodeSamples);

                // The samples are played when the slider ends, which is the last node
                result.Samples = nodeSamples[^1];
            }
            else if (type.HasFlag(LegacyHitObjectType.Spinner))
            {
                // [5]
                double endTime = Math.Max(startTime, Parsing.ParseDouble(lineTokenizer.Read()) + Offset);

                result = CreateSpinner(new Vector2(512, 384) / 2, combo, comboOffset, endTime);

                // [6]
                if (lineTokenizer.HasMore)
                    readCustomSampleBanks(lineTokenizer.Read(), bankInfo);
            }
            else if (type.HasFlag(LegacyHitObjectType.Hold))
            {
                // Note: Hold is generated by BMS converts

                double endTime = startTime;

                // [5]
                if (lineTokenizer.HasMore)
                {
                    LegacyLineTokenizer holdTokenizer = new LegacyLineTokenizer(lineTokenizer.Read(), ':');

                    if (holdTokenizer.HasMore)
                    {
                        endTime = Math.Max(startTime, Parsing.ParseDouble(holdTokenizer.Read()));
                        readCustomSampleBanks(holdTokenizer.ReadToEnd(), bankInfo);
                    }
                }

                result = CreateHold(pos, combo, comboOffset, endTime + Offset);
            }

            if (result == null)
                throw new InvalidDataException($"Unknown hit object type: {type}");

            result.StartTime = startTime;

            if (result.Samples.Count == 0)
                result.Samples = convertSoundType(soundType, bankInfo);

            FirstObject = false;

            return result;
        }

        private void readCustomSampleBanks(ReadOnlySpan<char> line, SampleBankInfo bankInfo)
        {
            LegacyLineTokenizer tokenizer = new LegacyLineTokenizer(line, ':');

            if (!tokenizer.HasMore)
                return;

            // [0]
            var bank = (LegacySampleBank)Parsing.ParseInt(tokenizer.Read());

            // [1]
            var addbank = (LegacySampleBank)Parsing.ParseInt(tokenizer.Read());

            string stringBank = bank.ToString().ToLowerInvariant();
            if (stringBank == @"none")
                stringBank = null;
            string stringAddBank = addbank.ToString().ToLowerInvariant();
            if (stringAddBank == @"none")
                stringAddBank = null;

            bankInfo.Normal = stringBank;
            bankInfo.Add = string.IsNullOrEmpty(stringAddBank) ? stringBank : stringAddBank;

            // [2]
            if (tokenizer.HasMore)
                bankInfo.CustomSampleBank = Parsing.ParseInt(tokenizer.Read());

            // [3]
            if (tokenizer.HasMore)
                bankInfo.Volume = Math.Max(0, Parsing.ParseInt(tokenizer.Read()));

            // [4]
            bankInfo.Filename = tokenizer.HasMore ? tokenizer.Read().ToString() : null;
        }

        private PathControlPoint[] convertControlPoints(List<Vector2> vertices, PathType type)
        {
            if (type == PathType.PerfectCurve)
            {
                if (vertices.Count != 3)
                    type = PathType.Bezier;
                else if (isLinear(vertices))
                {
                    // osu-stable special-cased colinear perfect curves to a linear path
                    type = PathType.Linear;
                }
            }

            var points = new List<PathControlPoint>(vertices.Count)
            {
                new PathControlPoint
                {
                    Position = { Value = vertices[0] },
                    Type = { Value = type }
                }
            };

            for (int i = 1; i < vertices.Count; i++)
            {
                if (vertices[i] == vertices[i - 1])
                {
                    points[^1].Type.Value = type;
                    continue;
                }

                points.Add(new PathControlPoint { Position = { Value = vertices[i] } });
            }

            return points.ToArray();

            static bool isLinear(List<Vector2> p) => Precision.AlmostEquals(0, (p[1].Y - p[0].Y) * (p[2].X - p[0].X) - (p[1].X - p[0].X) * (p[2].Y - p[0].Y));
        }

        /// <summary>
        /// Creates a legacy Hit-type hit object.
        /// </summary>
        /// <param name="position">The position of the hit object.</param>
        /// <param name="newCombo">Whether the hit object creates a new combo.</param>
        /// <param name="comboOffset">When starting a new combo, the offset of the new combo relative to the current one.</param>
        /// <returns>The hit object.</returns>
        protected abstract HitObject CreateHit(Vector2 position, bool newCombo, int comboOffset);

        /// <summary>
        /// Creats a legacy Slider-type hit object.
        /// </summary>
        /// <param name="position">The position of the hit object.</param>
        /// <param name="newCombo">Whether the hit object creates a new combo.</param>
        /// <param name="comboOffset">When starting a new combo, the offset of the new combo relative to the current one.</param>
        /// <param name="controlPoints">The slider control points.</param>
        /// <param name="length">The slider length.</param>
        /// <param name="repeatCount">The slider repeat count.</param>
        /// <param name="nodeSamples">The samples to be played when the slider nodes are hit. This includes the head and tail of the slider.</param>
        /// <returns>The hit object.</returns>
        protected abstract HitObject CreateSlider(Vector2 position, bool newCombo, int comboOffset, PathControlPoint[] controlPoints, double? length, int repeatCount,
                                                  List<IList<HitSampleInfo>> nodeSamples);

        /// <summary>
        /// Creates a legacy Spinner-type hit object.
        /// </summary>
        /// <param name="position">The position of the hit object.</param>
        /// <param name="newCombo">Whether the hit object creates a new combo.</param>
        /// <param name="comboOffset">When starting a new combo, the offset of the new combo relative to the current one.</param>
        /// <param name="endTime">The spinner end time.</param>
        /// <returns>The hit object.</returns>
        protected abstract HitObject CreateSpinner(Vector2 position, bool newCombo, int comboOffset, double endTime);

        /// <summary>
        /// Creates a legacy Hold-type hit object.
        /// </summary>
        /// <param name="position">The position of the hit object.</param>
        /// <param name="newCombo">Whether the hit object creates a new combo.</param>
        /// <param name="comboOffset">When starting a new combo, the offset of the new combo relative to the current one.</param>
        /// <param name="endTime">The hold end time.</param>
        protected abstract HitObject CreateHold(Vector2 position, bool newCombo, int comboOffset, double endTime);

        private List<HitSampleInfo> convertSoundType(LegacyHitSoundType type, SampleBankInfo bankInfo)
        {
            // Todo: This should return the normal SampleInfos if the specified sample file isn't found, but that's a pretty edge-case scenario
            if (!string.IsNullOrEmpty(bankInfo.Filename))
            {
                return new List<HitSampleInfo>
                {
                    new FileHitSampleInfo
                    {
                        Filename = bankInfo.Filename,
                        Volume = bankInfo.Volume
                    }
                };
            }

            var soundTypes = new List<HitSampleInfo>
            {
                new LegacyHitSampleInfo
                {
                    Bank = bankInfo.Normal,
                    Name = HitSampleInfo.HIT_NORMAL,
                    Volume = bankInfo.Volume,
                    CustomSampleBank = bankInfo.CustomSampleBank
                }
            };

            if (type.HasFlag(LegacyHitSoundType.Finish))
            {
                soundTypes.Add(new LegacyHitSampleInfo
                {
                    Bank = bankInfo.Add,
                    Name = HitSampleInfo.HIT_FINISH,
                    Volume = bankInfo.Volume,
                    CustomSampleBank = bankInfo.CustomSampleBank
                });
            }

            if (type.HasFlag(LegacyHitSoundType.Whistle))
            {
                soundTypes.Add(new LegacyHitSampleInfo
                {
                    Bank = bankInfo.Add,
                    Name = HitSampleInfo.HIT_WHISTLE,
                    Volume = bankInfo.Volume,
                    CustomSampleBank = bankInfo.CustomSampleBank
                });
            }

            if (type.HasFlag(LegacyHitSoundType.Clap))
            {
                soundTypes.Add(new LegacyHitSampleInfo
                {
                    Bank = bankInfo.Add,
                    Name = HitSampleInfo.HIT_CLAP,
                    Volume = bankInfo.Volume,
                    CustomSampleBank = bankInfo.CustomSampleBank
                });
            }

            return soundTypes;
        }

        private class SampleBankInfo
        {
            public string Filename;

            public string Normal;
            public string Add;
            public int Volume;

            public int CustomSampleBank;

            public SampleBankInfo Clone() => (SampleBankInfo)MemberwiseClone();
        }

        private class LegacyHitSampleInfo : HitSampleInfo
        {
            public int CustomSampleBank
            {
                set
                {
                    if (value > 1)
                        Suffix = value.ToString();
                }
            }
        }

        private class FileHitSampleInfo : HitSampleInfo
        {
            public string Filename;

            public override IEnumerable<string> LookupNames => new[]
            {
                Filename,
                Path.ChangeExtension(Filename, null)
            };
        }
    }
}
