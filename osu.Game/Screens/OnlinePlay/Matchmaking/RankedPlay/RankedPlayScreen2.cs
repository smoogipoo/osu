// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osu.Game.Graphics.UserInterface;
using osu.Game.Online.Multiplayer;
using osu.Game.Online.Multiplayer.MatchTypes.RankedPlay;
using osu.Game.Online.Rooms;

namespace osu.Game.Screens.OnlinePlay.Matchmaking.RankedPlay
{
    public partial class RankedPlayScreen2 : OsuScreen
    {
        public ShearedButton ActionButton { get; }

        [Resolved]
        private MultiplayerClient client { get; set; } = null!;

        private readonly Dictionary<RankedPlayCardItem, RevealableCardItem> revealedCards = [];

        private readonly Container<Card> playedCardContainer;
        private readonly OsuSpriteText stageText;
        private readonly Hand localUserHand;

        public RankedPlayScreen2()
        {
            InternalChildren = new Drawable[]
            {
                ActionButton = new ShearedButton(width: 150)
                {
                    Anchor = Anchor.BottomRight,
                    Origin = Anchor.BottomRight,
                    Y = -100,
                    Alpha = 0,
                    Action = onActionButtonClicked,
                    Enabled = { Value = false }
                },
                stageText = new OsuSpriteText
                {
                    Anchor = Anchor.TopCentre,
                    Origin = Anchor.TopCentre,
                    Font = OsuFont.Style.Title,
                    Y = 50
                },
                playedCardContainer = new Container<Card>
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    RelativeSizeAxes = Axes.X,
                    AutoSizeAxes = Axes.Y
                },
                localUserHand = new Hand
                {
                    Anchor = Anchor.BottomCentre,
                    Origin = Anchor.BottomCentre,
                    RelativeSizeAxes = Axes.Both
                }
            };
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            client.MatchRoomStateChanged += onMatchRoomStateChanged;
            client.RankedPlayCardAdded += onRankedPlayCardAdded;
            client.RankedPlayCardRemoved += onRankedPlayCardRemoved;
            client.RankedPlayCardPlayed += onRankedPlayCardPlayed;
            client.RankedPlayCardRevealed += onRankedPlayCardRevealed;

            var localUserState = (RankedPlayUserState)client.LocalUser!.MatchState!;
            foreach (var card in localUserState.Hand)
                localUserHand.AddCard(getRevealedCard(card));
        }

        private void onMatchRoomStateChanged(MatchRoomState state)
        {
            if (state is not RankedPlayRoomState rankedPlayState)
                return;

            ActionButton.Hide();
            localUserHand.AllowSelection.Value = false;

            switch (rankedPlayState.Stage)
            {
                case RankedPlayStage.CardDiscard:
                    stageText.Text = "discard beatmaps from your hand";

                    ActionButton.Show();
                    ActionButton.Text = "Discard";
                    ActionButton.Enabled.Value = true;

                    localUserHand.AllowSelection.Value = true;
                    localUserHand.SelectionLength = int.MaxValue;
                    break;

                case RankedPlayStage.CardPlay:
                    bool isActivePlayer = client.Room!.Users[rankedPlayState.ActivePlayerIndex].Equals(client.LocalUser);

                    if (isActivePlayer)
                    {
                        stageText.Text = "play a card from your hand!";

                        ActionButton.Show();
                        ActionButton.Text = "Play";
                        ActionButton.Enabled.Value = isActivePlayer;

                        localUserHand.AllowSelection.Value = true;
                        localUserHand.SelectionLength = 1;
                    }
                    else
                        stageText.Text = "waiting for the other player to play a card...";

                    break;
            }
        }

        private void onRankedPlayCardAdded(int userId, RankedPlayCardItem card)
        {
            if (userId == client.LocalUser!.UserID)
                localUserHand.AddCard(getRevealedCard(card));
        }

        private void onRankedPlayCardRemoved(int userId, RankedPlayCardItem card)
        {
            if (userId == client.LocalUser!.UserID)
                localUserHand.RemoveCard(getRevealedCard(card));
        }

        private void onRankedPlayCardPlayed(RankedPlayCardItem card)
        {
            localUserHand.RemoveCard(getRevealedCard(card));

            playedCardContainer.Child = new Card(getRevealedCard(card))
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre
            };
        }

        private void onRankedPlayCardRevealed(RankedPlayCardItem card, MultiplayerPlaylistItem item)
        {
            getRevealedCard(card).PlaylistItem.Value = item;
        }

        private void onActionButtonClicked()
        {
            RankedPlayCardItem[] selection = localUserHand.CurrentSelection.ToArray();

            bool finished = false;

            switch (((RankedPlayRoomState)client.Room!.MatchState!).Stage)
            {
                case RankedPlayStage.CardDiscard:
                    client.DiscardCards(selection).FireAndForget();
                    finished = true;
                    break;

                case RankedPlayStage.CardPlay:
                    if (selection.Length > 0)
                    {
                        client.PlayCard(selection.First()).FireAndForget();
                        finished = true;
                    }

                    break;
            }

            if (finished)
            {
                ActionButton.Hide();
                ActionButton.Enabled.Value = false;

                localUserHand.AllowSelection.Value = false;
            }
        }

        private RevealableCardItem getRevealedCard(RankedPlayCardItem card)
        {
            if (revealedCards.TryGetValue(card, out var existing))
                return existing;

            return revealedCards[card] = new RevealableCardItem(card);
        }
    }
}
