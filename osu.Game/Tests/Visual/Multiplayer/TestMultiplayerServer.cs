// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable enable

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using osu.Game.Online.API;
using osu.Game.Online.API.Requests.Responses;
using osu.Game.Online.Multiplayer;
using osu.Game.Online.Multiplayer.Countdown;
using osu.Game.Online.Multiplayer.MatchTypes.TeamVersus;
using osu.Game.Online.Rooms;
using osu.Game.Rulesets.Mods;
using osu.Game.Tests.Visual.OnlinePlay;

namespace osu.Game.Tests.Visual.Multiplayer
{
    public class TestMultiplayerServer : IMultiplayerRoomServer, IMultiplayerLoungeServer
    {
        public Action<MultiplayerRoom>? RoomSetupAction;

        private readonly TestRoomRequestsHandler requestsHandler;
        private readonly IAPIProvider api;
        private readonly IMultiplayerClient client;

        private Room? apiRoom;
        private MultiplayerRoom? multiplayerRoom;
        private long lastPlaylistItemId;

        private MultiplayerRoomUser getLocalUser() => multiplayerRoom!.Users.Single(u => u.UserID == api.LocalUser.Value.Id);

        public TestMultiplayerServer(TestRoomRequestsHandler requestsHandler, IAPIProvider api, IMultiplayerClient client)
        {
            this.requestsHandler = requestsHandler;
            this.api = api;
            this.client = client;
        }

        #region Test helpers

        public Task AddUser(int userId, bool markAsPlaying = false) => addUser(userId);

        public Task TestAddUnresolvedUser() => addUser(TestUserLookupCache.UNRESOLVED_USER_ID);

        private async Task addUser(int userId)
        {
            Debug.Assert(multiplayerRoom != null);

            var user = new MultiplayerRoomUser(userId);

            multiplayerRoom.Users.Add(user);
            await client.UserJoined(user);

            switch (multiplayerRoom.MatchState)
            {
                case TeamVersusRoomState teamVersus:
                    // simulate the server's automatic assignment of users to teams on join.
                    // the "best" team is the one with the least users on it.
                    int bestTeam = teamVersus.Teams
                                             .Select(team => (teamID: team.ID, userCount: multiplayerRoom.Users.Count(u => (u.MatchState as TeamVersusUserState)?.TeamID == team.ID)))
                                             .OrderBy(pair => pair.userCount)
                                             .First().teamID;

                    await client.MatchUserStateChanged(user.UserID, new TeamVersusUserState { TeamID = bestTeam });
                    break;
            }
        }

        public async Task RemoveUser(int userId, bool wasKick = false)
        {
            Debug.Assert(multiplayerRoom != null);

            var user = multiplayerRoom.Users.Single(u => u.UserID == userId);
            multiplayerRoom.Users.Remove(user);

            if (multiplayerRoom.Users.Count == 0)
            {
                multiplayerRoom = null;
                apiRoom = null;
                return;
            }

            await updateRoomStateIfRequired();

            if (multiplayerRoom.Host?.Equals(user) == true)
                await ((IMultiplayerRoomServer)this).TransferHost(multiplayerRoom.Users.First().UserID);

            if (wasKick)
                await client.UserKicked(user);
            else
            {
                if (user.UserID != api.LocalUser.Value.Id)
                    await client.UserLeft(user);
            }
        }

        public async Task ChangeUserState(int userId, MultiplayerUserState newState)
        {
            Debug.Assert(multiplayerRoom != null);

            var user = multiplayerRoom.Users.Single(u => u.UserID == userId);

            await changeAndBroadcastUserState(user, newState);

            // Signal newly-spectating users to load gameplay if currently in the middle of play.
            if (userId == getLocalUser().UserID
                && newState == MultiplayerUserState.Spectating
                && (multiplayerRoom.State == MultiplayerRoomState.WaitingForLoad || multiplayerRoom.State == MultiplayerRoomState.Playing))
            {
                await client.LoadRequested();
            }

            await updateRoomStateIfRequired();

            // Todo: This should probably do the whole LoadRequested() stuff for the local user...
        }

