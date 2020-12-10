// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable enable

using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR.Client;
using osu.Game.Online.RealtimeMultiplayer;

namespace osu.Game.Screens.Multi.Realtime
{
    public class RealtimeMultiplayerClient : StatefulMultiplayerClient
    {
        public override MultiplayerRoom? Room => room;
        private MultiplayerRoom? room;

        private readonly HubConnection connection;

        public RealtimeMultiplayerClient(int userId, HubConnection connection)
            : base(userId)
        {
            this.connection = connection;

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

        public override async Task<MultiplayerRoom> JoinRoom(long roomId)
            => room = await connection.InvokeAsync<MultiplayerRoom>(nameof(IMultiplayerServer.JoinRoom), roomId);

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
