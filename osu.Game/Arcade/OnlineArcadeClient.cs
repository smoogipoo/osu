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
        /// <summary>
        /// URL of the SignalR arcade server.
        /// </summary>
        public const string ARCADE_SERVER_URL = "http://localhost:8081/signalr/arcade";

        /// <summary>
        /// The URL which users will open in their browser to generate SSO tokens.
        /// </summary>
        public const string ARCADE_SSO_GENERATE_URL = "https://osu.ppy.sh/api/v2/one-time-key";

        /// <summary>
        /// The URL which the client will query to retrieve data associated with an SSO token.
        /// </summary>
        public const string ARCADE_SSO_RETRIVAL_URL = "https://osu.ppy.sh/api/v2/one-time-key/check";

        private IHubClientConnector? connector;

        public override IBindable<bool> IsConnected { get; } = new BindableBool();

        private HubConnection? connection => connector?.CurrentConnection;

        [BackgroundDependencyLoader]
        private void load(IAPIProvider api)
        {
            connector = api.GetHubConnector(nameof(OnlineArcadeClient), ARCADE_SERVER_URL);

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
            OsuJsonWebRequest<ArcadeIdentity> req = new OsuJsonWebRequest<ArcadeIdentity>(ARCADE_SSO_GENERATE_URL);
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
