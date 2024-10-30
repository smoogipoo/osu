// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Game.Database;
using osu.Game.Rulesets;

namespace osu.Game.Beatmaps
{
    /// <summary>
    /// A single beatmap difficulty.
    /// </summary>
    public interface IBeatmapInfo : IHasOnlineID<int>, IEquatable<IBeatmapInfo>
    {
        /// <summary>
        /// The user-specified name given to this beatmap.
        /// </summary>
        string DifficultyName { get; set; }

        /// <summary>
        /// The metadata representing this beatmap. May be shared between multiple beatmaps.
        /// </summary>
        IBeatmapMetadataInfo Metadata { get; set; }

        /// <summary>
        /// The difficulty settings for this beatmap.
        /// </summary>
        IBeatmapDifficultyInfo Difficulty { get; set; }

        /// <summary>
        /// The beatmap set this beatmap is part of.
        /// </summary>
        IBeatmapSetInfo? BeatmapSet { get; set; }

        /// <summary>
        /// The total length in milliseconds of this beatmap.
        /// </summary>
        double Length { get; set; }

        /// <summary>
        /// The most common BPM of this beatmap.
        /// </summary>
        double BPM { get; set; }

        /// <summary>
        /// The SHA-256 hash representing this beatmap's contents.
        /// </summary>
        string Hash { get; set; }

        /// <summary>
        /// MD5 is kept for legacy support (matching against replays etc.).
        /// </summary>
        string MD5Hash { get; set; }

        /// <summary>
        /// The ruleset this beatmap was made for.
        /// </summary>
        IRulesetInfo Ruleset { get; set; }

        /// <summary>
        /// The basic star rating for this beatmap (with no mods applied).
        /// Defaults to -1 (meaning not-yet-calculated).
        /// </summary>
        double StarRating { get; set; }

        /// <summary>
        /// The number of hitobjects in the beatmap with a distinct end time.
        /// Defaults to -1 (meaning not-yet-calculated).
        /// </summary>
        /// <remarks>
        /// Canonically, these are hitobjects are either sliders or spinners.
        /// </remarks>
        int EndTimeObjectCount { get; set; }

        /// <summary>
        /// The total number of hitobjects in the beatmap.
        /// Defaults to -1 (meaning not-yet-calculated).
        /// </summary>
        int TotalObjectCount { get; set; }

        int[] Bookmarks { get; set; }

        int BeatmapVersion { get; set; }
    }
}
