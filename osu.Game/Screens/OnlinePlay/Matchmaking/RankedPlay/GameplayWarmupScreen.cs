// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Allocation;
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
using osu.Game.Localisation;
using osu.Game.Online.API.Requests.Responses;
using osu.Game.Online.Chat;
using osu.Game.Online.Rooms;
using osu.Game.Overlays;
using osu.Game.Resources.Localisation.Web;
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
                    new Dimension(GridSizeMode.Absolute, 300),
                    new Dimension(GridSizeMode.Absolute, 10),
                    new Dimension(GridSizeMode.Absolute, 100)
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
                            RelativeSizeAxes = Axes.Both,
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
                                        RelativeSizeAxes = Axes.Both
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
                            RelativeSizeAxes = Axes.Both,
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
            [BackgroundDependencyLoader]
            private void load()
            {
                Masking = true;
                CornerRadius = 5;

                InternalChild = new WedgeBackground
                {
                    RelativeSizeAxes = Axes.Both
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
            }

            [BackgroundDependencyLoader]
            private void load()
            {
                InternalChild = new ShearAligningWrapper(new Container
                {
                    RelativeSizeAxes = Axes.X,
                    AutoSizeAxes = Axes.Y,
                    CornerRadius = 5,
                    Masking = true,
                    Children = new Drawable[]
                    {
                        new WedgeBackground(),
                        new Container
                        {
                            RelativeSizeAxes = Axes.X,
                            AutoSizeAxes = Axes.Y,
                            Shear = -OsuGame.SHEAR,
                            Padding = new MarginPadding { Vertical = 16, Left = 16, Right = 35 },
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
                });
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
            }

            [BackgroundDependencyLoader]
            private void load()
            {
                InternalChild = new ShearAligningWrapper(new Container
                {
                    RelativeSizeAxes = Axes.X,
                    AutoSizeAxes = Axes.Y,
                    CornerRadius = 5,
                    Masking = true,
                    Children = new Drawable[]
                    {
                        new WedgeBackground(),
                        new GridContainer
                        {
                            RelativeSizeAxes = Axes.X,
                            AutoSizeAxes = Axes.Y,
                            Shear = -OsuGame.SHEAR,
                            Padding = new MarginPadding { Vertical = 16, Left = 16, Right = 35 },
                            RowDimensions = new[] { new Dimension(GridSizeMode.AutoSize) },
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
                        },
                    }
                });
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

            private BeatmapMetadataWedge.FailRetryDisplay failRetryDisplay = null!;

            public FailRetryWedge(APIBeatmap beatmap)
            {
                this.beatmap = beatmap;
            }

            [BackgroundDependencyLoader]
            private void load()
            {
                InternalChild = new ShearAligningWrapper(new Container
                {
                    RelativeSizeAxes = Axes.X,
                    AutoSizeAxes = Axes.Y,
                    CornerRadius = 5,
                    Masking = true,
                    Children = new Drawable[]
                    {
                        new WedgeBackground(),
                        new Container
                        {
                            RelativeSizeAxes = Axes.X,
                            AutoSizeAxes = Axes.Y,
                            Shear = -OsuGame.SHEAR,
                            Padding = new MarginPadding { Vertical = 16, Left = 16, Right = 35 },
                            Child = failRetryDisplay = new BeatmapMetadataWedge.FailRetryDisplay(),
                        },
                    },
                });
            }

            protected override void LoadComplete()
            {
                base.LoadComplete();

                failRetryDisplay.Data = beatmap.FailTimes ?? new APIFailTimes();
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
    }
}
