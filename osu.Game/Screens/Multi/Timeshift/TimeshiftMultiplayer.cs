// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Game.Screens.Multi.Lounge;

namespace osu.Game.Screens.Multi.Timeshift
{
    public class TimeshiftMultiplayer : Multiplayer
    {
        protected override LoungeSubScreen CreateLounge() => new TimeShiftLoungeSubScreen();
    }
}
