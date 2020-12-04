// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Game.Screens.Multi.Lounge;
using osu.Game.Screens.Multi.Lounge.Components;

namespace osu.Game.Screens.Multi.Timeshift
{
    public class TimeshiftLounge : LoungeSubScreen
    {
        protected override FilterControl CreateFilterControl() => new TimeshiftFilterControl();
    }
}
