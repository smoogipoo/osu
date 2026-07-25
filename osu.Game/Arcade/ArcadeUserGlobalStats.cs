// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using MessagePack;
using Newtonsoft.Json;

namespace osu.Game.Arcade
{
    [MessagePackObject]
    [Serializable]
    public class ArcadeUserGlobalStats
    {
        [JsonProperty("ruleset_id")]
        [Key(0)]
        public int RulesetId { get; set; }

        [JsonProperty("variant_id")]
        [Key(1)]
        public int VariantId { get; set; }

        [JsonProperty("pp")]
        [Key(2)]
        public double Pp { get; set; }
    }
}
