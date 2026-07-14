// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Threading.Tasks;
using osu.Game.Online;

namespace osu.Game.Arcade
{
    public interface IArcadeClient : IStatefulUserHubClient
    {
        /// <summary>
        /// Notifies that a client has connected to the arcade server.
        /// </summary>
        /// <param name="clientId">The client that connected to the server.</param>
        /// <param name="identity">The client's identity.</param>
        Task UserConnected(int clientId, ArcadeIdentity identity);

        /// <summary>
        /// Notifies that a client has disconnected from the arcade server.
        /// </summary>
        /// <param name="clientId">The client that disconnected from the server.</param>
        /// <returns></returns>
        Task UserDisconnected(int clientId);
    }
}
