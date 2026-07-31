// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Colour;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Effects;
using osu.Framework.Graphics.Primitives;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.Textures;
using osu.Framework.Utils;
using osu.Game.Graphics;
using osu.Game.Graphics.Backgrounds;
using osu.Game.Graphics.Sprites;
using osu.Game.Overlays;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Screens.OnlinePlay.Matchmaking.RankedPlay.Card
{
    public partial class RankedPlayCardBackSide : CompositeDrawable
    {
        [Resolved]
        private OsuColour colours { get; set; } = null!;

        private readonly bool mystery;

        public RankedPlayCardBackSide(bool mystery)
        {
            this.mystery = mystery;
            Size = RankedPlayCard.SIZE;
        }

        [BackgroundDependencyLoader]
        private void load(OverlayColourProvider colourProvider, TextureStore textures)
        {
            Masking = true;
            CornerRadius = RankedPlayCard.CORNER_RADIUS;

            if (mystery)
            {
                BorderColour = Color4.White;
                BorderThickness = 3;
                EdgeEffect = new EdgeEffectParameters
                {
                    Type = EdgeEffectType.Glow,
                    Colour = colours.Cyan.Opacity(0.5f),
                    Radius = 20,
                    Hollow = true
                };
            }

            InternalChildren = new Drawable[]
            {
                new Box
                {
                    RelativeSizeAxes = Axes.Both,
                    Colour =
                        mystery
                            ? ColourInfo.GradientVertical(
                                new OverlayColourProvider(OverlayColourScheme.Purple).Background5,
                                new OverlayColourProvider(OverlayColourScheme.Purple).Background6)
                            : ColourInfo.GradientVertical(
                                colourProvider.Background3,
                                colourProvider.Background4),
                },
                new TrianglesV2
                {
                    RelativeSizeAxes = Axes.Both,
                    Colour = colourProvider.Background4,
                    SpawnRatio = 1.2f,
                    Velocity = 0.1f,
                },
                mystery
                    ? new OsuSpriteText
                    {
                        Anchor = Anchor.Centre,
                        Origin = Anchor.Centre,
                        Text = "?",
                        Font = OsuFont.GetFont(size: 42, weight: FontWeight.Black),
                        UseFullGlyphHeight = false,
                    }.WithEffect(new GlowEffect
                    {
                        Blending = BlendingParameters.Additive,
                        Colour = colours.Cyan,
                        BlurSigma = new Vector2(20),
                        PadExtent = true,
                        Strength = 10
                    })
                    : new Sprite
                    {
                        Texture = textures.Get(@"Menu/logo"),
                        Size = new Vector2(32),
                        Anchor = Anchor.Centre,
                        Origin = Anchor.Centre,
                    },
            };
        }

        [Resolved]
        private SparklesContainer? sparklesContainer { get; set; }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            if (mystery)
            {
                EdgeEffectParameters initialEdgeEffect = EdgeEffect;
                TweenEdgeEffectTo(new EdgeEffectParameters
                    {
                        Type = EdgeEffectType.Glow,
                        Colour = colours.Yellow.Opacity(0.3f),
                        Radius = 18,
                        Hollow = true
                    }, 3000)
                    .Then()
                    .Append(_ => TweenEdgeEffectTo(initialEdgeEffect, 3000, Easing.OutQuint))
                    .Loop();

                Scheduler.AddDelayed(() =>
                {
                    if (lastDrawQuad != null)
                        sparklesContainer?.AddSparkles(this, colours.Cyan, velocity);
                }, 25, true);
            }
        }

        protected override void UpdateAfterChildren()
        {
            base.UpdateAfterChildren();

            lastDrawQuad = ScreenSpaceDrawQuad;
        }

        private Quad? lastDrawQuad;

        private Vector2 velocity =>
            Precision.DefinitelyBigger(Time.Elapsed, 0)
                ? (ScreenSpaceDrawQuad.Centre - lastDrawQuad!.Value.Centre) / (float)Time.Elapsed
                : Vector2.Zero;

        protected override void Update()
        {
            base.Update();

            if (mystery)
            {
                float angle = (float)(Time.Current * 0.0004);
                const float angle_offset = 59 / 360f;

                BorderColour = new ColourInfo
                {
                    TopLeft = Colour4.FromHSV((angle - angle_offset) % 1, 0.5f, 1f),
                    TopRight = Colour4.FromHSV((angle + angle_offset) % 1, 0.5f, 1f),
                    BottomLeft = Colour4.FromHSV((angle - angle_offset + 0.5f) % 1, 0.5f, 1f),
                    BottomRight = Colour4.FromHSV((angle + angle_offset + 0.5f) % 1, 0.5f, 1f),
                };
            }
        }
    }
}