        public async Task ChangeUserBeatmapAvailability(int userId, BeatmapAvailability newBeatmapAvailability)
        {
            Debug.Assert(multiplayerRoom != null);

            var user = multiplayerRoom.Users.Single(u => u.UserID == userId);
            user.BeatmapAvailability = clone(newBeatmapAvailability);

            await client.UserBeatmapAvailabilityChanged(userId, clone(newBeatmapAvailability));
        }

        public async Task AddUserPlaylistItem(int userId, MultiplayerPlaylistItem item)
        {
            Debug.Assert(multiplayerRoom != null);

            if (multiplayerRoom.Settings.QueueMode == QueueMode.HostOnly && multiplayerRoom.Host?.UserID != getLocalUser().UserID)
                throw new InvalidOperationException("Local user is not the room host.");

            item.OwnerID = userId;

            await addItem(item).ConfigureAwait(false);
            await updateRoomStateIfRequired();
        }

        public async Task EditUserPlaylistItem(int userId, MultiplayerPlaylistItem item)
        {
            Debug.Assert(apiRoom != null);
            Debug.Assert(multiplayerRoom != null);

            item.OwnerID = userId;

            var existingItem = multiplayerRoom.Playlist.SingleOrDefault(i => i.ID == item.ID);

            if (existingItem == null)
                throw new InvalidOperationException("Attempted to change an item that doesn't exist.");

            if (existingItem.OwnerID != userId && multiplayerRoom.Host?.UserID != getLocalUser().UserID)
                throw new InvalidOperationException("Attempted to change an item which is not owned by the user.");

            if (existingItem.Expired)
                throw new InvalidOperationException("Attempted to change an item which has already been played.");

            // Ensure the playlist order doesn't change.
            item.PlaylistOrder = existingItem.PlaylistOrder;

            multiplayerRoom.Playlist[multiplayerRoom.Playlist.IndexOf(existingItem)] = item;
            apiRoom.Playlist[apiRoom.Playlist.IndexOf(apiRoom.Playlist.Single(i => i.ID == item.ID))] = new PlaylistItem(item);

            await client.PlaylistItemChanged(clone(item)).ConfigureAwait(false);
        }

        public async Task RemoveUserPlaylistItem(int userId, long playlistItemId)
        {
            Debug.Assert(apiRoom != null);
            Debug.Assert(multiplayerRoom != null);

            var item = multiplayerRoom.Playlist.Single(i => i.ID == playlistItemId);

            if (item == null)
                throw new InvalidOperationException("Item does not exist in the room.");

            if (item.ID == apiRoom.Playlist.GetCurrentItem()!.ID)
                throw new InvalidOperationException("The room's current item cannot be removed.");

            if (item.OwnerID != userId)
                throw new InvalidOperationException("Attempted to remove an item which is not owned by the user.");

            if (item.Expired)
                throw new InvalidOperationException("Attempted to remove an item which has already been played.");

            multiplayerRoom.Playlist.Remove(item);
            apiRoom.Playlist.RemoveAll(i => i.ID == item.ID);

            await client.PlaylistItemRemoved(playlistItemId).ConfigureAwait(false);
            await updateCurrentItem().ConfigureAwait(false);
            await updateRoomStateIfRequired();
        }

        public Task ChangeUserMods(int userId, IEnumerable<Mod> newMods) => ChangeUserMods(userId, newMods.Select(m => new APIMod(m)).ToList());

        public async Task ChangeUserMods(int userId, IEnumerable<APIMod> newMods)
        {
            Debug.Assert(multiplayerRoom != null);

            var user = multiplayerRoom.Users.Single(u => u.UserID == userId);
            user.Mods = newMods.ToList();

            await client.UserModsChanged(userId, clone(user.Mods));
        }

        /// <summary>
        /// Skips to the end of the currently-running countdown, if one is running,
        /// and runs the callback (e.g. to start the match) as soon as possible unless the countdown has been cancelled.
        /// </summary>
        public void SkipToEndOfCountdown() => countdownSkipSource?.Cancel();

        public Task FinishCurrentItem() => finishCurrentItem();

        public Task ChangeRoomState(MultiplayerRoomState newState) => changeRoomState(newState);

        #endregion

        #region IMultiplayerRoomServer / IMultiplayerLoungeServer implementations

        public Task<MultiplayerRoom> JoinRoom(long roomId) => ((IMultiplayerLoungeServer)this).JoinRoomWithPassword(roomId, string.Empty);

