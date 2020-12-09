// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Game.Online;
using osu.Game.Online.API;
using osu.Game.Online.Multiplayer;
using osu.Game.Screens.Multi.Lounge.Components;

namespace osu.Game.Screens.Multi.Components
{
    public class ListingPollingComponent : PollingComponent
    {
        public Action<List<Room>> RoomsReceived;

        public readonly Bindable<bool> InitialRoomsReceived = new Bindable<bool>();

        [Resolved]
        private IAPIProvider api { get; set; }

        [Resolved]
        private Bindable<FilterCriteria> currentFilter { get; set; }

        [BackgroundDependencyLoader]
        private void load()
        {
            currentFilter.BindValueChanged(_ =>
            {
                InitialRoomsReceived.Value = false;

                if (IsLoaded)
                    PollImmediately();
            });
        }

        private GetRoomsRequest pollReq;

        protected override Task Poll()
        {
            if (!api.IsLoggedIn)
                return base.Poll();

            var tcs = new TaskCompletionSource<bool>();

            pollReq?.Cancel();
            pollReq = new GetRoomsRequest(currentFilter.Value.Status, currentFilter.Value.Category);

            pollReq.Success += result =>
            {
                InitialRoomsReceived.Value = true;
                RoomsReceived?.Invoke(result);
                tcs.SetResult(true);
            };

            pollReq.Failure += _ => tcs.SetResult(false);

            api.Queue(pollReq);

            return tcs.Task;
        }
    }
}
