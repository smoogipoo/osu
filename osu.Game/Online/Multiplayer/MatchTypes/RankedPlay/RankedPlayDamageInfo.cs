// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using MessagePack;

namespace osu.Game.Online.Multiplayer.MatchTypes.RankedPlay
{
    [Serializable]
    [MessagePackObject]
    public class RankedPlayDamageInfo
    {
        /// <summary>
        /// Total amount of damage dealt.
        /// </summary>
        [Key(0)]
        public int Damage { get; init; }

        /// <summary>
        /// Damage dealt before multipliers are applied.
        /// </summary>
        [Key(1)]
        public int RawDamage { get; init; }

        /// <summary>
        /// Life before damage was applied.
        /// </summary>
        [Key(2)]
        public int OldLife { get; init; }

        /// <summary>
        /// Life after damage was applied.
        /// </summary>
        [Key(3)]
        public int NewLife { get; init; }

        /// <summary>
        /// Describes each source of damage.
        /// </summary>
        [Key(4)]
        public RankedPlayDamageBreakdown[] Breakdown { get; init; } = [];
    }

    [Serializable]
    [MessagePackObject]
    public class RankedPlayDamageBreakdown
    {
        /// <summary>
        /// The damage source.
        /// </summary>
        [Key(0)]
        public RankedPlayDamageSource Source { get; init; }

        /// <summary>
        /// Total amount of damage dealt from this source.
        /// </summary>
        [Key(1)]
        public int Damage { get; init; }

        /// <summary>
        /// Damage dealt before multipliers are applied.
        /// </summary>
        [Key(2)]
        public int RawDamage { get; init; }
    }

    public enum RankedPlayDamageSource
    {
        /// <summary>
        /// Base damage dealt for losing a round.
        /// </summary>
        Base,

        /// <summary>
        /// Attack damage dealt based on score difference.
        /// </summary>
        Attack
    }
}
