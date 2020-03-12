// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Colour;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Transforms;
using osu.Framework.Graphics.UserInterface;
using osu.Game.Graphics;
using osuTK;

namespace osu.Game.Tests.Visual.Results
{
    public class TestSceneAccuracyCircle : OsuTestScene
    {
        public TestSceneAccuracyCircle()
        {
            Child = new Container
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                Size = new Vector2(500, 700),
                Children = new Drawable[]
                {
                    new Box
                    {
                        RelativeSizeAxes = Axes.Both,
                        Colour = ColourInfo.GradientVertical(Color4Extensions.FromHex("#555"), Color4Extensions.FromHex("#333"))
                    }
                }
            };
            Add(new AccuracyCircle
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                Size = new Vector2(320)
            });
        }

        private class AccuracyCircle : CompositeDrawable
        {
            private const float accuracy_circle_radius = 0.2f;
            private const float rank_circle_radius = 0.04f;
            private const float accuracy_target = 1;

            private SmoothedCircularProgress accuracyCircle;
            private SmoothedCircularProgress innerMask;
            private Container<Badge> badges;

            [BackgroundDependencyLoader]
            private void load()
            {
                InternalChildren = new Drawable[]
                {
                    new SmoothedCircularProgress
                    {
                        Name = "Background circle",
                        Anchor = Anchor.Centre,
                        Origin = Anchor.Centre,
                        RelativeSizeAxes = Axes.Both,
                        Colour = OsuColour.Gray(47),
                        Alpha = 0.5f,
                        InnerRadius = accuracy_circle_radius + 0.01f, // Extends a little bit into the circle
                        Current = { Value = 1 },
                    },
                    accuracyCircle = new SmoothedCircularProgress
                    {
                        Name = "Accuracy circle",
                        Anchor = Anchor.Centre,
                        Origin = Anchor.Centre,
                        RelativeSizeAxes = Axes.Both,
                        Colour = ColourInfo.GradientVertical(Color4Extensions.FromHex("#7CF6FF"), Color4Extensions.FromHex("#BAFFA9")),
                        InnerRadius = accuracy_circle_radius,
                    },
                    new BufferedContainer
                    {
                        Name = "Graded circles",
                        Anchor = Anchor.Centre,
                        Origin = Anchor.Centre,
                        RelativeSizeAxes = Axes.Both,
                        Size = new Vector2(0.8f),
                        Padding = new MarginPadding(2),
                        Children = new Drawable[]
                        {
                            new SmoothedCircularProgress
                            {
                                RelativeSizeAxes = Axes.Both,
                                Colour = Color4Extensions.FromHex("#BE0089"),
                                InnerRadius = rank_circle_radius,
                                Current = { Value = 1 }
                            },
                            new SmoothedCircularProgress
                            {
                                RelativeSizeAxes = Axes.Both,
                                Colour = Color4Extensions.FromHex("#0096A2"),
                                InnerRadius = rank_circle_radius,
                                Current = { Value = 0.99f }
                            },
                            new SmoothedCircularProgress
                            {
                                RelativeSizeAxes = Axes.Both,
                                Colour = Color4Extensions.FromHex("#72C904"),
                                InnerRadius = rank_circle_radius,
                                Current = { Value = 0.95f }
                            },
                            new SmoothedCircularProgress
                            {
                                RelativeSizeAxes = Axes.Both,
                                Colour = Color4Extensions.FromHex("#D99D03"),
                                InnerRadius = rank_circle_radius,
                                Current = { Value = 0.9f }
                            },
                            new SmoothedCircularProgress
                            {
                                RelativeSizeAxes = Axes.Both,
                                Colour = Color4Extensions.FromHex("#EA7948"),
                                InnerRadius = rank_circle_radius,
                                Current = { Value = 0.8f }
                            },
                            new SmoothedCircularProgress
                            {
                                RelativeSizeAxes = Axes.Both,
                                Colour = Color4Extensions.FromHex("#FF5858"),
                                InnerRadius = rank_circle_radius,
                                Current = { Value = 0.7f }
                            },
                            new Notch(0, rank_circle_radius),
                            new Notch(0.99f, rank_circle_radius),
                            new Notch(0.95f, rank_circle_radius),
                            new Notch(0.9f, rank_circle_radius),
                            new Notch(0.8f, rank_circle_radius),
                            new Notch(0.7f, rank_circle_radius),
                            new BufferedContainer
                            {
                                Name = "Graded circle mask",
                                RelativeSizeAxes = Axes.Both,
                                Padding = new MarginPadding(1),
                                Blending = new BlendingParameters
                                {
                                    Source = BlendingType.DstColor,
                                    Destination = BlendingType.OneMinusSrcAlpha,
                                    SourceAlpha = BlendingType.One,
                                    DestinationAlpha = BlendingType.SrcAlpha
                                },
                                Child = innerMask = new SmoothedCircularProgress
                                {
                                    RelativeSizeAxes = Axes.Both,
                                    InnerRadius = rank_circle_radius - 0.01f,
                                }
                            }
                        }
                    },
                    badges = new Container<Badge>
                    {
                        Name = "Rank badges",
                        RelativeSizeAxes = Axes.Both,
                        Padding = new MarginPadding(-20),
                        Children = new[]
                        {
                            new Badge(0.99f, ScoreRank.X),
                            new Badge(0.95f, ScoreRank.S),
                            new Badge(0.9f, ScoreRank.A),
                            new Badge(0.8f, ScoreRank.B),
                            new Badge(0.7f, ScoreRank.C),
                        }
                    },
                };
            }

            protected override void LoadComplete()
            {
                base.LoadComplete();

                this.ScaleTo(0).Then().ScaleTo(1, 200, Easing.OutQuint);

                using (BeginDelayedSequence(150, true))
                {
                    innerMask.FillTo(1f, 800, Easing.OutPow10);

                    using (BeginDelayedSequence(300, true))
                    {
                        accuracyCircle.FillTo(accuracy_target, 3000, Easing.OutPow10);

                        foreach (var badge in badges)
                        {
                            if (badge.Value > accuracy_target)
                                continue;

                            using (BeginDelayedSequence(inverseEasing(Easing.OutPow10, badge.Value / accuracy_target) * 3000, true))
                                badge.Appear();
                        }
                    }
                }
            }

            private double inverseEasing(Easing easing, double targetValue)
            {
                double test = 0;
                double result = 0;
                int count = 2;

                while (Math.Abs(result - targetValue) > 0.005)
                {
                    int dir = Math.Sign(targetValue - result);

                    test += dir * 1.0 / count;
                    result = Interpolation.ApplyEasing(easing, test);

                    count++;
                }

                return test;
            }
        }
        private class Badge : CompositeDrawable
        {
            public readonly float Value;

            private readonly Drawable rankContainer;
            private readonly Drawable overlay;

            public Badge(float value, ScoreRank rank)
            {
                Value = value;

                RelativeSizeAxes = Axes.Both;
                Alpha = 0;

                InternalChild = rankContainer = new Container
                {
                    Origin = Anchor.Centre,
                    Size = new Vector2(32, 16),
                    Children = new[]
                    {
                        new DrawableRank(rank),
                        overlay = new CircularContainer
                        {
                            RelativeSizeAxes = Axes.Both,
                            Blending = BlendingParameters.Additive,
                            Masking = true,
                            EdgeEffect = new EdgeEffectParameters
                            {
                                Type = EdgeEffectType.Glow,
                                Colour = DrawableRank.GetRankColour(rank).Opacity(0.2f),
                                Radius = 10,
                            },
                            Child = new Box
                            {
                                RelativeSizeAxes = Axes.Both,
                                Alpha = 0,
                                AlwaysPresent = true,
                            }
                        }
                    }
                };
            }

            protected override void Update()
            {
                base.Update();
                rankContainer.Position = circlePosition(-MathF.PI / 2 - (1 - Value) * MathF.PI * 2);
            }

            public void Appear()
            {
                this.FadeIn(50);
                overlay.FadeIn().FadeOut(500, Easing.In);
            }

            private Vector2 circlePosition(float t)
                => DrawSize / 2 + new Vector2(MathF.Cos(t), MathF.Sin(t)) * DrawSize / 2;
        }

        private class Notch : CompositeDrawable
        {
            public Notch(float position, float height)
            {
                RelativeSizeAxes = Axes.Both;

                InternalChild = new Container
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    RelativeSizeAxes = Axes.Both,
                    Rotation = position * 360f,
                    Child = new Box
                    {
                        Anchor = Anchor.TopCentre,
                        Origin = Anchor.TopCentre,
                        RelativeSizeAxes = Axes.Y,
                        Height = height,
                        Width = 1f,
                        Colour = OsuColour.Gray(0.3f),
                        EdgeSmoothness = new Vector2(1f)
                    }
                };
            }
        }

        private class SmoothedCircularProgress : CompositeDrawable
        {
            public Bindable<double> Current
            {
                get => progress.Current;
                set => progress.Current = value;
            }

            public float InnerRadius
            {
                get => progress.InnerRadius;
                set
                {
                    progress.InnerRadius = value;
                    innerSmoothingContainer.Size = new Vector2(1 - value);
                    smoothingWedge.Height = value / 2;
                }
            }

            private readonly CircularProgress progress;
            private readonly Container innerSmoothingContainer;
            private readonly Drawable smoothingWedge;

            public SmoothedCircularProgress()
            {
                Container smoothingWedgeContainer;

                InternalChild = new BufferedContainer
                {
                    RelativeSizeAxes = Axes.Both,
                    Children = new Drawable[]
                    {
                        progress = new CircularProgress { RelativeSizeAxes = Axes.Both },
                        smoothingWedgeContainer = new Container
                        {
                            Anchor = Anchor.Centre,
                            Origin = Anchor.Centre,
                            RelativeSizeAxes = Axes.Both,
                            Child = smoothingWedge = new Box
                            {
                                Anchor = Anchor.TopCentre,
                                Origin = Anchor.TopCentre,
                                RelativeSizeAxes = Axes.Y,
                                Width = 1f,
                                EdgeSmoothness = new Vector2(2, 0),
                            }
                        },
                        new Container
                        {
                            RelativeSizeAxes = Axes.Both,
                            Padding = new MarginPadding(-1),
                            Child = new CircularContainer
                            {
                                RelativeSizeAxes = Axes.Both,
                                BorderThickness = 2,
                                Masking = true,
                                Blending = new BlendingParameters
                                {
                                    AlphaEquation = BlendingEquation.ReverseSubtract,
                                },
                                Child = new Box
                                {
                                    RelativeSizeAxes = Axes.Both,
                                    Alpha = 0,
                                    AlwaysPresent = true
                                }
                            }
                        },
                        innerSmoothingContainer = new Container
                        {
                            Anchor = Anchor.Centre,
                            Origin = Anchor.Centre,
                            RelativeSizeAxes = Axes.Both,
                            Size = Vector2.Zero,
                            Padding = new MarginPadding(-1),
                            Child = new CircularContainer
                            {
                                RelativeSizeAxes = Axes.Both,
                                BorderThickness = 2,
                                Masking = true,
                                Blending = new BlendingParameters
                                {
                                    AlphaEquation = BlendingEquation.ReverseSubtract,
                                },
                                Child = new Box
                                {
                                    RelativeSizeAxes = Axes.Both,
                                    Alpha = 0,
                                    AlwaysPresent = true
                                }
                            }
                        },
                    }
                };

                Current.BindValueChanged(c =>
                {
                    smoothingWedgeContainer.Alpha = c.NewValue > 0 ? 1 : 0;
                    smoothingWedgeContainer.Rotation = (float)(360 * c.NewValue);
                }, true);
            }

            public TransformSequence<CircularProgress> FillTo(double newValue, double duration = 0, Easing easing = Easing.None)
                => progress.FillTo(newValue, duration, easing);
        }
    }
}
