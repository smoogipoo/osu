// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Text.Json.Serialization;
using MessagePack;

namespace osu.Game.Arcade
{
    [MessagePackObject]
    [Serializable]
    public class ArcadeIdentity
    {
        [JsonPropertyName("user")]
        [Key(0)]
        public ArcadeUser User { get; set; } = new ArcadeUser();

        [JsonPropertyName("stats")]
        [Key(1)]
        public ArcadeUserGlobalStats[] UserStats { get; set; } = [];

        [JsonPropertyName("matchmaking_user_stats")]
        [Key(2)]
        public ArcadeUserMatchmakingStats[] MatchmakingStats { get; set; } = [];
    }
}
