// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Humanizer;
using osu.Framework.Allocation;
using osu.Framework.Audio;
using osu.Framework.Audio.Sample;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Logging;
using osu.Game.Audio;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osu.Game.Online.Multiplayer.MatchTypes.RankedPlay;
using osu.Game.Screens.OnlinePlay.Matchmaking.RankedPlay.Cards;
using osuTK;

namespace osu.Game.Screens.OnlinePlay.Matchmaking.RankedPlay
{
    public partial class OpponentPickScreen : RankedPlaySubScreen
    {
        public CardRow CenterRow { get; private set; } = null!;

        private PlayerCardHand playerHand = null!;
        private OpponentCardHand opponentHand = null!;
        private FillFlowContainer textContainer = null!;

        [Resolved]
        private RankedPlayMatchInfo matchInfo { get; set; } = null!;

        private const int card_play_samples = 2;
        private Sample?[]? cardPlaySamples;

        [BackgroundDependencyLoader]
        private void load(AudioManager audio)
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
                    Y = 100,
                    HoverYOffset = 90
                },
                opponentHand = new OpponentCardHand
                {
                    Anchor = Anchor.TopCentre,
                    Origin = Anchor.TopCentre,
                    RelativeSizeAxes = Axes.Both,
                    Height = 0.5f,
                },
                new CardHandReplayRecorder(playerHand),
                new CardHandReplayPlayer(opponentHand),
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
                            Text = "Your opponent is picking a map!",
                            Anchor = Anchor.TopCentre,
                            Origin = Anchor.TopCentre,
                            Colour = Color4Extensions.FromHex("FFA5B6"),
                            Font = OsuFont.GetFont(typeface: Typeface.TorusAlternate, size: 28, weight: FontWeight.SemiBold),
                        },
                    ]
                },
            ];

            cardPlaySamples = new Sample?[card_play_samples];
            for (int i = 0; i < card_play_samples; i++)
                cardPlaySamples[i] = audio.Samples.Get($@"Multiplayer/Matchmaking/Ranked/card-play-{1 + i}");
        }

        public override void OnEntering(RankedPlaySubScreen? previous)
        {
            base.OnEntering(previous);

            foreach (var card in matchInfo.PlayerCards)
            {
                playerHand.AddCard(card, c =>
                {
                    c.Position = ToSpaceOfOtherDrawable(new Vector2(DrawWidth / 2, DrawHeight), playerHand);
                });
            }

            foreach (var card in matchInfo.OpponentCards)
            {
                opponentHand.AddCard(card, c =>
                {
                    c.Position = ToSpaceOfOtherDrawable(new Vector2(DrawWidth / 2, 0), playerHand);
                });
            }

            playerHand.UpdateLayout(stagger: 50);
            opponentHand.UpdateLayout(stagger: 50);
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            matchInfo.CardPlayed += cardPlayed;
        }

        private void cardPlayed(RankedPlayCardWithPlaylistItem item) => Task.Run(async () =>
        {
            if (opponentHand.Cards.FirstOrDefault(it => it.Card.Item.Equals(item)) is { } c)
                await c.Card.CardRevealed.ConfigureAwait(false);

            Schedule(() =>
            {
                textContainer.FadeOut(50);

                RankedPlayCard? card;

                if (opponentHand.RemoveCard(item, out card, out var drawQuad))
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

                SamplePlaybackHelper.PlayWithRandomPitch(cardPlaySamples);

                card
                    .MoveTo(new Vector2(0), 600, Easing.OutExpo)
                    .ScaleTo(CENTERED_CARD_SCALE, 600, Easing.OutExpo)
                    .RotateTo(0, 400, Easing.OutExpo);

                opponentHand.Contract();
                playerHand.Contract();
            });
        });

        protected override void Dispose(bool isDisposing)
        {
            matchInfo.CardPlayed -= cardPlayed;

            base.Dispose(isDisposing);
        }
    }
}
