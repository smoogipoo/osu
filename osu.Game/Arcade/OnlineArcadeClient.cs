// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Diagnostics;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR.Client;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Game.Online;
using osu.Game.Online.API;

namespace osu.Game.Arcade
{
    public partial class OnlineArcadeClient : ArcadeClient
    {
        public override string KeyEndpoint { get; }

        private readonly string serverEndpoint;

        private IHubClientConnector? connector;

        public override IBindable<bool> IsConnected { get; } = new BindableBool();

        private HubConnection? connection => connector?.CurrentConnection;

        public OnlineArcadeClient(EndpointConfiguration endpoints)
        {
            serverEndpoint = endpoints.ArcadeUrl;
            KeyEndpoint = $"{endpoints.WebsiteUrl}/one-time-key";
        }

        [BackgroundDependencyLoader]
        private void load(IAPIProvider api)
        {
            connector = api.GetHubConnector(nameof(OnlineArcadeClient), serverEndpoint);

            if (connector != null)
            {
                connector.ConfigureConnection = connection =>
                {
                    // this is kind of SILLY
                    // https://github.com/dotnet/aspnetcore/issues/15198
                    connection.On<int, ArcadeIdentity>(nameof(IArcadeClient.UserConnected), ((IArcadeClient)this).UserConnected);
                    connection.On<int>(nameof(IArcadeClient.UserDisconnected), ((IArcadeClient)this).UserDisconnected);
                };

                IsConnected.BindTo(connector.IsConnected);
            }
        }

        public override async Task<ArcadeIdentity> GetUserWithCode(string code)
        {
            OsuJsonWebRequest<ArcadeIdentity> req = new OsuJsonWebRequest<ArcadeIdentity>($"{KeyEndpoint}/check");
            req.Method = HttpMethod.Post;
            req.AddParameter("key", code);
            await req.PerformAsync();
            return req.ResponseObject;
        }

        public override Task Connect(ArcadeIdentity identity)
        {
            if (!IsConnected.Value)
                return Task.CompletedTask;

            Debug.Assert(connection != null);

            return connection.InvokeAsync(nameof(IArcadeServer.Connect), identity);
        }

        public override Task Disconnect()
        {
            if (!IsConnected.Value)
                return Task.CompletedTask;

            Debug.Assert(connection != null);

            return connection.InvokeAsync(nameof(IArcadeServer.Disconnect));
        }
    }
}
