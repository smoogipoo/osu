// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Logging;
using osu.Framework.Screens;
using osu.Game.Screens.Multi.Lounge;

namespace osu.Game.Screens.Multi.Timeshift
{
    public class TimeshiftMultiplayer : Multiplayer
    {
        public override string Title => "Playlists";

        protected override void UpdatePollingRate(bool idle)
        {
            base.UpdatePollingRate(idle);

            var manager = (TimeshiftRoomManager)RoomManager;

            if (!this.IsCurrentScreen())
                manager.TimeBetweenSelectionPolls = 0;
            else
            {
                switch (ScreenStack.CurrentScreen)
                {
                    case LoungeSubScreen _:
                        manager.TimeBetweenSelectionPolls = idle ? 120000 : 15000;
                        break;

                    case RoomSubScreen _:
                        manager.TimeBetweenSelectionPolls = idle ? 30000 : 5000;
                        break;

                    default:
                        manager.TimeBetweenSelectionPolls = 0;
                        break;
                }
            }

            Logger.Log($"Polling adjusted (selection: {manager.TimeBetweenSelectionPolls})");
        }

        protected override IRoomManager CreateRoomManager() => new TimeshiftRoomManager();

        protected override LoungeSubScreen CreateLounge() => new TimeshiftLoungeSubScreen();
    }
}
