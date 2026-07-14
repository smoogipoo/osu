// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Text.Json.Serialization;
using MessagePack;

namespace osu.Game.Arcade
{
    [MessagePackObject]
    [Serializable]
    public class ArcadeUserMatchmakingStats
    {
        [JsonPropertyName("pool_id")]
        [Key(0)]
        public int PoolId { get; set; }

        [JsonPropertyName("rating")]
        [Key(1)]
        public int Rating { get; set; }

        [JsonPropertyName("ruleset_id")]
        [Key(2)]
        public int RulesetId { get; set; }

        [JsonPropertyName("variant_id")]
        [Key(3)]
        public int VariantId { get; set; }
    }
}
