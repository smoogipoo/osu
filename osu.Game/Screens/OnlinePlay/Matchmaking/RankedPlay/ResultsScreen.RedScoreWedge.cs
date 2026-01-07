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
        private partial class RedScoreWedge : CompositeDrawable
        {
            private readonly ScoreInfo score;

            public RedScoreWedge(ScoreInfo score)
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
                            Left = -shearWidth,
                            Right = -300,
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
                        Padding = new MarginPadding
                        {
                            Left = -shearWidth + 20,
                            Right = 20 + shearWidth
                        },
                        Y = -25,
                        ColumnDimensions =
                        [
                            new Dimension(GridSizeMode.AutoSize),
                            new Dimension(GridSizeMode.Absolute, 20),
                            new Dimension()
                        ],
                        Content = new[]
                        {
                            new Drawable?[]
                            {
                                new ScoreRankDisplay(score)
                                {
                                    Anchor = Anchor.BottomLeft,
                                    Origin = Anchor.BottomLeft,
                                },
                                null,
                                new ScoreStatisticsDisplay(score, RankedPlayColourScheme.Red)
                            }
                        }
                    },
                };
            }
        }
    }
}
