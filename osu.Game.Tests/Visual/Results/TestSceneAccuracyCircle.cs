// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Colour;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
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
                        Current = { Value = 0.97f }
                    },
                    new Container
                    {
                        Anchor = Anchor.Centre,
                        Origin = Anchor.Centre,
                        RelativeSizeAxes = Axes.Both,
                        Size = new Vector2(0.8f),
                        Padding = new MarginPadding(2),
                        Children = new Drawable[]
                        {
                            rankSSCircle = new CircularProgress
                            {
                                Anchor = Anchor.Centre,
                                Origin = Anchor.Centre,
                                RelativeSizeAxes = Axes.Both,
                                Colour = OsuColour.FromHex("#BE0089"),
                                InnerRadius = rank_circle_radius,
                                Current = { Value = 1f }
                            },
                            rankSCircle = new CircularProgress
                            {
                                Anchor = Anchor.Centre,
                                Origin = Anchor.Centre,
                                RelativeSizeAxes = Axes.Both,
                                Colour = OsuColour.FromHex("#0096A2"),
                                InnerRadius = rank_circle_radius,
                                Current = { Value = 0.95f }
                            },
                            rankACircle = new CircularProgress
                            {
                                Anchor = Anchor.Centre,
                                Origin = Anchor.Centre,
                                RelativeSizeAxes = Axes.Both,
                                Colour = OsuColour.FromHex("#72C904"),
                                InnerRadius = rank_circle_radius,
                                Current = { Value = 0.9f }
                            },
                            rankBCircle = new CircularProgress
                            {
                                Anchor = Anchor.Centre,
                                Origin = Anchor.Centre,
                                RelativeSizeAxes = Axes.Both,
                                Colour = OsuColour.FromHex("#D99D03"),
                                InnerRadius = rank_circle_radius,
                                Current = { Value = 0.85f }
                            },
                            rankCCircle = new CircularProgress
                            {
                                Anchor = Anchor.Centre,
                                Origin = Anchor.Centre,
                                RelativeSizeAxes = Axes.Both,
                                Colour = OsuColour.FromHex("#EA7948"),
                                InnerRadius = rank_circle_radius,
                                Current = { Value = 0.8f }
                            },
                            rankDCircle = new CircularProgress
                            {
                                Anchor = Anchor.Centre,
                                Origin = Anchor.Centre,
                                RelativeSizeAxes = Axes.Both,
                                Colour = OsuColour.FromHex("#FF5858"),
                                InnerRadius = rank_circle_radius,
                                Current = { Value = 0.7f }
                            },
                        }
                    }
                };
            }
        }
    }
}
