// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using osu.Framework.Allocation;
using osu.Framework.Extensions.ObjectExtensions;
using osu.Game.Online.Multiplayer;
using osu.Game.Online.Rooms;
using osu.Game.Screens.OnlinePlay.Lounge;

namespace osu.Game.Screens.OnlinePlay.Multiplayer
{
    public class MultiplayerLoungeListingPoller : LoungeListingPoller
    {
        [Resolved]
        private MultiplayerClient client { get; set; } = null!;

        private readonly Dictionary<long, Room> rooms = new Dictionary<long, Room>();

        protected override void LoadComplete()
        {
            base.LoadComplete();

            client.JoinLounge();
            client.LoungeRoomAdded += onRoomAdded;
            client.LoungeRoomRemoved += onRoomRemoved;
        }

        private void onRoomAdded(MultiplayerRoom room)
        {
            rooms[room.RoomID] = new Room(room);
            RoomsReceived(rooms.Values.ToArray());
        }

        private void onRoomRemoved(long roomId)
        {
            if (rooms.Remove(roomId))
                RoomsReceived(rooms.Values.ToArray());
        }

        protected override Task Poll()
        {
            // We don't really poll.
            RoomsReceived(rooms.Values.ToArray());
            return Task.CompletedTask;
        }

        protected override void Dispose(bool isDisposing)
        {
            base.Dispose(isDisposing);

            if (client.IsNotNull())
            {
                client.JoinLounge();
                client.LoungeRoomAdded += onRoomAdded;
                client.LoungeRoomRemoved += onRoomRemoved;
            }
        }
    }
}
