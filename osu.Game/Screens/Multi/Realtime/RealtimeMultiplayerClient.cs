// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable enable

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR.Client;
using osu.Game.Online.RealtimeMultiplayer;

namespace osu.Game.Screens.Multi.Realtime
{
    public class RealtimeMultiplayerClient : StatefulMultiplayerClient
    {
        public override MultiplayerRoom? Room => room;
        private MultiplayerRoom? room;

        private readonly List<IDisposable> boundDelegates = new List<IDisposable>();
        private HubConnection? connection;

        public void BindConnection(HubConnection connection)
        {
            this.connection = connection;

            // this is kind of SILLY
            // https://github.com/dotnet/aspnetcore/issues/15198
            boundDelegates.Add(connection.On<MultiplayerRoomState>(nameof(IMultiplayerClient.RoomStateChanged), ((IMultiplayerClient)this).RoomStateChanged));
            boundDelegates.Add(connection.On<MultiplayerRoomUser>(nameof(IMultiplayerClient.UserJoined), ((IMultiplayerClient)this).UserJoined));
            boundDelegates.Add(connection.On<MultiplayerRoomUser>(nameof(IMultiplayerClient.UserLeft), ((IMultiplayerClient)this).UserLeft));
            boundDelegates.Add(connection.On<long>(nameof(IMultiplayerClient.HostChanged), ((IMultiplayerClient)this).HostChanged));
            boundDelegates.Add(connection.On<MultiplayerRoomSettings>(nameof(IMultiplayerClient.SettingsChanged), ((IMultiplayerClient)this).SettingsChanged));
            boundDelegates.Add(connection.On<long, MultiplayerUserState>(nameof(IMultiplayerClient.UserStateChanged), ((IMultiplayerClient)this).UserStateChanged));
            boundDelegates.Add(connection.On(nameof(IMultiplayerClient.LoadRequested), ((IMultiplayerClient)this).LoadRequested));
            boundDelegates.Add(connection.On(nameof(IMultiplayerClient.MatchStarted), ((IMultiplayerClient)this).MatchStarted));
            boundDelegates.Add(connection.On(nameof(IMultiplayerClient.ResultsReady), ((IMultiplayerClient)this).ResultsReady));
        }

        public void UnbindConnection()
        {
            foreach (var b in boundDelegates)
                b.Dispose();
            boundDelegates.Clear();
            connection = null;
        }

        public override async Task<MultiplayerRoom> JoinRoom(long roomId)
        {
            var joinedRoom = await connection.InvokeAsync<MultiplayerRoom>(nameof(IMultiplayerServer.JoinRoom), roomId);

            foreach (var user in joinedRoom.Users)
                await PopulateUser(user);

            return room = joinedRoom;
        }

        public override async Task LeaveRoom()
        {
            if (Room == null)
                return;

            await connection.InvokeAsync(nameof(IMultiplayerServer.LeaveRoom));
            room = null;
        }

        public override Task TransferHost(long userId)
            => connection.InvokeAsync(nameof(IMultiplayerServer.TransferHost), userId);

        public override Task ChangeSettings(MultiplayerRoomSettings settings)
            => connection.InvokeAsync(nameof(IMultiplayerServer.ChangeSettings), settings);

        public override Task ChangeState(MultiplayerUserState newState)
            => connection.InvokeAsync(nameof(IMultiplayerServer.ChangeState), newState);

        public override Task StartMatch()
            => connection.InvokeAsync(nameof(IMultiplayerServer.StartMatch));
    }
}
