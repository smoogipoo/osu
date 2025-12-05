// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Game.Graphics.Sprites;
using osu.Game.Online.Multiplayer;
using osu.Game.Online.Multiplayer.MatchTypes.RankedPlay;
using osu.Game.Online.Rooms;
using osu.Game.Screens;
using osu.Game.Screens.OnlinePlay.Matchmaking.RankedPlay;
using osu.Game.Tests.Visual.Multiplayer;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Tests.Visual.RankedPlay
{
    public partial class TestSceneRankedPlayScreen2 : MultiplayerTestScene
    {
        public override void SetUpSteps()
        {
            base.SetUpSteps();

            AddStep("join room", () => JoinRoom(CreateDefaultRoom(MatchType.RankedPlay)));
            WaitForJoined();

            AddStep("load screen", () => LoadScreen(new RankedPlayScreen2()));
        }

        [Test]
        public void TestAddRemoveCards()
        {
            for (int i = 0; i < 3; i++)
                AddStep("add card", () => MultiplayerClient.RankedPlayAddCard(new RankedPlayCardItem()).WaitSafely());

            for (int i = 0; i < 3; i++)
                AddStep("remove card", () => MultiplayerClient.RankedPlayRemoveCard(((RankedPlayUserState)MultiplayerClient.LocalUser!.MatchState!).Hand[0]).WaitSafely());
        }

        [Test]
        public void TestRevealCards()
        {
            for (int i = 0; i < 3; i++)
            {
                int i2 = i;
                AddStep("reveal card", () => MultiplayerClient.RankedPlayRevealCard(((RankedPlayUserState)MultiplayerClient.LocalUser!.MatchState!).Hand[i2], new MultiplayerPlaylistItem
                {
                    ID = i2,
                    BeatmapID = i2
                }).WaitSafely());
            }
        }

        [Test]
        public void TestPlayCard()
        {
            AddStep("play card", () => MultiplayerClient.PlayCard(((RankedPlayUserState)MultiplayerClient.LocalUser!.MatchState!).Hand[0]).WaitSafely());
        }

        [Test]
        public void TestDiscardCards()
        {
            AddStep("discard cards", () => MultiplayerClient.DiscardCards(((RankedPlayUserState)MultiplayerClient.LocalUser!.MatchState!).Hand.Take(3).ToArray()).WaitSafely());
        }

        public partial class RankedPlayScreen2 : OsuScreen
        {
            [Resolved]
            private MultiplayerClient client { get; set; } = null!;

            private readonly Dictionary<RankedPlayCardItem, RevealedRankedPlayCardItem> revealedCards = [];
            private readonly Hand localUserHand;
            private readonly Container<Card> playedCardContainer;

            public RankedPlayScreen2()
            {
                InternalChildren = new Drawable[]
                {
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

                client.RankedPlayCardAdded += onRankedPlayCardAdded;
                client.RankedPlayCardRemoved += onRankedPlayCardRemoved;
                client.RankedPlayCardPlayed += onRankedPlayCardPlayed;
                client.RankedPlayCardRevealed += onRankedPlayCardRevealed;

                var localUserState = (RankedPlayUserState)client.LocalUser!.MatchState!;
                foreach (var card in localUserState.Hand)
                    localUserHand.AddCard(getRevealedCard(card));
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

            private RevealedRankedPlayCardItem getRevealedCard(RankedPlayCardItem card)
            {
                if (revealedCards.TryGetValue(card, out var existing))
                    return existing;

                return revealedCards[card] = new RevealedRankedPlayCardItem(card);
            }
        }

        public partial class Hand : CompositeDrawable
        {
            private readonly FillFlowContainer<Card> cards;

            public Hand()
            {
                InternalChild = cards = new FillFlowContainer<Card>
                {
                    RelativeSizeAxes = Axes.Both,
                    Spacing = new Vector2(10),
                };
            }

            public void AddCard(RevealedRankedPlayCardItem item)
            {
                cards.Add(new Card(item)
                {
                    Anchor = Anchor.BottomCentre,
                    Origin = Anchor.BottomCentre
                });
            }

            public void RemoveCard(RevealedRankedPlayCardItem item)
            {
                cards.RemoveAll(c => c.Item.Card.Equals(item.Card), true);
            }
        }

        public partial class Card : CompositeDrawable
        {
            public readonly RevealedRankedPlayCardItem Item;

            private readonly Bindable<MultiplayerPlaylistItem?> playlistItem = new Bindable<MultiplayerPlaylistItem?>();

            private readonly Box background;
            private readonly OsuSpriteText beatmapIdText;

            public Card(RevealedRankedPlayCardItem item)
            {
                Item = item;

                Size = new Vector2(100, 200);
                Masking = true;

                InternalChildren = new Drawable[]
                {
                    background = new Box
                    {
                        RelativeSizeAxes = Axes.Both,
                        Colour = Color4.DimGray
                    },
                    new FillFlowContainer
                    {
                        Anchor = Anchor.Centre,
                        Origin = Anchor.Centre,
                        Children = new Drawable[]
                        {
                            new OsuSpriteText
                            {
                                Anchor = Anchor.Centre,
                                Origin = Anchor.Centre,
                                Text = $"ID: {item.Card.ID.GetHashCode()}"
                            },
                            beatmapIdText = new OsuSpriteText
                            {
                                Anchor = Anchor.Centre,
                                Origin = Anchor.Centre,
                                Text = "Hidden"
                            }
                        }
                    }
                };
            }

            protected override void LoadComplete()
            {
                base.LoadComplete();

                playlistItem.BindTo(Item.PlaylistItem);
                playlistItem.BindValueChanged(onPlaylistItemChanged, true);
            }

            private void onPlaylistItemChanged(ValueChangedEvent<MultiplayerPlaylistItem?> e)
            {
                if (e.NewValue != null)
                {
                    background.Colour = Color4.SlateGray;
                    beatmapIdText.Text = $"Beatmap: {e.NewValue.BeatmapID}";
                }
            }
        }
    }
}
