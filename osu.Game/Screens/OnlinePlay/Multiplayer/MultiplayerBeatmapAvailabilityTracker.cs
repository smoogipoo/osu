// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Extensions.ObjectExtensions;
using osu.Game.Online.Multiplayer;
using osu.Game.Online.Rooms;

namespace osu.Game.Screens.OnlinePlay.Multiplayer
{
    public partial class MultiplayerBeatmapAvailabilityTracker : OnlinePlayBeatmapAvailabilityTracker
    {
        [Resolved]
        private MultiplayerClient client { get; set; } = null!;

        protected override void LoadComplete()
        {
            base.LoadComplete();

            client.RoomUpdated += onRoomUpdated;
            onRoomUpdated();
        }

        private void onRoomUpdated()
        {
            if (client.Room == null)
                return;

            MultiplayerPlaylistItem? currentItem = client.Room.CurrentPlaylistItem;
            PlaylistItem.Value = currentItem != null ? new PlaylistItem(currentItem) : null;
        }

        protected override void Dispose(bool isDisposing)
        {
            base.Dispose(isDisposing);

            if (client.IsNotNull())
                client.RoomUpdated -= onRoomUpdated;
        }
    }
}
