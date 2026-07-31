// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Utils;
using osuTK;
using osuTK.Graphics;
using Triangle = osu.Framework.Graphics.Shapes.Triangle;

namespace osu.Game.Screens.OnlinePlay.Matchmaking.RankedPlay.Card
{
    public partial class SparklesContainer : CompositeDrawable
    {
        public SparklesContainer()
        {
            RelativeSizeAxes = Axes.Both;
            Blending = BlendingParameters.Additive;
        }

        public void AddSparkles(Drawable source, Color4 seedColour, Vector2 velocity)
        {
            var drawQuad = ToLocalSpace(source.ScreenSpaceDrawQuad);

            float horizontal = RNG.NextSingle();
            var top = Vector2.Lerp(drawQuad.TopLeft, drawQuad.TopRight, horizontal);
            var bottom = Vector2.Lerp(drawQuad.BottomLeft, drawQuad.BottomRight, horizontal);
            var position = Vector2.Lerp(top, bottom, RNG.NextSingle());

            const float max_velocity = 0.5f;

            if (velocity.Length > max_velocity)
            {
                velocity = velocity.Normalized() * max_velocity;
            }

            var particle = new Particle(velocity * RNG.NextSingle(0.1f, 0.4f))
            {
                Position = position,
                Rotation = RNG.NextSingle(-3, 3),
                Colour = seedColour,
                Blending = BlendingParameters.Additive,
            };

            AddInternal(particle);

            particle.ScaleTo(0)
                    .ScaleTo(RNG.NextSingle(0.75f, 1f), 1000, Easing.OutElasticHalf)
                    .Then()
                    .FadeOut(5800, Easing.OutCubic)
                    .Expire();
        }

        private partial class Particle : CompositeDrawable
        {
            private Vector2 velocity;
            private readonly Triangle triangle;

            public Particle(Vector2 velocity)
            {
                this.velocity = velocity;
                Size = new Vector2(RNG.NextSingle(4, 10));
                Origin = Anchor.Centre;

                InternalChild = triangle = new Triangle
                {
                    RelativeSizeAxes = Axes.Both,
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                };
            }

            private float initialX;
            private readonly float seed = RNG.NextSingle() * MathF.PI * 2;
            private Vector2 offset;

            protected override void LoadComplete()
            {
                base.LoadComplete();

                initialX = X;

                triangle.Spin(5000, RNG.NextBool() ? RotationDirection.Clockwise : RotationDirection.Counterclockwise);

                triangle.Loop(it =>
                    it.ScaleTo(0.5f, 5000, Easing.InOutSine)
                      .FadeOut(5000)
                      .Then()
                      .ScaleTo(1f, 5000, Easing.InOutSine)
                      .FadeIn(5000)
                );
            }

            protected override void Update()
            {
                base.Update();

                offset += velocity * (float)Time.Elapsed;

                velocity = new Vector2(
                    (float)Interpolation.DampContinuously(velocity.X, 0, 1000, Time.Elapsed),
                    (float)Interpolation.DampContinuously(velocity.Y, 0, 1000, Time.Elapsed));

                X = offset.X + initialX + (float)Math.Cos(Time.Current * 0.002 + seed) * 15;
                Y -= (float)(Time.Elapsed * 0.02f) + velocity.Y * (float)Time.Elapsed;
            }
        }
    }
}
