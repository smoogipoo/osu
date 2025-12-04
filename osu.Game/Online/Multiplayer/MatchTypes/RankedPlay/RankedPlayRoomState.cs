// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using MessagePack;

namespace osu.Game.Online.Multiplayer.MatchTypes.RankedPlay
{
    [Serializable]
    [MessagePackObject]
    public class RankedPlayRoomState : MatchRoomState
    {
        /// <summary>
        /// The current room stage.
        /// </summary>
        [Key(0)]
        public RankedPlayStage Stage { get; set; }

        /// <summary>
        /// The current round number (1-based).
        /// </summary>
        [Key(1)]
        public int CurrentRound { get; set; }

        /// <summary>
        /// A multiplier applied to life point damage.
        /// </summary>
        [Key(2)]
        public double DamageMultiplier { get; set; }

        /// <summary>
        /// The index of the user currently playing a card.
        /// </summary>
        [Key(3)]
        public int ActivePlayerIndex { get; set; }
    }
}
