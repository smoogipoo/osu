// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using MessagePack;
using Newtonsoft.Json;

namespace osu.Game.Arcade
{
    [MessagePackObject]
    [Serializable]
    public class ArcadeUserMatchmakingStats
    {
        [JsonProperty("pool_id")]
        [Key(0)]
        public int PoolId { get; set; }

        [JsonProperty("rating")]
        [Key(1)]
        public int Rating { get; set; }

        [JsonProperty("ruleset_id")]
        [Key(2)]
        public int RulesetId { get; set; }

        [JsonProperty("variant_id")]
        [Key(3)]
        public int VariantId { get; set; }
    }
}
