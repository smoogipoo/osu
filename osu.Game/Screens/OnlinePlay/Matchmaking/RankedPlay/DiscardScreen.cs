// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Audio;
using osu.Framework.Audio.Sample;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Primitives;
using osu.Game.Audio;
using osu.Game.Graphics;
using osu.Game.Graphics.Containers;
using osu.Game.Graphics.Sprites;
using osu.Game.Graphics.UserInterface;
using osu.Game.Online.Multiplayer;
using osu.Game.Screens.OnlinePlay.Matchmaking.RankedPlay.Cards;
using osuTK;

namespace osu.Game.Screens.OnlinePlay.Matchmaking.RankedPlay
{
    public partial class DiscardScreen : RankedPlaySubScreen
    {
        public CardRow CenterRow { get; private set; } = null!;

        private PlayerCardHand playerHand = null!;
        private ShearedButton discardButton = null!;
        private OsuSpriteText readyToGo = null!;
        private OsuTextFlowContainer explainer = null!;

        [Resolved]
        private RankedPlayMatchInfo matchInfo { get; set; } = null!;

        private Sample? cardAddSample;
        private Sample? cardDiscardSample;

        private const int card_play_samples = 2;
        private Sample?[]? cardPlaySamples;

        [BackgroundDependencyLoader]
        private void load(AudioManager audio)
        {
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
                    SelectionMode = CardSelectionMode.Multiple,
                },
                new FillFlowContainer
                {
                    RelativeSizeAxes = Axes.X,
                    AutoSizeAxes = Axes.Y,
                    Direction = FillDirection.Vertical,
                    Children =
                    [
                        new OsuSpriteText
                        {
                            Text = "Discarding Phase",
                            Anchor = Anchor.TopCentre,
                            Origin = Anchor.TopCentre,
                            Font = OsuFont.GetFont(typeface: Typeface.TorusAlternate, size: 42, weight: FontWeight.SemiBold),
                            Margin = new MarginPadding(20)
                        },
                        readyToGo = new OsuSpriteText
                        {
                            Text = "You’re ready to go!",
                            Anchor = Anchor.TopCentre,
                            Origin = Anchor.TopCentre,
                            Colour = Color4Extensions.FromHex("87CDFF"),
                            Font = OsuFont.GetFont(typeface: Typeface.TorusAlternate, size: 28, weight: FontWeight.SemiBold),
                            Alpha = 0,
                        },
                    ],
                },
                explainer = new OsuTextFlowContainer(s => s.Font = OsuFont.GetFont(size: 24))
                {
                    AutoSizeAxes = Axes.Y,
                    RelativeSizeAxes = Axes.X,
                    Anchor = Anchor.Centre,
                    Origin = Anchor.BottomCentre,
                    TextAnchor = Anchor.TopCentre,
                    Y = 250,
                    ParagraphSpacing = 1,
                    Alpha = 0,
                }.With(d =>
                {
                    d.AddParagraph("These are your Cards for this match!");
                    d.AddParagraph("When it’s your pick, you can choose one card to go head-to-head with against your opponent!");
                })
            ];

            ButtonsContainer.Child = discardButton = new ShearedButton(width: 150)
            {
                Action = onDiscardButtonClicked,
                Enabled = { Value = true },
                Text = "Discard",
            };

            cardAddSample = audio.Samples.Get(@"Multiplayer/Matchmaking/Ranked/card-add-1");
            cardDiscardSample = audio.Samples.Get(@"Multiplayer/Matchmaking/Ranked/card-discard-1");

            cardPlaySamples = new Sample?[card_play_samples];
            for (int i = 0; i < card_play_samples; i++)
                cardPlaySamples[i] = audio.Samples.Get($@"Multiplayer/Matchmaking/Ranked/card-play-{1 + i}");
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            matchInfo.PlayerCardAdded += cardAdded;
            matchInfo.PlayerCardRemoved += cardRemoved;