        public async Task<MultiplayerRoom> JoinRoomWithPassword(long roomId, string? password)
        {
            apiRoom = requestsHandler.ServerSideRooms.Single(r => r.RoomID.Value == roomId);

            if (password != apiRoom.Password.Value)
                throw new InvalidOperationException("Invalid password.");

            lastPlaylistItemId = apiRoom.Playlist.Max(item => item.ID);

            var localUser = new MultiplayerRoomUser(api.LocalUser.Value.Id) { User = api.LocalUser.Value };

            multiplayerRoom = new MultiplayerRoom(roomId)
            {
                Settings =
                {
                    Name = apiRoom.Name.Value,
                    MatchType = apiRoom.Type.Value,
                    Password = password,
                    QueueMode = apiRoom.QueueMode.Value,
                    AutoStartDuration = apiRoom.AutoStartDuration.Value,
                    PlaylistItemId = apiRoom.Playlist.GetCurrentItem()?.ID ?? 0
                },
                Playlist = apiRoom.Playlist.Select(i => new MultiplayerPlaylistItem(i)).ToList(),
                Users = { localUser },
                Host = localUser
            };

            await updatePlaylistOrder().ConfigureAwait(false);
            await updateCurrentItem(false).ConfigureAwait(false);

            RoomSetupAction?.Invoke(multiplayerRoom);
            RoomSetupAction = null;

            var settings = new JsonSerializerSettings
            {
                TypeNameHandling = TypeNameHandling.Auto
            };

            // Return an isolated instance to clients.
            return JsonConvert.DeserializeObject<MultiplayerRoom>(JsonConvert.SerializeObject(multiplayerRoom, settings), settings)
                   ?? throw new InvalidOperationException();
        }

        public Task LeaveRoom()
        {
            Debug.Assert(multiplayerRoom != null);

            multiplayerRoom.Users.Remove(getLocalUser());

            apiRoom = null;
            multiplayerRoom = null;
            lastPlaylistItemId = 0;

            return Task.CompletedTask;
        }

        public async Task TransferHost(int userId)
        {
            Debug.Assert(multiplayerRoom != null);
            Debug.Assert(apiRoom != null);

            MultiplayerRoomUser newHost = multiplayerRoom.Users.Single(u => u.UserID == userId);

            multiplayerRoom.Host = newHost;
            apiRoom.Host.Value = new APIUser { Id = userId };

            await client.HostChanged(userId);
        }

        public Task KickUser(int userId) => RemoveUser(userId, true);

        public async Task ChangeSettings(MultiplayerRoomSettings settings)
        {
            Debug.Assert(apiRoom != null);
            Debug.Assert(multiplayerRoom != null);

            // Server is authoritative for the time being.
            settings.PlaylistItemId = multiplayerRoom.Settings.PlaylistItemId;

            multiplayerRoom.Settings = settings;

            await changeQueueMode(settings.QueueMode).ConfigureAwait(false);
            await changeMatchType(settings.MatchType).ConfigureAwait(false);

            foreach (var user in multiplayerRoom.Users.Where(u => u.State == MultiplayerUserState.Ready))
                await changeAndBroadcastUserState(user, MultiplayerUserState.Idle);

            await client.SettingsChanged(settings).ConfigureAwait(false);

            await updateRoomStateIfRequired();
        }

        public Task ChangeState(MultiplayerUserState newState) => ChangeUserState(getLocalUser().UserID, newState);

        public Task ChangeBeatmapAvailability(BeatmapAvailability newBeatmapAvailability) => ChangeUserBeatmapAvailability(getLocalUser().UserID, newBeatmapAvailability);

        public Task ChangeUserMods(IEnumerable<APIMod> newMods) => ChangeUserMods(getLocalUser().UserID, newMods);

        public async Task SendMatchRequest(MatchUserRequest request)
        {
            Debug.Assert(multiplayerRoom != null);

            switch (request)
            {
                case StartMatchCountdownRequest countdown:
                    startCountdown(new MatchStartCountdown { TimeRemaining = countdown.Duration }, ((IMultiplayerRoomServer)this).StartMatch);
                    break;

                case StopCountdownRequest _:
                    stopCountdown();
                    break;

                case ChangeTeamRequest changeTeam:
                    TeamVersusRoomState roomState = (TeamVersusRoomState)multiplayerRoom.MatchState!;
                    TeamVersusUserState userState = (TeamVersusUserState)getLocalUser().MatchState!;

                    var targetTeam = roomState.Teams.FirstOrDefault(t => t.ID == changeTeam.TeamID);

                    if (targetTeam != null)
                    {
                        userState.TeamID = targetTeam.ID;
                        await client.MatchUserStateChanged(getLocalUser().UserID, clone(userState)).ConfigureAwait(false);
                    }

                    break;
            }
        }

