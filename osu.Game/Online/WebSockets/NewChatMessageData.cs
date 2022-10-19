// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using osu.Game.Online.API.Requests.Responses;
using osu.Game.Online.Chat;

namespace osu.Game.Online.WebSockets
{
    [JsonObject(MemberSerialization.OptIn)]
    public class NewChatMessageData
    {
        [JsonProperty("messages")]
        private List<MessageData> messages { get; set; } = null!;

        [JsonProperty("users")]
        private List<APIUser> users { get; set; } = null!;

        public IEnumerable<Message> GetMessages() => messages.Select(m => new Message(m.MessageId)
        {
            Sender = users.Single(u => u.Id == m.SenderId),
            ChannelId = m.ChannelId,
            Content = m.Content,
            IsAction = m.IsAction
        });
    }
}
