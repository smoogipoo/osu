// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

namespace osu.Game.Online
{
    public class DevelopmentEndpointConfiguration : EndpointConfiguration
    {
        public DevelopmentEndpointConfiguration()
        {
            WebsiteRootUrl = APIEndpointUrl = @"http://localhost:8080";
            APIClientSecret = @"WV5L9onzRK5VabnfbGve1SjUDaaBkrYtOVGjmBnL";
            APIClientID = "1";
            SpectatorEndpointUrl = @"http://localhost:8081/spectator";
            MultiplayerEndpointUrl = @"http://localhost:8081/multiplayer";
            MetadataEndpointUrl = @"http://localhost:8081/metadata";
        }
    }
}