        public async Task StartMatch()
        {
            Debug.Assert(multiplayerRoom != null);

            var readyUsers = multiplayerRoom.Users.Where(u => u.State == MultiplayerUserState.Ready).ToArray();

            // If no users are ready, skip the current item in the queue.
            if (readyUsers.Length == 0)
            {
                await finishCurrentItem();
                return;
            }

            foreach (var u in readyUsers)
                await changeAndBroadcastUserState(u, MultiplayerUserState.WaitingForLoad);

            await changeRoomState(MultiplayerRoomState.WaitingForLoad);

            await client.LoadRequested();
        }

        public async Task AbortGameplay()
        {
            Debug.Assert(multiplayerRoom != null);

            await changeAndBroadcastUserState(getLocalUser(), MultiplayerUserState.Idle);
            await updateRoomStateIfRequired();
        }

        public Task AddPlaylistItem(MultiplayerPlaylistItem item) => AddUserPlaylistItem(getLocalUser().UserID, item);

        public Task EditPlaylistItem(MultiplayerPlaylistItem item) => EditUserPlaylistItem(getLocalUser().UserID, item);

        public Task RemovePlaylistItem(long playlistItemId) => RemoveUserPlaylistItem(getLocalUser().UserID, playlistItemId);

        private CancellationTokenSource? countdownSkipSource;
        private CancellationTokenSource? countdownStopSource;
        private Task countdownTask = Task.CompletedTask;

        private void startCountdown(MultiplayerCountdown countdown, Func<Task> continuation)
        {
            Debug.Assert(multiplayerRoom != null);

            stopCountdown();

            // Note that this will leak CTSs, however this is a test method and we haven't noticed foregoing disposal of non-linked CTSs to be detrimental.
            // If necessary, this can be moved into the final schedule below, and the class-level fields be nulled out accordingly.
            var stopSource = countdownStopSource = new CancellationTokenSource();
            var skipSource = countdownSkipSource = new CancellationTokenSource();

            Task lastCountdownTask = countdownTask;
            countdownTask = start();

            async Task start()
            {
                try
                {
                    await lastCountdownTask;
                }
                catch
                {
                }

                if (stopSource.IsCancellationRequested)
                    return;

                multiplayerRoom.Countdown = countdown;
                await client.MatchEvent(clone(new CountdownChangedEvent { Countdown = countdown }));

                try
                {
                    using (var cancellationSource = CancellationTokenSource.CreateLinkedTokenSource(stopSource.Token, skipSource.Token))
                        await Task.Delay(countdown.TimeRemaining, cancellationSource.Token).ConfigureAwait(false);
                }
                catch (OperationCanceledException)
                {
                    // Clients need to be notified of cancellations in the following code.
                }

                if (multiplayerRoom.Countdown != countdown)
                    return;

                multiplayerRoom.Countdown = null;
                await client.MatchEvent(clone(new CountdownChangedEvent { Countdown = null }));

                if (stopSource.IsCancellationRequested)
                    return;

                await continuation();
            }
        }

        private void stopCountdown() => countdownStopSource?.Cancel();

