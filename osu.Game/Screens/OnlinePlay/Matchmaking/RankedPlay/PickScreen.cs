// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using osu.Framework.Allocation;
using osu.Framework.Audio;
using osu.Framework.Audio.Sample;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
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
                Depth = float.MinValue
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
                    this.FindClosestParent<OsuGameBase>()!.AddRange([
                        mysteryLayer.CreateProxy(),
                        sparklesContainer?.CreateProxy() ?? Empty(),
                        card.CreateProxy()
                    ]);

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

        private class MysteryLayer : VisibilityContainer
        {
            public override bool IsPresent => base.IsPresent || Scheduler.HasPendingTasks;

            private readonly Bindable<int> textIndex = new Bindable<int>();

            [Resolved]
            private BeatmapLookupCache beatmapLookupCache { get; set; } = null!;

            private OsuSpriteText centreText = null!;

            [BackgroundDependencyLoader]
            private void load()
            {
                Children = new Drawable[]
                {
                    new Box
                    {
                        RelativeSizeAxes = Axes.Both,
                        Colour = Color4.Black
                    },
                    centreText = new OsuSpriteText
                    {
                        Anchor = Anchor.Centre,
                        Origin = Anchor.Centre,
                        Font = OsuFont.GetFont(typeface: Typeface.Inter, size: 30),
                        Text = "FEATURING",
                        Alpha = 0
                    }
                };
            }

            protected override void LoadComplete()
            {
                base.LoadComplete();

                textIndex.BindValueChanged(idx =>
                {
                    centreText.Text = artists[idx.NewValue];
                });
            }

            public void ShowWithCard(RankedPlayCardWithPlaylistItem card)
                => fetchAndShow(card.PlaylistItem.Value!.BeatmapID).FireAndForget();

            private async Task fetchAndShow(int beatmapId)
            {
                APIBeatmap? beatmap = await beatmapLookupCache.GetBeatmapAsync(beatmapId).ConfigureAwait(false);

                if (beatmap == null)
                    return;

                Scheduler.Add(() =>
                {
                    State.Value = Visibility.Visible;

                    using (BeginDelayedSequence(3000))
                    {
                        centreText.FadeInFromZero(1000);

                        using (BeginDelayedSequence(4000))
                        {
                            Schedule(() => centreText.Text = artists[0]);

                            centreText.FadeIn();

                            this.Delay(500)
                                .TransformBindableTo(textIndex, artists.Length - 2, 5000, Easing.InOutExpo)
                                .Then()
                                .Delay(500)
                                .TransformBindableTo(textIndex, artists.Length - 1);
                        }
                    }
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

            private static readonly string[] artists =
            [
                "nekodex",
                "cYsmix",
                "IAHN",
                "yuki.",
                "Helblinde",
                "dark cat",
                "Loki / Thaehan",
                "Sylvir / sakuraburst",
                "S3RL",
                "nanobii",
                "Ben Briggs",
                "LukHash",
                "Kuba Oms",
                "Rin / Function Phantom",
                "Fractal Dreamers",
                "Wisp X",
                "OISHII",
                "*namirin",
                "MOtOLOiD",
                "Trial &amp; Error",
                "niu arx",
                "VINXIS",
                "Cranky",
                "antiPLUR / Internet Death Machine",
                "Nakanojojo",
                "High Tea Music",
                "KIRA",
                "Virtual Self",
                "Culprate",
                "SOOOO",
                "Camellia",
                "onumi",
                "HyuN / INFX",
                "tieff",
                "Imperial Circus Dead Decadence",
                "Creo",
                "The Flashbulb",
                "BilliumMoto",
                "James Landino",
                "RiraN",
                "Rising Sun Traxx",
                "Carpool Tunnel",
                "Inferi",
                "Disko Warp",
                "UNDEAD CORPORATION",
                "Billain",
                "Æther Realm",
                "KNOWER",
                "KOAN Sound",
                "Native Construct",
                "Akira Complex",
                "False Noise",
                "Nekrogoblikon",
                "Ricky Montgomery",
                "Panda Eyes",
                "Klayton / Celldweller",
                "goreshit",
                "Kurokotei",
                "Voicians",
                "F-777",
                "MDK",
                "MYLK",
                "Rivers of Nihil",
                "Teminite",
                "Blue Stahli",
                "Station Earth / Blue Marble",
                "Kola Kid",
                "Frums",
                "ELFENSJóN",
                "Sound Souler",
                "Venetian Snares",
                "ginkiha",
                "LeaF",
                "O2i3",
                "meganeko",
                "Zekk",
                "MIMI",
                "CircusP",
                "PUP",
                "BLANKFIELD",
                "Disasterpeace",
                "Rohi",
                "Thank You Scientist",
                "Noisia",
                "Hyper Potions",
                "Ne Obliviscaris",
                "Rusty K",
                "LEAF XCEED Music Division",
                "Street",
                "Numtack05",
                "Receptor",
                "Silentroom",
                "Ariabl&#039;eyeS",
                "Magnetude",
                "ARForest",
                "Kobaryo",
                "Task Horizon",
                "Umeboshi Chazuke",
                "Imy",
                "EPICA",
                "kiraku",
                "siqlo",
                "Sable Hills",
                "Omoi",
                "cute girls doing cute things",
                "P4koo",
                "ALEPH",
                "Morimori Atsushi",
                "Joe Ford",
                "Sennzai",
                "DUAL ALTER WORLD",
                "Se-U-Ra",
                "glass beach",
                "Fuki",
                "BLOOD CODE",
                "Lime / Kankitsu",
                "I love you Orchestra",
                "orangentle / Yu_Asahina",
                "Bossfight",
                "MuryokuP / Powerless",
                "MYUKKE.",
                "Lifetheory",
                "kanone",
                "Jun Kuroda",
                "Erik McClure",
                "BLOOD STAIN CHILD",
                "Aitsuki Nakuru",
                "Getty",
                "Phantom Sage",
                "Symholic",
                "Tiny Waves",
                "Yuyoyuppe / DJ&#039;TEKINA//SOMETHING ",
                "Geoxor",
                "Dimrain47",
                "Irreversible Mechanism",
                "Masahiro &quot;Godspeed&quot; Aoki",
                "SECONDWALL",
                "seleP",
                "Shawn Wasabi",
                "ovEnola",
                "nora2r",
                "The Gentle Men",
                "Extra Terra",
                "Tanchiky",
                "Empty Peperoncino",
                "polysha",
                "Cres.",
                "RYUJIN / GYZE",
                "URBANGARDE",
                "BlackY",
                "Amidst",
                "Xanthochroid",
                "Aethoro",
                "Boxplot",
                "YUZUKINGDOM",
                "fiend",
                "2TD",
                "Vektor",
                "Grynpyret",
                "Emille&#039;s Moonlight Serenade",
                "m108",
                "miraie",
                "Reku Mochizuki",
                "Agressor Bunx",
                "Kitazawa Kyouhei",
                "Aiobahn",
                "yukitani",
                "A-One",
                "Sewerslvt / Cynthoni",
                "Fleshgod Apocalypse",
                "love solfege",
                "II-L",
                "Aquestion",
                "La prière",
                "Pratanallis",
                "katagiri",
                "Andromedik",
                "wotoha",
                "ABSOLUTE CASTAWAY",
                "Fred V &amp; Grafix",
                "Rish",
                "Michael Cera Palin",
                "Aoi",
                "Redside",
                "seatrus",
                "SEBii",
                "3R2 / DJ Mashiro",
                "T &amp; Sugah",
                "Natsume Itsuki",
                "rN",
                "Aether",
                "Wiklund",
                "technoplanet",
                "Release Hallucination",
                "Alkome",
                "Sephid",
                "Harumaki Gohan",
                "Avizura",
                "rejection",
                "Our Stolen Theory",
                "litmus* / Ester",
                "tokiwa",
                "Lundy",
                "DJ Raisei",
                "HARDCORE UTOPIA",
                "METAROOM",
                "solfa",
                "Lexurus",
                "Mameyudoufu",
                "iFeature",
                "Risa Yuzuki",
                "garlagan",
                "linear ring",
                "LV.4",
                "Chroma",
                "FATE GEAR",
                "Kurubukko",
                "Yooh",
                "Matduke",
                "Marmalade butcher",
                "Tasty",
                "KINEMA106",
                "Tedjimo yomigY",
                "Zomboy",
                "Seraph",
                "siromaru",
                "PTB10",
                "NIWASHI",
                "DGK",
                "Haywyre",
                "satella",
                "Vansire",
                "Boom Kitty",
                "Annabel",
                "Maduk",
                "bill wurtz",
                "Atavistia",
                "HEAD PHONES PRESIDENT",
                "MisoilePunch",
                "Good Kid",
                "Riya",
                "Rabbit House",
                "MisomyL",
                "Yunosuke",
                "City Girl",
                "EmoCosine",
                "Raimukun",
                "USAO",
                "Plum",
                "luvlxckdown",
                "Never Say Die",
                "my sound life",
                "A.SAKA",
                "Stonebank",
                "Monstercat",
                "Archspire",
                "in love with a ghost",
                "Mage",
                "Aethral",
                "Nile",
                "MUZZ",
                "RINYA",
                "Ardolf",
                "Allegaeon",
                "Tenchio",
                "Neko Hacker",
                "JOYLESS",
                "69 de 74",
                "Origami Angel",
                "Rameses B",
                "Ponchi",
                "Hino Isuka",
                "SAMString",
                "Kanpyohgo",
                ":Poin7less",
                "Tokyo Machine",
                "Beach Bunny",
                "NILFRUITS",
                "Toromaru",
                "Rootkit",
                "Grant",
                "aran",
                "Liar-soft",
                "Blind Guardian",
                "Exyl",
                "NOISZ",
                "Pegboard Nerds",
                "ZxNX",
                "Hamu",
                "Darkney",
                "Waterflame",
                "Mitsukiyo",
                "Koven",
                "Abuse",
                "DJ Genki / Gram",
                "Aiyru",
                "-45",
                "Nashimoto Ui",
                "Nitro Fun",
                "Rezonate",
                "AAAA",
                "Andy Gillion",
                "Vorso",
                "Alestorm",
                "Ritorikal",
                "Feint",
                "Mono.",
                "Tokyo.MeltiMelt",
                "Ata",
                "Hybrid Minds",
                "GLORYHAMMER",
                "Au5",
                "Fractal",
                "Kikuo",
                "Hinkik",
                "Zenpaku",
                "happy30",
                "Massive New Krew",
                "HoneyComeBear",
                "Ensou",
                "Crywolf",
                "E0ri4",
                "Innocent Key",
                "Noisestorm",
                "LilyPichu",
                "Ekcle",
                "LandRoot",
                "Satyr",
                "tsunamix",
                "nyankobrq",
                "Mili",
                "Fellowship",
                "Euchaeta",
                "Sound piercer",
                "Etherwood",
                "kakichoco",
                "WINTERHORDE",
                "Sobrem",
                "FRASER EDWARDS",
                "Liquicity",
                "Cinamoro",
                "yaseta",
                "KASHIWA Daisuke",
                "True North",
                "Kabocha",
                "Mediks",
                "Whispered",
                "SWORD OF JUSTICE",
                "Minstrel",
                "Down",
                "Kenneyon",
                "Ashrount",
                "Stars Hollow",
                "CHON",
                "Yokomin",
                "7_7",
                "Synthion",
                "Genkaku Aria",
                "cygnus",
                "Aice room",
                "tephe",
                "Shadren",
                "Andora",
                "kaitendaentai",
                "Junk",
                "passchooo",
                "kanemiko",
                "Halv",
                "kuro",
                "Ruby My Dear",
                "Krimek",
                "YUC&#039;e",
                "Supire",
                "ALLMYFRIENDS",
                "Kardashev",
                "soowamisu",
                "beignet",
                "C-Show",
                "Kommisar",
                "Aquellex",
                "Corsace",
                "1914",
                "Kou!",
                "ColBreakz",
                "Will Stetson",
                "Dustvoxx",
                "PHAZE",
                "Solarbear",
                "you",
                "DOT96",
                "Knife",
                "Sydosys",
                "Sorry about my face",
                "Asa",
                "Hitori Tori",
                "t+pazolite",
                "anubasu-anubasu",
                "Sparxe",
                "iroha(sasaki)",
                "Rocket Start",
                "Akiri",
                "uselet",
                "Draper",
                "Takamachi Walk",
                "Pandize",
                "YonKaGor",
                "Cansol",
                "Yuuni",
                "ptar124",
                "Grabbitz",
                "ODDEEO",
                "Pa&#039;s Lam System",
                "REAPER",
                "PaceMKR",
                "lemm",
                "watering",
                "Billx",
                "Rubatonin",
                "633397",
                "TEST Open",
                "Strelitzia",
                "DeBisco",
                "xiiiac13",
                "0 K",
                "Doomsday",
                "Nakura",
                "WangleLine",
                "takehirotei",
                "Rose Quartz",
                "MetaHumanBoi",
                "Candle",
                "ShibayanRecords",
                "Xyris",
                "SiLiS",
                "Supa7onyz",
                "cast heal",
                "tom^s",
                "Kagetora.",
                "Raytrax Music Collective",
                "Trina Lydia",
                "Badly Wood Cup",
                "sugosugiii",
                "Midian",
                "Future Witness",
                "WyvernP",
                "ikaruga_nex",
                "nagiha",
                "Kry.exe",
                "Lusumi",
                "Zmey Gorynich",
                "Attoclef",
                "XH",
                "uynet",
                "Anamanaguchi",
                "ZVLIAN",
                "ntyn",
                "Powerwolf",
                "EBIMAYO",
                "Ludicin",
                "WEARY",
                "SEVEN LIVES",
                "osu! community music",
                "hikota",
                "rae",
                "Slax",
                "ak+q",
                "AQUASINE",
                "lexycat",
                "Quarkee",
                "Terminal 11",
                "Naikou",
                "yoho",
                "1zm8",
                "Xeven",
                "Kyutatsuki",
                "JinoBeats",
                "The Musical Ghost",
                "UNTONE Music",
                "TONE::FURY",
                "Simon Safhalter",
                "XenjeS",
                "Shirobon",
                "Ice",
                "Dispel",
                "Aspect",
                "DraGonis",
                "SPIRIT GARDEN *",
                "Juwubi",
                "muyu",
                "GRYSCL",
                "d0tc0mmie",
                "Sad Keyboard Guy",
                "Myntian",
                "Drazically",
                "kikoyu",
                "Xeon Diversity",
                "LUZE",
                "roer",
                "Raphiiel",
                "Dvwnpour",
                "MARETU",
                "mimizu",
                "BrayanKitsn",
                "Tanger",
                "MEMODEMO",
                "unfeeling",
                "lexxndr",
                "gladde paling",
                "WhiteSakata",
                "Akts",
                "steelplus",
                "MIDInco",
                "AKA",
                "Stariah",
                "Memme",
                "ePiaeon",
                "tachibanaka",
                "voira",
                "gmtn. / witch&#039;s slave",
                "kefi",
                "John Grant",
                "qfeileadh",
                "penoreri",
                "Laur",
                "celtix",
                "Genesis",
                "SHK",
                "Juztan",
                "LaXal",
                "Ariz Kayaba",
                "Brandy",
                "OLDUCT",
                "Getter Jaani",
                "dennoko-P",
                "jeko",
                "Ennnn",
                "log() / ohm002",
                "Pashtetue",
                "Sukima Altera",
                "trung-nova",
                "dandeless",
                "Dazzling",
                "Junshi",
                "elwood",
                "Adust Rain",
                "Katari",
                "TRIAL",
                "Link&quot;0",
                "endofsystem",
                "Otsukisama Koukyoukyoku",
                "awoKen",
                "xi",
            ];
        }
    }
}
