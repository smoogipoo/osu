// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Net.Http;
using Newtonsoft.Json;
using osu.Framework.IO.Network;
using osu.Game.Online.API;

namespace osu.Game.Online.Auth
{
    public class SubmitAttestationRequest : APIRequest
    {
        private readonly string clientKey;
        private readonly byte[] attestationData;

        public SubmitAttestationRequest(string clientKey, byte[] attestationData)
        {
            this.clientKey = clientKey;
            this.attestationData = attestationData;
        }

        protected override string Uri
        {
            get
            {
                // can be removed once the service has been successfully deployed to production
                if (API!.Endpoints.AttestationServiceUrl == null)
                    throw new NotSupportedException("Attestation not supported in this configuration!");

                return $@"{API!.Endpoints.AttestationServiceUrl!}/attestation/attest";
            }
        }

        protected override string Target => throw new NotSupportedException();

        protected override WebRequest CreateWebRequest()
        {
            var req = base.CreateWebRequest();

            req.Method = HttpMethod.Post;
            req.ContentType = "application/json";
            req.AddRaw(JsonConvert.SerializeObject(new AttestationRequestData
            {
                ClientKey = clientKey,
                AttestationData = attestationData
            }));

            return req;
        }

        private class AttestationRequestData
        {
            [JsonProperty("client_key")]
            public string ClientKey { get; set; } = string.Empty;

            [JsonProperty("attestation_data")]
            public byte[] AttestationData { get; set; } = [];
        }
    }
}
