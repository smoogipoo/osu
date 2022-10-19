// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using Newtonsoft.Json;

namespace osu.Game.Online.WebSockets
{
    [JsonObject(MemberSerialization.OptIn)]
    public class MessageData
    {
        [JsonProperty("message_id")]
        public long MessageId { get; set; }

        [JsonProperty("sender_id")]
        public int SenderId { get; set; }

        [JsonProperty("channel_id")]
        public long ChannelId { get; set; }

        [JsonProperty("content")]
        public string Content { get; set; } = null!;

        [JsonProperty("is_action")]
        public bool IsAction { get; set; }
    }
}
