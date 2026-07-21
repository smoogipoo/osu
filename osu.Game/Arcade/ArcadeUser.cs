// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Text.Json.Serialization;
using MessagePack;
using osu.Game.Online.API.Requests.Responses;
using osu.Game.Users;

namespace osu.Game.Arcade
{
    [MessagePackObject]
    [Serializable]
    public class ArcadeUser
    {
        [JsonPropertyName("id")]
        [Key(0)]
        public int UserId { get; set; }

        [JsonPropertyName("username")]
        [Key(1)]
        public string Username { get; set; } = string.Empty;

        [JsonPropertyName("avatar_url")]
        [Key(2)]
        public string AvatarUrl { get; set; } = string.Empty;

        [JsonPropertyName("cover")]
        [Key(3)]
        public UserCover Cover { get; set; } = new UserCover();

        [JsonPropertyName("country_code")]
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

        public void TransferTo(APIUser user)
        {
            user.Id = UserId;
            user.Username = Username;
            user.AvatarUrl = AvatarUrl;
            user.CoverUrl = Cover.Url;
            user.CountryCode = CountryCode;
        }

        [MessagePackObject]
        [Serializable]
        public class UserCover
        {
            [JsonPropertyName(@"url")]
            [Key(0)]
            public string Url { get; set; } = string.Empty;
        }
    }
}
