// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.WebSockets;
using System.Threading.Tasks;
using Newtonsoft.Json;
using osu.Game.Online.API;
using osu.Game.Online.WebSockets;

namespace osu.Game.Online.Chat
{
    public class ChatWebSocketConnector : WebSocketConnector
    {
        public Action<IEnumerable<Message>>? NewMessages;

        public ChatWebSocketConnector(IAPIProvider api)
            : base(api)
        {
        }

        protected override async Task OnConnectedAsync(ClientWebSocket connection)
        {
            await SendMessage(new StartChatSocketMessage());
        }

        protected override Task ProcessMessage(SocketMessage message)
        {
            switch (message.Event)
            {
                case "chat.message.new":
                    Debug.Assert(message.Data != null);

                    NewChatMessageData? messageData = JsonConvert.DeserializeObject<NewChatMessageData>(message.Data.ToString());
                    Debug.Assert(messageData != null);

                    NewMessages?.Invoke(messageData.GetMessages().Where(m => m.Sender.OnlineID != API.LocalUser.Value.OnlineID));
                    break;
            }

            return Task.CompletedTask;
        }
    }
}
