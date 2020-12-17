// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable enable

using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Game.Database;
using osu.Game.Online.API;
using osu.Game.Online.API.Requests;
using osu.Game.Online.Multiplayer;
using osu.Game.Online.Multiplayer.RoomStatuses;
using osu.Game.Online.RealtimeMultiplayer;
using osu.Game.Rulesets;
using osu.Game.Utils;

namespace osu.Game.Screens.Multi.Realtime
{
    public abstract class StatefulMultiplayerClient : Component, IMultiplayerClient, IMultiplayerRoomServer
    {
        public event Action? RoomChanged;
        public event Action? LoadRequested;
        public event Action? MatchStarted;
        public event Action? ResultsReady;

        public abstract MultiplayerRoom? Room { get; }
        public abstract IBindable<bool> IsConnected { get; }

        public readonly BindableList<int> PlayingUsers = new BindableList<int>();

        [Resolved]
        private UserLookupCache userLookupCache { get; set; } = null!;

        [Resolved]
        private IAPIProvider api { get; set; } = null!;

        [Resolved]
        private RulesetStore rulesets { get; set; } = null!;

        private Room? apiRoom;

        public virtual Task JoinRoom(Room room)
        {
            Debug.Assert(Room == null);
            Debug.Assert(room.RoomID.Value != null);

            apiRoom = room;
            return Task.CompletedTask;
        }

        public virtual Task LeaveRoom()
        {
            if (apiRoom == null)
                return Task.CompletedTask;

            apiRoom = null;
            return Task.CompletedTask;
        }

        public void ChangeSettings(Optional<string> name = default, Optional<PlaylistItem> item = default)
        {
            if (Room == null)
                return;

            var newSettings = new MultiplayerRoomSettings
            {
                Name = name.GetOr(Room.Settings.Name),
                BeatmapID = item.GetOr(new PlaylistItem { BeatmapID = Room.Settings.BeatmapID }).BeatmapID,
                RulesetID = item.GetOr(new PlaylistItem { RulesetID = Room.Settings.RulesetID!.Value }).RulesetID,
                Mods = item.HasValue ? item.Value!.RequiredMods.Select(m => new APIMod(m)).ToList() : Room.Settings.Mods
            };

            // Make sure there would be a meaningful change in settings.
            if (newSettings.Equals(Room.Settings))
                return;

            ChangeSettings(newSettings);
        }

        public abstract Task TransferHost(int userId);

        public abstract Task ChangeSettings(MultiplayerRoomSettings settings);

        public abstract Task ChangeState(MultiplayerUserState newState);

        public abstract Task StartMatch();

        Task IMultiplayerClient.RoomStateChanged(MultiplayerRoomState state)
        {
            Schedule(() =>
            {
                if (Room == null)
                    return;

                Debug.Assert(apiRoom != null);

                Room.State = state;

                switch (state)
                {
                    case MultiplayerRoomState.Open:
                        apiRoom.Status.Value = new RoomStatusOpen();
                        break;

                    case MultiplayerRoomState.Playing:
                        apiRoom.Status.Value = new RoomStatusPlaying();
                        break;

                    case MultiplayerRoomState.Closed:
                        apiRoom.Status.Value = new RoomStatusEnded();
                        break;
                }

                InvokeRoomChanged();
            });

            return Task.CompletedTask;
        }

        async Task IMultiplayerClient.UserJoined(MultiplayerRoomUser user)
        {
            await PopulateUser(user);

            Schedule(() =>
            {
                if (Room == null)
                    return;

                Room.Users.Add(user);

                InvokeRoomChanged();
            });
        }

        Task IMultiplayerClient.UserLeft(MultiplayerRoomUser user)
        {
            Schedule(() =>
            {
                if (Room == null)
                    return;

                Room.Users.Remove(user);
                PlayingUsers.Remove(user.UserID);

                InvokeRoomChanged();
            });

            return Task.CompletedTask;
        }

        Task IMultiplayerClient.HostChanged(int userId)
        {
            Schedule(() =>
            {
                if (Room == null)
                    return;

                Debug.Assert(apiRoom != null);

                var user = Room.Users.FirstOrDefault(u => u.UserID == userId);

                Room.Host = user;
                apiRoom.Host.Value = user?.User;

                InvokeRoomChanged();
            });

            return Task.CompletedTask;
        }

        async Task IMultiplayerClient.SettingsChanged(MultiplayerRoomSettings newSettings)
        {
            var req = new GetBeatmapSetRequest(newSettings.BeatmapID, BeatmapSetLookupType.BeatmapId);
            bool reqCompleted = false;
            Exception? reqException = null;
            PlaylistItem? playlistItem = null;

            req.Success += res => Task.Run(() =>
            {
                try
                {
                    var beatmapSet = res.ToBeatmapSet(rulesets);

                    var beatmap = beatmapSet.Beatmaps.Single(b => b.OnlineBeatmapID == newSettings.BeatmapID);
                    var ruleset = rulesets.GetRuleset(newSettings.RulesetID ?? 0);
                    var mods = newSettings.Mods.Select(m => m.ToMod(ruleset.CreateInstance()));

                    playlistItem = new PlaylistItem
                    {
                        Beatmap = { Value = beatmap },
                        Ruleset = { Value = ruleset },
                    };

                    playlistItem.RequiredMods.AddRange(mods);
                }
                finally
                {
                    reqCompleted = true;
                }
            });

            req.Failure += e =>
            {
                reqException = e;
                reqCompleted = true;
            };

            api.Queue(req);

            while (!reqCompleted)
                await Task.Delay(100);

            if (reqException != null)
                throw reqException;

            Debug.Assert(playlistItem != null);

            Schedule(() =>
            {
                if (Room == null)
                    return;

                Debug.Assert(apiRoom != null);

                Room.Settings = newSettings;
                apiRoom.Name.Value = newSettings.Name;
                apiRoom.Playlist.Clear();
                apiRoom.Playlist.Add(playlistItem);

                InvokeRoomChanged();
            });
        }

        Task IMultiplayerClient.UserStateChanged(int userId, MultiplayerUserState state)
        {
            Schedule(() =>
            {
                if (Room == null)
                    return;

                Room.Users.Single(u => u.UserID == userId).State = state;

                if (state != MultiplayerUserState.Playing)
                    PlayingUsers.Remove(userId);

                InvokeRoomChanged();
            });

            return Task.CompletedTask;
        }

        Task IMultiplayerClient.LoadRequested()
        {
            Schedule(() =>
            {
                if (Room == null)
                    return;

                LoadRequested?.Invoke();
            });

            return Task.CompletedTask;
        }

        Task IMultiplayerClient.MatchStarted()
        {
            Debug.Assert(Room != null);
            var players = Room.Users.Where(u => u.State == MultiplayerUserState.Playing).Select(u => u.UserID).ToList();

            Schedule(() =>
            {
                if (Room == null)
                    return;

                PlayingUsers.AddRange(players);

                MatchStarted?.Invoke();
            });

            return Task.CompletedTask;
        }

        Task IMultiplayerClient.ResultsReady()
        {
            Schedule(() =>
            {
                if (Room == null)
                    return;

                ResultsReady?.Invoke();
            });

            return Task.CompletedTask;
        }

        protected void InvokeRoomChanged() => RoomChanged?.Invoke();

        protected async Task PopulateUser(MultiplayerRoomUser multiplayerUser) => multiplayerUser.User ??= await userLookupCache.GetUserAsync(multiplayerUser.UserID);
    }
}
