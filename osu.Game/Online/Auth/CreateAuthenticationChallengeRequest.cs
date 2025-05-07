// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Net.Http;
using Newtonsoft.Json;
using osu.Framework.IO.Network;
using osu.Game.Online.API;

namespace osu.Game.Online.Auth
{
    public class CreateAuthenticationChallengeRequest : APIRequest<AuthenticationChallenge>
    {
        private readonly string clientKey;

        public CreateAuthenticationChallengeRequest(string clientKey)
        {
            this.clientKey = clientKey;
        }

        protected override string Uri
        {
            get
            {
                // can be removed once the service has been successfully deployed to production
                if (API!.Endpoints.AttestationServiceUrl == null)
                    throw new NotSupportedException("Attestation not supported in this configuration!");

                return $@"{API!.Endpoints.AttestationServiceUrl!}/attestation/challenge";
            }
        }

        protected override string Target => throw new NotSupportedException();

        protected override WebRequest CreateWebRequest()
        {
            var req = base.CreateWebRequest();

            req.Method = HttpMethod.Post;
            req.ContentType = "application/json";
            req.AddRaw(JsonConvert.SerializeObject(new ChallengeRequest
            {
                ClientKey = clientKey
            }));

            return req;
        }

        private class ChallengeRequest
        {
            [JsonProperty("client_key")]
            public string ClientKey { get; set; } = string.Empty;
        }
    }
}