            playerHand.SelectionChanged += onSelectionChanged;
        }

        private void onSelectionChanged()
        {
            discardButton.Text = playerHand.Selection.Any() ? "Discard" : "Keep Cards";
        }

        private void onDiscardButtonClicked()
        {
            discardButton.Hide();

            Client.DiscardCards(playerHand.Selection.Select(it => it.Card).ToArray()).FireAndForget();
            playerHand.SelectionMode = CardSelectionMode.Disabled;
        }

        public override void OnEntering(RankedPlaySubScreen? previous)
        {
            base.OnEntering(previous);

            var screenBottomCenter = new Vector2(DrawWidth / 2, DrawHeight);
            int cardCount = 0;

            foreach (var card in matchInfo.PlayerCards)
            {
                playerHand.AddCard(card, c =>
                {
                    c.Position = ToSpaceOfOtherDrawable(screenBottomCenter, playerHand);
                });
                Scheduler.AddDelayed(() =>
                {
                    SamplePlaybackHelper.PlayWithRandomPitch(cardAddSample);
                }, 50 * cardCount);
                cardCount++;
            }

            playerHand.UpdateLayout(stagger: 50);
        }

        private readonly List<RankedPlayCardWithPlaylistItem> discardedCards = new List<RankedPlayCardWithPlaylistItem>();

        private void cardRemoved(RankedPlayCardWithPlaylistItem item) => discardedCards.Add(item);

        private void playDiscardAnimation()
        {
            const double stagger = 100;
            double delay = 0;

            foreach (var item in discardedCards)
            {
                if (!playerHand.RemoveCard(item, out var card, out Quad drawQuad))
                    return;

                card.Anchor = Anchor.Centre;
                card.Origin = Anchor.Centre;

                card.MatchScreenSpaceDrawQuad(drawQuad, CenterRow);

                CenterRow.Add(card);

                using (BeginDelayedSequence(1000 + delay))
                {
                    card.PopOutAndExpire();
                }

                Scheduler.AddDelayed(() =>
                {
                    SamplePlaybackHelper.PlayWithRandomPitch(cardPlaySamples);
                }, delay);

                delay += stagger;
            }

            Scheduler.AddDelayed(() =>
            {
                cardDiscardSample?.Play();
            }, 1000);

            discardedCards.Clear();
            CenterRow.LayoutCards(stagger: stagger);
        }

        private double nextCardDrawTime;
        private double earliestPresentationTime;

        private void cardAdded(RankedPlayCardWithPlaylistItem card)
        {
            if (discardedCards.Count > 0)
            {
                playDiscardAnimation();
                nextCardDrawTime = Math.Max(nextCardDrawTime, Time.Current + 2000);
            }

            double delay = Math.Max(0, nextCardDrawTime - Time.Current);
            nextCardDrawTime = Time.Current + delay + 100;

            earliestPresentationTime = Time.Current + 3500;

            Scheduler.AddDelayed(() =>
            {
                playerHand.AddCard(card, d =>
                {
                    d.Position = ToSpaceOfOtherDrawable(new Vector2(DrawWidth, DrawHeight * 0.5f), playerHand);
                    d.Rotation = -30;
                });

                SamplePlaybackHelper.PlayWithRandomPitch(cardAddSample);
            }, delay);
        }

        public void PresentRemainingCards()
        {
            double presentationTime = Math.Max(earliestPresentationTime, Time.Current);

            Scheduler.AddDelayed(presentRemainingCards, presentationTime - Time.Current);
        }

        private void presentRemainingCards()
        {
            int delay = 0;

            foreach (var item in matchInfo.PlayerCards)
            {
                if (playerHand.RemoveCard(item, out var card, out Quad drawQuad))
                {
                    card.MatchScreenSpaceDrawQuad(drawQuad, CenterRow);

                    CenterRow.Add(card);

                    Scheduler.AddDelayed(() =>
                    {
                        SamplePlaybackHelper.PlayWithRandomPitch(cardPlaySamples);
                    }, delay);

                    delay += 50;
                }
                else
                {
                    CenterRow.Add(new RankedPlayCard(item)
                    {
                        Anchor = Anchor.Centre,
                        Origin = Anchor.Centre,
                    });
                }
            }

            CenterRow.LayoutCards(stagger: 50, duration: 600);

            readyToGo.FadeIn(50);
            explainer
                .Delay(100)
                .MoveToOffset(new Vector2(0, 50))
                .MoveToOffset(new Vector2(0, -50), 600, Easing.OutExpo)
                .FadeIn(250);
        }

        protected override void Dispose(bool isDisposing)
        {
            matchInfo.PlayerCardAdded -= cardAdded;
            matchInfo.PlayerCardRemoved -= cardRemoved;

            base.Dispose(isDisposing);
        }
    }
}
