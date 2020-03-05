// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Colour;
using osu.Framework.Graphics.Containers;
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

            private Container<CircularProgress> rankCircleContainers;

            private CircularProgress rankSSCircle;
            private CircularProgress rankSCircle;
            private CircularProgress rankACircle;
            private CircularProgress rankBCircle;
            private CircularProgress rankCCircle;
            private CircularProgress rankDCircle;
            private CircularProgress accuracyCircle;

            [BackgroundDependencyLoader]
            private void load()
            {
                InternalChildren = new Drawable[]
                {
                    new CircularProgress
                    {
                        Anchor = Anchor.Centre,
                        Origin = Anchor.Centre,
                        RelativeSizeAxes = Axes.Both,
                        Colour = OsuColour.Gray(47),
                        Alpha = 0.5f,
                        InnerRadius = accuracy_circle_radius,
                        Current = { Value = 1 },
                    },
                    accuracyCircle = new CircularProgress
                    {
                        Anchor = Anchor.Centre,
                        Origin = Anchor.Centre,
                        RelativeSizeAxes = Axes.Both,
                        Colour = ColourInfo.GradientVertical(OsuColour.FromHex("#7CF6FF"), OsuColour.FromHex("#BAFFA9")),
                        InnerRadius = accuracy_circle_radius,
                    },
                    rankCircleContainers = new Container<CircularProgress>
                    {
                        Anchor = Anchor.Centre,
                        Origin = Anchor.Centre,
                        RelativeSizeAxes = Axes.Both,
                        Size = new Vector2(0.8f),
                        Padding = new MarginPadding(2),
                        Children = new[]
                        {
                            rankSSCircle = new CircularProgress
                            {
                                Anchor = Anchor.Centre,
                                Origin = Anchor.Centre,
                                RelativeSizeAxes = Axes.Both,
                                Colour = OsuColour.FromHex("#BE0089"),
                                InnerRadius = rank_circle_radius,
                            },
                            rankSCircle = new CircularProgress
                            {
                                Anchor = Anchor.Centre,
                                Origin = Anchor.Centre,
                                RelativeSizeAxes = Axes.Both,
                                Colour = OsuColour.FromHex("#0096A2"),
                                InnerRadius = rank_circle_radius,
                            },
                            rankACircle = new CircularProgress
                            {
                                Anchor = Anchor.Centre,
                                Origin = Anchor.Centre,
                                RelativeSizeAxes = Axes.Both,
                                Colour = OsuColour.FromHex("#72C904"),
                                InnerRadius = rank_circle_radius,
                            },
                            rankBCircle = new CircularProgress
                            {
                                Anchor = Anchor.Centre,
                                Origin = Anchor.Centre,
                                RelativeSizeAxes = Axes.Both,
                                Colour = OsuColour.FromHex("#D99D03"),
                                InnerRadius = rank_circle_radius,
                            },
                            rankCCircle = new CircularProgress
                            {
                                Anchor = Anchor.Centre,
                                Origin = Anchor.Centre,
                                RelativeSizeAxes = Axes.Both,
                                Colour = OsuColour.FromHex("#EA7948"),
                                InnerRadius = rank_circle_radius,
                            },
                            rankDCircle = new CircularProgress
                            {
                                Anchor = Anchor.Centre,
                                Origin = Anchor.Centre,
                                RelativeSizeAxes = Axes.Both,
                                Colour = OsuColour.FromHex("#FF5858"),
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
    }
}
