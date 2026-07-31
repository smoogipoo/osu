// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Colour;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Effects;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.Textures;
using osu.Game.Graphics;
using osu.Game.Graphics.Backgrounds;
using osu.Game.Graphics.Sprites;
using osu.Game.Overlays;
using osuTK;

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
                EdgeEffect = new EdgeEffectParameters
                {
                    Type = EdgeEffectType.Glow,
                    Colour = colours.Yellow.Opacity(0.5f),
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
                        ColourInfo.GradientVertical(
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
                        Font = OsuFont.GetFont(size: 32),
                        UseFullGlyphHeight = false
                    }
                    : new Sprite
                    {
                        Texture = textures.Get(@"Menu/logo"),
                        Size = new Vector2(32),
                        Anchor = Anchor.Centre,
                        Origin = Anchor.Centre,
                    },
            };
        }

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
            }
        }
    }
}
