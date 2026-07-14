// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Threading.Tasks;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Game.Database;
using osu.Game.Online.API.Requests.Responses;

namespace osu.Game.Arcade
{
    public abstract partial class ArcadeClient : Component, IArcadeClient, IArcadeServer
    {
        public readonly BindableDictionary<int, ArcadeIdentity> ConnectedClients = new BindableDictionary<int, ArcadeIdentity>();

        public abstract IBindable<bool> IsConnected { get; }

        [Resolved]
        private UserLookupCache userLookupCache { get; set; } = null!;

        /// <summary>
        /// Attempts to retrieve a user's identity through a single-sign-on code.
        /// </summary>
        /// <param name="code">The single-sign-on code.</param>
        public abstract Task<ArcadeIdentity> GetUserWithCode(string code);

        public async Task UserConnected(int clientId, ArcadeIdentity identity)
        {
            APIUser? user = await userLookupCache.GetUserAsync(clientId).ConfigureAwait(false);

            if (user == null)
                return;

            Scheduler.Add(() =>
            {
                ConnectedClients[clientId] = identity;

                // This is very dodgy and abuses the fact that UserLookupStore provides permanent mutable references to stored objects.
                user.Id = identity.User.UserId;
                user.Username = identity.User.Username;
                user.AvatarUrl = identity.User.AvatarUrl;
                user.CoverUrl = identity.User.CoverUrl;
            });
        }

        public Task UserDisconnected(int clientId)
        {
            Scheduler.Add(() =>
            {
                ConnectedClients.Remove(clientId);
            });

            return Task.CompletedTask;
        }

        public abstract Task Connect(ArcadeIdentity identity);

        public abstract Task Disconnect();

        public Task DisconnectRequested()
        {
            return Task.CompletedTask;
        }

        public Task ServerShuttingDown()
        {
            return Task.CompletedTask;
        }
    }
}
