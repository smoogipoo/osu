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
using osu.Game.Online.API.Requests;
using osu.Game.Online.WebSockets;

namespace osu.Game.Online.Chat
{
    public class ChatWebSocketConnector : WebSocketConnector
    {
        public Action<Channel>? ChannelJoined;
        public Action<List<Message>>? NewMessages;
        public Action? PresenceReceived;

        private readonly IAPIProvider api;

        private long lastMessageId;

        public ChatWebSocketConnector(IAPIProvider api)
            : base(api)
        {
            this.api = api;
        }

        protected override async Task OnConnectedAsync(ClientWebSocket connection)
        {
            await SendMessage(new StartChatSocketMessage());

            var fetchReq = new GetUpdatesRequest(lastMessageId);

            fetchReq.Success += updates =>
            {
                if (updates?.Presence != null)
                {
                    foreach (var channel in updates.Presence)
                        handleJoinedChannel(channel);

                    //todo: handle left channels

                    handleMessages(updates.Messages);
                }

                PresenceReceived?.Invoke();
            };

            api.Queue(fetchReq);
        }

        protected override Task ProcessMessage(SocketMessage message)
        {
            switch (message.Event)
            {
                case "chat.message.new":
                    Debug.Assert(message.Data != null);

                    NewChatMessageData? messageData = JsonConvert.DeserializeObject<NewChatMessageData>(message.Data.ToString());
                    Debug.Assert(messageData != null);

                    List<Message> messages = messageData.GetMessages().Where(m => m.Sender.OnlineID != API.LocalUser.Value.OnlineID).ToList();

                    foreach (var msg in messages)
                        handleJoinedChannel(new Channel(msg.Sender) { Id = msg.ChannelId });

                    handleMessages(messages);
                    break;
            }

            return Task.CompletedTask;
        }

        private void handleJoinedChannel(Channel channel)
        {
            // we received this from the server so should mark the channel already joined.
            channel.Joined.Value = true;
            ChannelJoined?.Invoke(channel);
        }

        private void handleMessages(List<Message> messages)
        {
            NewMessages?.Invoke(messages);
            lastMessageId = messages.LastOrDefault()?.Id ?? lastMessageId;
        }
    }
}
