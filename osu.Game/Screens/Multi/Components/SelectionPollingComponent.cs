// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Threading.Tasks;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Game.Online;
using osu.Game.Online.API;
using osu.Game.Online.Multiplayer;

namespace osu.Game.Screens.Multi.Components
{
    public class SelectionPollingComponent : PollingComponent
    {
        public Action<Room> RoomReceived;

        [Resolved]
        private IAPIProvider api { get; set; }

        [Resolved]
        private Bindable<Room> selectedRoom { get; set; }

        [BackgroundDependencyLoader]
        private void load()
        {
            selectedRoom.BindValueChanged(_ =>
            {
                if (IsLoaded)
                    PollImmediately();
            });
        }

        private GetRoomRequest pollReq;

        protected override Task Poll()
        {
            if (!api.IsLoggedIn)
                return base.Poll();

            if (selectedRoom.Value?.RoomID.Value == null)
                return base.Poll();

            var tcs = new TaskCompletionSource<bool>();

            pollReq?.Cancel();
            pollReq = new GetRoomRequest(selectedRoom.Value.RoomID.Value.Value);

            pollReq.Success += result =>
            {
                RoomReceived?.Invoke(result);
                tcs.SetResult(true);
            };

            pollReq.Failure += _ => tcs.SetResult(false);

            api.Queue(pollReq);

            return tcs.Task;
        }
    }
}
