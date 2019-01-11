// Copyright (c) 2007-2019 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using System.Collections.Generic;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Configuration;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Logging;
using osu.Game.Beatmaps;
using osu.Game.Online.API;
using osu.Game.Online.API.Requests;
using osu.Game.Online.Multiplayer;
using osu.Game.Rulesets;
using osu.Game.Screens.Multi.Lounge.Components;

namespace osu.Game.Screens.Multi
{
    public class RoomManagerNew : CachedModelContainer<Room>, IRoomManager
    {
        public event Action RoomsUpdated;

        private readonly BindableList<Room> rooms = new BindableList<Room>();
        public IBindableList<Room> Rooms => rooms;

        [Resolved]
        private APIAccess api { get; set; }

        [Resolved]
        private RulesetStore rulesets { get; set; }

        [Resolved]
        private BeatmapManager beatmaps { get; set; }

        private readonly Bindable<PrimaryFilter> filter = new Bindable<PrimaryFilter>();

        protected override Container<Drawable> Content => content;
        private readonly Container content;

        public RoomManagerNew()
        {
            RoomPollingComponent pollingComponent;

            InternalChildren = new Drawable[]
            {
                content = new Container { RelativeSizeAxes = Axes.Both },
                pollingComponent = new RoomPollingComponent { RoomsReceived = roomsReceived }
            };

            pollingComponent.Filter.BindTo(filter);
        }

        protected override void Dispose(bool isDisposing)
        {
            base.Dispose(isDisposing);
            PartRoom();
        }

        public void CreateRoom(Room room, Action<Room> onSuccess = null, Action<string> onError = null)
        {
            room.Host.Value = api.LocalUser;

            var req = new CreateRoomRequest(room);

            req.Success += result =>
            {
                update(room, result);
                addRoom(room);

                RoomsUpdated?.Invoke();

                onSuccess?.Invoke(room);
            };

            req.Failure += exception =>
            {
                if (req.Result != null)
                    onError?.Invoke(req.Result.Error);
                else
                    Logger.Log($"Failed to create the room: {exception}", level: LogLevel.Important);
            };

            api.Queue(req);
        }

        private JoinRoomRequest currentJoinRoomRequest;

        public void JoinRoom(Room room, Action<Room> onSuccess = null, Action<string> onError = null)
        {
            currentJoinRoomRequest?.Cancel();
            currentJoinRoomRequest = null;

            currentJoinRoomRequest = new JoinRoomRequest(room, api.LocalUser.Value);
            currentJoinRoomRequest.Success += () =>
            {
                Model = room;
                onSuccess?.Invoke(room);
            };

            currentJoinRoomRequest.Failure += exception =>
            {
                Logger.Log($"Failed to join room: {exception}", level: LogLevel.Important);
                onError?.Invoke(exception.ToString());
            };

            api.Queue(currentJoinRoomRequest);
        }

        public void PartRoom()
        {
            if (Model == null)
                return;

            api.Queue(new PartRoomRequest(Model, api.LocalUser.Value));

            Model = null;
        }

        public void Filter(FilterCriteria criteria) => filter.Value = criteria.PrimaryFilter;

        private void roomsReceived(IEnumerable<Room> result)
        {
            // Remove past matches
            foreach (var r in rooms.ToList())
            {
                if (result.All(e => e.RoomID.Value != r.RoomID.Value))
                    rooms.Remove(r);
            }

            int index = 0;
            foreach (var r in result)
            {
                r.Position = index++;

                update(r, r);
                addRoom(r);
            }

            RoomsUpdated?.Invoke();
        }

        /// <summary>
        /// Updates a local <see cref="Room"/> with a remote copy.
        /// </summary>
        /// <param name="local">The local <see cref="Room"/> to update.</param>
        /// <param name="remote">The remote <see cref="Room"/> to update with.</param>
        private void update(Room local, Room remote)
        {
            foreach (var pi in remote.Playlist)
                pi.MapObjects(beatmaps, rulesets);

            local.CopyFrom(remote);
        }

        /// <summary>
        /// Adds a <see cref="Room"/> to the list of available rooms.
        /// </summary>
        /// <param name="room">The <see cref="Room"/> to add.<</param>
        private void addRoom(Room room)
        {
            var existing = rooms.FirstOrDefault(e => e.RoomID.Value == room.RoomID.Value);
            if (existing == null)
                rooms.Add(room);
            else
                existing.CopyFrom(room);
        }
    }
}
