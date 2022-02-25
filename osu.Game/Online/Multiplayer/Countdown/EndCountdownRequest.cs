// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using MessagePack;

namespace osu.Game.Online.Multiplayer.Countdown
{
    /// <summary>
    /// Request to end the current countdown.
    /// </summary>
    [MessagePackObject]
    public class EndCountdownRequest : MatchUserRequest
    {
    }
}
