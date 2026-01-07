// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Game.Scoring;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Screens.OnlinePlay.Matchmaking.RankedPlay
{
    public partial class ResultsScreen
    {
        private partial class BlueScoreWedge : CompositeDrawable
        {
            private readonly ScoreInfo score;

            public BlueScoreWedge(ScoreInfo score)
            {
                this.score = score;

                RelativeSizeAxes = Axes.X;
                Size = new Vector2(0.51f, 160);
            }

            [BackgroundDependencyLoader]
            private void load()
            {
                float shearWidth = OsuGame.SHEAR.X * Height;

                InternalChildren = new Drawable[]
                {
                    new Container
                    {
                        RelativeSizeAxes = Axes.Both,
                        Padding = new MarginPadding
                        {
                            Left = -300,
                            Right = -shearWidth
                        },
                        Child = new Container
                        {
                            RelativeSizeAxes = Axes.Both,
                            Shear = OsuGame.SHEAR,
                            Masking = true,
                            CornerRadius = 8,
                            Child = new Box
                            {
                                RelativeSizeAxes = Axes.Both,
                                Colour = Color4.Black,
                                Alpha = 0.5f,
                            }
                        }
                    },
                    new GridContainer
                    {
                        RelativeSizeAxes = Axes.Both,
                        Padding = new MarginPadding { Horizontal = 20 },
                        Y = -25,
                        ColumnDimensions =
                        [
                            new Dimension(),
                            new Dimension(GridSizeMode.Absolute, 20),
                            new Dimension(GridSizeMode.AutoSize)
                        ],
                        Content = new[]
                        {
                            new Drawable?[]
                            {
                                new ScoreStatisticsDisplay(score, RankedPlayColourScheme.Blue),
                                null,
                                new ScoreRankDisplay(score)
                                {
                                    Y = 60,
                                }
                            }
                        }
                    },
                };
            }
        }
    }
}
