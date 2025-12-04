// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Extensions.ObjectExtensions;
using osu.Game.Online.Multiplayer;
using osu.Game.Online.Multiplayer.MatchTypes.RankedPlay;
using osu.Game.Online.Rooms;

namespace osu.Game.Screens.OnlinePlay.Matchmaking.RankedPlay
{
    public partial class RankedPlayScreen : OsuScreen
    {
        private readonly Dictionary<RankedPlayCardItem, RevealableRankedPlayCardItem> cards = [];
        private readonly MultiplayerRoom room;

        public RankedPlayScreen(MultiplayerRoom room)
        {
            this.room = room;
        }

        [Resolved]
        private MultiplayerClient client { get; set; } = null!;

        protected override void LoadComplete()
        {
            base.LoadComplete();

            client.MatchRoomStateChanged += onMatchRoomStateChanged;
            client.RankedPlayCardRevealed += onRankedPlayCardRevealed;

            onMatchRoomStateChanged(client.Room!.MatchState);
        }

        private void onRankedPlayCardRevealed(RankedPlayCardItem card, MultiplayerPlaylistItem item) => Scheduler.Add(() =>
        {
            getOrAddCard(card).PlaylistItem.Value = item;
        });

        private void onMatchRoomStateChanged(MatchRoomState? state) => Scheduler.Add(() =>
        {
            if (state is not RankedPlayRoomState rankedPlayState)
                return;

            switch (rankedPlayState.Stage)
            {
                case RankedPlayStage.CardDiscard:
                    RankedPlayUserState userState = (RankedPlayUserState)client.LocalUser!.MatchState!;
                    InternalChild = new DiscardScreen(userState.Hand.Select(getOrAddCard).ToArray());
                    break;
            }
        });

        private RevealableRankedPlayCardItem getOrAddCard(RankedPlayCardItem item)
        {
            if (cards.TryGetValue(item, out var revealable))
                return revealable;

            return cards[item] = new RevealableRankedPlayCardItem(item);
        }

        protected override void Dispose(bool isDisposing)
        {
            base.Dispose(isDisposing);

            if (client.IsNotNull())
                client.MatchRoomStateChanged -= onMatchRoomStateChanged;
        }
    }
}
