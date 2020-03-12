// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Allocation;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Effects;
using osu.Framework.Graphics.Shapes;
using osu.Game.Online.Leaderboards;
using osu.Game.Scoring;
using osuTK;

namespace osu.Game.Screens.Results
{
    public class AccuracyCircleBadge : CompositeDrawable
    {
        public readonly float Value;
        private readonly ScoreRank rank;

        private Drawable rankContainer;
        private Drawable overlay;

        public AccuracyCircleBadge(float value, ScoreRank rank)
        {
            Value = value;
            this.rank = rank;

            RelativeSizeAxes = Axes.Both;
            Alpha = 0;
        }

        [BackgroundDependencyLoader]
        private void load()
        {
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
}
