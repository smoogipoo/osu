// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using osu.Framework.Extensions;
using osu.Game.Beatmaps.ControlPoints;
using osu.Game.Beatmaps.Legacy;
using osu.Game.Beatmaps.Timing;
using osu.Game.IO;
using osu.Game.Rulesets.Objects.Legacy;

namespace osu.Game.Beatmaps.Formats
{
    public class LegacyBeatmapDecoder : LegacyDecoder<Beatmap>
    {
        public const int LATEST_VERSION = 14;

        private Beatmap beatmap;

        private ConvertHitObjectParser parser;

        private LegacySampleBank defaultSampleBank;
        private int defaultSampleVolume = 100;

        public static void Register()
        {
            AddDecoder<Beatmap>(@"osu file format v", m => new LegacyBeatmapDecoder(Parsing.ParseInt(m.Split('v').Last())));
            SetFallbackDecoder<Beatmap>(() => new LegacyBeatmapDecoder());
        }

        /// <summary>
        /// Whether or not beatmap or runtime offsets should be applied. Defaults on; only disable for testing purposes.
        /// </summary>
        public bool ApplyOffsets = true;

        private readonly int offset;

        public LegacyBeatmapDecoder(int version = LATEST_VERSION)
            : base(version)
        {
            // BeatmapVersion 4 and lower had an incorrect offset (stable has this set as 24ms off)
            offset = FormatVersion < 5 ? 24 : 0;
        }

        protected override void ParseStreamInto(LineBufferedReader stream, Beatmap beatmap)
        {
            this.beatmap = beatmap;
            this.beatmap.BeatmapInfo.BeatmapVersion = FormatVersion;

            base.ParseStreamInto(stream, beatmap);

            flushPendingPoints();

            // Objects may be out of order *only* if a user has manually edited an .osu file.
            // Unfortunately there are ranked maps in this state (example: https://osu.ppy.sh/s/594828).
            // OrderBy is used to guarantee that the parsing order of hitobjects with equal start times is maintained (stably-sorted)
            // The parsing order of hitobjects matters in mania difficulty calculation
            this.beatmap.HitObjects = this.beatmap.HitObjects.OrderBy(h => h.StartTime).ToList();

            foreach (var hitObject in this.beatmap.HitObjects)
                hitObject.ApplyDefaults(this.beatmap.ControlPointInfo, this.beatmap.BeatmapInfo.BaseDifficulty);
        }

        protected override bool ShouldSkipLine(string line) => base.ShouldSkipLine(line) || line.StartsWith(' ') || line.StartsWith('_');

        protected override void ParseLine(Beatmap beatmap, Section section, string line)
        {
            switch (section)
            {
                case Section.General:
                    handleGeneral(line);
                    return;

                case Section.Editor:
                    handleEditor(line);
                    return;

                case Section.Metadata:
                    handleMetadata(line);
                    return;

                case Section.Difficulty:
                    handleDifficulty(line);
                    return;

                case Section.Events:
                    handleEvent(line);
                    return;

                case Section.TimingPoints:
                    handleTimingPoint(line);
                    return;

                case Section.HitObjects:
                    handleHitObject(line);
                    return;
            }

            base.ParseLine(beatmap, section, line);
        }

        private void handleGeneral(ReadOnlySpan<char> line)
        {
            SplitKeyVal(line, out var key, out var value);

            var metadata = beatmap.BeatmapInfo.Metadata;

            switch (key)
            {
                case @"AudioFilename":
                    metadata.AudioFile = value.ToString().ToStandardisedPath();
                    break;

                case @"AudioLeadIn":
                    beatmap.BeatmapInfo.AudioLeadIn = Parsing.ParseInt(value);
                    break;

                case @"PreviewTime":
                    metadata.PreviewTime = getOffsetTime(Parsing.ParseInt(value));
                    break;

                case @"Countdown":
                    beatmap.BeatmapInfo.Countdown = Parsing.ParseInt(value) == 1;
                    break;

                case @"SampleSet":
                    defaultSampleBank = (LegacySampleBank)Enum.Parse(typeof(LegacySampleBank), value.ToString());
                    break;

                case @"SampleVolume":
                    defaultSampleVolume = Parsing.ParseInt(value);
                    break;

                case @"StackLeniency":
                    beatmap.BeatmapInfo.StackLeniency = Parsing.ParseFloat(value);
                    break;

                case @"Mode":
                    beatmap.BeatmapInfo.RulesetID = Parsing.ParseInt(value);

                    switch (beatmap.BeatmapInfo.RulesetID)
                    {
                        case 0:
                            parser = new Rulesets.Objects.Legacy.Osu.ConvertHitObjectParser(getOffsetTime(), FormatVersion);
                            break;

                        case 1:
                            parser = new Rulesets.Objects.Legacy.Taiko.ConvertHitObjectParser(getOffsetTime(), FormatVersion);
                            break;

                        case 2:
                            parser = new Rulesets.Objects.Legacy.Catch.ConvertHitObjectParser(getOffsetTime(), FormatVersion);
                            break;

                        case 3:
                            parser = new Rulesets.Objects.Legacy.Mania.ConvertHitObjectParser(getOffsetTime(), FormatVersion);
                            break;
                    }

                    break;

                case @"LetterboxInBreaks":
                    beatmap.BeatmapInfo.LetterboxInBreaks = Parsing.ParseInt(value) == 1;
                    break;

                case @"SpecialStyle":
                    beatmap.BeatmapInfo.SpecialStyle = Parsing.ParseInt(value) == 1;
                    break;

                case @"WidescreenStoryboard":
                    beatmap.BeatmapInfo.WidescreenStoryboard = Parsing.ParseInt(value) == 1;
                    break;
            }
        }

