// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Extensions;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Extensions.LocalisationExtensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Localisation;
using osu.Framework.Logging;
using osu.Game.Beatmaps;
using osu.Game.Beatmaps.Drawables;
using osu.Game.Database;
using osu.Game.Extensions;
using osu.Game.Graphics;
using osu.Game.Graphics.Containers;
using osu.Game.Graphics.Sprites;
using osu.Game.Localisation;
using osu.Game.Online.API.Requests.Responses;
using osu.Game.Online.Multiplayer;
using osu.Game.Online.Multiplayer.MatchTypes.RankedPlay;
using osu.Game.Online.Rooms;
using osu.Game.Overlays;
using osu.Game.Resources.Localisation.Web;
using osu.Game.Rulesets;
using osu.Game.Screens.OnlinePlay.Matchmaking.RankedPlay.Cards;
using osu.Game.Screens.SelectV2;
using osu.Game.Utils;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Screens.OnlinePlay.Matchmaking.RankedPlay
{
    public class GameplayWarmupScreen : RankedPlaySubScreen
    {
        public CardRow CenterRow { get; private set; } = null!;

        [Cached(typeof(IBindable<SongSelect.BeatmapSetLookupResult?>))]
        private readonly Bindable<SongSelect.BeatmapSetLookupResult?> lastLookupResult = new Bindable<SongSelect.BeatmapSetLookupResult?>();

        [Resolved]
        private BeatmapLookupCache beatmapLookupCache { get; set; } = null!;

        [Resolved]
        private RankedPlayMatchInfo matchInfo { get; set; } = null!;

        [BackgroundDependencyLoader]
        private void load()
        {
            APIBeatmap beatmap = beatmapLookupCache.GetBeatmapAsync(Client.Room!.CurrentPlaylistItem.BeatmapID).GetResultSafely()!;
            lastLookupResult.Value = SongSelect.BeatmapSetLookupResult.Completed(beatmap.BeatmapSet);

            var matchState = Client.Room?.MatchState as RankedPlayRoomState;
            Debug.Assert(matchState != null);

            Children =
            [
                CenterRow = new CardRow
                {
                    RelativeSizeAxes = Axes.Both,
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    Alpha = 0,
                },
            ];

            CenterColumn.Children =
            [
                new Container
                {
                    RelativeSizeAxes = Axes.Both,
                    Masking = true,
                    Child = new Container
                    {
                        RelativeSizeAxes = Axes.Both,
                        Shear = OsuGame.SHEAR,
                        Padding = new MarginPadding
                        {
                            Top = -SongSelect.CORNER_RADIUS_HIDE_OFFSET,
                            Left = -SongSelect.CORNER_RADIUS_HIDE_OFFSET,
                        },
                        Child = new FillFlowContainer
                        {
                            RelativeSizeAxes = Axes.Both,
                            Spacing = new Vector2(0f, 4f),
                            Direction = FillDirection.Vertical,
                            Children =
                            [
                                new ShearAligningWrapper(new GameplayWarmupScreenTitleWedge(beatmap)),
                                new ShearAligningWrapper(new GameplayWarmupScreenMetadataWedge(beatmap))
                                {
                                    Shear = -OsuGame.SHEAR,
                                },
                            ]
                        }
                    }
                }
            ];
        }

        public override void OnEntering(RankedPlaySubScreen? previous)
        {
            base.OnEntering(previous);

            if (matchInfo.LastPlayedCard == null)
                return;

            RankedPlayCard? card = null;

            switch (previous)
            {
                case PickScreen pick:
                {
                    if (pick.CenterRow.RemoveCard(matchInfo.LastPlayedCard, out card, out var screenSpaceDrawQuad))
                        card.MatchScreenSpaceDrawQuad(screenSpaceDrawQuad, CenterRow);
                    break;
                }

                case OpponentPickScreen opponentPick:
                {
                    if (opponentPick.CenterRow.RemoveCard(matchInfo.LastPlayedCard, out card, out var screenSpaceDrawQuad))
                        card.MatchScreenSpaceDrawQuad(screenSpaceDrawQuad, CenterRow);
                    break;
                }
            }

            if (card == null)
            {
                Logger.Log($"Played card {matchInfo.LastPlayedCard.Card.ID} was not on the screen.", level: LogLevel.Error);

                card = new RankedPlayCard(matchInfo.LastPlayedCard)
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                };
            }

            CenterRow.Add(card);

            card.MoveToX(-100, 800, Easing.OutPow10);
        }
    }

    public class GameplayWarmupScreenTitleWedge : CompositeDrawable
    {
        private const float corner_radius = 10;

        private readonly APIBeatmap beatmap;

        private BeatmapSetOnlineStatusPill statusPill = null!;
        private MarqueeContainer titleLabel = null!;
        private MarqueeContainer artistLabel = null!;

        private BeatmapTitleWedge.StatisticPlayCount playCount = null!;
        private BeatmapTitleWedge.FavouriteButton favouriteButton = null!;
        private BeatmapTitleWedge.Statistic lengthStatistic = null!;
        private BeatmapTitleWedge.Statistic bpmStatistic = null!;

        public GameplayWarmupScreenTitleWedge(APIBeatmap beatmap)
        {
            this.beatmap = beatmap;
            RelativeSizeAxes = Axes.X;
            AutoSizeAxes = Axes.Y;
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            Masking = true;
            CornerRadius = corner_radius;

            InternalChildren = new Drawable[]
            {
                new WedgeBackground(),
                new FillFlowContainer
                {
                    RelativeSizeAxes = Axes.X,
                    AutoSizeAxes = Axes.Y,
                    Direction = FillDirection.Vertical,
                    Padding = new MarginPadding
                    {
                        Top = SongSelect.WEDGE_CONTENT_MARGIN,
                        Left = SongSelect.WEDGE_CONTENT_MARGIN
                    },
                    Spacing = new Vector2(0f, 4f),
                    Children = new Drawable[]
                    {
                        new ShearAligningWrapper(statusPill = new BeatmapSetOnlineStatusPill
                        {
                            Shear = -OsuGame.SHEAR,
                            ShowUnknownStatus = true,
                            TextSize = OsuFont.Style.Caption1.Size,
                            TextPadding = new MarginPadding { Horizontal = 6, Vertical = 1 },
                        }),
                        new ShearAligningWrapper(new Container
                        {
                            Shear = -OsuGame.SHEAR,
                            RelativeSizeAxes = Axes.X,
                            Height = OsuFont.Style.Title.Size,
                            Margin = new MarginPadding { Bottom = -4f },
                            Child = titleLabel = new MarqueeContainer
                            {
                                OverflowSpacing = 50,
                            }
                        }),
                        new ShearAligningWrapper(new Container
                        {
                            Shear = -OsuGame.SHEAR,
                            RelativeSizeAxes = Axes.X,
                            Height = OsuFont.Style.Heading2.Size,
                            Margin = new MarginPadding { Left = 1f },
                            Child = artistLabel = new MarqueeContainer
                            {
                                OverflowSpacing = 50,
                            }
                        }),
                        new ShearAligningWrapper(new FillFlowContainer
                        {
                            Shear = -OsuGame.SHEAR,
                            AutoSizeAxes = Axes.X,
                            Height = 30,
                            Direction = FillDirection.Horizontal,
                            Spacing = new Vector2(2f, 0f),
                            Children = new Drawable[]
                            {
                                playCount = new BeatmapTitleWedge.StatisticPlayCount(background: true, leftPadding: SongSelect.WEDGE_CONTENT_MARGIN, minSize: 50f)
                                {
                                    Margin = new MarginPadding { Left = -SongSelect.WEDGE_CONTENT_MARGIN },
                                },
                                favouriteButton = new BeatmapTitleWedge.FavouriteButton(),
                                lengthStatistic = new BeatmapTitleWedge.Statistic(OsuIcon.Clock),
                                bpmStatistic = new BeatmapTitleWedge.Statistic(OsuIcon.Metronome)
                                {
                                    TooltipText = BeatmapsetsStrings.ShowStatsBpm,
                                    Margin = new MarginPadding { Left = 5f },
                                },
                            },
                        }),
                        new ShearAligningWrapper(new Container
                        {
                            Shear = -OsuGame.SHEAR,
                            RelativeSizeAxes = Axes.X,
                            AutoSizeAxes = Axes.Y,
                            Margin = new MarginPadding { Left = -SongSelect.WEDGE_CONTENT_MARGIN },
                            Padding = new MarginPadding { Right = -SongSelect.WEDGE_CONTENT_MARGIN },
                            Child = new GameplayWarmupScreenDifficultyDisplay(beatmap),
                        }),
                    },
                }
            };

            statusPill.Status = beatmap.Status;

            var titleText = new RomanisableString(beatmap.BeatmapSet!.TitleUnicode, beatmap.BeatmapSet.Title);
            titleLabel.CreateContent = () => new OsuSpriteText
            {
                Text = titleText,
                Shadow = true,
                Font = OsuFont.Style.Title,
            };

            var artistText = new RomanisableString(beatmap.BeatmapSet.ArtistUnicode, beatmap.BeatmapSet.Artist);
            artistLabel.CreateContent = () => new OsuSpriteText
            {
                Text = artistText,
                Shadow = true,
                Font = OsuFont.Style.Heading2,
            };

            double rate = ModUtils.CalculateRateWithMods([]); // Todo: mods
            double drainLength = Math.Round(beatmap.Length / rate);
            double hitLength = Math.Round(beatmap.HitLength / rate);

            lengthStatistic.Text = hitLength.ToFormattedDuration();
            lengthStatistic.TooltipText = BeatmapsetsStrings.ShowStatsTotalLength(drainLength.ToFormattedDuration());
            bpmStatistic.Text = beatmap.BPM.ToLocalisableString();

            playCount.Value = new BeatmapTitleWedge.StatisticPlayCount.Data(beatmap.PlayCount, beatmap.UserPlayCount);
            favouriteButton.SetBeatmapSet(beatmap.BeatmapSet);
        }
    }

    public class GameplayWarmupScreenDifficultyDisplay : CompositeDrawable
    {
        private const float border_weight = 2;

        [Resolved]
        private OsuColour colours { get; set; } = null!;

        [Resolved]
        private MultiplayerClient client { get; set; } = null!;

        [Resolved]
        private BeatmapManager beatmapManager { get; set; } = null!;

        [Resolved]
        private RulesetStore rulesets { get; set; } = null!;

        private readonly APIBeatmap beatmap;

        private StarRatingDisplay starRatingDisplay = null!;
        private FillFlowContainer nameLine = null!;
        private OsuSpriteText difficultyText = null!;
        private OsuSpriteText mappedByText = null!;
        private OsuSpriteText mapperText = null!;

        private BeatmapTitleWedge.DifficultyStatisticsDisplay countStatisticsDisplay = null!;
        private BeatmapTitleWedge.DifficultyStatisticsDisplay difficultyStatisticsDisplay = null!;

        public GameplayWarmupScreenDifficultyDisplay(APIBeatmap beatmap)
        {
            this.beatmap = beatmap;
            RelativeSizeAxes = Axes.X;
            AutoSizeAxes = Axes.Y;
        }

        [BackgroundDependencyLoader]
        private void load(OverlayColourProvider colourProvider)
        {
            Masking = true;
            CornerRadius = 10;
            Shear = OsuGame.SHEAR;

            InternalChildren = new Drawable[]
            {
                new WedgeBackground(),
                new FillFlowContainer
                {
                    RelativeSizeAxes = Axes.X,
                    AutoSizeAxes = Axes.Y,
                    Direction = FillDirection.Vertical,
                    Children = new Drawable[]
                    {
                        new ShearAligningWrapper(new GridContainer
                        {
                            Shear = -OsuGame.SHEAR,
                            AlwaysPresent = true,
                            RelativeSizeAxes = Axes.X,
                            Height = 20,
                            Margin = new MarginPadding { Vertical = 5f },
                            Padding = new MarginPadding { Left = SongSelect.WEDGE_CONTENT_MARGIN },
                            RowDimensions = new[] { new Dimension(GridSizeMode.AutoSize) },
                            ColumnDimensions = new[]
                            {
                                new Dimension(GridSizeMode.AutoSize),
                                new Dimension(GridSizeMode.Absolute, 6),
                                new Dimension(),
                            },
                            Content = new[]
                            {
                                new[]
                                {
                                    starRatingDisplay = new StarRatingDisplay(default, animated: true)
                                    {
                                        Anchor = Anchor.CentreLeft,
                                        Origin = Anchor.CentreLeft,
                                    },
                                    Empty(),
                                    nameLine = new FillFlowContainer
                                    {
                                        Anchor = Anchor.CentreLeft,
                                        Origin = Anchor.CentreLeft,
                                        RelativeSizeAxes = Axes.X,
                                        AutoSizeAxes = Axes.Y,
                                        Direction = FillDirection.Horizontal,
                                        Margin = new MarginPadding { Bottom = 2f },
                                        Children = new Drawable[]
                                        {
                                            difficultyText = new TruncatingSpriteText
                                            {
                                                Anchor = Anchor.BottomLeft,
                                                Origin = Anchor.BottomLeft,
                                                Font = OsuFont.Style.Body.With(weight: FontWeight.SemiBold),
                                            },
                                            mappedByText = new OsuSpriteText
                                            {
                                                Anchor = Anchor.BottomLeft,
                                                Origin = Anchor.BottomLeft,
                                                Text = " mapped by ",
                                                Font = OsuFont.Style.Body,
                                            },
                                            mapperText = new TruncatingSpriteText
                                            {
                                                Shadow = true,
                                                Font = OsuFont.Style.Body.With(weight: FontWeight.SemiBold),
                                            },
                                        },
                                    },
                                }
                            },
                        }),
                        new ShearAligningWrapper(new Container
                        {
                            Shear = -OsuGame.SHEAR,
                            RelativeSizeAxes = Axes.X,
                            Height = 53,
                            Padding = new MarginPadding { Bottom = border_weight, Right = border_weight },
                            Child = new Container
                            {
                                RelativeSizeAxes = Axes.X,
                                AutoSizeAxes = Axes.Y,
                                Masking = true,
                                CornerRadius = 10 - border_weight,
                                Shear = OsuGame.SHEAR,
                                Children = new Drawable[]
                                {
                                    new Box
                                    {
                                        RelativeSizeAxes = Axes.Both,
                                        Colour = colourProvider.Background5.Opacity(0.8f),
                                    },
                                    new GridContainer
                                    {
                                        RelativeSizeAxes = Axes.X,
                                        AutoSizeAxes = Axes.Y,
                                        Padding = new MarginPadding { Left = SongSelect.WEDGE_CONTENT_MARGIN, Right = 20f, Vertical = 7.5f },
                                        Shear = -OsuGame.SHEAR,
                                        RowDimensions = new[] { new Dimension(GridSizeMode.AutoSize) },
                                        ColumnDimensions = new[]
                                        {
                                            new Dimension(),
                                            new Dimension(GridSizeMode.Absolute, 30),
                                            new Dimension(GridSizeMode.AutoSize),
                                        },
                                        Content = new[]
                                        {
                                            new[]
                                            {
                                                countStatisticsDisplay = new BeatmapTitleWedge.DifficultyStatisticsDisplay
                                                {
                                                    RelativeSizeAxes = Axes.X,
                                                },
                                                Empty(),
                                                difficultyStatisticsDisplay = new BeatmapTitleWedge.DifficultyStatisticsDisplay(autoSize: true),
                                            }
                                        },
                                    }
                                },
                            }
                        }),
                    }
                },
            };
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            MultiplayerPlaylistItem item = client.Room!.CurrentPlaylistItem;

            RulesetInfo ruleset = rulesets.GetRuleset(item.RulesetID)!;
            Ruleset rulesetInstance = ruleset.CreateInstance();
            BeatmapInfo? localBeatmap = beatmapManager.QueryBeatmap($@"{nameof(BeatmapInfo.OnlineID)} == $0 AND {nameof(BeatmapInfo.MD5Hash)} == {nameof(BeatmapInfo.OnlineMD5Hash)}", item.BeatmapID);
            WorkingBeatmap workingBeatmap = beatmapManager.GetWorkingBeatmap(localBeatmap);
            IBeatmap playableBeatmap = workingBeatmap.GetPlayableBeatmap(ruleset);

            difficultyText.Text = beatmap.DifficultyName;
            mapperText.Text = beatmap.Metadata.Author.Username;
            starRatingDisplay.Current.Value = new StarDifficulty(beatmap.StarRating, beatmap.MaxCombo ?? 0);

            countStatisticsDisplay.Statistics = playableBeatmap.GetStatistics()
                                                               .Select(s => new BeatmapTitleWedge.StatisticDifficulty.Data(s.Name, s.BarDisplayLength ?? 0, s.BarDisplayLength ?? 0, 1, s.Content))
                                                               .ToList();

            difficultyStatisticsDisplay.Statistics = rulesetInstance.GetBeatmapAttributesForDisplay(beatmap, [])
                                                                    .Select(a => new BeatmapTitleWedge.StatisticDifficulty.Data(a))
                                                                    .ToList();
        }

        protected override void Update()
        {
            base.Update();

            difficultyText.MaxWidth = Math.Max(nameLine.DrawWidth - mappedByText.DrawWidth - mapperText.DrawWidth - 20, 0);

            // Use difficulty colour until it gets too dark to be visible against dark backgrounds.
            Color4 col = starRatingDisplay.DisplayedStars.Value >= OsuColour.STAR_DIFFICULTY_DEFINED_COLOUR_CUTOFF ? colours.Orange1 : starRatingDisplay.DisplayedDifficultyColour;

            difficultyText.Colour = col;
            mappedByText.Colour = col;
            countStatisticsDisplay.AccentColour = col;
            difficultyStatisticsDisplay.AccentColour = col;
        }
    }

    public class GameplayWarmupScreenMetadataWedge : CompositeDrawable
    {
        private readonly APIBeatmap beatmap;

        private BeatmapMetadataWedge.MetadataDisplay creator = null!;
        private BeatmapMetadataWedge.MetadataDisplay source = null!;
        private BeatmapMetadataWedge.MetadataDisplay genre = null!;
        private BeatmapMetadataWedge.MetadataDisplay language = null!;
        private BeatmapMetadataWedge.MetadataDisplay userTags = null!;
        private BeatmapMetadataWedge.MetadataDisplay mapperTags = null!;
        private BeatmapMetadataWedge.MetadataDisplay submitted = null!;
        private BeatmapMetadataWedge.MetadataDisplay ranked = null!;

        private BeatmapMetadataWedge.SuccessRateDisplay successRateDisplay = null!;
        private BeatmapMetadataWedge.UserRatingDisplay userRatingDisplay = null!;
        private BeatmapMetadataWedge.RatingSpreadDisplay ratingSpreadDisplay = null!;
        private BeatmapMetadataWedge.FailRetryDisplay failRetryDisplay = null!;

        public GameplayWarmupScreenMetadataWedge(APIBeatmap beatmap)
        {
            this.beatmap = beatmap;

            RelativeSizeAxes = Axes.X;
            AutoSizeAxes = Axes.Y;

            Width = 0.9f;
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            InternalChild = new FillFlowContainer
            {
                RelativeSizeAxes = Axes.X,
                AutoSizeAxes = Axes.Y,
                Direction = FillDirection.Vertical,
                Spacing = new Vector2(0f, 4f),
                Shear = OsuGame.SHEAR,
                Children = new[]
                {
                    new ShearAligningWrapper(new Container
                    {
                        CornerRadius = 10,
                        Masking = true,
                        RelativeSizeAxes = Axes.X,
                        AutoSizeAxes = Axes.Y,
                        Children = new Drawable[]
                        {
                            new WedgeBackground(),
                            new Container
                            {
                                RelativeSizeAxes = Axes.X,
                                AutoSizeAxes = Axes.Y,
                                Shear = -OsuGame.SHEAR,
                                Padding = new MarginPadding { Left = SongSelect.WEDGE_CONTENT_MARGIN, Right = 35, Vertical = 16 },
                                Children = new Drawable[]
                                {
                                    new FillFlowContainer
                                    {
                                        RelativeSizeAxes = Axes.X,
                                        AutoSizeAxes = Axes.Y,
                                        Direction = FillDirection.Vertical,
                                        Spacing = new Vector2(0f, 10f),
                                        Children = new Drawable[]
                                        {
                                            new GridContainer
                                            {
                                                RelativeSizeAxes = Axes.X,
                                                AutoSizeAxes = Axes.Y,
                                                RowDimensions = new[] { new Dimension(GridSizeMode.AutoSize) },
                                                ColumnDimensions = new[]
                                                {
                                                    new Dimension(),
                                                    new Dimension(),
                                                    new Dimension(),
                                                },
                                                Content = new[]
                                                {
                                                    new[]
                                                    {
                                                        new FillFlowContainer
                                                        {
                                                            RelativeSizeAxes = Axes.X,
                                                            AutoSizeAxes = Axes.Y,
                                                            Direction = FillDirection.Vertical,
                                                            Spacing = new Vector2(0f, 10f),
                                                            Children = new[]
                                                            {
                                                                creator = new BeatmapMetadataWedge.MetadataDisplay(EditorSetupStrings.Creator),
                                                                genre = new BeatmapMetadataWedge.MetadataDisplay(BeatmapsetsStrings.ShowInfoGenre),
                                                            },
                                                        },
                                                        new FillFlowContainer
                                                        {
                                                            RelativeSizeAxes = Axes.X,
                                                            AutoSizeAxes = Axes.Y,
                                                            Direction = FillDirection.Vertical,
                                                            Spacing = new Vector2(0f, 10f),
                                                            Children = new[]
                                                            {
                                                                source = new BeatmapMetadataWedge.MetadataDisplay(BeatmapsetsStrings.ShowInfoSource),
                                                                language = new BeatmapMetadataWedge.MetadataDisplay(BeatmapsetsStrings.ShowInfoLanguage),
                                                            },
                                                        },
                                                        new FillFlowContainer
                                                        {
                                                            RelativeSizeAxes = Axes.X,
                                                            AutoSizeAxes = Axes.Y,
                                                            Direction = FillDirection.Vertical,
                                                            Spacing = new Vector2(0f, 10f),
                                                            Children = new[]
                                                            {
                                                                submitted = new BeatmapMetadataWedge.MetadataDisplay(SongSelectStrings.Submitted),
                                                                ranked = new BeatmapMetadataWedge.MetadataDisplay(SongSelectStrings.Ranked),
                                                            },
                                                        },
                                                    },
                                                },
                                            },
                                            userTags = new BeatmapMetadataWedge.MetadataDisplay(BeatmapsetsStrings.ShowInfoUserTags)
                                            {
                                                Alpha = 0,
                                            },
                                            mapperTags = new BeatmapMetadataWedge.MetadataDisplay(BeatmapsetsStrings.ShowInfoMapperTags),
                                        },
                                    },
                                },
                            },
                        },
                    }),
                    new ShearAligningWrapper(new Container
                    {
                        CornerRadius = 10,
                        Masking = true,
                        RelativeSizeAxes = Axes.X,
                        AutoSizeAxes = Axes.Y,
                        Children = new Drawable[]
                        {
                            new WedgeBackground(),
                            new GridContainer
                            {
                                RelativeSizeAxes = Axes.X,
                                AutoSizeAxes = Axes.Y,
                                Shear = -OsuGame.SHEAR,
                                RowDimensions = new[] { new Dimension(GridSizeMode.AutoSize) },
                                ColumnDimensions = new[]
                                {
                                    new Dimension(),
                                    new Dimension(GridSizeMode.Absolute, 10),
                                    new Dimension(),
                                    new Dimension(GridSizeMode.Absolute, 10),
                                    new Dimension(),
                                },
                                Padding = new MarginPadding { Left = SongSelect.WEDGE_CONTENT_MARGIN, Right = 40f, Vertical = 16 },
                                Content = new[]
                                {
                                    new[]
                                    {
                                        successRateDisplay = new BeatmapMetadataWedge.SuccessRateDisplay(),
                                        Empty(),
                                        userRatingDisplay = new BeatmapMetadataWedge.UserRatingDisplay(),
                                        Empty(),
                                        ratingSpreadDisplay = new BeatmapMetadataWedge.RatingSpreadDisplay(),
                                    },
                                },
                            },
                        }
                    }),
                    new ShearAligningWrapper(new Container
                    {
                        CornerRadius = 10,
                        Masking = true,
                        RelativeSizeAxes = Axes.X,
                        AutoSizeAxes = Axes.Y,
                        Children = new Drawable[]
                        {
                            new WedgeBackground(),
                            new Container
                            {
                                RelativeSizeAxes = Axes.X,
                                AutoSizeAxes = Axes.Y,
                                Shear = -OsuGame.SHEAR,
                                Padding = new MarginPadding { Left = SongSelect.WEDGE_CONTENT_MARGIN, Right = 40f, Vertical = 16 },
                                Child = failRetryDisplay = new BeatmapMetadataWedge.FailRetryDisplay(),
                            },
                        },
                    }),
                }
            };
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            var metadata = beatmap.Metadata;
            var beatmapSet = beatmap.BeatmapSet!;

            creator.Data = (metadata.Author.Username, null);

            if (!string.IsNullOrEmpty(metadata.Source))
                source.Data = (metadata.Source, null);
            else
                source.Data = ("-", null);

            if (!string.IsNullOrEmpty(metadata.Tags))
                mapperTags.Tags = (metadata.Tags.Split(' '), _ => { });
            else
                mapperTags.Tags = (Array.Empty<string>(), _ => { });

            submitted.Date = beatmapSet.Submitted;
            ranked.Date = beatmapSet.Ranked;

            genre.Data = (beatmapSet.Genre.Name, null);
            language.Data = (beatmapSet.Language.Name, null);

            userRatingDisplay.Data = beatmapSet.Ratings;
            ratingSpreadDisplay.Data = beatmapSet.Ratings;
            successRateDisplay.Data = (beatmap.PassCount, beatmap.PlayCount);
            failRetryDisplay.Data = beatmap.FailTimes ?? new APIFailTimes();

            var tagsById = beatmapSet.RelatedTags?.ToDictionary(t => t.Id) ?? new Dictionary<long, APITag>();
            string[] topUserTags = beatmap.TopTags?
                                          .Select(t => (topTag: t, relatedTag: tagsById.GetValueOrDefault(t.TagId)))
                                          .Where(t => t.relatedTag != null)
                                          // see https://github.com/ppy/osu-web/blob/bb3bd2e7c6f84f26066df5ea20a81c77ec9bb60a/resources/js/beatmapsets-show/controller.ts#L103-L106 for sort criteria
                                          .OrderByDescending(t => t.topTag.VoteCount)
                                          .ThenBy(t => t.relatedTag!.Name)
                                          .Select(t => t.relatedTag!.Name)
                                          .ToArray() ?? [];

            userTags.Tags = (topUserTags, _ => { });

            if (topUserTags.Length > 0)
                userTags.Show();
        }
    }
}
