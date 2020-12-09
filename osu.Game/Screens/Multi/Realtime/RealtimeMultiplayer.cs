// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Graphics;
using osu.Game.Online.Multiplayer;
using osu.Game.Screens.Multi.Lounge;

namespace osu.Game.Screens.Multi.Realtime
{
    public class RealtimeMultiplayer : Multiplayer
    {
        public override string Title => "Multiplayer";

        protected override IRoomManager CreateRoomManager() => new RealtimeMatchManager();

        protected override LoungeSubScreen CreateLounge() => new RealtimeLoungeSubScreen();

        protected override CreateRoomButton CreateCreateRoomButton() => base.CreateCreateRoomButton().With(b => b.Text = "Create match");

        protected override Room CreateNewRoom()
        {
            var room = base.CreateNewRoom();
            room.Category.Value = RoomCategory.Realtime;
            return room;
        }
    }
}
