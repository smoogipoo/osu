// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable enable

using System;
using osu.Game.Online.RealtimeMultiplayer;

namespace osu.Game.Screens.Multi.Realtime
{
    public interface IStatefulMultiplayerClient : IMultiplayerClient, IMultiplayerServer
    {
        event Action RoomChanged;

        MultiplayerRoom? Room { get; }
    }
}
