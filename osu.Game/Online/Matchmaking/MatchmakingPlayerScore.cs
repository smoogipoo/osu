// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using MessagePack;

namespace osu.Game.Online.Matchmaking
{
    [MessagePackObject]
    [Serializable]
    public class MatchmakingPlayerScore
    {
        [Key(0)]
        public int UserID { get; set; }

        [Key(1)]
        public int Life { get; set; }

        [Key(2)]
        public int Score { get; set; }
    }
}
