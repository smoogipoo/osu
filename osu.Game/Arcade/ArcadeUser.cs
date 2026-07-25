// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using MessagePack;
using Newtonsoft.Json;
using osu.Game.Online.API.Requests.Responses;
using osu.Game.Users;

namespace osu.Game.Arcade
{
    [MessagePackObject]
    [Serializable]
    public class ArcadeUser
    {
        [JsonProperty("id")]
        [Key(0)]
        public int UserId { get; set; }

        [JsonProperty("username")]
        [Key(1)]
        public string Username { get; set; } = string.Empty;

        [JsonProperty("avatar_url")]
        [Key(2)]
        public string AvatarUrl { get; set; } = string.Empty;

        [JsonProperty("cover")]
        [Key(3)]
        public UserCover Cover { get; set; } = new UserCover();

        [JsonProperty("country_code")]
        [Key(4)]
        public string CountryCodeString { get; set; } = string.Empty;

        [JsonIgnore]
        [IgnoreMember]
        public CountryCode CountryCode
        {
            get => Enum.TryParse(CountryCodeString, out CountryCode result) ? result : CountryCode.Unknown;
            set => CountryCodeString = value.ToString();
        }

        public APIUser ToAPIUser() => new APIUser
        {
            Id = UserId,
            Username = Username,
            AvatarUrl = AvatarUrl,
            CoverUrl = Cover.Url,
            CountryCode = CountryCode
        };

        [MessagePackObject]
        [Serializable]
        public class UserCover
        {
            [JsonProperty(@"url")]
            [Key(0)]
            public string Url { get; set; } = string.Empty;
        }
    }
}
