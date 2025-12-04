// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Bindables;
using osu.Game.Online.Multiplayer.MatchTypes.RankedPlay;
using osu.Game.Online.Rooms;

namespace osu.Game.Screens.OnlinePlay.Matchmaking.RankedPlay
{
    public class RevealedRankedPlayCardItem
    {
        public readonly Bindable<MultiplayerPlaylistItem?> PlaylistItem = new Bindable<MultiplayerPlaylistItem?>();
        public readonly RankedPlayCardItem Card;

        public RevealedRankedPlayCardItem(RankedPlayCardItem card)
        {
            Card = card;
        }
    }
}
