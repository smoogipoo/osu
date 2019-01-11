// Copyright (c) 2007-2019 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using osu.Framework.Allocation;
using osu.Framework.Configuration;
using osu.Game.Online;
using osu.Game.Online.API;
using osu.Game.Online.API.Requests;
using osu.Game.Online.Multiplayer;
using osu.Game.Screens.Multi.Lounge.Components;

namespace osu.Game.Screens.Multi
{
    public class RoomPollingComponent : PollingComponent
    {
        public Action<IEnumerable<Room>> RoomsReceived;

        public readonly IBindable<PrimaryFilter> Filter = new Bindable<PrimaryFilter>();

        [Resolved]
        private APIAccess api { get; set; }

        public RoomPollingComponent()
        {
            Filter.BindValueChanged(_ => PollImmediately());
        }

        private GetRoomsRequest pollReq;

        protected override Task Poll()
        {
            if (!api.IsLoggedIn)
                return base.Poll();

            var tcs = new TaskCompletionSource<bool>();

            pollReq?.Cancel();
            pollReq = new GetRoomsRequest(Filter.Value);

            pollReq.Success += result =>
            {
                RoomsReceived?.Invoke(result);
                tcs.SetResult(true);
            };

            pollReq.Failure += _ => tcs.SetResult(false);

            api.Queue(pollReq);

            return tcs.Task;
        }
    }
}
