// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Threading.Tasks;

namespace osu.Game.Arcade
{
    public interface IArcadeServer
    {
        /// <summary>
        /// Connects to the arcade server with the given identity.
        /// </summary>
        /// <param name="identity">The identity.</param>
        Task Connect(ArcadeIdentity identity);

        /// <summary>
        /// Disconnects from the arcade server.
        /// </summary>
        Task Disconnect();
    }
}
