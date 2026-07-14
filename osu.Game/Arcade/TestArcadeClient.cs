// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Threading.Tasks;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Game.Online.API;

namespace osu.Game.Arcade
{
    public partial class TestArcadeClient : ArcadeClient
    {
        public override IBindable<bool> IsConnected { get; } = new Bindable<bool>(true);

        public Func<string, ArcadeIdentity> GetUserWithCodeFunc { get; set; } = _ => throw new NotImplementedException();

        [Resolved]
        private IAPIProvider api { get; set; } = null!;

        public override Task<ArcadeIdentity> GetUserWithCode(string code)
            => Task.FromResult(GetUserWithCodeFunc(code));

        public override Task Connect(ArcadeIdentity identity)
            => ConnectUser(api.LocalUser.Value.OnlineID, identity);

        public Task ConnectUser(int clientId, ArcadeIdentity identity)
            => UserConnected(clientId, identity);

        public override Task Disconnect()
            => DisconnectUser(api.LocalUser.Value.OnlineID);

        public Task DisconnectUser(int clientId)
            => UserDisconnected(clientId);
    }
}
