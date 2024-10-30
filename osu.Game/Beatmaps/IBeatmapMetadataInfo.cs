// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Game.Users;

namespace osu.Game.Beatmaps
{
    /// <summary>
    /// Metadata representing a beatmap. May be shared between multiple beatmap difficulties.
    /// </summary>
    public interface IBeatmapMetadataInfo : IEquatable<IBeatmapMetadataInfo>
    {
        /// <summary>
        /// The romanised title of this beatmap.
        /// </summary>
        string Title { get; set; }

        /// <summary>
        /// The unicode title of this beatmap.
        /// </summary>
        string TitleUnicode { get; set; }

        /// <summary>
        /// The romanised artist of this beatmap.
        /// </summary>
        string Artist { get; set; }

        /// <summary>
        /// The unicode artist of this beatmap.
        /// </summary>
        string ArtistUnicode { get; set; }

        /// <summary>
        /// The author of this beatmap.
        /// </summary>
        IUser Author { get; set; }

        /// <summary>
        /// The source of this beatmap.
        /// </summary>
        string Source { get; set; }

        /// <summary>
        /// The tags of this beatmap.
        /// </summary>
        string Tags { get; set; }

        /// <summary>
        /// The time in milliseconds to begin playing the track for preview purposes.
        /// If -1, the track should begin playing at 40% of its length.
        /// </summary>
        int PreviewTime { get; set; }

        /// <summary>
        /// The filename of the audio file consumed by this beatmap.
        /// </summary>
        string AudioFile { get; set; }

        /// <summary>
        /// The filename of the background image file consumed by this beatmap.
        /// </summary>
        string BackgroundFile { get; set; }

        bool IEquatable<IBeatmapMetadataInfo>.Equals(IBeatmapMetadataInfo? other)
        {
            if (other == null)
                return false;

            return Title == other.Title
                   && TitleUnicode == other.TitleUnicode
                   && Artist == other.Artist
                   && ArtistUnicode == other.ArtistUnicode
                   && Author.Equals(other.Author)
                   && Source == other.Source
                   && Tags == other.Tags
                   && PreviewTime == other.PreviewTime
                   && AudioFile == other.AudioFile
                   && BackgroundFile == other.BackgroundFile;
        }
    }
}
