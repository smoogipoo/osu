// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable enable

using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Game.Online.API;
using osu.Game.Online.RealtimeMultiplayer;
using osu.Game.Screens.Multi.Realtime;
using osu.Game.Users;

namespace osu.Game.Tests.Visual.Multiplayer
{
    public class RealtimeTestMultiplayerClient : StatefulMultiplayerClient
    {
        public override IBindable<bool> IsConnected { get; } = new Bindable<bool>(true);

        [Resolved]
        private IAPIProvider api { get; set; } = null!;

        public void AddUser(User user) => ((IMultiplayerClient)this).UserJoined(new MultiplayerRoomUser(user.Id) { User = user });

        public void RemoveUser(User user)
        {
            Debug.Assert(Room != null);
            ((IMultiplayerClient)this).UserLeft(Room.Users.Single(u => u.User == user));
        }

        public void ChangeUserState(User user, MultiplayerUserState newState)
        {
            Debug.Assert(Room != null);

            ((IMultiplayerClient)this).UserStateChanged(user.Id, newState);

            Schedule(() =>
            {
                switch (newState)
                {
                    case MultiplayerUserState.Loaded:
                        if (Room.Users.All(u => u.State != MultiplayerUserState.WaitingForLoad))
                        {
                            foreach (var u in Room.Users.Where(u => u.State == MultiplayerUserState.Loaded))
                            {
                                Debug.Assert(u.User != null);
                                ChangeUserState(u.User, MultiplayerUserState.Playing);
                            }

                            ((IMultiplayerClient)this).MatchStarted();
                        }

                        break;

                    case MultiplayerUserState.FinishedPlay:
                        if (Room.Users.All(u => u.State != MultiplayerUserState.Playing))
                        {
                            foreach (var u in Room.Users.Where(u => u.State == MultiplayerUserState.FinishedPlay))
                            {
                                Debug.Assert(u.User != null);
                                ChangeUserState(u.User, MultiplayerUserState.Results);
                            }

                            ((IMultiplayerClient)this).ResultsReady();
                        }

                        break;
                }
            });
        }

        protected override Task<MultiplayerRoom> JoinRoom(long roomId)
        {
            var user = new MultiplayerRoomUser(api.LocalUser.Value.Id) { User = api.LocalUser.Value };

            var room = new MultiplayerRoom(roomId);
            room.Users.Add(user);

            if (room.Users.Count == 1)
                room.Host = user;

            return Task.FromResult(room);
        }

        public override Task TransferHost(int userId) => ((IMultiplayerClient)this).HostChanged(userId);

        public override Task ChangeSettings(MultiplayerRoomSettings settings) => ((IMultiplayerClient)this).SettingsChanged(settings);

        public override Task ChangeState(MultiplayerUserState newState)
        {
            ChangeUserState(api.LocalUser.Value, newState);
            return Task.CompletedTask;
        }

        public override Task StartMatch()
        {
            Debug.Assert(Room != null);

            foreach (var user in Room.Users)
            {
                Debug.Assert(user.User != null);
                ChangeUserState(user.User, MultiplayerUserState.WaitingForLoad);
            }

            ((IMultiplayerClient)this).LoadRequested();

            return Task.CompletedTask;
        }
    }
}
