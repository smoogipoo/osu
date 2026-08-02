// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;
using osu.Framework.Allocation;
using osu.Framework.Audio;
using osu.Framework.Audio.Sample;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Primitives;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Textures;
using osu.Framework.Graphics.Transforms;
using osu.Framework.Localisation;
using osu.Framework.Logging;
using osu.Game.Audio;
using osu.Game.Database;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osu.Game.Online.API.Requests.Responses;
using osu.Game.Online.Multiplayer;
using osu.Game.Online.Multiplayer.MatchTypes.RankedPlay;
using osu.Game.Online.RankedPlay;
using osu.Game.Screens.OnlinePlay.Matchmaking.RankedPlay.Card;
using osu.Game.Screens.OnlinePlay.Matchmaking.RankedPlay.Components;
using osu.Game.Screens.OnlinePlay.Matchmaking.RankedPlay.Hand;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Screens.OnlinePlay.Matchmaking.RankedPlay
{
    public partial class PickScreen : RankedPlaySubScreen
    {
        // When the 'time running out' warning sample starts to play (in remaining seconds)
        private const int warning_time_threshold = 11;

        public CardFlow CenterRow { get; private set; } = null!;

        [Resolved]
        private SparklesContainer? sparklesContainer { get; set; }

        public override bool ShowStageOverlay => true;

        public override LocalisableString StageHeading => "Pick Phase";

        private MysteryLayer mysteryLayer = null!;

        private PlayerHandOfCards playerHand = null!;
        private OpponentHandOfCards opponentHand = null!;

        [Resolved]
        private RankedPlayMatchInfo matchInfo { get; set; } = null!;

        private Sample? cardAddSample;

        private const int card_play_samples = 2;
        private Sample?[]? cardPlaySamples;

        private Sample? timeRunningOutSample;
        private SampleChannel? timeRunningOutSampleChannel;

        private Sample? finalCountdownSample;
        private double? lastFinalCountdownSamplePlayback;

        private Sample? timeUpSample;
        private bool finalBuzzerPlayed;

        private DateTimeOffset stageEndTime;
        private TimeSpan stageDuration;

        /// <summary>
        /// Whether the local user has played a card themselves.
        /// </summary>
        private bool hasPlayedCard;

        public PickScreen()
        {
            StageCaption = "It's your turn to play a card!";
        }

        [BackgroundDependencyLoader]
        private void load(AudioManager audio)
        {
            var matchState = Client.Room?.MatchState as RankedPlayRoomState;

            Debug.Assert(matchState != null);

            Children =
            [
                CenterRow = new CardFlow
                {
                    RelativeSizeAxes = Axes.Both,
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                },
            ];

            CenterColumn.Children =
            [
                opponentHand = new OpponentHandOfCards
                {
                    Anchor = Anchor.TopCentre,
                    Origin = Anchor.TopCentre,
                    RelativeSizeAxes = Axes.Both,
                    Height = 0.5f,
                    Y = -100,
                },
                playerHand = new PlayerHandOfCards
                {
                    Anchor = Anchor.BottomCentre,
                    Origin = Anchor.BottomCentre,
                    RelativeSizeAxes = Axes.Both,
                    Height = 0.5f,
                    SelectionMode = HandSelectionMode.Single,
                    PlayCardAction = onPlayButtonClicked
                },
                new HandReplayRecorder(playerHand),
                new HandReplayPlayer(matchInfo.OpponentId, opponentHand),
            ];

            AddInternal(mysteryLayer = new MysteryLayer
            {
                RelativeSizeAxes = Axes.Both,
                Depth = float.MinValue,
            });

            cardAddSample = audio.Samples.Get(@"Multiplayer/Matchmaking/Ranked/card-add-1");

            cardPlaySamples = new Sample?[card_play_samples];
            for (int i = 0; i < card_play_samples; i++)
                cardPlaySamples[i] = audio.Samples.Get($@"Multiplayer/Matchmaking/Ranked/card-play-{1 + i}");

            timeRunningOutSample = audio.Samples.Get(@"Multiplayer/Matchmaking/Ranked/time-running-out");
            finalCountdownSample = audio.Samples.Get(@"Multiplayer/Matchmaking/Ranked/time-running-out-final");
            timeUpSample = audio.Samples.Get(@"Multiplayer/Matchmaking/Ranked/time-up");
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            matchInfo.CardPlayed += cardPlayed;

            Client.CountdownStarted += onCountdownStarted;
            Client.CountdownStopped += onCountdownStopped;

            if (Client.Room != null)
            {
                foreach (var countdown in Client.Room.ActiveCountdowns)
                    onCountdownStarted(countdown);
            }
        }

        private bool warningSamplesEnabled
            => matchInfo.Stage.Value == RankedPlayStage.CardPlay
               && stageDuration > TimeSpan.FromSeconds(warning_time_threshold)
               && !hasPlayedCard;

        private bool shouldPlayWarningSample
            => warningSamplesEnabled
               && stageEndTime - DateTimeOffset.Now > TimeSpan.FromSeconds(0)
               && stageEndTime - DateTimeOffset.Now <= TimeSpan.FromSeconds(warning_time_threshold);

        private bool shouldPlayFinalWarningSamples
            => warningSamplesEnabled
               && stageEndTime - DateTimeOffset.Now > TimeSpan.FromSeconds(0)
               && stageEndTime - DateTimeOffset.Now < TimeSpan.FromSeconds(4);

        private bool shouldPlayFinalBuzzer
            => warningSamplesEnabled
               && !finalBuzzerPlayed
               && stageEndTime - DateTimeOffset.Now <= TimeSpan.FromSeconds(0);

        protected override void Update()
        {
            base.Update();

            if (shouldPlayFinalWarningSamples && (lastFinalCountdownSamplePlayback == null || Time.Current - lastFinalCountdownSamplePlayback > 1000))
            {
                finalCountdownSample?.Play();
                lastFinalCountdownSamplePlayback = Time.Current;
            }

            if (shouldPlayFinalBuzzer)
            {
                timeUpSample?.Play();
                finalBuzzerPlayed = true;
            }

            if (shouldPlayWarningSample)
            {
                timeRunningOutSampleChannel ??= timeRunningOutSample?.GetChannel();

                if (timeRunningOutSampleChannel == null || timeRunningOutSampleChannel.Playing)
                    return;

                timeRunningOutSampleChannel.ManualFree = true;
                timeRunningOutSampleChannel.Looping = true;
                timeRunningOutSampleChannel.Play();
            }
            else
                timeRunningOutSampleChannel?.Stop();
        }

        public override void OnEntering(RankedPlaySubScreen? previous)
        {
            base.OnEntering(previous);

            const double stagger = 50;
            double delay = 0;

            foreach (var item in matchInfo.PlayerCards)
            {
                double currentDelay = delay;

                if ((previous as DiscardScreen)?.CenterRow.RemoveCard(item, out var card, out var drawQuad) == true)
                {
                    playerHand.AddCard(card, c =>
                    {
                        c.MatchScreenSpaceDrawQuad(drawQuad, playerHand);
                        c.DelayMovementOnEntering(currentDelay);
                    });
                }
                else
                {
                    playerHand.AddCard(item, c =>
                    {
                        c.Position = playerHand.BottomCardInsertPosition;
                        c.DelayMovementOnEntering(currentDelay);
                    });
                    Scheduler.AddDelayed(() =>
                    {
                        SamplePlaybackHelper.PlayWithRandomPitch(cardAddSample);
                    }, delay);
                }

                delay += stagger;
            }

            delay = 0;

            foreach (var item in matchInfo.OpponentCards)
            {
                double currentDelay = delay;

                opponentHand.AddCard(item, c =>
                {
                    c.Position = ToSpaceOfOtherDrawable(new Vector2(DrawWidth / 2, 0), playerHand);
                    c.DelayMovementOnEntering(currentDelay);
                });

                delay += 50;
            }
        }

        private void onCountdownStarted(MultiplayerCountdown countdown) => Scheduler.Add(() =>
        {
            if (countdown is not RankedPlayStageCountdown)
                return;

            stageEndTime = DateTimeOffset.Now + countdown.TimeRemaining;
            stageDuration = countdown.TimeRemaining;
            finalBuzzerPlayed = false;
        });

        private void onCountdownStopped(MultiplayerCountdown countdown) => Scheduler.Add(() =>
        {
            if (countdown is not RankedPlayStageCountdown)
                return;

            stageEndTime = DateTimeOffset.Now;
            stageDuration = TimeSpan.Zero;
        });

        private void onPlayButtonClicked()
        {
            var selection = playerHand.Selection.SingleOrDefault();

            if (selection != null)
            {
                hasPlayedCard = true;
                playerHand.SelectionMode = HandSelectionMode.Disabled;

                Client.PlayCard(selection.Card).FireAndForget();
            }

            playerHand.PlayCardAction = null;
        }

        private void cardPlayed(RankedPlayCardWithPlaylistItem item)
        {
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

            SamplePlaybackHelper.PlayWithRandomPitch(cardPlaySamples);

            opponentHand.Contract();
            playerHand.Contract();

            playerHand.SelectionMode = HandSelectionMode.Disabled;

            if (item.Card.Mystery)
            {
                this.Delay(0).Schedule(() =>
                {
                    // this.FindClosestParent<OsuGameBase>()!.AddRange([
                    //     mysteryLayer.CreateProxy(),
                    //     sparklesContainer?.CreateProxy() ?? Empty(),
                    //     card.CreateProxy()
                    // ]);

                    mysteryLayer.Add(card.CreateProxy());

                    card.Delay(1500)
                        .FadeOut(1000);
                    sparklesContainer?
                        .Delay(1500)
                        .FadeOut(2000);

                    mysteryLayer.ShowWithCard(item);
                    CornerPieceVisibility.Value = Visibility.Hidden;
                });

                Scheduler.AddDelayed(() =>
                {
                    if (sparklesContainer != null)
                        sparklesContainer.Enabled = false;
                }, 500);
            }
        }

        protected override void Dispose(bool isDisposing)
        {
            timeRunningOutSampleChannel?.Stop();
            timeRunningOutSampleChannel?.Dispose();

            matchInfo.CardPlayed -= cardPlayed;

            base.Dispose(isDisposing);
        }

        public bool RemoveCard(RankedPlayCardWithPlaylistItem item, [MaybeNullWhen(false)] out RankedPlayCard card, out Quad screenSpaceDrawQuad)
        {
            if (mysteryLayer.Card != null)
            {
                screenSpaceDrawQuad = mysteryLayer.Card.ScreenSpaceDrawQuad;
                mysteryLayer.Remove(mysteryLayer.Card, false);
                card = mysteryLayer.Card;
                return true;
            }

            return CenterRow.RemoveCard(item, out card, out screenSpaceDrawQuad);
        }

        public class MysteryLayer : VisibilityContainer
        {
            public override bool IsPresent => base.IsPresent || Scheduler.HasPendingTasks;

            [Resolved]
            private BeatmapLookupCache beatmapLookupCache { get; set; } = null!;

            private OsuSpriteText centreText = null!;

            private Box background = null!;

            [BackgroundDependencyLoader]
            private void load()
            {
                Children = new Drawable[]
                {
                    background = new Box
                    {
                        RelativeSizeAxes = Axes.Both,
                        Colour = Color4.Black
                    },
                    centreText = new OsuSpriteText
                    {
                        Anchor = Anchor.Centre,
                        Origin = Anchor.Centre,
                        Font = OsuFont.GetFont(typeface: Typeface.Inter, size: 42),
                        Text = "REVEALED FIRST AT COE",
                        Alpha = 0
                    },
                };
            }

            public void ShowWithCard(RankedPlayCardWithPlaylistItem card)
                => fetchAndShow(card.PlaylistItem.Value!.BeatmapID, card).FireAndForget();

            [Resolved]
            private TextureStore textures { get; set; } = null!;

            public RankedPlayCard? Card;

            private async Task fetchAndShow(int beatmapId, RankedPlayCardWithPlaylistItem item)
            {
                APIBeatmap? beatmap = await beatmapLookupCache.GetBeatmapAsync(beatmapId).ConfigureAwait(false);

                if (beatmap == null)
                    return;

                Scheduler.Add(() =>
                {
                    State.Value = Visibility.Visible;

                    this.Delay(3000)
                        .Schedule(() => centreText.FadeInFromZero(1000))
                        .Delay(3000)
                        .Schedule(() => centreText.FadeOut(300))
                        .Delay(300)
                        .Schedule(() =>
                        {
                            centreText.Text = "NEW FEATURED ARTIST";
                            centreText.FadeInFromZero(1000);
                        })
                        .Delay(3000)
                        .Schedule(() =>
                        {
                            centreText.FadeOut(100);

                            LogoAnimation s;
                            Add(s = new LogoAnimation
                            {
                                Anchor = Anchor.Centre,
                                Origin = Anchor.Centre,
                                Texture = textures.Get("https://osuc.ad/yzZIyRA3"),
                                FillMode = FillMode.Fit,
                            });

                            s.TransformTo(nameof(s.AnimationProgress), 1f, 6000, Easing.OutCubic);

                            s.ScaleTo(1f)
                             .ScaleTo(0.3f, 5000, new CubicBezierEasingFunction(0, .75, 0, .75))
                             .Delay(4000)
                             .ResizeTo(0, 400, Easing.InCubic)
                             .Delay(350)
                             .Schedule(() =>
                             {
                                 background.FadeOut();
                                 s.FadeOut();
                                 Add(Card = new RankedPlayCard(item)
                                 {
                                     Anchor = Anchor.Centre,
                                     Origin = Anchor.Centre,
                                     RevealMysteryCard = true,
                                 });

                                 Card.ScaleTo(1.3f)
                                     .ScaleTo(1f, 500, Easing.OutExpo);
                             });
                        });
                });
            }

            protected override void PopIn()
            {
                this.FadeIn(350);
            }

            protected override void PopOut()
            {
                this.FadeOut();
            }
        }
    }
}
