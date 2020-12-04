// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Graphics;
using osu.Game.Screens.Multi.Lounge;

namespace osu.Game.Screens.Multi.Realtime
{
    public class RealtimeMultiplayer : Multiplayer
    {
        protected override LoungeSubScreen CreateLounge() => new RealtimeLounge();

        protected override CreateRoomButton CreateCreateRoomButton() => base.CreateCreateRoomButton().With(b => b.Text = "Create match");
    }
}
