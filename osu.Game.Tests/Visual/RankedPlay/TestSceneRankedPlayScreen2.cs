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
using osu.Framework.Input.Events;
using osu.Framework.Testing;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osu.Game.Graphics.UserInterface;
using osu.Game.Online.Multiplayer;
using osu.Game.Online.Multiplayer.MatchTypes.RankedPlay;
using osu.Game.Online.Rooms;
using osu.Game.Screens;
using osu.Game.Screens.OnlinePlay.Matchmaking.RankedPlay;
using osu.Game.Tests.Visual.Multiplayer;
using osuTK;
using osuTK.Graphics;
using osuTK.Input;

namespace osu.Game.Tests.Visual.RankedPlay
{
    public partial class TestSceneRankedPlayScreen2 : MultiplayerTestScene
    {
        private RankedPlayScreen2 screen = null!;

        public override void SetUpSteps()
        {
            base.SetUpSteps();

            AddStep("join room", () => JoinRoom(CreateDefaultRoom(MatchType.RankedPlay)));
            WaitForJoined();

            AddStep("load screen", () => LoadScreen(screen = new RankedPlayScreen2()));
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
        public void TestPlayCardDirect()
        {
            AddStep("play card", () => MultiplayerClient.PlayCard(((RankedPlayUserState)MultiplayerClient.LocalUser!.MatchState!).Hand[0]).WaitSafely());
        }

        [Test]
        public void TestDiscardCardsDirect()
        {
            AddStep("discard cards", () => MultiplayerClient.DiscardCards(((RankedPlayUserState)MultiplayerClient.LocalUser!.MatchState!).Hand.Take(3).ToArray()).WaitSafely());
        }

        [Test]
        public void TestDiscardCardsStage()
        {
            AddStep("set discard phase", () => MultiplayerClient.RankedPlayChangeStage(RankedPlayStage.CardDiscard).WaitSafely());

            for (int i = 0; i < 3; i++)
            {
                int i2 = i;
                AddStep($"click card {i2}", () =>
                {
                    InputManager.MoveMouseTo(this.ChildrenOfType<Card>().ElementAt(i2));
                    InputManager.Click(MouseButton.Left);
                });
            }

            AddStep("click discard button", () =>
            {
                InputManager.MoveMouseTo(screen.DiscardButton);
                InputManager.Click(MouseButton.Left);
            });
        }

        public partial class RankedPlayScreen2 : OsuScreen
        {
            public ShearedButton DiscardButton { get; }

            [Resolved]
            private MultiplayerClient client { get; set; } = null!;

            private readonly Dictionary<RankedPlayCardItem, RevealedRankedPlayCardItem> revealedCards = [];

            private readonly Container<Card> playedCardContainer;
            private readonly OsuSpriteText stageText;
            private readonly Hand localUserHand;

            public RankedPlayScreen2()
            {
                InternalChildren = new Drawable[]
                {
                    DiscardButton = new ShearedButton(width: 150)
                    {
                        Anchor = Anchor.BottomRight,
                        Origin = Anchor.BottomRight,
                        Text = "Discard",
                        Y = -100,
                        Alpha = 0,
                        Action = onDiscardClicked,
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

                DiscardButton.Hide();
                localUserHand.AllowSelection.Value = false;

                switch (rankedPlayState.Stage)
                {
                    case RankedPlayStage.CardDiscard:
                        stageText.Text = "Discard Phase";

                        DiscardButton.Show();
                        DiscardButton.Enabled.Value = true;

                        localUserHand.AllowSelection.Value = true;
                        localUserHand.SelectionLength = int.MaxValue;
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

            private void onDiscardClicked()
            {
                var selection = localUserHand.CurrentSelection.ToArray();

                DiscardButton.Hide();
                DiscardButton.Enabled.Value = false;

                localUserHand.AllowSelection.Value = false;

                client.DiscardCards(selection).FireAndForget();
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
            public readonly Bindable<bool> AllowSelection = new Bindable<bool>();
            public int SelectionLength { get; set; }

            private readonly FillFlowContainer<Card> cards;

            public Hand()
            {
                InternalChild = cards = new FillFlowContainer<Card>
                {
                    RelativeSizeAxes = Axes.Both,
                    Spacing = new Vector2(10),
                };
            }

            public IEnumerable<RankedPlayCardItem> CurrentSelection
                => cards.Where(c => c.Selected.Value).Select(c => c.Item.Card);

            public void AddCard(RevealedRankedPlayCardItem item)
            {
                var card = new Card(item)
                {
                    Anchor = Anchor.BottomCentre,
                    Origin = Anchor.BottomCentre,
                    AllowSelection = { BindTarget = AllowSelection },
                };

                card.Selected.BindValueChanged(onCardSelected, true);

                cards.Add(card);
            }

            public void RemoveCard(RevealedRankedPlayCardItem item)
            {
                cards.RemoveAll(c => c.Item.Card.Equals(item.Card), true);
            }

            private void onCardSelected(ValueChangedEvent<bool> e)
            {
                if (SelectionLength == 0)
                    return;

                while (CurrentSelection.Count() >= SelectionLength)
                    cards.First(c => c.Selected.Value).Selected.Value = false;
            }
        }

        public partial class Card : CompositeDrawable
        {
            public readonly Bindable<bool> AllowSelection = new Bindable<bool>();
            public readonly BindableBool Selected = new BindableBool();

            public readonly RevealedRankedPlayCardItem Item;

            private readonly Bindable<MultiplayerPlaylistItem?> playlistItem = new Bindable<MultiplayerPlaylistItem?>();

            private readonly Box background;
            private readonly OsuSpriteText beatmapIdText;

            public Card(RevealedRankedPlayCardItem item)
            {
                Item = item;

                Size = new Vector2(100, 200);
                Masking = true;
                BorderColour = Color4.Yellow;
                BorderThickness = 0;

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

                AllowSelection.BindValueChanged(onAllowSelectionChanged, true);
                Selected.BindValueChanged(onSelectedChanged, true);
            }

            private void onPlaylistItemChanged(ValueChangedEvent<MultiplayerPlaylistItem?> e)
            {
                if (e.NewValue != null)
                {
                    background.Colour = Color4.SlateGray;
                    beatmapIdText.Text = $"Beatmap: {e.NewValue.BeatmapID}";
                }
            }

            private void onAllowSelectionChanged(ValueChangedEvent<bool> e)
            {
                if (!e.NewValue)
                    Selected.Value = false;
            }

            private void onSelectedChanged(ValueChangedEvent<bool> e)
            {
                BorderThickness = e.NewValue ? 5 : 0;
            }

            protected override bool OnClick(ClickEvent e)
            {
                if (AllowSelection.Value)
                    Selected.Toggle();

                return true;
            }
        }
    }
}