        private void handleEditor(ReadOnlySpan<char> line)
        {
            SplitKeyVal(line, out var key, out var value);

            switch (key)
            {
                case @"Bookmarks":
                    beatmap.BeatmapInfo.StoredBookmarks = value.ToString();
                    break;

                case @"DistanceSpacing":
                    beatmap.BeatmapInfo.DistanceSpacing = Math.Max(0, Parsing.ParseDouble(value));
                    break;

                case @"BeatDivisor":
                    beatmap.BeatmapInfo.BeatDivisor = Parsing.ParseInt(value);
                    break;

                case @"GridSize":
                    beatmap.BeatmapInfo.GridSize = Parsing.ParseInt(value);
                    break;

                case @"TimelineZoom":
                    beatmap.BeatmapInfo.TimelineZoom = Math.Max(0, Parsing.ParseDouble(value));
                    break;
            }
        }

        private void handleMetadata(ReadOnlySpan<char> line)
        {
            SplitKeyVal(line, out var key, out var value);

            var metadata = beatmap.BeatmapInfo.Metadata;

            switch (key)
            {
                case @"Title":
                    metadata.Title = value.ToString();
                    break;

                case @"TitleUnicode":
                    metadata.TitleUnicode = value.ToString();
                    break;

                case @"Artist":
                    metadata.Artist = value.ToString();
                    break;

                case @"ArtistUnicode":
                    metadata.ArtistUnicode = value.ToString();
                    break;

                case @"Creator":
                    metadata.AuthorString = value.ToString();
                    break;

                case @"Version":
                    beatmap.BeatmapInfo.Version = value.ToString();
                    break;

                case @"Source":
                    metadata.Source = value.ToString();
                    break;

                case @"Tags":
                    metadata.Tags = value.ToString();
                    break;

                case @"BeatmapID":
                    beatmap.BeatmapInfo.OnlineBeatmapID = Parsing.ParseInt(value);
                    break;

                case @"BeatmapSetID":
                    beatmap.BeatmapInfo.BeatmapSet = new BeatmapSetInfo { OnlineBeatmapSetID = Parsing.ParseInt(value) };
                    break;
            }
        }

        private void handleDifficulty(ReadOnlySpan<char> line)
        {
            SplitKeyVal(line, out var key, out var value);

            var difficulty = beatmap.BeatmapInfo.BaseDifficulty;

            switch (key)
            {
                case @"HPDrainRate":
                    difficulty.DrainRate = Parsing.ParseFloat(value);
                    break;

                case @"CircleSize":
                    difficulty.CircleSize = Parsing.ParseFloat(value);
                    break;

                case @"OverallDifficulty":
                    difficulty.OverallDifficulty = Parsing.ParseFloat(value);
                    break;

                case @"ApproachRate":
                    difficulty.ApproachRate = Parsing.ParseFloat(value);
                    break;

                case @"SliderMultiplier":
                    difficulty.SliderMultiplier = Parsing.ParseDouble(value);
                    break;

                case @"SliderTickRate":
                    difficulty.SliderTickRate = Parsing.ParseDouble(value);
                    break;
            }
        }

        private void handleEvent(ReadOnlySpan<char> line)
        {
            LegacyLineTokenizer tokenizer = new LegacyLineTokenizer(line);

            string strType = tokenizer.Read().ToString();

            if (!Enum.TryParse(strType, out LegacyEventType type))
                throw new InvalidDataException($@"Unknown event type: {strType}");

            switch (type)
            {
                case LegacyEventType.Background:
                    tokenizer.Read(); // Ignore the element at index 1
                    beatmap.BeatmapInfo.Metadata.BackgroundFile = CleanFilename(tokenizer.Read());
                    break;

                case LegacyEventType.Break:
                    double start = getOffsetTime(Parsing.ParseDouble(tokenizer.Read()));
                    double end = Math.Max(start, getOffsetTime(Parsing.ParseDouble(tokenizer.Read())));

                    var breakEvent = new BreakPeriod(start, end);

                    if (!breakEvent.HasEffect)
                        return;

                    beatmap.Breaks.Add(breakEvent);
                    break;
            }
        }