        private async Task updateRoomStateIfRequired()
        {
            Debug.Assert(multiplayerRoom != null);
            Debug.Assert(apiRoom != null);

            //check whether a room state change is required.
            switch (multiplayerRoom.State)
            {
                case MultiplayerRoomState.Open:
                    if (multiplayerRoom.Settings.AutoStartEnabled)
                    {
                        bool shouldHaveCountdown = !apiRoom.Playlist.GetCurrentItem()!.Expired && multiplayerRoom.Users.Any(u => u.State == MultiplayerUserState.Ready);

                        if (shouldHaveCountdown && multiplayerRoom.Countdown == null)
                            startCountdown(new MatchStartCountdown { TimeRemaining = multiplayerRoom.Settings.AutoStartDuration }, ((IMultiplayerRoomServer)this).StartMatch);
                    }

                    break;

                case MultiplayerRoomState.WaitingForLoad:
                    if (multiplayerRoom.Users.All(u => u.State != MultiplayerUserState.WaitingForLoad))
                    {
                        var loadedUsers = multiplayerRoom.Users.Where(u => u.State == MultiplayerUserState.Loaded).ToArray();

                        if (loadedUsers.Length == 0)
                        {
                            // all users have bailed from the load sequence. cancel the game start.
                            await changeRoomState(MultiplayerRoomState.Open);
                            return;
                        }

                        foreach (var u in loadedUsers)
                            await changeAndBroadcastUserState(u, MultiplayerUserState.Playing);

                        await client.MatchStarted();

                        await changeRoomState(MultiplayerRoomState.Playing);
                    }

                    break;

                case MultiplayerRoomState.Playing:
                    if (multiplayerRoom.Users.All(u => u.State != MultiplayerUserState.Playing))
                    {
                        foreach (var u in multiplayerRoom.Users.Where(u => u.State == MultiplayerUserState.FinishedPlay))
                            await changeAndBroadcastUserState(u, MultiplayerUserState.Results);

                        await changeRoomState(MultiplayerRoomState.Open);
                        await client.ResultsReady();

                        await finishCurrentItem();
                    }

                    break;
            }
        }

        private async Task changeRoomState(MultiplayerRoomState newState)
        {
            Debug.Assert(multiplayerRoom != null);

            multiplayerRoom.State = newState;

            await client.RoomStateChanged(newState);
        }

        private async Task changeAndBroadcastUserState(MultiplayerRoomUser user, MultiplayerUserState state)
        {
            Debug.Assert(multiplayerRoom != null);

            user.State = state;

            await client.UserStateChanged(user.UserID, state);
        }

        private async Task changeMatchType(MatchType type)
        {
            Debug.Assert(multiplayerRoom != null);

            switch (type)
            {
                case MatchType.HeadToHead:
                    multiplayerRoom.MatchState = null;
                    foreach (var user in multiplayerRoom.Users)
                        user.MatchState = null;
                    break;

                case MatchType.TeamVersus:
                    multiplayerRoom.MatchState = new TeamVersusRoomState();
                    foreach (var user in multiplayerRoom.Users)
                        user.MatchState = new TeamVersusUserState();
                    break;
            }

            await client.MatchRoomStateChanged(clone(multiplayerRoom.MatchState)).ConfigureAwait(false);

            foreach (var user in multiplayerRoom.Users)
                await client.MatchUserStateChanged(user.UserID, clone(user.MatchState)).ConfigureAwait(false);
        }

        private async Task changeQueueMode(QueueMode newMode)
        {
            Debug.Assert(apiRoom != null);
            Debug.Assert(multiplayerRoom != null);

            // When changing to host-only mode, ensure that at least one non-expired playlist item exists by duplicating the current item.
            if (newMode == QueueMode.HostOnly && multiplayerRoom.Playlist.All(item => item.Expired))
                await duplicateCurrentItem().ConfigureAwait(false);

            await updatePlaylistOrder().ConfigureAwait(false);
            await updateCurrentItem().ConfigureAwait(false);
        }

        private async Task finishCurrentItem()
        {
            Debug.Assert(apiRoom != null);
            Debug.Assert(multiplayerRoom != null);

            // Expire the current playlist item.
            var currentItem = multiplayerRoom.Playlist.Single(i => i.ID == apiRoom.Playlist.GetCurrentItem()!.ID);
            currentItem.Expired = true;
            currentItem.PlayedAt = DateTimeOffset.Now;

            await client.PlaylistItemChanged(clone(currentItem)).ConfigureAwait(false);
            await updatePlaylistOrder().ConfigureAwait(false);

            // In host-only mode, a duplicate playlist item will be used for the next round.
            if (multiplayerRoom.Settings.QueueMode == QueueMode.HostOnly && multiplayerRoom.Playlist.All(item => item.Expired))
                await duplicateCurrentItem().ConfigureAwait(false);

            await updateCurrentItem().ConfigureAwait(false);
        }

