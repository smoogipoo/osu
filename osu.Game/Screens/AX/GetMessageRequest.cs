// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using Newtonsoft.Json;
using osu.Framework.IO.Network;
using osu.Game.Online.API;

namespace osu.Game.Screens.AX
{
    public class GetMessageRequest : APIRequest<GetMessageResponse>
    {
        protected override WebRequest CreateWebRequest()
        {
            var req = base.CreateWebRequest();
            return req;
        }

        protected override string Target => $@"guestbook";
    }

    public class GetMessageResponse
    {
        [JsonProperty(@"entry")]
        public MessageEntry Entry;
    }
}
