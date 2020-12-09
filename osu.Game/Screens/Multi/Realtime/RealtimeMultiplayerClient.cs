// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable enable

using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR.Client;
using osu.Game.Online.RealtimeMultiplayer;

namespace osu.Game.Screens.Multi.Realtime
{
    public class RealtimeMultiplayerClient : IStatefulMultiplayerClient
    {
        public MultiplayerUserState State { get; private set; }

        public MultiplayerRoom? Room { get; private set; }

        public readonly int UserID;

        private readonly HubConnection connection;

        public RealtimeMultiplayerClient(HubConnection connection, int userId)
        {
            this.connection = connection;
            UserID = userId;

            // this is kind of SILLY
            // https://github.com/dotnet/aspnetcore/issues/15198
            connection.On<MultiplayerRoomState>(nameof(IMultiplayerClient.RoomStateChanged), ((IMultiplayerClient)this).RoomStateChanged);
            connection.On<MultiplayerRoomUser>(nameof(IMultiplayerClient.UserJoined), ((IMultiplayerClient)this).UserJoined);
            connection.On<MultiplayerRoomUser>(nameof(IMultiplayerClient.UserLeft), ((IMultiplayerClient)this).UserLeft);
            connection.On<long>(nameof(IMultiplayerClient.HostChanged), ((IMultiplayerClient)this).HostChanged);
            connection.On<MultiplayerRoomSettings>(nameof(IMultiplayerClient.SettingsChanged), ((IMultiplayerClient)this).SettingsChanged);
            connection.On<long, MultiplayerUserState>(nameof(IMultiplayerClient.UserStateChanged), ((IMultiplayerClient)this).UserStateChanged);
            connection.On(nameof(IMultiplayerClient.LoadRequested), ((IMultiplayerClient)this).LoadRequested);
            connection.On(nameof(IMultiplayerClient.MatchStarted), ((IMultiplayerClient)this).MatchStarted);
            connection.On(nameof(IMultiplayerClient.ResultsReady), ((IMultiplayerClient)this).ResultsReady);
        }

        async Task<MultiplayerRoom> IMultiplayerServer.JoinRoom(long roomId)
            => Room = await connection.InvokeAsync<MultiplayerRoom>(nameof(IMultiplayerServer.JoinRoom), roomId);

        async Task IMultiplayerServer.LeaveRoom()
        {
            if (Room == null)
                return;

            await connection.InvokeAsync(nameof(IMultiplayerServer.LeaveRoom));
            Room = null;
        }

        Task IMultiplayerServer.TransferHost(long userId)
            => connection.InvokeAsync(nameof(IMultiplayerServer.TransferHost), userId);

        Task IMultiplayerServer.ChangeSettings(MultiplayerRoomSettings settings)
            => connection.InvokeAsync(nameof(IMultiplayerServer.ChangeSettings), settings);

        Task IMultiplayerServer.ChangeState(MultiplayerUserState newState)
            => connection.InvokeAsync(nameof(IMultiplayerServer.ChangeState), newState);

        Task IMultiplayerServer.StartMatch()
            => connection.InvokeAsync(nameof(IMultiplayerServer.StartMatch));

        Task IMultiplayerClient.RoomStateChanged(MultiplayerRoomState state)
        {
            Debug.Assert(Room != null);
            Room.State = state;

            return Task.CompletedTask;
        }

        Task IMultiplayerClient.UserJoined(MultiplayerRoomUser user)
        {
            Debug.Assert(Room != null);
            Room.Users.Add(user);

            return Task.CompletedTask;
        }

        Task IMultiplayerClient.UserLeft(MultiplayerRoomUser user)
        {
            Debug.Assert(Room != null);
            Room.Users.Remove(user);

            return Task.CompletedTask;
        }

        Task IMultiplayerClient.HostChanged(long userId)
        {
            Debug.Assert(Room != null);
            Room.Host = Room.Users.FirstOrDefault(u => u.UserID == userId);

            return Task.CompletedTask;
        }

        Task IMultiplayerClient.SettingsChanged(MultiplayerRoomSettings newSettings)
        {
            Debug.Assert(Room != null);
            Room.Settings = newSettings;

            return Task.CompletedTask;
        }

        Task IMultiplayerClient.UserStateChanged(long userId, MultiplayerUserState state)
        {
            if (userId == UserID)
                State = state;

            return Task.CompletedTask;
        }

        Task IMultiplayerClient.LoadRequested()
        {
            Console.WriteLine($"User {UserID} was requested to load");
            return Task.CompletedTask;
        }

        Task IMultiplayerClient.MatchStarted()
        {
            Console.WriteLine($"User {UserID} was informed the game started");
            return Task.CompletedTask;
        }

        Task IMultiplayerClient.ResultsReady()
        {
            Console.WriteLine($"User {UserID} was informed the results are ready");
            return Task.CompletedTask;
        }
    }
}
