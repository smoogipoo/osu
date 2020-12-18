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
using osu.Game.Beatmaps;
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

        public abstract IBindable<bool> IsConnected { get; }

        public MultiplayerRoom? Room { get; private set; }

        public readonly BindableList<int> PlayingUsers = new BindableList<int>();

        [Resolved]
        private UserLookupCache userLookupCache { get; set; } = null!;

        [Resolved]
        private IAPIProvider api { get; set; } = null!;

        [Resolved]
        private RulesetStore rulesets { get; set; } = null!;

        private Room? apiRoom;
        private int playlistItemId; // Todo: THIS IS SUPER TEMPORARY!!

        public async Task JoinRoom(Room room)
        {
            Debug.Assert(Room == null);
            Debug.Assert(room.RoomID.Value != null);

            apiRoom = room;
            playlistItemId = room.Playlist.SingleOrDefault()?.ID ?? 0;

            Room = await JoinRoom(room.RoomID.Value.Value);

            Debug.Assert(Room != null);

            foreach (var user in Room.Users)
                await PopulateUser(user);

            await updateLocalRoomSettings(Room.Settings);
        }

        protected abstract Task<MultiplayerRoom> JoinRoom(long roomId);

        public virtual Task LeaveRoom()
        {
            if (Room == null)
                return Task.CompletedTask;

            apiRoom = null;
            Room = null;

            Schedule(InvokeRoomChanged);

            return Task.CompletedTask;
        }

        public void ChangeSettings(Optional<string> name = default, Optional<PlaylistItem> item = default)
        {
            if (Room == null)
                return;

            // A dummy playlist item filled with the current room settings (except mods).
            var existingPlaylistItem = new PlaylistItem
            {
                Beatmap =
                {
                    Value = new BeatmapInfo
                    {
                        OnlineBeatmapID = Room.Settings.BeatmapID,
                        MD5Hash = Room.Settings.BeatmapChecksum
                    }
                },
                RulesetID = Room.Settings.RulesetID
            };

            var newSettings = new MultiplayerRoomSettings
            {
                Name = name.GetOr(Room.Settings.Name),
                BeatmapID = item.GetOr(existingPlaylistItem).BeatmapID,
                BeatmapChecksum = item.GetOr(existingPlaylistItem).Beatmap.Value.MD5Hash,
                RulesetID = item.GetOr(existingPlaylistItem).RulesetID,
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

        async Task IMultiplayerClient.SettingsChanged(MultiplayerRoomSettings newSettings) => await updateLocalRoomSettings(newSettings);

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

        private Task updateLocalRoomSettings(MultiplayerRoomSettings settings)
        {
            if (Room == null)
                return Task.CompletedTask;

            Schedule(() =>
            {
                if (Room == null)
                    return;

                Debug.Assert(apiRoom != null);

                Room.Settings = settings;

                apiRoom.Name.Value = Room.Settings.Name;
                apiRoom.Playlist.Clear();

                InvokeRoomChanged();
            });

            var req = new GetBeatmapSetRequest(settings.BeatmapID, BeatmapSetLookupType.BeatmapId);
            req.Success += res =>
            {
                var beatmapSet = res.ToBeatmapSet(rulesets);

                var beatmap = beatmapSet.Beatmaps.Single(b => b.OnlineBeatmapID == settings.BeatmapID);
                beatmap.MD5Hash = settings.BeatmapChecksum;

                var ruleset = rulesets.GetRuleset(settings.RulesetID);
                var mods = settings.Mods.Select(m => m.ToMod(ruleset.CreateInstance()));

                PlaylistItem playlistItem = new PlaylistItem
                {
                    ID = playlistItemId,
                    Beatmap = { Value = beatmap },
                    Ruleset = { Value = ruleset },
                };

                playlistItem.RequiredMods.AddRange(mods);

                Schedule(() =>
                {
                    if (Room == null || !Room.Settings.Equals(settings))
                        return;

                    Debug.Assert(apiRoom != null);

                    apiRoom.Playlist.Clear();
                    apiRoom.Playlist.Add(playlistItem);
                });
            };

            api.Queue(req);
            return Task.CompletedTask;
        }
    }
}
