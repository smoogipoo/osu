// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using MessagePack;

namespace osu.Game.Online.Multiplayer.MatchTypes.RankedPlay
{
    [Serializable]
    [MessagePackObject]
    public class RankedPlayUserInfo
    {
        /// <summary>
        /// The current life points.
        /// </summary>
        [Key(0)]
        public int Life { get; set; } = 1_000_000;

        /// <summary>
        /// The cards in this user's hand.
        /// </summary>
        [Key(1)]
        public List<RankedPlayCardItem> Hand { get; set; } = [];
    }
}
