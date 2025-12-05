// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Effects;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Transforms;
using osu.Framework.Input.Events;
using osu.Game.Graphics;
using osu.Game.Graphics.Containers;
using osu.Game.Graphics.Sprites;
using osu.Game.Graphics.UserInterface;
using osu.Game.Graphics.UserInterfaceV2;
using osu.Game.Online.Rooms;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Screens.OnlinePlay.Matchmaking.RankedPlay
{
    public partial class DiscardScreen(MultiplayerPlaylistItem[] hand) : RankedPlaySubScreen
    {
        private Container<Card> cardFlow = null!;
        private OsuButton discardButton = null!;
        private OsuSpriteText readyToGo = null!;
        private OsuTextFlowContainer explainer = null!;

        private readonly List<Card> cards = new List<Card>();

        [BackgroundDependencyLoader]
        private void load()
        {
            cards.AddRange(hand.Select(item => new Card(item)
            {
                Origin = Anchor.Centre,
                Anchor = Anchor.Centre,
                Action = cardClicked
            }).ToArray());

            InternalChildren =
            [
                cardFlow = new Container<Card>
                {
                    RelativeSizeAxes = Axes.Both,
                    Size = new Vector2(0.5f),
                    Anchor = Anchor.BottomCentre,
                    Origin = Anchor.BottomCentre,
                    Children = cards,
                },
                new FillFlowContainer
                {
                    RelativeSizeAxes = Axes.Both,
                    Size = new Vector2(0.25f, 0.5f),
                    Anchor = Anchor.BottomRight,
                    Origin = Anchor.BottomRight,
                    Direction = FillDirection.Vertical,
                    Spacing = new Vector2(10),
                    Children = new[]
                    {
                        discardButton = new RoundedButton
                        {
                            Text = "Discard",
                            Anchor = Anchor.Centre,
                            Origin = Anchor.Centre,
                            Size = new Vector2(100, 40),
                            Action = doDiscard,
                            Name = "Discard"
                        },
                    }
                },
                new FillFlowContainer
                {
                    RelativeSizeAxes = Axes.Both,
                    Width = 0.5f,
                    Anchor = Anchor.TopCentre,
                    Origin = Anchor.TopCentre,
                    Direction = FillDirection.Vertical,
                    Padding = new MarginPadding { Top = 50 },
                    Spacing = new Vector2(10),
                    Children =
                    [
                        new OsuSpriteText
                        {
                            Anchor = Anchor.TopCentre,
                            Origin = Anchor.TopCentre,
                            Text = "Discarding Phase",
                            Font = OsuFont.Style.Title
                        },
                        readyToGo = new OsuSpriteText
                        {
                            Anchor = Anchor.TopCentre,
                            Origin = Anchor.TopCentre,
                            Text = "You’re ready to go!",
                            Font = OsuFont.Style.Subtitle,
                            Colour = Color4Extensions.FromHex("#47B4EC"),
                            Alpha = 0,
                        },
                        explainer = new OsuTextFlowContainer(s => s.Font = OsuFont.Style.Heading2)
                        {
                            RelativeSizeAxes = Axes.Both,
                            Anchor = Anchor.TopCentre,
                            Origin = Anchor.TopCentre,
                            TextAnchor = Anchor.TopCentre,
                            Margin = new MarginPadding { Top = 50 },
                            ParagraphSpacing = 1,
                            Alpha = 0,
                        }.With(d =>
                        {
                            d.AddParagraph("These are your Cards for this match!");
                            d.AddParagraph("When it’s your pick, you can choose one card to go head-to-head with against your opponent!");
                        })
                    ]
                }
            ];

            foreach (var (card, x) in layoutCards(cardFlow, -20))
            {
                card.X = x;
                card.Y = cardYOffsetAt(x);
                card.Rotation = cardRotationAt(x);
            }
        }

        private static float cardRotationAt(float x) => x * 0.03f;
        private static float cardYOffsetAt(float x) => MathF.Pow(MathF.Abs(x / 250), 2) * 20;

        private void cardClicked(Card target)
        {
            target.Selected = !target.Selected;

            discardButton.Text = cardFlow.Any(it => it.Selected) ? "Discard" : "Don't Discard";
        }

        private void doDiscard()
        {
            discardButton.Hide();

            foreach (var card in cards)
                card.Action = null;

            double delay = 0;

            playDiscardAnimation(ref delay);
            flyInNewCards(ref delay /*TODO: pass in newCards parameter */);
            presentRemainingCards(ref delay);
        }

        #region Animations

        private void playDiscardAnimation(ref double delay)
        {
            cards.RemoveAll(card => card.Selected);

            var selectedCards = cardFlow.Where(it => it.Selected).ToList();

            if (selectedCards.Count == 0)
                return;

            foreach (var (card, x) in layoutCards(selectedCards, 20))
            {
                card.Delay(delay)
                    .MoveTo(new Vector2(x, -200), 400, Easing.OutExpo)
                    .RotateTo(0, 400, Easing.OutExpo)
                    .Then(400)
                    .ScaleTo(0, 350)
                    .FadeOut(200)
                    .Expire();

                delay += 50;
            }

            foreach (var (card, x) in layoutCards(cards, -20))
            {
                card
                    .Delay(delay)
                    .MoveToX(x, 400, Easing.OutExpo)
                    .MoveToY(cardYOffsetAt(x), 400, Easing.OutExpo)
                    .RotateTo(cardRotationAt(x), 400, Easing.OutExpo);

                delay += 50;
            }

            delay += 1500;
        }

        private void flyInNewCards(ref double delay)
        {
            if (cards.Count == 5)
                return;

            const double stagger = 100;

            var newCards = new List<Card>();

            while (cards.Count < 5)
            {
                var newCard = new Card(new MultiplayerPlaylistItem())
                {
                    Origin = Anchor.Centre,
                    Anchor = Anchor.Centre,
                    Alpha = 0,
                    X = DrawWidth * 0.75f
                };

                cards.Add(newCard);
                cardFlow.Add(newCard);
                newCards.Add(newCard);
            }

            var cardLayout = layoutCards(cards, -20)
                             .Select(x => new { x.card, x.x, isNew = newCards.Contains(x.card) })
                             .ToList();

            foreach (var entry in cardLayout.Where(it => !it.isNew))
            {
                entry.card
                     .Delay(delay)
                     .MoveToX(entry.x, 1000, Easing.OutExpo)
                     .MoveToY(cardYOffsetAt(entry.x), 1000, Easing.OutExpo)
                     .RotateTo(cardRotationAt(entry.x), 1000, Easing.OutExpo);

                delay += stagger / 2;
            }

            foreach (var entry in cardLayout.Where(it => it.isNew))
            {
                entry.card
                     .Delay(delay)
                     .FadeIn()
                     .MoveToX(entry.x, 600, Easing.OutExpo)
                     .MoveToY(cardYOffsetAt(entry.x), 600, Easing.OutExpo)
                     .RotateTo(cardRotationAt(entry.x), 600, Easing.OutExpo);

                delay += stagger;
            }

            delay += 1000;
        }

        private void presentRemainingCards(ref double delay)
        {
            const double stagger = 20;

            readyToGo.Delay(delay).FadeIn(50);
            explainer.Delay(delay).FadeIn(50);

            foreach (var (card, x) in layoutCards(cards, 20))
            {
                card
                    .Delay(delay)
                    .MoveTo(new Vector2(x, -30), 400, new CubicBezierEasingFunction(0.3, -0.05, 0.0, 1.0))
                    .RotateTo(0, 400, new CubicBezierEasingFunction(0.3, -0.05, 0.0, 1.0));

                delay += stagger;
            }
        }

        #endregion

        private IEnumerable<(Card card, float x)> layoutCards(IEnumerable<Card> cards, float spacing)
        {
            float totalWidth = cards.Sum(card => card.Width + spacing) - spacing;

            float x = -totalWidth / 2;

            foreach (var card in cards)
            {
                yield return (card, x + card.LayoutSize.X / 2);

                x += card.LayoutSize.X + spacing;
            }
        }

        public partial class Card : CompositeDrawable
        {
            public readonly MultiplayerPlaylistItem Item;

            private readonly Drawable content;

            public Card(MultiplayerPlaylistItem item)
            {
                Item = item;
                Size = new Vector2(150, 250);

                InternalChild = content = new Container
                {
                    RelativeSizeAxes = Axes.Both,
                    Masking = true,
                    CornerRadius = 10,
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    EdgeEffect = new EdgeEffectParameters
                    {
                        Type = EdgeEffectType.Shadow,
                        Radius = 10,
                        Colour = Color4.Black.Opacity(0.1f),
                    },
                    Children =
                    [
                        new Box
                        {
                            RelativeSizeAxes = Axes.Both,
                            Colour = Color4.Gray
                        },
                        new FillFlowContainer
                        {
                            Direction = FillDirection.Vertical,
                            Anchor = Anchor.Centre,
                            Origin = Anchor.Centre,
                            AutoSizeAxes = Axes.Both,
                            Children =
                            [
                                new OsuSpriteText
                                {
                                    Anchor = Anchor.Centre,
                                    Origin = Anchor.Centre,
                                    Text = $"Playlist Item Id {item.ID}",
                                },
                                new OsuSpriteText
                                {
                                    Anchor = Anchor.Centre,
                                    Origin = Anchor.Centre,
                                    Text = $"Beatmap Id {item.BeatmapID}",
                                }
                            ]
                        }
                    ]
                };
            }

            public Action<Card>? Action;

            private bool selected;

            public bool Selected
            {
                get => selected;
                set
                {
                    selected = value;
                    content.MoveToY(value ? -100 : 0, 400, Easing.OutElasticQuarter);
                }
            }

            public override bool HandlePositionalInput => Action != null;

            public override bool Contains(Vector2 screenSpacePos) => content.Contains(screenSpacePos);

            protected override bool OnClick(ClickEvent e)
            {
                Action?.Invoke(this);
                return true;
            }

            protected override bool OnHover(HoverEvent e)
            {
                content.ScaleTo(1.1f, 300, Easing.OutElasticQuarter);

                return true;
            }

            protected override void OnHoverLost(HoverLostEvent e)
            {
                content.ScaleTo(1f, 300, Easing.OutElasticQuarter);
            }
        }
    }
}