        private async Task duplicateCurrentItem()
        {
            Debug.Assert(multiplayerRoom != null);

            var currentItem = multiplayerRoom.Playlist.Single(i => i.ID == multiplayerRoom.Settings.PlaylistItemId);

            await addItem(new MultiplayerPlaylistItem
            {
                BeatmapID = currentItem.BeatmapID,
                BeatmapChecksum = currentItem.BeatmapChecksum,
                RulesetID = currentItem.RulesetID,
                RequiredMods = currentItem.RequiredMods,
                AllowedMods = currentItem.AllowedMods
            }).ConfigureAwait(false);
        }

        private async Task addItem(MultiplayerPlaylistItem item)
        {
            Debug.Assert(apiRoom != null);
            Debug.Assert(multiplayerRoom != null);

            item.ID = ++lastPlaylistItemId;

            multiplayerRoom.Playlist.Add(item);
            apiRoom.Playlist.Add(new PlaylistItem(item));

            await client.PlaylistItemAdded(clone(item)).ConfigureAwait(false);

            await updateCurrentItem();
            await updatePlaylistOrder().ConfigureAwait(false);
        }

        private async Task updatePlaylistOrder()
        {
            Debug.Assert(apiRoom != null);
            Debug.Assert(multiplayerRoom != null);

            List<MultiplayerPlaylistItem> orderedActiveItems;

            switch (multiplayerRoom.Settings.QueueMode)
            {
                default:
                    orderedActiveItems = multiplayerRoom.Playlist.Where(item => !item.Expired).OrderBy(item => item.ID).ToList();
                    break;

                case QueueMode.AllPlayersRoundRobin:
                    var itemsByPriority = new List<(MultiplayerPlaylistItem item, int priority)>();

                    // Assign a priority for items from each user, starting from 0 and increasing in order which the user added the items.
                    foreach (var group in multiplayerRoom.Playlist.Where(item => !item.Expired).OrderBy(item => item.ID).GroupBy(item => item.OwnerID))
                    {
                        int priority = 0;
                        itemsByPriority.AddRange(group.Select(item => (item, priority++)));
                    }

                    orderedActiveItems = itemsByPriority
                                         // Order by each user's priority.
                                         .OrderBy(i => i.priority)
                                         // Many users will have the same priority of items, so attempt to break the tie by maintaining previous ordering.
                                         // Suppose there are two users: User1 and User2. User1 adds two items, and then User2 adds a third. If the previous order is not maintained,
                                         // then after playing the first item by User1, their second item will become priority=0 and jump to the front of the queue (because it was added first).
                                         .ThenBy(i => i.item.PlaylistOrder)
                                         // If there are still ties (normally shouldn't happen), break ties by making items added earlier go first.
                                         // This could happen if e.g. the item orders get reset.
                                         .ThenBy(i => i.item.ID)
                                         .Select(i => i.item)
                                         .ToList();

                    break;
            }

            for (int i = 0; i < orderedActiveItems.Count; i++)
            {
                var item = orderedActiveItems[i];

                if (item.PlaylistOrder == i)
                    continue;

                item.PlaylistOrder = (ushort)i;
                apiRoom.Playlist.Single(apiItem => apiItem.ID == item.ID).PlaylistOrder = (ushort)i;

                await client.PlaylistItemChanged(clone(item)).ConfigureAwait(false);
            }
        }

        private async Task updateCurrentItem(bool notify = true)
        {
            Debug.Assert(apiRoom != null);
            Debug.Assert(multiplayerRoom != null);

            long nextItemId = apiRoom.Playlist.GetCurrentItem()!.ID;
            long lastItemId = multiplayerRoom.Settings.PlaylistItemId;

            if (nextItemId == lastItemId)
                return;

            multiplayerRoom.Settings.PlaylistItemId = nextItemId;

            if (notify)
                await client.SettingsChanged(clone(multiplayerRoom.Settings)).ConfigureAwait(false);
        }

        private T clone<T>(T obj)
        {
            if (obj == null)
                return (T)(object)null!;

            var settings = new JsonSerializerSettings
            {
                TypeNameHandling = TypeNameHandling.Auto
            };

            return JsonConvert.DeserializeObject<T>(JsonConvert.SerializeObject(obj, settings), settings)
                   ?? throw new InvalidOperationException();
        }

        #endregion
    }
}
