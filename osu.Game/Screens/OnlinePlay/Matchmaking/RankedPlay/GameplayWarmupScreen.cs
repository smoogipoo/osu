// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Extensions;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Colour;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Game.Beatmaps;
using osu.Game.Beatmaps.Drawables;
using osu.Game.Database;
using osu.Game.Graphics;
using osu.Game.Graphics.Containers;
using osu.Game.Graphics.Sprites;
using osu.Game.Localisation;
using osu.Game.Online.API.Requests.Responses;
using osu.Game.Online.Chat;
using osu.Game.Online.Rooms;
using osu.Game.Overlays;
using osu.Game.Resources.Localisation.Web;
using osu.Game.Rulesets;
using osu.Game.Rulesets.Mods;
using osu.Game.Screens.SelectV2;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Screens.OnlinePlay.Matchmaking.RankedPlay
{
    public partial class GameplayWarmupScreen : RankedPlaySubScreen
    {
        [Resolved]
        private BeatmapLookupCache beatmapLookupCache { get; set; } = null!;

        private readonly MultiplayerPlaylistItem item;

        private Drawable headerWedge = null!;
        private Drawable statisticsWedge = null!;
        private Drawable metadataWedge = null!;
        private Drawable ratingsWedge = null!;
        private Drawable failRetryWedge = null!;

        public GameplayWarmupScreen(MultiplayerPlaylistItem item)
        {
            this.item = item;
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            APIBeatmap beatmap = beatmapLookupCache.GetBeatmapAsync(item.BeatmapID).GetResultSafely()!;

            InternalChild = new GridContainer
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                RelativeSizeAxes = Axes.X,
                AutoSizeAxes = Axes.Y,
                Width = 0.7f,
                Shear = OsuGame.SHEAR,
                RowDimensions = new[]
                {
                    new Dimension(GridSizeMode.Absolute, 100),
                    new Dimension(GridSizeMode.Absolute, 10),
                    new Dimension(GridSizeMode.AutoSize),
                    new Dimension(GridSizeMode.Absolute, 10),
                    new Dimension(GridSizeMode.AutoSize)
                },
                Content = new[]
                {
                    new[]
                    {
                        headerWedge = new HeaderWedge(item, beatmap)
                    },
                    null,
                    new Drawable[]
                    {
                        new GridContainer
                        {
                            RelativeSizeAxes = Axes.X,
                            AutoSizeAxes = Axes.Y,
                            RowDimensions = new[]
                            {
                                new Dimension(GridSizeMode.AutoSize)
                            },
                            ColumnDimensions = new[]
                            {
                                new Dimension(),
                                new Dimension(GridSizeMode.Absolute, 10),
                                new Dimension()
                            },
                            Content = new[]
                            {
                                new[]
                                {
                                    statisticsWedge = new StatisticsWedge
                                    {
                                        Anchor = Anchor.CentreLeft,
                                        Origin = Anchor.CentreLeft
                                    },
                                    null,
                                    new GridContainer
                                    {
                                        Anchor = Anchor.CentreLeft,
                                        Origin = Anchor.CentreLeft,
                                        RelativeSizeAxes = Axes.X,
                                        AutoSizeAxes = Axes.Y,
                                        RowDimensions = new[]
                                        {
                                            new Dimension(GridSizeMode.AutoSize),
                                            new Dimension(GridSizeMode.Absolute, 10),
                                            new Dimension(GridSizeMode.AutoSize)
                                        },
                                        Content = new[]
                                        {
                                            new[]
                                            {
                                                metadataWedge = new MetadataWedge(beatmap)
                                            },
                                            null,
                                            new[]
                                            {
                                                ratingsWedge = new RatingsWedge(beatmap)
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    },
                    null,
                    new[]
                    {
                        failRetryWedge = new FailRetryWedge(beatmap)
                        {
                            Anchor = Anchor.Centre,
                            Origin = Anchor.Centre,
                            Width = 0.7f
                        }
                    }
                }
            };
        }

        public override void OnEntering(RankedPlaySubScreen? previous)
        {
            base.OnEntering(previous);

            headerWedge.MoveToY(-200).MoveToY(0, 800, Easing.OutPow10);
            statisticsWedge.MoveToX(-200).MoveToX(0, 800, Easing.OutPow10);
            metadataWedge.MoveToX(200).MoveToX(0, 800, Easing.OutPow10);
            ratingsWedge.MoveToX(400).MoveToX(0, 800, Easing.OutPow10);
            failRetryWedge.MoveToY(200).MoveToY(0, 800, Easing.OutPow10);
        }

        public partial class HeaderWedge : CompositeDrawable
        {
            private readonly MultiplayerPlaylistItem item;
            private readonly APIBeatmap beatmap;

            public HeaderWedge(MultiplayerPlaylistItem item, APIBeatmap beatmap)
            {
                this.item = item;
                this.beatmap = beatmap;

                RelativeSizeAxes = Axes.Both;
            }

            [BackgroundDependencyLoader]
            private void load(OsuColour colours)
            {
                Colour4 srColour = colours.ForStarDifficulty(item.StarRating);

                OsuTextFlowContainer titleLine;
                LinkFlowContainer difficultyLine;

                InternalChild = new ShearAligningWrapper(new Container
                {
                    RelativeSizeAxes = Axes.Both,
                    CornerRadius = 5,
                    Masking = true,
                    Children = new Drawable[]
                    {
                        new Box
                        {
                            RelativeSizeAxes = Axes.Both,
                            Colour = ColourInfo.GradientHorizontal(Color4.Black.Opacity(0), srColour),
                        },
                        new Container
                        {
                            RelativeSizeAxes = Axes.Both,
                            Padding = new MarginPadding { Right = 5, Bottom = 5 },
                            Child = new Container
                            {
                                RelativeSizeAxes = Axes.Both,
                                CornerRadius = 5,
                                Masking = true,
                                Children = new Drawable[]
                                {
                                    new Box
                                    {
                                        RelativeSizeAxes = Axes.Both,
                                        Alpha = 0.7f,
                                        Colour = Colour4.FromHex("363138")
                                    },
                                    new UpdateableBeatmapBackgroundSprite
                                    {
                                        Anchor = Anchor.Centre,
                                        Origin = Anchor.Centre,
                                        RelativeSizeAxes = Axes.Both,
                                        Beatmap = { Value = beatmap },
                                        BackgroundLoadDelay = 0,
                                        Colour = ColourInfo.GradientHorizontal(Color4.White.Opacity(0.15f), Color4.White.Opacity(0.5f)),
                                    },
                                }
                            }
                        },
                        new FillFlowContainer
                        {
                            RelativeSizeAxes = Axes.Both,
                            Shear = -OsuGame.SHEAR,
                            Padding = new MarginPadding { Vertical = 16, Left = 16, Right = 35 },
                            Direction = FillDirection.Vertical,
                            Spacing = new Vector2(5),
                            Children = new Drawable[]
                            {
                                titleLine = new OsuTextFlowContainer(s => s.Font = s.Font.With(weight: FontWeight.SemiBold))
                                {
                                    Anchor = Anchor.CentreLeft,
                                    Origin = Anchor.CentreLeft,
                                    AutoSizeAxes = Axes.Both
                                },
                                new FillFlowContainer
                                {
                                    Anchor = Anchor.CentreLeft,
                                    Origin = Anchor.CentreLeft,
                                    AutoSizeAxes = Axes.Both,
                                    Direction = FillDirection.Horizontal,
                                    Spacing = new Vector2(5),
                                    Children = new Drawable[]
                                    {
                                        new StarRatingDisplay(new StarDifficulty(item.StarRating, 0))
                                        {
                                            Anchor = Anchor.CentreLeft,
                                            Origin = Anchor.CentreLeft,
                                        },
                                        difficultyLine = new LinkFlowContainer(s => s.Font = s.Font.With(weight: FontWeight.SemiBold))
                                        {
                                            Anchor = Anchor.CentreLeft,
                                            Origin = Anchor.CentreLeft,
                                            AutoSizeAxes = Axes.Both
                                        }
                                    }
                                }
                            }
                        }
                    }
                });

                titleLine.AddText(beatmap.Metadata.Title, s => s.Font = OsuFont.Style.Heading1);
                titleLine.AddText($" by {beatmap.Metadata.Artist}", s => s.Font = OsuFont.Style.Heading2);

                difficultyLine.AddText(beatmap.DifficultyName, s => s.Font = OsuFont.Style.Heading2);
                difficultyLine.AddLink($" mapped by {beatmap.Metadata.Author.Username}", LinkAction.OpenUserProfile, beatmap.Metadata.Author, creationParameters: s =>
                {
                    s.Font = OsuFont.Style.Heading2;
                    // s.Margin = new MarginPadding { Top = -5 };
                });
            }
        }

        public partial class StatisticsWedge : CompositeDrawable
        {
            [Resolved]
            private IBindable<WorkingBeatmap> beatmap { get; set; } = null!;

            [Resolved]
            private IBindable<RulesetInfo> ruleset { get; set; } = null!;

            [Resolved]
            private IBindable<IReadOnlyList<Mod>> mods { get; set; } = null!;

            public StatisticsWedge()
            {
                RelativeSizeAxes = Axes.X;
                AutoSizeAxes = Axes.Y;

                Masking = true;
                CornerRadius = 5;
            }

            [BackgroundDependencyLoader]
            private void load()
            {
                IBeatmap playableBeatmap = beatmap.Value.GetPlayableBeatmap(ruleset.Value);
                Ruleset rulesetInstance = ruleset.Value.CreateInstance();
                List<BeatmapTitleWedge.StatisticDifficulty.Data> statistics = [];

                foreach (var stat in playableBeatmap.GetStatistics()
                                                    .Select(s => new BeatmapTitleWedge.StatisticDifficulty.Data(s.Name, s.BarDisplayLength ?? 0, s.BarDisplayLength ?? 0, 1, s.Content)))
                {
                    statistics.Add(stat);
                }

                foreach (var stat in rulesetInstance.GetBeatmapAttributesForDisplay(beatmap.Value.BeatmapInfo, mods.Value)
                                                    .Select(a => new BeatmapTitleWedge.StatisticDifficulty.Data(a)))
                {
                    statistics.Add(stat);
                }

                List<Dimension> rowDimensions = [];
                List<Drawable?[]?> rowContents = [];

                foreach (var row in statistics.Chunk(3))
                {
                    if (rowContents.Count > 0)
                    {
                        rowDimensions.Add(new Dimension(GridSizeMode.Absolute, 10));
                        rowContents.Add(null);
                    }

                    List<Drawable?> thisRow = [];

                    foreach (var cell in row)
                    {
                        thisRow.Add(new UnshearingWrapper(new BeatmapTitleWedge.StatisticDifficulty
                        {
                            RelativeSizeAxes = Axes.X,
                            Value = cell
                        }));
                    }

                    while (thisRow.Count < 3)
                        thisRow.Add(null);

                    rowDimensions.Add(new Dimension(GridSizeMode.AutoSize));
                    rowContents.Add(thisRow.ToArray());
                }

                InternalChildren = new Drawable[]
                {
                    new WedgeBackground
                    {
                        RelativeSizeAxes = Axes.Both
                    },
                    new GridContainer
                    {
                        RelativeSizeAxes = Axes.X,
                        AutoSizeAxes = Axes.Y,
                        Padding = new MarginPadding(16),
                        RowDimensions = rowDimensions.ToArray(),
                        Content = rowContents.ToArray()
                    }
                };
            }
        }

        public partial class MetadataWedge : CompositeDrawable
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

            public MetadataWedge(APIBeatmap beatmap)
            {
                this.beatmap = beatmap;

                RelativeSizeAxes = Axes.X;
                AutoSizeAxes = Axes.Y;

                CornerRadius = 5;
                Masking = true;
            }

            [BackgroundDependencyLoader]
            private void load()
            {
                InternalChildren = new Drawable[]
                {
                    new WedgeBackground(),
                    new FillFlowContainer
                    {
                        RelativeSizeAxes = Axes.X,
                        AutoSizeAxes = Axes.Y,
                        Padding = new MarginPadding(16),
                        Direction = FillDirection.Vertical,
                        Spacing = new Vector2(5),
                        Children = new Drawable[]
                        {
                            new UnshearingWrapper(new GridContainer
                            {
                                RelativeSizeAxes = Axes.X,
                                AutoSizeAxes = Axes.Y,
                                RowDimensions = new[]
                                {
                                    new Dimension(GridSizeMode.AutoSize)
                                },
                                Content = new Drawable[][]
                                {
                                    [
                                        creator = new BeatmapMetadataWedge.MetadataDisplay(EditorSetupStrings.Creator),
                                        source = new BeatmapMetadataWedge.MetadataDisplay(BeatmapsetsStrings.ShowInfoSource),
                                        submitted = new BeatmapMetadataWedge.MetadataDisplay(SongSelectStrings.Submitted),
                                    ]
                                }
                            }),
                            new UnshearingWrapper(new GridContainer
                            {
                                RelativeSizeAxes = Axes.X,
                                AutoSizeAxes = Axes.Y,
                                RowDimensions = new[]
                                {
                                    new Dimension(GridSizeMode.AutoSize)
                                },
                                Content = new Drawable[][]
                                {
                                    [
                                        genre = new BeatmapMetadataWedge.MetadataDisplay(BeatmapsetsStrings.ShowInfoGenre),
                                        language = new BeatmapMetadataWedge.MetadataDisplay(BeatmapsetsStrings.ShowInfoLanguage),
                                        ranked = new BeatmapMetadataWedge.MetadataDisplay(SongSelectStrings.Ranked),
                                    ]
                                }
                            }),
                            new UnshearingWrapper(userTags = new BeatmapMetadataWedge.MetadataDisplay(BeatmapsetsStrings.ShowInfoUserTags)
                            {
                                Alpha = 0,
                            }),
                            new UnshearingWrapper(mapperTags = new BeatmapMetadataWedge.MetadataDisplay(BeatmapsetsStrings.ShowInfoMapperTags)),
                        }
                    }
                };
            }

            protected override void LoadComplete()
            {
                base.LoadComplete();

                creator.Data = (beatmap.Metadata.Author.Username, null);

                if (!string.IsNullOrEmpty(beatmap.Metadata.Source))
                    source.Data = (beatmap.Metadata.Source, null);
                else
                    source.Data = ("-", null);

                if (!string.IsNullOrEmpty(beatmap.Metadata.Tags))
                    mapperTags.Tags = (beatmap.Metadata.Tags.Split(' '), _ => { });
                else
                    mapperTags.Tags = (Array.Empty<string>(), _ => { });

                submitted.Date = beatmap.BeatmapSet!.Submitted;
                ranked.Date = beatmap.BeatmapSet!.Ranked;

                genre.Data = (beatmap.BeatmapSet!.Genre.Name, null);
                language.Data = (beatmap.BeatmapSet!.Language.Name, null);

                userTags.Tags = (beatmap.GetTopUserTags(), _ => { });
            }
        }

        public partial class RatingsWedge : CompositeDrawable
        {
            private readonly APIBeatmap beatmap;

            private BeatmapMetadataWedge.SuccessRateDisplay successRateDisplay = null!;
            private BeatmapMetadataWedge.UserRatingDisplay userRatingDisplay = null!;
            private BeatmapMetadataWedge.RatingSpreadDisplay ratingSpreadDisplay = null!;

            public RatingsWedge(APIBeatmap beatmap)
            {
                this.beatmap = beatmap;

                RelativeSizeAxes = Axes.X;
                AutoSizeAxes = Axes.Y;

                CornerRadius = 5;
                Masking = true;
            }

            [BackgroundDependencyLoader]
            private void load()
            {
                InternalChildren = new Drawable[]
                {
                    new WedgeBackground(),
                    new UnshearingWrapper(new GridContainer
                    {
                        RelativeSizeAxes = Axes.X,
                        AutoSizeAxes = Axes.Y,
                        RowDimensions = new[] { new Dimension(GridSizeMode.AutoSize) },
                        Padding = new MarginPadding(16),
                        ColumnDimensions = new[]
                        {
                            new Dimension(),
                            new Dimension(GridSizeMode.Absolute, 10),
                            new Dimension(),
                            new Dimension(GridSizeMode.Absolute, 10),
                            new Dimension(),
                        },
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
                    }),
                };
            }

            protected override void LoadComplete()
            {
                base.LoadComplete();

                userRatingDisplay.Data = beatmap.BeatmapSet!.Ratings;
                ratingSpreadDisplay.Data = beatmap.BeatmapSet.Ratings;
                successRateDisplay.Data = (beatmap.PassCount, beatmap.PlayCount);
            }
        }

        public partial class FailRetryWedge : CompositeDrawable
        {
            private readonly APIBeatmap beatmap;

            private BeatmapMetadataWedge.FailRetryDisplay.GraphDrawable retriesGraph = null!;
            private BeatmapMetadataWedge.FailRetryDisplay.GraphDrawable failsGraph = null!;

            public FailRetryWedge(APIBeatmap beatmap)
            {
                this.beatmap = beatmap;

                RelativeSizeAxes = Axes.X;
                AutoSizeAxes = Axes.Y;

                CornerRadius = 5;
                Masking = true;
            }

            [BackgroundDependencyLoader]
            private void load(OsuColour colours)
            {
                InternalChildren = new Drawable[]
                {
                    new WedgeBackground(),
                    new FillFlowContainer
                    {
                        RelativeSizeAxes = Axes.X,
                        AutoSizeAxes = Axes.Y,
                        Padding = new MarginPadding(16),
                        Direction = FillDirection.Vertical,
                        Spacing = new Vector2(0f, 4f),
                        Children = new Drawable[]
                        {
                            new UnshearingWrapper(new OsuSpriteText
                            {
                                Text = BeatmapsetsStrings.ShowInfoPointsOfFailure,
                                Font = OsuFont.Style.Caption1.With(weight: FontWeight.SemiBold),
                                Margin = new MarginPadding { Bottom = 4f },
                            }),
                            new UnshearingWrapper(new Container
                            {
                                RelativeSizeAxes = Axes.X,
                                Height = 65f,
                                Children = new[]
                                {
                                    retriesGraph = new BeatmapMetadataWedge.FailRetryDisplay.GraphDrawable
                                    {
                                        RelativeSizeAxes = Axes.Both,
                                        Y = -1f,
                                        Colour = colours.Orange1
                                    },
                                    failsGraph = new BeatmapMetadataWedge.FailRetryDisplay.GraphDrawable
                                    {
                                        RelativeSizeAxes = Axes.Both,
                                        Colour = colours.DarkOrange2
                                    },
                                },
                            }),
                        },
                    },
                };
            }

            protected override void LoadComplete()
            {
                base.LoadComplete();

                setData(beatmap.FailTimes ?? new APIFailTimes());
            }

            private void setData(APIFailTimes data)
            {
                int[] retries = data.Retries ?? Array.Empty<int>();
                int[] fails = data.Fails ?? Array.Empty<int>();
                int[] total = retries.Zip(fails, (r, f) => r + f).ToArray();

                int maximum = total.DefaultIfEmpty(0).Max();

                retriesGraph.Data = total.Select(r => maximum == 0 ? 0 : (float)r / maximum).ToArray();
                failsGraph.Data = fails.Select(r => maximum == 0 ? 0 : (float)r / maximum).ToArray();
            }
        }

        private sealed partial class WedgeBackground : InputBlockingContainer
        {
            [BackgroundDependencyLoader]
            private void load(OverlayColourProvider colourProvider)
            {
                RelativeSizeAxes = Axes.Both;

                InternalChild = new Box
                {
                    RelativeSizeAxes = Axes.Both,
                    Colour = ColourInfo.GradientVertical(Colour4.FromHex("1D1B1E").Opacity(0.75f), Colour4.FromHex("1D1B1E").Opacity(0.35f))
                };
            }
        }

        private partial class UnshearingWrapper : CompositeDrawable
        {
            public UnshearingWrapper(Drawable drawable)
            {
                RelativeSizeAxes = drawable.RelativeSizeAxes;
                AutoSizeAxes = Axes.Both & ~drawable.RelativeSizeAxes;

                Shear = -OsuGame.SHEAR;
                Padding = new MarginPadding { Right = 19, Bottom = 6 };

                InternalChild = drawable;
            }
        }
    }
}
