// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable enable

using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Game.Database;
using osu.Game.Online.RealtimeMultiplayer;

namespace osu.Game.Screens.Multi.Realtime
{
    public abstract class StatefulMultiplayerClient : Component, IStatefulMultiplayerClient
    {
        public event Action? RoomChanged;

        public abstract MultiplayerRoom? Room { get; }

        [Resolved]
        private UserLookupCache userLookupCache { get; set; } = null!;

        public abstract Task<MultiplayerRoom> JoinRoom(long roomId);

        public abstract Task LeaveRoom();

        public abstract Task TransferHost(long userId);

        public abstract Task ChangeSettings(MultiplayerRoomSettings settings);

        public abstract Task ChangeState(MultiplayerUserState newState);

        public abstract Task StartMatch();

        Task IMultiplayerClient.RoomStateChanged(MultiplayerRoomState state)
        {
            Schedule(() =>
            {
                Debug.Assert(Room != null);
                Room.State = state;

                RoomChanged?.Invoke();
            });

            return Task.CompletedTask;
        }

        async Task IMultiplayerClient.UserJoined(MultiplayerRoomUser user)
        {
            await PopulateUser(user);

            Schedule(() =>
            {
                Debug.Assert(Room != null);
                Room.Users.Add(user);

                RoomChanged?.Invoke();
            });
        }

        Task IMultiplayerClient.UserLeft(MultiplayerRoomUser user)
        {
            Schedule(() =>
            {
                Debug.Assert(Room != null);
                Room.Users.Remove(user);

                RoomChanged?.Invoke();
            });

            return Task.CompletedTask;
        }

        Task IMultiplayerClient.HostChanged(long userId)
        {
            Schedule(() =>
            {
                Debug.Assert(Room != null);
                Room.Host = Room.Users.FirstOrDefault(u => u.UserID == userId);

                RoomChanged?.Invoke();
            });

            return Task.CompletedTask;
        }

        Task IMultiplayerClient.SettingsChanged(MultiplayerRoomSettings newSettings)
        {
            Schedule(() =>
            {
                Debug.Assert(Room != null);
                Room.Settings = newSettings;

                RoomChanged?.Invoke();
            });

            return Task.CompletedTask;
        }

        Task IMultiplayerClient.UserStateChanged(long userId, MultiplayerUserState state)
        {
            Schedule(() =>
            {
                Debug.Assert(Room != null);
                Room.Users.Single(u => u.UserID == userId).State = state;

                RoomChanged?.Invoke();
            });

            return Task.CompletedTask;
        }

        Task IMultiplayerClient.LoadRequested()
        {
            return Task.CompletedTask;
        }

        Task IMultiplayerClient.MatchStarted()
        {
            return Task.CompletedTask;
        }

        Task IMultiplayerClient.ResultsReady()
        {
            return Task.CompletedTask;
        }

        protected async Task PopulateUser(MultiplayerRoomUser multiplayerUser) => multiplayerUser.User ??= await userLookupCache.GetUserAsync(multiplayerUser.UserID);
    }
}
