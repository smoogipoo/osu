// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Text.Json.Serialization;
using MessagePack;

namespace osu.Game.Arcade
{
    [MessagePackObject]
    [Serializable]
    public class ArcadeUser
    {
        [JsonPropertyName("id")]
        [Key(0)]
        public int UserId { get; set; }

        [JsonPropertyName("username")]
        [Key(1)]
        public string Username { get; set; } = string.Empty;

        [JsonPropertyName("avatar_url")]
        [Key(2)]
        public string AvatarUrl { get; set; } = string.Empty;

        [JsonPropertyName("cover_url")]
        [Key(3)]
        public string CoverUrl { get; set; } = string.Empty;
    }
}
