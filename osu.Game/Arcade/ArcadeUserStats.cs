// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using MessagePack;

namespace osu.Game.Arcade
{
    [Serializable]
    [MessagePackObject]
    public class ArcadeUserStats
    {
        [Key(0)]
        public int UserId { get; set; }

        [Key(1)]
        public string Username { get; set; } = string.Empty;

        [Key(2)]
        public int Victories { get; set; }
    }
}
