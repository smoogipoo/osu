// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using osu.Framework.Extensions;
using osu.Framework.Logging;
using osu.Game.Audio;
using osu.Game.Beatmaps.ControlPoints;
using osu.Game.IO;
using osuTK.Graphics;

namespace osu.Game.Beatmaps.Formats
{
    public abstract class LegacyDecoder<T> : Decoder<T>
        where T : new()
    {
        protected readonly int FormatVersion;

        protected LegacyDecoder(int version)
        {
            FormatVersion = version;
        }

        protected override void ParseStreamInto(LineBufferedReader stream, T output)
        {
            Section section = Section.None;

            string line;

            while ((line = stream.ReadLine()) != null)
            {
                ReadOnlySpan<char> lineSpan = line.AsSpan();

                if (ShouldSkipLine(lineSpan))
                    continue;

                if (lineSpan[0] == '[' && lineSpan[^1] == ']')
                {
                    if (!Enum.TryParse(lineSpan[1..^1].ToString(), out section))
                    {
                        Logger.Log($"Unknown section \"{line}\" in \"{output}\"");
                        section = Section.None;
                    }

                    OnBeginNewSection(section);
                    continue;
                }

                try
                {
                    ParseLine(output, section, line);
                }
                catch (Exception e)
                {
                    Logger.Log($"Failed to process line \"{line}\" into \"{output}\": {e.Message}", LoggingTarget.Runtime, LogLevel.Important);
                }
            }
        }

        protected virtual bool ShouldSkipLine(ReadOnlySpan<char> line) => line.IsEmpty || line.IsWhiteSpace() || line.TrimStart().StartsWith("//", StringComparison.Ordinal);

        /// <summary>
        /// Invoked when a new <see cref="Section"/> has been entered.
        /// </summary>
        /// <param name="section">The entered <see cref="Section"/>.</param>
        protected virtual void OnBeginNewSection(Section section)
        {
        }

        protected virtual void ParseLine(T output, Section section, ReadOnlySpan<char> line)
        {
            StripComments(ref line);

            switch (section)
            {
                case Section.Colours:
                    HandleColours(output, line);
                    return;
            }
        }

        protected void StripComments(ref ReadOnlySpan<char> line)
        {
            var index = line.IndexOf("//");
            if (index > 0)
                line = line.Slice(0, index);
        }

        protected void HandleColours<TModel>(TModel output, ReadOnlySpan<char> line)
        {
            SplitKeyVal(line, out var key, out var value);

            bool isCombo = key.StartsWith(@"Combo");

            int length = 1;

            for (int i = 0; i < line.Length; i++)
            {
                if (line[i] == ',')
                    length++;
            }

            if (length != 3 && length != 4)
                throw new InvalidOperationException($@"Color specified in incorrect format (should be R,G,B or R,G,B,A): {line.ToString()}");

            LegacyLineTokenizer tokenizer = new LegacyLineTokenizer(value);

            Color4 colour;

            try
            {
                colour = new Color4(
                    byte.Parse(tokenizer.Read()),
                    byte.Parse(tokenizer.Read()),
                    byte.Parse(tokenizer.Read()),
                    tokenizer.HasMore ? byte.Parse(tokenizer.Read()) : (byte)255);
            }
            catch
            {
                throw new InvalidOperationException(@"Color must be specified with 8-bit integer components");
            }

            if (isCombo)
            {
                if (!(output is IHasComboColours tHasComboColours)) return;

                tHasComboColours.AddComboColours(colour);
            }
            else
            {
                if (!(output is IHasCustomColours tHasCustomColours)) return;

                tHasCustomColours.CustomColours[key] = colour;
            }
        }

        protected KeyValuePair<string, string> SplitKeyVal(string line, char separator = ':')
        {
            var split = line.Split(separator, 2);

            return new KeyValuePair<string, string>
            (
                split[0].Trim(),
                split.Length > 1 ? split[1].Trim() : string.Empty
            );
        }

        protected void SplitKeyVal(in ReadOnlySpan<char> line, out string key, out ReadOnlySpan<char> value, char separator = ':')
        {
            LegacyLineTokenizer tokenizer = new LegacyLineTokenizer(line, separator);

            ReadOnlySpan<char> first = tokenizer.Read();
            ReadOnlySpan<char> second = tokenizer.ReadToEnd();

            key = first.Trim().ToString();
            value = second.Trim();
        }

        protected string CleanFilename(string path) => path.Trim('"').ToStandardisedPath();

        protected string CleanFilename(ReadOnlySpan<char> path) => path.Trim('"').ToString().ToStandardisedPath();

        protected enum Section
        {
            None,
            General,
            Editor,
            Metadata,
            Difficulty,
            Events,
            TimingPoints,
            Colours,
            HitObjects,
            Variables,
            Fonts,
            Mania
        }

        internal class LegacyDifficultyControlPoint : DifficultyControlPoint
        {
            public LegacyDifficultyControlPoint()
            {
                SpeedMultiplierBindable.Precision = double.Epsilon;
            }
        }

        internal class LegacySampleControlPoint : SampleControlPoint
        {
            public int CustomSampleBank;

            public override HitSampleInfo ApplyTo(HitSampleInfo hitSampleInfo)
            {
                var baseInfo = base.ApplyTo(hitSampleInfo);

                if (string.IsNullOrEmpty(baseInfo.Suffix) && CustomSampleBank > 1)
                    baseInfo.Suffix = CustomSampleBank.ToString();

                return baseInfo;
            }

            public override bool EquivalentTo(ControlPoint other) =>
                base.EquivalentTo(other) && other is LegacySampleControlPoint otherTyped &&
                CustomSampleBank == otherTyped.CustomSampleBank;
        }
    }
}
