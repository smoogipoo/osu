// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable enable

using System;
using MessagePack;

namespace osu.Game.Online.Multiplayer
{
    [MessagePackObject]
    [Union(0, typeof(MatchStartCountdown))] // IMPORTANT: Add rules to SignalRUnionWorkaroundResolver for new derived types.
    public abstract class MultiplayerCountdown
    {
        [Key(0)]
        public DateTimeOffset EndTime { get; set; }
    }
}
