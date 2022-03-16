// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable enable

using MessagePack;

namespace osu.Game.Online.Multiplayer.Countdown
{
    [MessagePackObject]
    public class CountdownChangedEvent : MatchServerEvent
    {
        [Key(0)]
        public MultiplayerCountdown? Countdown { get; set; }
    }
}
