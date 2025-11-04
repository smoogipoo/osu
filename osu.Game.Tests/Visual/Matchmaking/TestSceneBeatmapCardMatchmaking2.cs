// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Localisation;
using osu.Game.Beatmaps;
using osu.Game.Beatmaps.Drawables;
using osu.Game.Beatmaps.Drawables.Cards;
using osu.Game.Graphics;
using osu.Game.Graphics.Containers;
using osu.Game.Graphics.Cursor;
using osu.Game.Graphics.Sprites;
using osu.Game.Online.API.Requests.Responses;
using osu.Game.Overlays;
using osu.Game.Overlays.BeatmapSet;
using osu.Game.Resources.Localisation.Web;
using osu.Game.Screens.Ranking;
using osu.Game.Tests.Visual.Multiplayer;
using osuTK;

namespace osu.Game.Tests.Visual.Matchmaking
{
    public class TestSceneBeatmapCardMatchmaking2 : MultiplayerTestScene
    {
        [Cached]
        private readonly OverlayColourProvider colourProvider = new OverlayColourProvider(OverlayColourScheme.Purple);

        [Test]
        public void TestBeatmapPanel()
        {
            AddStep("add panel", () =>
            {
                APIBeatmap beatmap = CreateAPIBeatmap();
                beatmap.BeatmapSet!.HasVideo = true;
                beatmap.BeatmapSet!.HasStoryboard = true;
                beatmap.BeatmapSet!.HasExplicitContent = true;
                beatmap.BeatmapSet!.FeaturedInSpotlight = true;
                beatmap.BeatmapSet!.TrackId = 1;
                beatmap.BeatmapSet!.RelatedTags =
                [
                    new APITag { Id = 1, Name = "tech/slider tech" },
                    new APITag { Id = 2, Name = "skillset/tech" }
                ];
                beatmap.TopTags =
                [
                    new APIBeatmapTag { TagId = 1 },
                    new APIBeatmapTag { TagId = 2 }
                ];

                Child = new OsuContextMenuContainer
                {
                    RelativeSizeAxes = Axes.Both,
                    Child = new BeatmapCardMatchmaking2(beatmap)
                    {
                        Anchor = Anchor.Centre,
                        Origin = Anchor.Centre,
                    }
                };
            });
        }

        private class BeatmapCardMatchmaking2 : CompositeDrawable
        {
            private static readonly Vector2 size = new Vector2(350, 100);

            [Resolved]
            private OverlayColourProvider colourProvider { get; set; } = null!;

            private readonly APIBeatmap beatmap;

            private Drawable beatmapInfoDisplay = null!;
            private Drawable beatmapStatisticsDisplay = null!;
            private FillFlowContainer<UserTagControl.DrawableUserTag> tagsContainer = null!;

            public BeatmapCardMatchmaking2(APIBeatmap beatmap)
            {
                this.beatmap = beatmap;

                Size = size;
                Masking = true;
                CornerRadius = 8;
            }

