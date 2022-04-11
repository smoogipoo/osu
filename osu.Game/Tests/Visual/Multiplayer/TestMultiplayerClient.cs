// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable enable

using System.Collections.Generic;
using System.Threading.Tasks;
using osu.Framework.Bindables;
using osu.Game.Online.API;
using osu.Game.Online.Multiplayer;
using osu.Game.Online.Rooms;

namespace osu.Game.Tests.Visual.Multiplayer
{
    /// <summary>
    /// A <see cref="MultiplayerClient"/> for use in multiplayer test scenes. Should generally not be used by itself outside of a <see cref="MultiplayerTestScene"/>.
    /// </summary>
    public class TestMultiplayerClient : MultiplayerClient
    {
        public override IBindable<bool> IsConnected => isConnected;
        private readonly Bindable<bool> isConnected = new Bindable<bool>(true);

        public new Room? APIRoom => base.APIRoom;

        public bool RoomJoined { get; private set; }
        public TestMultiplayerServer Server { get; set; } = null!;

        public void Connect() => isConnected.Value = true;

        public void Disconnect() => isConnected.Value = false;

        protected override Task<MultiplayerRoom> JoinRoom(long roomId, string? password = null)
        {
            return Server.JoinRoomWithPassword(roomId, password);
        }

        protected override Task LeaveRoomInternal()
        {
            return Server.LeaveRoom();
        }

        public override Task TransferHost(int userId)
        {
            return Server.TransferHost(userId);
        }

        public override Task KickUser(int userId)
        {
            return Server.KickUser(userId);
        }

        public override Task ChangeSettings(MultiplayerRoomSettings settings)
        {
            return Server.ChangeSettings(settings);
        }

        public override Task ChangeState(MultiplayerUserState newState)
        {
            return Server.ChangeState(newState);
        }

        public override Task ChangeBeatmapAvailability(BeatmapAvailability newBeatmapAvailability)
        {
            return Server.ChangeBeatmapAvailability(newBeatmapAvailability);
        }

        public override Task ChangeUserMods(IEnumerable<APIMod> newMods)
        {
            return Server.ChangeUserMods(newMods);
        }

        public override Task SendMatchRequest(MatchUserRequest request)
        {
            return Server.SendMatchRequest(request);
        }

        public override Task StartMatch()
        {
            return Server.StartMatch();
        }

        public override Task AbortGameplay()
        {
            return Server.AbortGameplay();
        }

        public override Task AddPlaylistItem(MultiplayerPlaylistItem item)
        {
            return Server.AddPlaylistItem(item);
        }

        public override Task EditPlaylistItem(MultiplayerPlaylistItem item)
        {
            return Server.EditPlaylistItem(item);
        }

        public override Task RemovePlaylistItem(long playlistItemId)
        {
            return Server.RemovePlaylistItem(playlistItemId);
        }
    }
}
