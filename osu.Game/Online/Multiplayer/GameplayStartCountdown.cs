// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using MessagePack;

namespace osu.Game.Online.Multiplayer
{
    /// <summary>
    /// A <see cref="MultiplayerCountdown"/> which will start gameplay after a given period of time.
    /// </summary>
    [MessagePackObject]
    public class GameplayStartCountdown : MultiplayerCountdown
    {
    }
}
