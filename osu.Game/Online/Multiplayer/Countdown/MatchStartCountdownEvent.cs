// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using MessagePack;

namespace osu.Game.Online.Multiplayer.Countdown
{
    /// <summary>
    /// An event indicating the start of a countdown to begin a match.
    /// </summary>
    public class MatchStartCountdownEvent : MatchServerEvent
    {
        [Key(0)]
        public DateTimeOffset EndTime { get; set; }
    }
}
