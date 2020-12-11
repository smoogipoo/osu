// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable enable

using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using osu.Framework.Allocation;
using osu.Framework.Threading;
using osu.Game.Database;
using osu.Game.Online.API;
using osu.Game.Online.API.Requests;
using osu.Game.Online.Multiplayer;
using osu.Game.Online.Multiplayer.RoomStatuses;
using osu.Game.Online.RealtimeMultiplayer;
using osu.Game.Rulesets;

namespace osu.Game.Screens.Multi.Realtime
{
    public abstract class StatefulMultiplayerClient : MultiplayerComposite, IStatefulMultiplayerClient
    {
        public event Action? RoomChanged;

        public abstract MultiplayerRoom? Room { get; }

        [Resolved]
        private UserLookupCache userLookupCache { get; set; } = null!;

        [Resolved]
        private IAPIProvider api { get; set; } = null!;

        [Resolved]
        private RulesetStore rulesets { get; set; } = null!;

        protected override void LoadComplete()
        {
            base.LoadComplete();

            RoomName.BindValueChanged(_ => scheduleRoomUpdate());
            Playlist.BindCollectionChanged((_, __) => scheduleRoomUpdate());
        }

        private ScheduledDelegate? scheduledUpdate;

        private void scheduleRoomUpdate()
        {
            if (RoomID.Value == null)
                return;

            scheduledUpdate?.Cancel();
            scheduledUpdate = Schedule(() =>
            {
                Debug.Assert(Room != null);

                var newSettings = new MultiplayerRoomSettings
                {
                    Name = RoomName.Value,
                    BeatmapID = Playlist.Single().BeatmapID,
                    RulesetID = Playlist.Single().RulesetID,
                    Mods = Playlist.Single().RequiredMods.Select(m => new APIMod(m)).ToList()
                };

                // Make sure there would be a meaningful change in settings.
                if (newSettings.Equals(Room.Settings))
                    return;

                ChangeSettings(newSettings);
            });
        }

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

                switch (state)
                {
                    case MultiplayerRoomState.Open:
                        Status.Value = new RoomStatusOpen();
                        break;

                    case MultiplayerRoomState.Playing:
                        Status.Value = new RoomStatusPlaying();
                        break;

                    case MultiplayerRoomState.Closed:
                        Status.Value = new RoomStatusEnded();
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
                Debug.Assert(Room != null);
                Room.Users.Add(user);

                InvokeRoomChanged();
            });
        }

        Task IMultiplayerClient.UserLeft(MultiplayerRoomUser user)
        {
            Schedule(() =>
            {
                Debug.Assert(Room != null);
                Room.Users.Remove(user);

                InvokeRoomChanged();
            });

            return Task.CompletedTask;
        }

        Task IMultiplayerClient.HostChanged(long userId)
        {
            Schedule(() =>
            {
                Debug.Assert(Room != null);

                var user = Room.Users.FirstOrDefault(u => u.UserID == userId);

                Room.Host = user;
                Host.Value = user?.User;

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
                Debug.Assert(Room != null);

                Room.Settings = newSettings;
                RoomName.Value = newSettings.Name;
                Playlist.Clear();
                Playlist.Add(playlistItem);

                InvokeRoomChanged();
            });
        }

        Task IMultiplayerClient.UserStateChanged(long userId, MultiplayerUserState state)
        {
            Schedule(() =>
            {
                Debug.Assert(Room != null);
                Room.Users.Single(u => u.UserID == userId).State = state;

                InvokeRoomChanged();
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

        protected void InvokeRoomChanged() => RoomChanged?.Invoke();

        protected async Task PopulateUser(MultiplayerRoomUser multiplayerUser) => multiplayerUser.User ??= await userLookupCache.GetUserAsync(multiplayerUser.UserID);
    }
}
