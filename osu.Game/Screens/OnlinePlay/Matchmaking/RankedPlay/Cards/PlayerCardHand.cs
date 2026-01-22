// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Audio;
using osu.Framework.Audio.Sample;
using osu.Framework.Bindables;
using osu.Framework.Input.Events;
using osu.Game.Audio;
using osu.Game.Graphics.UserInterface;
using osu.Game.Online.RankedPlay;
using osuTK.Input;

namespace osu.Game.Screens.OnlinePlay.Matchmaking.RankedPlay.Cards
{
    /// <summary>
    /// Card hand representing the player's current hand, intended to be placed at the bottom edge of the screen.
    /// This version of the card hand reacts to player inputs like hovering a card.
    /// </summary>
    public partial class PlayerCardHand : CardHand
    {
        /// <summary>
        /// Fired if any card is selected or deselected
        /// </summary>
        public event Action? SelectionChanged;

        /// <summary>
        /// Fired if a card's <see cref="CardHand.HandCard.State"/> has changed
        /// </summary>
        public event Action? StateChanged;

        private CardSelectionMode selectionMode;

        /// <summary>
        /// Current selection mode.
        /// </summary>
        /// <remarks>
        /// <see cref="CardSelectionMode.Disabled"/> will disable some of the card's mouse interactions.
        /// </remarks>
        public CardSelectionMode SelectionMode
        {
            get => selectionMode;
            set
            {
                selectionMode = value;
                allowSelection.Value = value != CardSelectionMode.Disabled;

                if (value == CardSelectionMode.Disabled)
                {
                    foreach (var card in Cards)
                        card.Selected = false;
                }
            }
        }

        private IEnumerable<PlayerHandCard> selection => Cards.OfType<PlayerHandCard>().Where(it => it.Selected);

        /// <summary>
        /// Currently selected cards.
        /// </summary>
        public IEnumerable<RankedPlayCardWithPlaylistItem> Selection => selection.Select(it => it.Card.Item);

        private readonly BindableBool allowSelection = new BindableBool(true);

        private const int select_samples = 1;
        private const int deselect_samples = 2;

        private Sample?[]? cardSelectSamples;
        private Sample?[]? cardDeselectSamples;

        [BackgroundDependencyLoader]
        private void load(AudioManager audio)
        {
            cardSelectSamples = new Sample?[select_samples];
            for (int i = 0; i < select_samples; i++)
                cardSelectSamples[i] = audio.Samples.Get(@$"Multiplayer/Matchmaking/Ranked/card-select-{i + 1}");

            cardDeselectSamples = new Sample?[deselect_samples];
            for (int i = 0; i < deselect_samples; i++)
                cardDeselectSamples[i] = audio.Samples.Get(@$"Multiplayer/Matchmaking/Ranked/card-deselect-{i + 1}");
        }

        protected override HandCard CreateHandCard(RankedPlayCard card) => new PlayerHandCard(card)
        {
            Action = cardClicked,
            AllowSelection = allowSelection.GetBoundCopy(),
        };

        private void cardClicked(PlayerHandCard card)
        {
            if (selectionMode == CardSelectionMode.Disabled)
                return;

            try
            {
                if (selectionMode == CardSelectionMode.Single)
                {
                    // only play feedback SFX if the selected card has changed
                    if (!card.Selected)
                        SamplePlaybackHelper.PlayWithRandomPitch(cardSelectSamples);

                    foreach (var c in Cards)
                    {
                        ((PlayerHandCard)c).Selected = c == card;
                    }

                    return;
                }

                card.Selected = !card.Selected;

                SamplePlaybackHelper.PlayWithRandomPitch(card.Selected ? cardSelectSamples : cardDeselectSamples);
            }
            finally
            {
                SelectionChanged?.Invoke();
            }
        }

        protected override void OnCardStateChanged(HandCard card, RankedPlayCardState state)
        {
            StateChanged?.Invoke();

            base.OnCardStateChanged(card, state);
        }

        public Dictionary<Guid, RankedPlayCardState> State => Cards.Select(static card => new KeyValuePair<Guid, RankedPlayCardState>(card.Item.Card.ID, card.State)).ToDictionary();

        public partial class PlayerHandCard : HandCard
        {
            public required Action<PlayerHandCard> Action;

            public required IBindable<bool> AllowSelection;

            public PlayerHandCard(RankedPlayCard card)
                : base(card)
            {
            }

            protected override void LoadComplete()
            {
                base.LoadComplete();

                AddInternal(new HoverSounds());
            }

            protected override bool OnHover(HoverEvent e)
            {
                CardHovered = true;

                return true;
            }

            protected override void OnHoverLost(HoverLostEvent e)
            {
                CardHovered = false;
            }

            protected override bool OnMouseDown(MouseDownEvent e)
            {
                if (e.Button == MouseButton.Left && AllowSelection.Value)
                {
                    CardPressed = true;

                    return true;
                }

                return false;
            }

            protected override void OnMouseUp(MouseUpEvent e)
            {
                if (e.Button == MouseButton.Left)
                    CardPressed = false;
            }

            protected override bool OnClick(ClickEvent e)
            {
                if (!AllowSelection.Value)
                    return false;

                Action(this);

                return true;
            }
        }
    }
}