        private void handleTimingPoint(ReadOnlySpan<char> line)
        {
            LegacyLineTokenizer tokenizer = new LegacyLineTokenizer(line);

            double time = getOffsetTime(Parsing.ParseDouble(tokenizer.Read().Trim()));
            double beatLength = Parsing.ParseDouble(tokenizer.Read().Trim());
            double speedMultiplier = beatLength < 0 ? 100.0 / -beatLength : 1;

            TimeSignatures timeSignature = TimeSignatures.SimpleQuadruple;

            if (tokenizer.HasMore)
            {
                var span = tokenizer.Read();
                timeSignature = span[0] == '0' ? TimeSignatures.SimpleQuadruple : (TimeSignatures)Parsing.ParseInt(span);
            }

            LegacySampleBank sampleSet = defaultSampleBank;
            if (tokenizer.HasMore)
                sampleSet = (LegacySampleBank)Parsing.ParseInt(tokenizer.Read());

            int customSampleBank = 0;
            if (tokenizer.HasMore)
                customSampleBank = Parsing.ParseInt(tokenizer.Read());

            int sampleVolume = defaultSampleVolume;
            if (tokenizer.HasMore)
                sampleVolume = Parsing.ParseInt(tokenizer.Read());

            bool timingChange = true;
            if (tokenizer.HasMore)
                timingChange = tokenizer.Read()[0] == '1';

            bool kiaiMode = false;
            bool omitFirstBarSignature = false;

            if (tokenizer.HasMore)
            {
                LegacyEffectFlags effectFlags = (LegacyEffectFlags)Parsing.ParseInt(tokenizer.Read());
                kiaiMode = effectFlags.HasFlag(LegacyEffectFlags.Kiai);
                omitFirstBarSignature = effectFlags.HasFlag(LegacyEffectFlags.OmitFirstBarLine);
            }

            string stringSampleSet = sampleSet.ToString().ToLowerInvariant();
            if (stringSampleSet == @"none")
                stringSampleSet = @"normal";

            if (timingChange)
            {
                var controlPoint = CreateTimingControlPoint();

                controlPoint.BeatLength = beatLength;
                controlPoint.TimeSignature = timeSignature;

                addControlPoint(time, controlPoint, true);
            }

            addControlPoint(time, new LegacyDifficultyControlPoint
            {
                SpeedMultiplier = speedMultiplier,
            }, timingChange);

            addControlPoint(time, new EffectControlPoint
            {
                KiaiMode = kiaiMode,
                OmitFirstBarLine = omitFirstBarSignature,
            }, timingChange);

            addControlPoint(time, new LegacySampleControlPoint
            {
                SampleBank = stringSampleSet,
                SampleVolume = sampleVolume,
                CustomSampleBank = customSampleBank,
            }, timingChange);

            // To handle the scenario where a non-timing line shares the same time value as a subsequent timing line but
            // appears earlier in the file, we buffer non-timing control points and rewrite them *after* control points from the timing line
            // with the same time value (allowing them to overwrite as necessary).
            //
            // The expected outcome is that we prefer the non-timing line's adjustments over the timing line's adjustments when time is equal.
            if (timingChange)
                flushPendingPoints();
        }

        private readonly List<ControlPoint> pendingControlPoints = new List<ControlPoint>();
        private double pendingControlPointsTime;

        private void addControlPoint(double time, ControlPoint point, bool timingChange)
        {
            if (time != pendingControlPointsTime)
                flushPendingPoints();

            if (timingChange)
            {
                beatmap.ControlPointInfo.Add(time, point);
                return;
            }

            pendingControlPoints.Add(point);
            pendingControlPointsTime = time;
        }

        private void flushPendingPoints()
        {
            foreach (var p in pendingControlPoints)
                beatmap.ControlPointInfo.Add(pendingControlPointsTime, p);

            pendingControlPoints.Clear();
        }

        private void handleHitObject(ReadOnlySpan<char> line)
        {
            // If the ruleset wasn't specified, assume the osu!standard ruleset.
            if (parser == null)
                parser = new Rulesets.Objects.Legacy.Osu.ConvertHitObjectParser(getOffsetTime(), FormatVersion);

            var obj = parser.Parse(line);
            if (obj != null)
                beatmap.HitObjects.Add(obj);
        }

        private int getOffsetTime(int time) => time + (ApplyOffsets ? offset : 0);

        private double getOffsetTime() => ApplyOffsets ? offset : 0;

        private double getOffsetTime(double time) => time + (ApplyOffsets ? offset : 0);

        protected virtual TimingControlPoint CreateTimingControlPoint() => new TimingControlPoint();
    }
}
