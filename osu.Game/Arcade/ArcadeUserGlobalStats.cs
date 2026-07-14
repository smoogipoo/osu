// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Text.Json.Serialization;
using MessagePack;

namespace osu.Game.Arcade
{
    [MessagePackObject]
    [Serializable]
    public class ArcadeUserGlobalStats
    {
        [JsonPropertyName("ruleset_id")]
        [Key(0)]
        public int RulesetId { get; set; }

        [JsonPropertyName("variant_id")]
        [Key(1)]
        public int VariantId { get; set; }

        [JsonPropertyName("pp")]
        [Key(2)]
        public double Pp { get; set; }
    }
}
