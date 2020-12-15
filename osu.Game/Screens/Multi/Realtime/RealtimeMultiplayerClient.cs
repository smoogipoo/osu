// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable enable

using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Logging;
using osu.Game.Online.API;
using osu.Game.Online.Multiplayer;
using osu.Game.Online.RealtimeMultiplayer;

namespace osu.Game.Screens.Multi.Realtime
{
    public class RealtimeMultiplayerClient : StatefulMultiplayerClient
    {
        private const string endpoint = "https://spectator.ppy.sh/multiplayer";

        public override IBindable<bool> IsConnected => isConnected; // Not thread-safe!!

        public override MultiplayerRoom? Room => joinedRoom;

        private readonly Bindable<bool> isConnected = new Bindable<bool>();
        private readonly IBindable<APIState> apiState = new Bindable<APIState>();

        [Resolved]
        private IAPIProvider api { get; set; } = null!;

        private MultiplayerRoom? joinedRoom;
        private HubConnection? connection;

        [BackgroundDependencyLoader]
        private void load()
        {
            apiState.BindTo(api.State);
            apiState.BindValueChanged(apiStateChanged, true);
        }

        private void apiStateChanged(ValueChangedEvent<APIState> state)
        {
            switch (state.NewValue)
            {
                case APIState.Failing:
                case APIState.Offline:
                    connection?.StopAsync();
                    connection = null;
                    break;

                case APIState.Online:
                    Task.Run(Connect);
                    break;
            }
        }

        protected virtual async Task Connect()
        {
            if (connection != null)
                return;

            connection = new HubConnectionBuilder()
                         .WithUrl(endpoint, options =>
                         {
                             options.Headers.Add("Authorization", $"Bearer {api.AccessToken}");
                         })
                         .AddNewtonsoftJsonProtocol(options => { options.PayloadSerializerSettings.ReferenceLoopHandling = ReferenceLoopHandling.Ignore; })
                         .Build();

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

            connection.Closed += async ex =>
            {
                isConnected.Value = false;

                if (ex != null)
                {
                    Logger.Log($"Multiplayer client lost connection: {ex}", LoggingTarget.Network);
                    await tryUntilConnected();
                }
            };

            await tryUntilConnected();

            async Task tryUntilConnected()
            {
                Logger.Log("Multiplayer client connecting...", LoggingTarget.Network);

                while (api.State.Value == APIState.Online)
                {
                    try
                    {
                        // reconnect on any failure
                        await connection.StartAsync();
                        Logger.Log("Multiplayer client connected!", LoggingTarget.Network);

                        // Success.
                        isConnected.Value = true;
                        break;
                    }
                    catch (Exception e)
                    {
                        Logger.Log($"Multiplayer client connection error: {e}", LoggingTarget.Network);
                        await Task.Delay(5000);
                    }
                }
            }
        }

        public override async Task JoinRoom(Room room)
        {
            await base.JoinRoom(room);
            var joined = await connection.InvokeAsync<MultiplayerRoom>(nameof(IMultiplayerServer.JoinRoom), room.RoomID.Value);

            foreach (var user in joined.Users)
                await PopulateUser(user);

            Schedule(() =>
            {
                joinedRoom = joined;
                InvokeRoomChanged();
            });
        }

        public override async Task LeaveRoom()
        {
            await base.LeaveRoom();
            await connection.InvokeAsync(nameof(IMultiplayerServer.LeaveRoom));

            Schedule(() =>
            {
                joinedRoom = null;
                InvokeRoomChanged();
            });
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
