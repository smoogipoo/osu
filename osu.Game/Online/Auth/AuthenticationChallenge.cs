// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using Newtonsoft.Json;

namespace osu.Game.Online.Auth
{
    public class AuthenticationChallenge
    {
        /// <summary>
        /// The challenge data.
        /// </summary>
        [JsonProperty("data")]
        public byte[] Data { get; set; } = [];

        /// <summary>
        /// Whether the client will need to use this challenge to attest to its authenticity.
        /// </summary>
        [JsonProperty("must_attest")]
        public bool MustAttest { get; set; }
    }
}
