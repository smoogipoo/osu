// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using MessagePack;
using Newtonsoft.Json;

namespace osu.Game.Arcade
{
    [MessagePackObject]
    [Serializable]
    public class ArcadeIdentity
    {
        [JsonProperty("user")]
        [Key(0)]
        public ArcadeUser User { get; set; } = new ArcadeUser();

        [JsonProperty("stats")]
        [Key(1)]
        public ArcadeUserGlobalStats[] UserStats { get; set; } = [];

        [JsonProperty("matchmaking_user_stats")]
        [Key(2)]
        public ArcadeUserMatchmakingStats[] MatchmakingStats { get; set; } = [];
    }
}
