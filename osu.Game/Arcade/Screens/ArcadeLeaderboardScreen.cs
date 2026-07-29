// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Colour;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Game.Screens;
using osuTK.Graphics;

namespace osu.Game.Arcade.Screens
{
    public class ArcadeLeaderboardScreen : OsuScreen
    {
        [BackgroundDependencyLoader]
        private void load()
        {
            InternalChild = new Container
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                RelativeSizeAxes = Axes.Both,
                Height = 0.5f,
                Children = new Drawable[]
                {
                    new Box
                    {
                        RelativeSizeAxes = Axes.Both,
                        Colour = Color4.Black,
                        Alpha = 0.5f
                    },
                    new BufferedContainer
                    {
                        RelativeSizeAxes = Axes.Both,
                        Children = new Drawable[]
                        {
                            new ArcadeLeaderboardCloud
                            {
                                RelativeSizeAxes = Axes.Both,
                            },
                            new FillFlowContainer
                            {
                                Anchor = Anchor.Centre,
                                Origin = Anchor.Centre,
                                AutoSizeAxes = Axes.X,
                                RelativeSizeAxes = Axes.Y,
                                Direction = FillDirection.Horizontal,
                                Blending = new BlendingParameters
                                {
                                    Source = BlendingType.Zero,
                                    Destination = BlendingType.One,
                                    SourceAlpha = BlendingType.Zero,
                                    DestinationAlpha = BlendingType.OneMinusSrcAlpha,
                                    RGBEquation = BlendingEquation.Add,
                                    AlphaEquation = BlendingEquation.ReverseSubtract,
                                },
                                Children = new Drawable[]
                                {
                                    new Box
                                    {
                                        RelativeSizeAxes = Axes.Y,
                                        Width = 100,
                                        Colour = ColourInfo.GradientHorizontal(Color4.Transparent, Color4.White)
                                    },
                                    new Box
                                    {
                                        RelativeSizeAxes = Axes.Y,
                                        Width = 300
                                    },
                                    new Box
                                    {
                                        RelativeSizeAxes = Axes.Y,
                                        Width = 100,
                                        Colour = ColourInfo.GradientHorizontal(Color4.White, Color4.Transparent)
                                    },
                                }
                            },
                            new ArcadeLeaderboard
                            {
                                Anchor = Anchor.Centre,
                                Origin = Anchor.Centre,
                                Width = 300,
                                MaxPanels = 5
                            }
                        }
                    }
                }
            };
        }
    }
}
