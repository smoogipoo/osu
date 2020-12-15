// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Game.Online.Multiplayer;
using osu.Game.Screens.Multi.Lounge;

namespace osu.Game.Screens.Multi.Realtime
{
    public class RealtimeMultiplayer : Multiplayer
    {
        public override string Title => "Multiplayer";

        [Cached(typeof(RealtimeRoomManager))]
        protected override IRoomManager RoomManager => base.RoomManager;

        protected override IRoomManager CreateRoomManager() => new RealtimeRoomManager();

        protected override LoungeSubScreen CreateLounge() => new RealtimeLoungeSubScreen();

        protected override CreateRoomButton CreateCreateRoomButton() => new CreateMatchButton();

        protected override Room CreateNewRoom()
        {
            var room = base.CreateNewRoom();
            room.Category.Value = RoomCategory.Realtime;
            return room;
        }

        protected class CreateMatchButton : CreateRoomButton
        {
            private readonly IBindable<bool> isConnected = new Bindable<bool>();

            [Resolved]
            private StatefulMultiplayerClient client { get; set; }

            public CreateMatchButton()
            {
                Text = "Create match";
            }

            protected override void LoadComplete()
            {
                base.LoadComplete();

                isConnected.BindTo(client.IsConnected);
                isConnected.BindValueChanged(c => Enabled.Value = c.NewValue, true);
            }
        }
    }
}
