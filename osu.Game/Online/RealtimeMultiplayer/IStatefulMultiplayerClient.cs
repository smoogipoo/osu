// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;

#nullable enable

namespace osu.Game.Online.RealtimeMultiplayer
{
    /// <summary>
    /// A multiplayer client which maintains local room and user state. Also provides a proxy to access the <see cref="IMultiplayerServer"/>.
    /// </summary>
    public interface IStatefulMultiplayerClient : IMultiplayerClient, IMultiplayerServer
    {
        event Action? RoomChanged;
        new event Action? LoadRequested;
        new event Action? MatchStarted;
        new event Action? ResultsReady;

        MultiplayerRoom? Room { get; }
    }
}
