// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using Newtonsoft.Json;
using osu.Framework.IO.Network;
using osu.Game.Online.API;

namespace osu.Game.Screens.AX
{
    public class PostMessageRequest : APIRequest<PostMessageResponse>
    {
        private readonly string message;

        public PostMessageRequest(string message)
        {
            this.message = message;
        }

        protected override WebRequest CreateWebRequest()
        {
            var req = base.CreateWebRequest();
            req.Method = HttpMethod.POST;
            req.AddParameter("message", message);
            return req;
        }

        protected override string Target => $@"guestbook";
    }

    public class PostMessageResponse
    {
        [JsonProperty("entry")]
        public MessageEntry Entry;

        [JsonProperty("first_visit")]
        public bool FirstVisit;
    }
}
