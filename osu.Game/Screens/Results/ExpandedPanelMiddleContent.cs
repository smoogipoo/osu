// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Extensions;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Colour;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Localisation;
using osu.Game.Beatmaps;
using osu.Game.Graphics;
using osu.Game.Graphics.Containers;
using osu.Game.Graphics.Sprites;
using osu.Game.Graphics.UserInterface;
using osu.Game.Scoring;
using osu.Game.Screens.Play.HUD;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Screens.Results
{
    public class ExpandedPanelMiddleContent : CompositeDrawable
    {
        private readonly ScoreInfo score;

        private readonly List<StatisticDisplay> statisticDisplays = new List<StatisticDisplay>();
        private RollingCounter<long> scoreCounter;

        public ExpandedPanelMiddleContent(ScoreInfo score)
        {
            this.score = score;

            RelativeSizeAxes = Axes.Both;
            Masking = true;

            Padding = new MarginPadding { Horizontal = 40 };
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            var topStatistics = new List<StatisticDisplay>
            {
                new TextStatistic("accuracy", score.DisplayAccuracy),
                new ComboStatistic(score.MaxCombo, true),
                new TextStatistic("pp", score.PP?.ToString() ?? "0"),
            };

            var bottomStatistics = new List<StatisticDisplay>();
            foreach (var stat in score.SortedStatistics)
                bottomStatistics.Add(new TextStatistic(stat.Key.GetDescription(), stat.Value.ToString()));

            statisticDisplays.AddRange(topStatistics);
            statisticDisplays.AddRange(bottomStatistics);

            InternalChild = new FillFlowContainer
            {
                RelativeSizeAxes = Axes.Both,
                Direction = FillDirection.Vertical,
                Spacing = new Vector2(20),
                Children = new Drawable[]
                {
                    new FillFlowContainer
                    {
                        Anchor = Anchor.TopCentre,
                        Origin = Anchor.TopCentre,
                        RelativeSizeAxes = Axes.X,
                        AutoSizeAxes = Axes.Y,
                        Direction = FillDirection.Vertical,
                        Children = new Drawable[]
                        {
                            new OsuSpriteText
                            {
                                Anchor = Anchor.TopCentre,
                                Origin = Anchor.TopCentre,
                                Text = new LocalisedString((score.Beatmap.Metadata.Title, score.Beatmap.Metadata.TitleUnicode)),
                                Font = OsuFont.Torus.With(size: 20, weight: FontWeight.SemiBold),
                            },
                            new OsuSpriteText
                            {
                                Anchor = Anchor.TopCentre,
                                Origin = Anchor.TopCentre,
                                Text = new LocalisedString((score.Beatmap.Metadata.Artist, score.Beatmap.Metadata.ArtistUnicode)),
                                Font = OsuFont.Torus.With(size: 16, weight: FontWeight.SemiBold)
                            },
                            new Container
                            {
                                Anchor = Anchor.TopCentre,
                                Origin = Anchor.TopCentre,
                                Margin = new MarginPadding { Top = 30 },
                                RelativeSizeAxes = Axes.X,
                                Height = 340,
                                Child = new AccuracyCircle(score)
                                {
                                    Anchor = Anchor.Centre,
                                    Origin = Anchor.Centre,
                                    RelativeSizeAxes = Axes.Both,
                                    FillMode = FillMode.Fit,
                                }
                            },
                            scoreCounter = new TotalScoreCounter
                            {
                                Margin = new MarginPadding { Vertical = 10 },
                                Current = { Value = 0 },
                                Alpha = 0,
                                AlwaysPresent = true
                            },
                            new FillFlowContainer
                            {
                                Anchor = Anchor.TopCentre,
                                Origin = Anchor.TopCentre,
                                AutoSizeAxes = Axes.Both,
                                Children = new Drawable[]
                                {
                                    new StarRatingDisplay(score.Beatmap)
                                    {
                                        Anchor = Anchor.CentreLeft,
                                        Origin = Anchor.CentreLeft
                                    },
                                    new ModDisplay
                                    {
                                        Anchor = Anchor.CentreLeft,
                                        Origin = Anchor.CentreLeft,
                                        DisplayUnrankedText = false,
                                        ExpansionMode = ExpansionMode.AlwaysExpanded,
                                        Scale = new Vector2(0.5f),
                                        Current = { Value = score.Mods }
                                    }
                                }
                            },
                            new FillFlowContainer
                            {
                                Anchor = Anchor.TopCentre,
                                Origin = Anchor.TopCentre,
                                Direction = FillDirection.Vertical,
                                AutoSizeAxes = Axes.Both,
                                Children = new Drawable[]
                                {
                                    new OsuSpriteText
                                    {
                                        Anchor = Anchor.TopCentre,
                                        Origin = Anchor.TopCentre,
                                        Text = score.Beatmap.Version,
                                        Font = OsuFont.Torus.With(size: 16, weight: FontWeight.SemiBold),
                                    },
                                    new OsuTextFlowContainer(s => s.Font = OsuFont.Torus)
                                    {
                                        AutoSizeAxes = Axes.Both,
                                        Direction = FillDirection.Horizontal,
                                    }.With(t =>
                                    {
                                        t.AddText("mapped by ", s => s.Font = s.Font.With(size: 14));
                                        t.AddText(score.UserString, s => s.Font = s.Font.With(size: 14, weight: FontWeight.SemiBold));
                                    })
                                }
                            },
                        }
                    },
                    new FillFlowContainer
                    {
                        RelativeSizeAxes = Axes.X,
                        AutoSizeAxes = Axes.Y,
                        Direction = FillDirection.Vertical,
                        Spacing = new Vector2(0, 5),
                        Children = new Drawable[]
                        {
                            new GridContainer
                            {
                                RelativeSizeAxes = Axes.X,
                                AutoSizeAxes = Axes.Y,
                                Content = new[] { topStatistics.Cast<Drawable>().ToArray() },
                                RowDimensions = new[]
                                {
                                    new Dimension(GridSizeMode.AutoSize),
                                }
                            },
                            new GridContainer
                            {
                                RelativeSizeAxes = Axes.X,
                                AutoSizeAxes = Axes.Y,
                                Content = new[] { bottomStatistics.Cast<Drawable>().ToArray() },
                                RowDimensions = new[]
                                {
                                    new Dimension(GridSizeMode.AutoSize),
                                }
                            }
                        }
                    }
                }
            };
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            // Score counter value setting must be scheduled so it isn't transferred instantaneously
            ScheduleAfterChildren(() =>
            {
                using (BeginDelayedSequence(AccuracyCircle.ACCURACY_TRANSFORM_DELAY, true))
                {
                    scoreCounter.FadeIn();
                    scoreCounter.Current.Value = score.TotalScore;
                }
            });

            using (BeginDelayedSequence(AccuracyCircle.ACCURACY_TRANSFORM_DELAY + AccuracyCircle.ACCURACY_TRANSFORM_DURATION, true))
            {
                double delay = 0;

                foreach (var stat in statisticDisplays)
                {
                    using (BeginDelayedSequence(delay, true))
                        stat.Appear();

                    delay += 100;
                }
            }
        }

        private abstract class StatisticDisplay : CompositeDrawable
        {
            private readonly string header;

            protected StatisticDisplay(string header)
            {
                this.header = header;
                RelativeSizeAxes = Axes.X;
                AutoSizeAxes = Axes.Y;
            }

            [BackgroundDependencyLoader]
            private void load()
            {
                InternalChild = new FillFlowContainer
                {
                    RelativeSizeAxes = Axes.X,
                    AutoSizeAxes = Axes.Y,
                    Direction = FillDirection.Vertical,
                    Children = new[]
                    {
                        new CircularContainer
                        {
                            RelativeSizeAxes = Axes.X,
                            Height = 12,
                            Masking = true,
                            Children = new Drawable[]
                            {
                                new Box
                                {
                                    RelativeSizeAxes = Axes.Both,
                                    Colour = Color4Extensions.FromHex("#222")
                                },
                                new OsuSpriteText
                                {
                                    Anchor = Anchor.Centre,
                                    Origin = Anchor.Centre,
                                    Font = OsuFont.Torus.With(size: 12, weight: FontWeight.SemiBold),
                                    Text = header.ToUpperInvariant(),
                                }
                            }
                        },
                        CreateContent().With(d =>
                        {
                            d.Anchor = Anchor.TopCentre;
                            d.Origin = Anchor.TopCentre;
                        })
                    }
                };
            }

            public abstract void Appear();

            protected abstract Drawable CreateContent();
        }

        private class TextStatistic : StatisticDisplay
        {
            private readonly string text;

            private Drawable textDrawable;

            public TextStatistic(string header, string text)
                : base(header)
            {
                this.text = text;
            }

            public override void Appear()
            {
                textDrawable.FadeIn();
            }

            protected override Drawable CreateContent() => textDrawable = new OsuSpriteText
            {
                Font = OsuFont.Torus.With(size: 20),
                Text = text,
                Alpha = 0
            };
        }

        private class ComboStatistic : StatisticDisplay
        {
            private readonly int combo;
            private readonly bool isPerfect;

            private Drawable flow;

            public ComboStatistic(int combo, bool isPerfect)
                : base("combo")
            {
                this.combo = combo;
                this.isPerfect = isPerfect;
            }

            public override void Appear()
            {
                flow.FadeIn();
            }

            protected override Drawable CreateContent() => flow = new FillFlowContainer
            {
                AutoSizeAxes = Axes.Both,
                Direction = FillDirection.Horizontal,
                Spacing = new Vector2(10, 0),
                Alpha = 0,
                Children = new Drawable[]
                {
                    new OsuSpriteText
                    {
                        Anchor = Anchor.CentreLeft,
                        Origin = Anchor.CentreLeft,
                        Text = $"{combo}x",
                        Font = OsuFont.Torus.With(size: 20),
                    },
                    new OsuSpriteText
                    {
                        Anchor = Anchor.CentreLeft,
                        Origin = Anchor.CentreLeft,
                        Text = "PERFECT",
                        Font = OsuFont.Torus.With(size: 11, weight: FontWeight.SemiBold),
                        Colour = ColourInfo.GradientVertical(Color4Extensions.FromHex("#66FFCC"), Color4Extensions.FromHex("#FF9AD7")),
                        Alpha = isPerfect ? 1 : 0,
                        UseFullGlyphHeight = false,
                    }
                }
            };
        }

        private class StarRatingDisplay : CompositeDrawable
        {
            private readonly BeatmapInfo beatmap;

            public StarRatingDisplay(BeatmapInfo beatmap)
            {
                this.beatmap = beatmap;
                AutoSizeAxes = Axes.Both;
            }

            [BackgroundDependencyLoader]
            private void load(OsuColour colours)
            {
                InternalChildren = new Drawable[]
                {
                    new CircularContainer
                    {
                        RelativeSizeAxes = Axes.Both,
                        Masking = true,
                        Children = new Drawable[]
                        {
                            new Box
                            {
                                RelativeSizeAxes = Axes.Both,
                                Colour = colours.ForDifficultyRating(beatmap.DifficultyRating)
                            },
                        }
                    },
                    new FillFlowContainer
                    {
                        AutoSizeAxes = Axes.Both,
                        Padding = new MarginPadding { Horizontal = 10, Vertical = 5 },
                        Direction = FillDirection.Horizontal,
                        Spacing = new Vector2(2, 0),
                        Children = new Drawable[]
                        {
                            new SpriteIcon
                            {
                                Anchor = Anchor.CentreLeft,
                                Origin = Anchor.CentreLeft,
                                Size = new Vector2(7),
                                Icon = FontAwesome.Solid.Star,
                                Colour = Color4.Black
                            },
                            new OsuTextFlowContainer(s => s.Font = OsuFont.Numeric.With(weight: FontWeight.Black))
                            {
                                Anchor = Anchor.CentreLeft,
                                Origin = Anchor.CentreLeft,
                                AutoSizeAxes = Axes.Both,
                                Direction = FillDirection.Horizontal,
                                TextAnchor = Anchor.BottomLeft,
                            }.With(t =>
                            {
                                t.AddText($"{beatmap.StarDifficulty:0}", s =>
                                {
                                    s.Colour = Color4.Black;
                                    s.Font = s.Font.With(size: 14);
                                    s.UseFullGlyphHeight = false;
                                });

                                t.AddText($"{beatmap.StarDifficulty:.00}", s =>
                                {
                                    s.Colour = Color4.Black;
                                    s.Font = s.Font.With(size: 7);
                                    s.UseFullGlyphHeight = false;
                                });
                            })
                        }
                    }
                };
            }
        }

        private class TotalScoreCounter : RollingCounter<long>
        {
            protected override double RollingDuration => 3000;

            protected override Easing RollingEasing => Easing.OutPow10;

            public TotalScoreCounter()
            {
                // Todo: AutoSize X removed here due to https://github.com/ppy/osu-framework/issues/3369
                AutoSizeAxes = Axes.Y;
                RelativeSizeAxes = Axes.X;
                DisplayedCountSpriteText.Anchor = Anchor.TopCentre;
                DisplayedCountSpriteText.Origin = Anchor.TopCentre;

                DisplayedCountSpriteText.Font = OsuFont.Torus.With(size: 76, weight: FontWeight.Light, fixedWidth: true);
                DisplayedCountSpriteText.Spacing = new Vector2(-5, 0);
            }

            public override void Increment(long amount)
                => Current.Value += amount;
        }
    }
}
