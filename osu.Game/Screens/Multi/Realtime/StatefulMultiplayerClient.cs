// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable enable

using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using osu.Game.Database;
using osu.Game.Online.RealtimeMultiplayer;

namespace osu.Game.Screens.Multi.Realtime
{
    public abstract class StatefulMultiplayerClient : IStatefulMultiplayerClient
    {
        public event Action? RoomChanged;

        public abstract MultiplayerRoom? Room { get; }

        public readonly int UserID;

        private readonly UserLookupCache userLookupCache;

        protected StatefulMultiplayerClient(int userId, UserLookupCache userLookupCache)
        {
            UserID = userId;
            this.userLookupCache = userLookupCache;
        }

        public abstract Task<MultiplayerRoom> JoinRoom(long roomId);

        public abstract Task LeaveRoom();

        public abstract Task TransferHost(long userId);

        public abstract Task ChangeSettings(MultiplayerRoomSettings settings);

        public abstract Task ChangeState(MultiplayerUserState newState);

        public abstract Task StartMatch();

        Task IMultiplayerClient.RoomStateChanged(MultiplayerRoomState state)
        {
            Debug.Assert(Room != null);
            Room.State = state;

            return Task.CompletedTask;
        }

        async Task IMultiplayerClient.UserJoined(MultiplayerRoomUser user)
        {
            await PopulateUser(user);

            Debug.Assert(Room != null);
            Room.Users.Add(user);

            RoomChanged?.Invoke();
        }

        Task IMultiplayerClient.UserLeft(MultiplayerRoomUser user)
        {
            Debug.Assert(Room != null);
            Room.Users.Remove(user);

            RoomChanged?.Invoke();

            return Task.CompletedTask;
        }

        Task IMultiplayerClient.HostChanged(long userId)
        {
            Debug.Assert(Room != null);
            Room.Host = Room.Users.FirstOrDefault(u => u.UserID == userId);

            RoomChanged?.Invoke();

            return Task.CompletedTask;
        }

        Task IMultiplayerClient.SettingsChanged(MultiplayerRoomSettings newSettings)
        {
            Debug.Assert(Room != null);
            Room.Settings = newSettings;

            RoomChanged?.Invoke();

            return Task.CompletedTask;
        }

        Task IMultiplayerClient.UserStateChanged(long userId, MultiplayerUserState state)
        {
            Debug.Assert(Room != null);
            Room.Users.Single(u => u.UserID == userId).State = state;

            RoomChanged?.Invoke();

            return Task.CompletedTask;
        }

        Task IMultiplayerClient.LoadRequested()
        {
            Console.WriteLine($"User {UserID} was requested to load");
            return Task.CompletedTask;
        }

        Task IMultiplayerClient.MatchStarted()
        {
            Console.WriteLine($"User {UserID} was informed the game started");
            return Task.CompletedTask;
        }

        Task IMultiplayerClient.ResultsReady()
        {
            Console.WriteLine($"User {UserID} was informed the results are ready");
            return Task.CompletedTask;
        }

        protected async Task PopulateUser(MultiplayerRoomUser multiplayerUser) => multiplayerUser.User ??= await userLookupCache.GetUserAsync(multiplayerUser.UserID);
    }
}
