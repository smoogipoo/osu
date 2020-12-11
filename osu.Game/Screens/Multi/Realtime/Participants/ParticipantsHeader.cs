// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Game.Screens.Multi.Components;

namespace osu.Game.Screens.Multi.Realtime.Participants
{
    public class ParticipantsHeader : OverlinedHeader
    {
        [Resolved]
        private RealtimeRoomManager roomManager { get; set; }

        public ParticipantsHeader()
            : base("Participants")
        {
        }

        protected override void Update()
        {
            base.Update();

            var room = roomManager.Client.Room;
            if (room == null)
                return;

            Details.Value = room.Users.Count.ToString();
        }
    }
}
