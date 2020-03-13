// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Allocation;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Colour;
using osu.Framework.Graphics.Containers;
using osu.Framework.Utils;
using osu.Game.Graphics;
using osu.Game.Scoring;
using osuTK;

namespace osu.Game.Screens.Results
{
    public class AccuracyCircle : CompositeDrawable
    {
        public const double ACCURACY_TRANSFORM_DELAY = 450;
        public const double ACCURACY_TRANSFORM_DURATION = 3000;
        public const float RANK_CIRCLE_RADIUS = 0.04f;
        private const float accuracy_circle_radius = 0.2f;

        private readonly ScoreInfo score;

        private SmoothCircularProgress accuracyCircle;
        private SmoothCircularProgress innerMask;
        private Container<AccuracyCircleBadge> badges;
        private AccuracyCircleText rankText;

        public AccuracyCircle(ScoreInfo score)
        {
            this.score = score;
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            InternalChildren = new Drawable[]
            {
                new SmoothCircularProgress
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
                accuracyCircle = new SmoothCircularProgress
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
                        new SmoothCircularProgress
                        {
                            RelativeSizeAxes = Axes.Both,
                            Colour = Color4Extensions.FromHex("#BE0089"),
                            InnerRadius = RANK_CIRCLE_RADIUS,
                            Current = { Value = 1 }
                        },
                        new SmoothCircularProgress
                        {
                            RelativeSizeAxes = Axes.Both,
                            Colour = Color4Extensions.FromHex("#0096A2"),
                            InnerRadius = RANK_CIRCLE_RADIUS,
                            Current = { Value = 0.99f }
                        },
                        new SmoothCircularProgress
                        {
                            RelativeSizeAxes = Axes.Both,
                            Colour = Color4Extensions.FromHex("#72C904"),
                            InnerRadius = RANK_CIRCLE_RADIUS,
                            Current = { Value = 0.95f }
                        },
                        new SmoothCircularProgress
                        {
                            RelativeSizeAxes = Axes.Both,
                            Colour = Color4Extensions.FromHex("#D99D03"),
                            InnerRadius = RANK_CIRCLE_RADIUS,
                            Current = { Value = 0.9f }
                        },
                        new SmoothCircularProgress
                        {
                            RelativeSizeAxes = Axes.Both,
                            Colour = Color4Extensions.FromHex("#EA7948"),
                            InnerRadius = RANK_CIRCLE_RADIUS,
                            Current = { Value = 0.8f }
                        },
                        new SmoothCircularProgress
                        {
                            RelativeSizeAxes = Axes.Both,
                            Colour = Color4Extensions.FromHex("#FF5858"),
                            InnerRadius = RANK_CIRCLE_RADIUS,
                            Current = { Value = 0.7f }
                        },
                        new AccuracyCircleNotch(0),
                        new AccuracyCircleNotch(0.99f),
                        new AccuracyCircleNotch(0.95f),
                        new AccuracyCircleNotch(0.9f),
                        new AccuracyCircleNotch(0.8f),
                        new AccuracyCircleNotch(0.7f),
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
                            Child = innerMask = new SmoothCircularProgress
                            {
                                RelativeSizeAxes = Axes.Both,
                                InnerRadius = RANK_CIRCLE_RADIUS - 0.01f,
                            }
                        }
                    }
                },
                badges = new Container<AccuracyCircleBadge>
                {
                    Name = "Rank badges",
                    RelativeSizeAxes = Axes.Both,
                    Padding = new MarginPadding(-20),
                    Children = new[]
                    {
                        new AccuracyCircleBadge(0.99f, ScoreRank.X),
                        new AccuracyCircleBadge(0.95f, ScoreRank.S),
                        new AccuracyCircleBadge(0.9f, ScoreRank.A),
                        new AccuracyCircleBadge(0.8f, ScoreRank.B),
                        new AccuracyCircleBadge(0.7f, ScoreRank.C),
                    }
                },
                rankText = new AccuracyCircleText(score.Rank)
            };
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            this.ScaleTo(0).Then().ScaleTo(1, 200, Easing.OutQuint);

            using (BeginDelayedSequence(150, true))
                innerMask.FillTo(1f, 800, Easing.OutPow10);

            using (BeginDelayedSequence(ACCURACY_TRANSFORM_DELAY, true))
            {
                accuracyCircle.FillTo(score.Accuracy, ACCURACY_TRANSFORM_DURATION, Easing.OutPow10);

                foreach (var badge in badges)
                {
                    if (badge.Value > score.Accuracy)
                        continue;

                    using (BeginDelayedSequence(inverseEasing(Easing.OutPow10, badge.Value / score.Accuracy) * ACCURACY_TRANSFORM_DURATION, true))
                        badge.Appear();
                }

                using (BeginDelayedSequence(ACCURACY_TRANSFORM_DURATION / 2, true))
                    rankText.Appear();
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
}
