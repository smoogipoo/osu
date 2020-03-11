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
            Add(new AccuracyCircle
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                Size = new Vector2(300)
            });
        }

        private class AccuracyCircle : CompositeDrawable
        {
            private const float accuracy_circle_radius = 0.2f;
            private const float rank_circle_radius = 0.03f;

            private SmoothedCircularProgress rankSSCircle;
            private SmoothedCircularProgress rankSCircle;
            private SmoothedCircularProgress rankACircle;
            private SmoothedCircularProgress rankBCircle;
            private SmoothedCircularProgress rankCCircle;
            private SmoothedCircularProgress rankDCircle;
            private SmoothedCircularProgress accuracyCircle;

            [BackgroundDependencyLoader]
            private void load()
            {
                InternalChildren = new Drawable[]
                {
                    new SmoothedCircularProgress
                    {
                        Anchor = Anchor.Centre,
                        Origin = Anchor.Centre,
                        RelativeSizeAxes = Axes.Both,
                        Colour = OsuColour.Gray(47),
                        Alpha = 0.5f,
                        InnerRadius = accuracy_circle_radius,
                        Current = { Value = 1 },
                    },
                    accuracyCircle = new SmoothedCircularProgress
                    {
                        Anchor = Anchor.Centre,
                        Origin = Anchor.Centre,
                        RelativeSizeAxes = Axes.Both,
                        Colour = ColourInfo.GradientVertical(Color4Extensions.FromHex("#7CF6FF"), Color4Extensions.FromHex("#BAFFA9")),
                        InnerRadius = accuracy_circle_radius,
                    },
                    new Container<SmoothedCircularProgress>
                    {
                        Anchor = Anchor.Centre,
                        Origin = Anchor.Centre,
                        RelativeSizeAxes = Axes.Both,
                        Size = new Vector2(0.8f),
                        Padding = new MarginPadding(2),
                        Children = new[]
                        {
                            rankSSCircle = new SmoothedCircularProgress
                            {
                                Anchor = Anchor.Centre,
                                Origin = Anchor.Centre,
                                RelativeSizeAxes = Axes.Both,
                                Colour = Color4Extensions.FromHex("#BE0089"),
                                InnerRadius = rank_circle_radius,
                            },
                            rankSCircle = new SmoothedCircularProgress
                            {
                                Anchor = Anchor.Centre,
                                Origin = Anchor.Centre,
                                RelativeSizeAxes = Axes.Both,
                                Colour = Color4Extensions.FromHex("#0096A2"),
                                InnerRadius = rank_circle_radius,
                            },
                            rankACircle = new SmoothedCircularProgress
                            {
                                Anchor = Anchor.Centre,
                                Origin = Anchor.Centre,
                                RelativeSizeAxes = Axes.Both,
                                Colour = Color4Extensions.FromHex("#72C904"),
                                InnerRadius = rank_circle_radius,
                            },
                            rankBCircle = new SmoothedCircularProgress
                            {
                                Anchor = Anchor.Centre,
                                Origin = Anchor.Centre,
                                RelativeSizeAxes = Axes.Both,
                                Colour = Color4Extensions.FromHex("#D99D03"),
                                InnerRadius = rank_circle_radius,
                            },
                            rankCCircle = new SmoothedCircularProgress
                            {
                                Anchor = Anchor.Centre,
                                Origin = Anchor.Centre,
                                RelativeSizeAxes = Axes.Both,
                                Colour = Color4Extensions.FromHex("#EA7948"),
                                InnerRadius = rank_circle_radius,
                            },
                            rankDCircle = new SmoothedCircularProgress
                            {
                                Anchor = Anchor.Centre,
                                Origin = Anchor.Centre,
                                RelativeSizeAxes = Axes.Both,
                                Colour = Color4Extensions.FromHex("#FF5858"),
                                InnerRadius = rank_circle_radius,
                            },
                        }
                    }
                };
            }

            protected override void LoadComplete()
            {
                base.LoadComplete();

                this.ScaleTo(0).Then().ScaleTo(1, 200, Easing.OutQuint);

                using (BeginDelayedSequence(150, true))
                {
                    rankSSCircle.FillTo(1, 800, Easing.OutPow10);
                    rankSCircle.FillTo(0.99f, 800, Easing.OutPow10);
                    rankACircle.FillTo(0.95f, 800, Easing.OutPow10);
                    rankBCircle.FillTo(0.9f, 800, Easing.OutPow10);
                    rankCCircle.FillTo(0.8f, 800, Easing.OutPow10);
                    rankDCircle.FillTo(0.7f, 800, Easing.OutPow10);

                    using (BeginDelayedSequence(300, true))
                    {
                        accuracyCircle.FillTo(0.97f, 3000, Easing.OutPow10);
                    }
                }
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
                                Blending = new BlendingParameters { AlphaEquation = BlendingEquation.ReverseSubtract },
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
                                Blending = new BlendingParameters { AlphaEquation = BlendingEquation.ReverseSubtract },
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
