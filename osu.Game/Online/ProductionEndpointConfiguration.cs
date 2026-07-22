// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

namespace osu.Game.Online
{
    public class ProductionEndpointConfiguration : EndpointConfiguration
    {
        public ProductionEndpointConfiguration()
        {
            WebsiteUrl = @"https://osu.ppy.sh";
            APIUrl = @"https://osu.ppy.sh";
            APIClientSecret = @"FGc9GAtyHzeQDshWP5Ah7dega8hJACAJpQtw6OXk";
            APIClientID = @"5";
            SpectatorUrl = @"https://spectator.osu.ppy.sh/spectator";
            MultiplayerUrl = @"https://spectator.osu.ppy.sh/multiplayer";
            MetadataUrl = @"https://spectator.osu.ppy.sh/metadata";
            ArcadeUrl = @"https://spectator.osu.ppy.sh/arcade";
            BeatmapSubmissionServiceUrl = @"https://bss.ppy.sh";
        }
    }
}
