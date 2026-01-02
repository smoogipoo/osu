// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Diagnostics;
using System.Linq;
using Humanizer;
using osu.Framework.Allocation;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Logging;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osu.Game.Graphics.UserInterface;
using osu.Game.Online.Multiplayer;
using osu.Game.Online.Multiplayer.MatchTypes.RankedPlay;
using osu.Game.Screens.OnlinePlay.Matchmaking.RankedPlay.Cards;
using osuTK;

namespace osu.Game.Screens.OnlinePlay.Matchmaking.RankedPlay
{
    public partial class PickScreen : RankedPlaySubScreen
    {
        public CardRow CenterRow { get; private set; } = null!;

        private PlayerCardHand playerHand = null!;
        private OpponentCardHand opponentHand = null!;
        private ShearedButton playButton = null!;
        private FillFlowContainer textContainer = null!;

        [Resolved]
        private RankedPlayMatchInfo matchInfo { get; set; } = null!;

        [BackgroundDependencyLoader]
        private void load()
        {
            var matchState = Client.Room?.MatchState as RankedPlayRoomState;

            Debug.Assert(matchState != null);

            Children =
            [
                CenterRow = new CardRow
                {
                    RelativeSizeAxes = Axes.Both,
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                },
            ];

            CenterColumn.Children =
            [
                playerHand = new PlayerCardHand
                {
                    Anchor = Anchor.BottomCentre,
                    Origin = Anchor.BottomCentre,
                    RelativeSizeAxes = Axes.Both,
                    Height = 0.5f,
                    SelectionMode = CardSelectionMode.Single
                },
                opponentHand = new OpponentCardHand
                {
                    Anchor = Anchor.TopCentre,
                    Origin = Anchor.TopCentre,
                    RelativeSizeAxes = Axes.Both,
                    Height = 0.5f,
                    Y = -100,
                },
                textContainer = new FillFlowContainer
                {
                    RelativeSizeAxes = Axes.X,
                    AutoSizeAxes = Axes.Y,
                    Direction = FillDirection.Vertical,
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    Spacing = new Vector2(20),
                    Children =
                    [
                        new OsuSpriteText
                        {
                            Text = $"{FormatRoundIndex(matchState.CurrentRound).Titleize()} pick!",
                            Anchor = Anchor.TopCentre,
                            Origin = Anchor.TopCentre,
                            Font = OsuFont.GetFont(typeface: Typeface.TorusAlternate, size: 42, weight: FontWeight.Regular),
                        },
                        new OsuSpriteText
                        {
                            Text = "It’s your turn!",
                            Anchor = Anchor.TopCentre,
                            Origin = Anchor.TopCentre,
                            Colour = Color4Extensions.FromHex("87CDFF"),
                            Font = OsuFont.GetFont(typeface: Typeface.TorusAlternate, size: 28, weight: FontWeight.SemiBold),
                        },
                    ]
                },
            ];

            ButtonsContainer.Child = playButton = new ShearedButton(width: 150)
            {
                Action = onPlayButtonClicked,
                Enabled = { Value = false },
                Text = "Play",
            };
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            matchInfo.CardPlayed += cardPlayed;
            playerHand.SelectionChanged += onSelectionChanged;
        }

        private void onSelectionChanged()
        {
            playButton.Enabled.Value = playerHand.Selection.Any();
        }

        private void onPlayButtonClicked()
        {
            var selection = playerHand.Selection.SingleOrDefault();

            if (selection != null)
            {
                playerHand.SelectionMode = CardSelectionMode.Disabled;
                Client.PlayCard(selection.Card).FireAndForget();
                playButton.Hide();
            }
        }

        public override void OnEntering(RankedPlaySubScreen? previous)
        {
            base.OnEntering(previous);

            foreach (var item in matchInfo.PlayerCards)
            {
                if ((previous as DiscardScreen)?.CenterRow.RemoveCard(item, out var card, out var drawQuad) == true)
                {
                    playerHand.AddCard(card, c =>
                    {
                        c.MatchScreenSpaceDrawQuad(drawQuad, playerHand);
                    });
                }
                else
                {
                    playerHand.AddCard(item, c =>
                    {
                        c.Position = ToSpaceOfOtherDrawable(new Vector2(DrawWidth / 2, DrawHeight), playerHand);
                    });
                }
            }

            foreach (var item in matchInfo.OpponentCards)
            {
                opponentHand.AddCard(item, c =>
                {
                    c.Position = ToSpaceOfOtherDrawable(new Vector2(DrawWidth / 2, 0), playerHand);
                });
            }

            playerHand.UpdateLayout(stagger: 50);
            opponentHand.UpdateLayout(stagger: 50);
        }

        private void cardPlayed(RankedPlayCardWithPlaylistItem item)
        {
            textContainer.FadeOut(50);

            RankedPlayCard? card;

            if (playerHand.RemoveCard(item, out card, out var drawQuad))
            {
                card.MatchScreenSpaceDrawQuad(drawQuad, CenterRow);
            }
            else
            {
                Logger.Log($"Played card {item.Card.ID} was not present in hand.", level: LogLevel.Error);

                card = new RankedPlayCard(item)
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                };
            }

            CenterRow.Add(card);

            card
                .MoveTo(new Vector2(0), 600, Easing.OutExpo)
                .ScaleTo(CENTERED_CARD_SCALE, 600, Easing.OutExpo)
                .RotateTo(0, 400, Easing.OutExpo);

            opponentHand.Contract();
            playerHand.Contract();
        }

        protected override void Dispose(bool isDisposing)
        {
            matchInfo.CardPlayed -= cardPlayed;

            base.Dispose(isDisposing);
        }
    }
}
