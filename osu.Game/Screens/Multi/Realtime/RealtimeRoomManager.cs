// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Logging;
using osu.Game.Beatmaps;
using osu.Game.Online.API;
using osu.Game.Online.Multiplayer;
using osu.Game.Rulesets;
using osu.Game.Screens.Multi.Components;

namespace osu.Game.Screens.Multi.Realtime
{
    public class RealtimeRoomManager : CompositeDrawable, IRoomManager
    {
        public event Action RoomsUpdated;

        private readonly BindableList<Room> rooms = new BindableList<Room>();

        public Bindable<bool> InitialRoomsReceived { get; } = new Bindable<bool>();

        public IBindableList<Room> Rooms => rooms;

        private double timeBetweenListingPolls;

        public double TimeBetweenListingPolls
        {
            get => timeBetweenListingPolls;
            set
            {
                timeBetweenListingPolls = value;

                if (listingPollingComponent != null)
                    listingPollingComponent.TimeBetweenPolls = value;
            }
        }

        private readonly IBindable<bool> isConnected = new Bindable<bool>();

        [Resolved]
        private RulesetStore rulesets { get; set; }

        [Resolved]
        private BeatmapManager beatmaps { get; set; }

        [Resolved]
        private IAPIProvider api { get; set; }

        [Resolved]
        private Bindable<Room> selectedRoom { get; set; }

        [Resolved]
        private StatefulMultiplayerClient multiplayerClient { get; set; }

        private readonly Container pollingComponents;
        private ListingPollingComponent listingPollingComponent;
        private JoinRoomRequest currentJoinRoomRequest;
        private Room joinedRoom;

        public RealtimeRoomManager()
        {
            RelativeSizeAxes = Axes.Both;

            InternalChildren = new[]
            {
                pollingComponents = new Container { RelativeSizeAxes = Axes.Both },
            };
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            isConnected.BindTo(multiplayerClient.IsConnected);
            isConnected.BindValueChanged(onIsConnectedChanged, true);
        }

        private void onIsConnectedChanged(ValueChangedEvent<bool> connected) => Schedule(() =>
        {
            if (connected.NewValue)
            {
                rooms.Clear();
                InitialRoomsReceived.Value = false;
                pollingComponents.Clear();
            }
            else
            {
                pollingComponents.Add(listingPollingComponent = new ListingPollingComponent
                {
                    TimeBetweenPolls = TimeBetweenListingPolls,
                    InitialRoomsReceived = { BindTarget = InitialRoomsReceived },
                    RoomsReceived = onListingReceived
                });
            }
        });

        public void CreateRoom(Room room, Action<Room> onSuccess, Action<string> onError)
        {
            room.Host.Value = api.LocalUser.Value;
            room.Category.Value = RoomCategory.Realtime;

            var req = new CreateRoomRequest(room);

            req.Success += result =>
            {
                joinedRoom = room;

                update(room, result);
                addRoom(room);

                RoomsUpdated?.Invoke();

                joinMultiplayerRoom(room, onSuccess, onException);
            };

            req.Failure += exception =>
            {
                if (req.Result != null)
                    onError?.Invoke(req.Result.Error);
                else
                    onException(exception);
            };

            api.Queue(req);

            void onException(Exception ex) => Logger.Log($"Failed to create the room: {ex}", level: LogLevel.Important);
        }

        public void JoinRoom(Room room, Action<Room> onSuccess, Action<string> onError)
        {
            // The API is joined first to join chat channels/etc, with the multiplayer room joined afterwards.
            currentJoinRoomRequest?.Cancel();
            currentJoinRoomRequest = new JoinRoomRequest(room);

            currentJoinRoomRequest.Success += () =>
            {
                joinedRoom = room;
                joinMultiplayerRoom(room, onSuccess, onException);
            };

            currentJoinRoomRequest.Failure += onException;

            api.Queue(currentJoinRoomRequest);

            void onException(Exception ex)
            {
                if (!(ex is OperationCanceledException))
                    Logger.Log($"Failed to join room: {ex}", level: LogLevel.Important);
                onError?.Invoke(ex.ToString());
            }
        }

        public void PartRoom()
        {
            currentJoinRoomRequest?.Cancel();

            if (joinedRoom == null)
                return;

            api.Queue(new PartRoomRequest(joinedRoom));
            multiplayerClient.LeaveRoom().Wait();

            // Todo: This is not the way to do this. Basically when we're the only participant and the room closes, there's no way to know if this is actually the case.
            rooms.Remove(joinedRoom);

            // This is delayed one frame because upon exiting the match subscreen, multiplayer sets TimeBetweenPolls and messes with the polling.
            // Todo: Have I said this is not the way to do this?
            Schedule(() =>
            {
                listingPollingComponent?.PollImmediately();
            });

            joinedRoom = null;
        }

        private void joinMultiplayerRoom(Room room, Action<Room> onSuccess, Action<Exception> onError)
        {
            Debug.Assert(room.RoomID.Value != null);

            try
            {
                multiplayerClient.JoinRoom(room).Wait();
                onSuccess?.Invoke(room);
            }
            catch (Exception ex)
            {
                PartRoom();
                onError?.Invoke(ex);
            }
        }

        private readonly HashSet<int> ignoredRooms = new HashSet<int>();

        /// <summary>
        /// Invoked when the listing of all <see cref="Room"/>s is received from the server.
        /// </summary>
        /// <param name="listing">The listing.</param>
        private void onListingReceived(List<Room> listing)
        {
            if (!isConnected.Value)
                return;

            // Remove past matches
            foreach (var r in rooms.ToList())
            {
                if (listing.All(e => e.RoomID.Value != r.RoomID.Value))
                    rooms.Remove(r);
            }

            for (int i = 0; i < listing.Count; i++)
            {
                if (selectedRoom.Value?.RoomID?.Value == listing[i].RoomID.Value)
                {
                    // The listing request contains less data than the selection request, so data from the selection request is always preferred while the room is selected.
                    continue;
                }

                var room = listing[i];

                Debug.Assert(room.RoomID.Value != null);

                if (ignoredRooms.Contains(room.RoomID.Value.Value))
                    continue;

                room.Position.Value = i;

                try
                {
                    update(room, room);
                    addRoom(room);
                }
                catch (Exception ex)
                {
                    Logger.Error(ex, $"Failed to update room: {room.Name.Value}.");

                    ignoredRooms.Add(room.RoomID.Value.Value);
                    rooms.Remove(room);
                }
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
        /// <param name="room">The <see cref="Room"/> to add.</param>
        private void addRoom(Room room)
        {
            var existing = rooms.FirstOrDefault(e => e.RoomID.Value == room.RoomID.Value);
            if (existing == null)
                rooms.Add(room);
            else
                existing.CopyFrom(room);
        }

        protected override void Dispose(bool isDisposing)
        {
            base.Dispose(isDisposing);
            PartRoom();
        }
    }
}
