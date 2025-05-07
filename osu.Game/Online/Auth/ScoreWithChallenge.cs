// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using Newtonsoft.Json;
using osu.Game.Online.API.Requests.Responses;

namespace osu.Game.Online.Auth
{
    public class ScoreWithChallenge
    {
        [JsonProperty("score")]
        public required SoloScoreInfo Score { get; set; }

        [JsonProperty("challenge")]
        public required byte[] Challenge { get; set; }
    }
}
