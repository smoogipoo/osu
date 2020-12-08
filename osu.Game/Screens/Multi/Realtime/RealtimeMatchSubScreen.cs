// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR.Client;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Game.Online.Multiplayer;
using osu.Game.Online.RealtimeMultiplayer;

namespace osu.Game.Screens.Multi.Realtime
{
    public class RealtimeMatchSubScreen : RoomSubScreen, IMultiplayerClient
    {
        [Cached(typeof(IBindable<MultiplayerRoomState>))]
        private readonly Bindable<MultiplayerRoomState> roomState = new Bindable<MultiplayerRoomState>();

        [Cached(typeof(IBindable<MultiplayerRoomSettings>))]
        private readonly Bindable<MultiplayerRoomSettings> roomSettings = new Bindable<MultiplayerRoomSettings>();

        [Cached(typeof(IBindableList<MultiplayerRoomUser>))]
        private readonly BindableList<MultiplayerRoomUser> roomUsers = new BindableList<MultiplayerRoomUser>();

        [Cached(typeof(IBindable<MultiplayerRoomUser>))]
        private readonly Bindable<MultiplayerRoomUser> roomHost = new Bindable<MultiplayerRoomUser>();

        public RealtimeMatchSubScreen(Room room)
        {
        }

        Task IMultiplayerClient.RoomStateChanged(MultiplayerRoomState state)
        {
            Schedule(() => roomState.Value = state);
            return Task.CompletedTask;
        }

        Task IMultiplayerClient.UserJoined(MultiplayerRoomUser user)
        {
            Schedule(() => roomUsers.Add(user));
            return Task.CompletedTask;
        }

        Task IMultiplayerClient.UserLeft(MultiplayerRoomUser user)
        {
            Schedule(() => roomUsers.Remove(user));
            return Task.CompletedTask;
        }

        Task IMultiplayerClient.HostChanged(long userId)
        {
            Schedule(() => roomHost.Value = roomUsers.Single(u => u.UserID == userId));
            return Task.CompletedTask;
        }

        Task IMultiplayerClient.SettingsChanged(MultiplayerRoomSettings newSettings)
        {
            Schedule(() => roomSettings.Value = newSettings);
            return Task.CompletedTask;
        }

        Task IMultiplayerClient.UserStateChanged(long userId, MultiplayerUserState state)
        {
            Schedule(() =>
            {
                // Remove the user.
                var user = roomUsers.Single(u => u.UserID == userId);
                roomUsers.Remove(user);

                // Add the user back with the new state.
                user.State = state;
                roomUsers.Add(user);
            });

            return Task.CompletedTask;
        }
    }
}
