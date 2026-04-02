// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using MessagePack;

namespace osu.Game.Online.Matchmaking
{
    [MessagePackObject]
    [Serializable]
    public class MatchmakingMatchResult
    {
        /// <summary>
        /// The multiplayer room ID corresponding to the match.
        /// </summary>
        [Key(0)]
        public long RoomID { get; set; }

        [Key(1)]
        public List<MatchmakingPlayerScore> Scores { get; set; } = [];

        /// <summary>
        /// The winner of either <see cref="Player1"/> or <see cref="Player2"/>.
        /// </summary>
        [Key(2)]
        public int WinningUser { get; set; }
    }
}