            [BackgroundDependencyLoader]
            private void load()
            {
                InternalChildren = new Drawable[]
                {
                    new BeatmapCardContentBackground(beatmap.BeatmapSet!, true)
                    {
                        RelativeSizeAxes = Axes.Both,
                    },
                    new Container
                    {
                        RelativeSizeAxes = Axes.Both,
                        Padding = new MarginPadding
                        {
                            Horizontal = 10,
                            Vertical = 4
                        },
                        Children = new[]
                        {
                            beatmapInfoDisplay = new FillFlowContainer
                            {
                                RelativeSizeAxes = Axes.Both,
                                Direction = FillDirection.Vertical,
                                Alpha = 0,
                                Children = new Drawable[]
                                {
                                    new TruncatingSpriteText
                                    {
                                        Text = new RomanisableString(beatmap.BeatmapSet!.TitleUnicode, beatmap.BeatmapSet!.Title),
                                        Font = OsuFont.Default.With(size: 18f, weight: FontWeight.SemiBold),
                                        RelativeSizeAxes = Axes.X,
                                    },
                                    new TruncatingSpriteText
                                    {
                                        Text = createArtistText(),
                                        Font = OsuFont.Default.With(size: 14f, weight: FontWeight.SemiBold),
                                        RelativeSizeAxes = Axes.X,
                                    },
                                    new LinkFlowContainer(s =>
                                    {
                                        s.Shadow = false;
                                        s.Font = OsuFont.GetFont(size: 11f, weight: FontWeight.SemiBold);
                                    }).With(d =>
                                    {
                                        d.AutoSizeAxes = Axes.Both;
                                        d.Margin = new MarginPadding { Top = 1 };
                                        d.AddText("mapped by ", t => t.Colour = colourProvider.Content2);
                                        d.AddUserLink(beatmap.BeatmapSet!.Author);
                                    }),
                                }
                            },
                            beatmapStatisticsDisplay = new FillFlowContainer
                            {
                                AutoSizeAxes = Axes.Both,
                                Direction = FillDirection.Horizontal,
                                Spacing = new Vector2(10),
                                Children = new Drawable[]
                                {
                                    new GridContainer
                                    {
                                        AutoSizeAxes = Axes.Both,
                                        ColumnDimensions =
                                        [
                                            new Dimension(GridSizeMode.AutoSize),
                                            new Dimension(GridSizeMode.AutoSize)
                                        ],
                                        RowDimensions =
                                        [
                                            new Dimension(GridSizeMode.AutoSize),
                                            new Dimension(GridSizeMode.AutoSize)
                                        ],
                                        Content = new Drawable[][]
                                        {
                                            [
                                                new OsuSpriteText { Text = "Circle Size", Font = OsuFont.Default.With(size: 14), Margin = new MarginPadding { Right = 10 } },
                                                new OsuSpriteText { Text = "4.3" }
                                            ],
                                            [
                                                new OsuSpriteText { Text = "Approach Rate", Font = OsuFont.Default.With(size: 14), Margin = new MarginPadding { Right = 10 } },
                                                new OsuSpriteText { Text = "9.6" }
                                            ]
                                        }
                                    },
                                    new GridContainer
                                    {
                                        AutoSizeAxes = Axes.Both,
                                        ColumnDimensions =
                                        [
                                            new Dimension(GridSizeMode.AutoSize),
                                            new Dimension(GridSizeMode.AutoSize)
                                        ],
                                        RowDimensions =
                                        [
                                            new Dimension(GridSizeMode.AutoSize),
                                            new Dimension(GridSizeMode.AutoSize)
                                        ],
                                        Content = new Drawable[][]
                                        {
                                            [
                                                new OsuSpriteText { Text = "Accuracy", Font = OsuFont.Default.With(size: 14), Margin = new MarginPadding { Right = 10 } },
                                                new OsuSpriteText { Text = "8.6" }
                                            ],
                                            [
                                                new OsuSpriteText { Text = "HP Drain", Font = OsuFont.Default.With(size: 14), Margin = new MarginPadding { Right = 10 } },
                                                new OsuSpriteText { Text = "4.6" }
                                            ]
                                        }
                                    },
                                }
                            },
                            new FillFlowContainer
                            {
                                Anchor = Anchor.BottomLeft,
                                Origin = Anchor.BottomLeft,
                                Direction = FillDirection.Vertical,
                                RelativeSizeAxes = Axes.X,
                                AutoSizeAxes = Axes.Y,
                                Padding = new MarginPadding { Bottom = 4 },
                                Spacing = new Vector2(6),
                                Children = new Drawable[]
                                {
                                    tagsContainer = new FillFlowContainer<UserTagControl.DrawableUserTag>
                                    {
                                        RelativeSizeAxes = Axes.X,
                                        AutoSizeAxes = Axes.Y,
                                        Direction = FillDirection.Horizontal,
                                        Scale = new Vector2(0.75f),
                                        Spacing = new Vector2(6),
                                    },
                                    new FillFlowContainer
                                    {
                                        RelativeSizeAxes = Axes.X,
                                        AutoSizeAxes = Axes.Y,
                                        Direction = FillDirection.Horizontal,
                                        Spacing = new Vector2(6),
                                        Children = new Drawable[]
                                        {
                                            new StarRatingDisplay(new StarDifficulty(beatmap.StarRating, 0), StarRatingDisplaySize.Small, animated: true)
                                            {
                                                Origin = Anchor.CentreLeft,
                                                Anchor = Anchor.CentreLeft,
                                                Scale = new Vector2(0.9f),
                                            },
                                            new TruncatingSpriteText
                                            {
                                                Text = beatmap.DifficultyName,
                                                Font = OsuFont.Style.Caption1.With(weight: FontWeight.Bold),
                                                Anchor = Anchor.CentreLeft,
                                                Origin = Anchor.CentreLeft,
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                };

                updateStatistics();
                updateTags();
            }

            private void updateStatistics()
            {
            }

            private void updateTags()
            {
                if (beatmap.TopTags == null || beatmap.TopTags.Length == 0 || beatmap.BeatmapSet!.RelatedTags == null)
                    return;

                var tagsById = beatmap.BeatmapSet.RelatedTags.ToDictionary(t => t.Id);

                IEnumerable<APITag> topTags = beatmap.TopTags
                                                     .Select(t => (topTag: t, relatedTag: tagsById.GetValueOrDefault(t.TagId)))
                                                     .Where(t => t.relatedTag != null)
                                                     // see https://github.com/ppy/osu-web/blob/bb3bd2e7c6f84f26066df5ea20a81c77ec9bb60a/resources/js/beatmapsets-show/controller.ts#L103-L106 for sort criteria
                                                     .OrderByDescending(t => t.topTag.VoteCount)
                                                     .Select(t => t.relatedTag!)
                                                     .ToArray();

                foreach (var tag in topTags)
                    tagsContainer.Add(new UserTagControl.DrawableUserTag(new UserTag(tag)));
            }

            private LocalisableString createArtistText()
            {
                var romanisableArtist = new RomanisableString(beatmap.BeatmapSet!.ArtistUnicode, beatmap.BeatmapSet!.Artist);
                return BeatmapsetsStrings.ShowDetailsByArtist(romanisableArtist);
            }
        }
    }
}
