// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using Newtonsoft.Json;

namespace osu.Game.Screens.AX
{
    public class MessageEntry
    {
        [JsonProperty("user_id")]
        public int UserId;

        [JsonProperty("message")]
        public string Message;
    }
}
