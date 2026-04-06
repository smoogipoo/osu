// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Linq;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Cursor;
using osu.Framework.Graphics.Lines;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Input.Events;
using osu.Framework.Layout;
using osu.Game.Graphics;
using osu.Game.Graphics.Containers;
using osu.Game.Graphics.Sprites;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Screens.OnlinePlay.Matchmaking.Queue
{
    public class RatingDistributionGraph : CompositeDrawable, IHasCustomTooltip<RatingDistributionGraph.RatingDistributionGraphTooltipData>
    {
        private const int y_divisions = 8;
        private const int x_divisions = 16;

        private readonly LayoutValue pathCache = new LayoutValue(Invalidation.RequiredParentSizeToFit);

        private readonly Container grid;

        private readonly Container pathContainer;
        private readonly SmoothPath cumulativePath;
        private readonly SmoothPath distributionPath;

        private readonly Drawable hoverMarker;
        private readonly Drawable hoverMarkerFill;

        private readonly OsuTextFlowContainer descriptionText;

        private (int x, int y)[]? data;
        private int? userRating;
        private (int min, int max) xRange;
        private (int min, int max) yRange;

        public RatingDistributionGraph()
        {
            InternalChild = new GridContainer
            {
                RelativeSizeAxes = Axes.Both,
                RowDimensions =
                [
                    new Dimension(),
                    new Dimension(GridSizeMode.AutoSize)
                ],
                Content = new[]
                {
                    new Drawable[]
                    {
                        new Container
                        {
                            RelativeSizeAxes = Axes.Both,
                            Children = new Drawable[]
                            {
                                new OsuSpriteText
                                {
                                    Anchor = Anchor.CentreLeft,
                                    Origin = Anchor.TopCentre,
                                    Text = "Players",
                                    Font = OsuFont.Default.With(size: 12),
                                    Rotation = -90,
                                    Colour = OsuColour.Gray(0.5f)
                                },
                                new OsuSpriteText
                                {
                                    Anchor = Anchor.BottomCentre,
                                    Origin = Anchor.BottomCentre,
                                    Text = "Rating",
                                    Font = OsuFont.Default.With(size: 12),
                                    Colour = OsuColour.Gray(0.5f),
                                },
                                new OsuSpriteText
                                {
                                    Anchor = Anchor.CentreRight,
                                    Origin = Anchor.TopCentre,
                                    Text = "Cumulative",
                                    Font = OsuFont.Default.With(size: 12),
                                    Rotation = 90,
                                    Colour = OsuColour.Gray(0.5f),
                                },
                                new Container
                                {
                                    RelativeSizeAxes = Axes.Both,
                                    Padding = new MarginPadding
                                    {
                                        Left = 50,
                                        Top = 20,
                                        Bottom = 40,
                                        Right = 50
                                    },
                                    Children = new[]
                                    {
                                        grid = new Container
                                        {
                                            RelativeSizeAxes = Axes.Both
                                        },
                                        pathContainer = new Container
                                        {
                                            RelativeSizeAxes = Axes.Both,
                                            // Margin and padding to better align the paths.
                                            Margin = new MarginPadding { Left = -2 },
                                            Padding = new MarginPadding { Right = -2 },
                                            Children = new Drawable[]
                                            {
                                                distributionPath = new SmoothPath
                                                {
                                                    AutoSizeAxes = Axes.None,
                                                    RelativeSizeAxes = Axes.Both,
                                                    PathRadius = 2,
                                                    Colour = Color4.SlateGray
                                                },
                                                cumulativePath = new SmoothPath
                                                {
                                                    AutoSizeAxes = Axes.None,
                                                    RelativeSizeAxes = Axes.Both,
                                                    PathRadius = 2,
                                                    Colour = Color4.Yellow
                                                },
                                            }
                                        },
                                        hoverMarker = new CircularContainer
                                        {
                                            Origin = Anchor.Centre,
                                            Size = new Vector2(12),
                                            Masking = true,
                                            BorderThickness = 2,
                                            BorderColour = Color4.White,
                                            Alpha = 0,
                                            Child = hoverMarkerFill = new Box
                                            {
                                                RelativeSizeAxes = Axes.Both,
                                                Colour = Color4.Yellow,
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    },
                    new Drawable[]
                    {
                        descriptionText = new OsuTextFlowContainer
                        {
                            Anchor = Anchor.Centre,
                            Origin = Anchor.Centre,
                            AutoSizeAxes = Axes.Both,
                            Padding = new MarginPadding { Top = 5 }
                        },
                    }
                }
            };

            AddLayout(pathCache);
        }

        public void SetData((int x, int y)[] data, int? userRating)
        {
            this.data = data;
            this.userRating = userRating;

            xRange = (data.Select(d => d.x).DefaultIfEmpty().Min(), data.Select(d => d.x).DefaultIfEmpty().Max());
            yRange = (0, (int)roundToSignificant(data.Select(d => d.y).DefaultIfEmpty().Max()));

            updateGraph();
        }

        protected override void UpdateAfterChildren()
        {
            base.UpdateAfterChildren();

            if (!pathCache.IsValid)
            {
                updatePaths();
                pathCache.Validate();
            }
        }

        private void updateGraph()
        {
            grid.Clear();

            for (int step = 0; step <= x_divisions; step++)
            {
                grid.Add(new VerticalLine
                {
                    RelativeSizeAxes = Axes.Y,
                    RelativePositionAxes = Axes.X,
                    X = (float)step / x_divisions,
                    Colour = OsuColour.Gray(0.5f)
                });

                grid.Add(new OsuSpriteText
                {
                    Anchor = Anchor.BottomLeft,
                    Origin = Anchor.CentreRight,
                    Rotation = -40,
                    RelativePositionAxes = Axes.X,
                    X = (float)step / x_divisions,
                    Y = 5,
                    Text = (xRange.min + (xRange.max - xRange.min) / x_divisions * step).ToString(),
                    UseFullGlyphHeight = false,
                    Font = OsuFont.Default.With(size: 12),
                    Colour = OsuColour.Gray(0.5f)
                });
            }

            for (int step = 0; step <= y_divisions; step++)
            {
                grid.Add(new HorizontalLine
                {
                    RelativeSizeAxes = Axes.X,
                    RelativePositionAxes = Axes.Y,
                    Y = (float)step / y_divisions,
                    Colour = OsuColour.Gray(0.5f)
                });

                grid.Add(new OsuSpriteText
                {
                    Origin = Anchor.CentreRight,
                    RelativePositionAxes = Axes.Y,
                    Y = (float)step / y_divisions,
                    X = -5,
                    Text = (yRange.max - (yRange.max - yRange.min) / y_divisions * step).ToString(),
                    UseFullGlyphHeight = false,
                    Font = OsuFont.Default.With(size: 12),
                    Colour = OsuColour.Gray(0.5f)
                });

                grid.Add(new OsuSpriteText
                {
                    Anchor = Anchor.TopRight,
                    Origin = Anchor.CentreLeft,
                    RelativePositionAxes = Axes.Y,
                    Y = (float)step / y_divisions,
                    X = 5,
                    Text = $"{1.0 - (float)step / y_divisions:P1}",
                    UseFullGlyphHeight = false,
                    Font = OsuFont.Default.With(size: 12),
                    Colour = OsuColour.Gray(0.5f)
                });
            }

            if (userRating != null)
            {
                grid.Add(new UserRatingLine(userRating.Value)
                {
                    RelativeSizeAxes = Axes.Y,
                    RelativePositionAxes = Axes.X,
                    X = pointOnGraph(userRating.Value, 0).X,
                    Colour = Color4.Green
                });
            }

            foreach (var point in data!)
            {
                grid.Add(new Circle
                {
                    Origin = Anchor.Centre,
                    RelativePositionAxes = Axes.Both,
                    Position = pointOnGraph(point.x, point.y),
                    Size = new Vector2(8),
                    Colour = Color4.SlateGray
                });
            }

            if (data.Length == 0)
                descriptionText.Text = "No games have been played yet.";
            else if (userRating == null)
                descriptionText.Text = "Play more games to get rated!";
            else
            {
                int countPlayersBelow = data.Where(d => d.x < userRating).Sum(d => d.y);
                int countPlayersAbove = data.Where(d => d.x >= userRating).Sum(d => d.y);
                float p = (float)countPlayersBelow / (countPlayersBelow + countPlayersAbove);

                descriptionText.AddText("You are better than ");
                descriptionText.AddText($"{p:P0}", s =>
                {
                    s.Font = OsuFont.GetFont(weight: FontWeight.SemiBold);
                    s.Colour = Color4.Yellow;
                });
                descriptionText.AddText(" of players.");
            }

            updatePaths();
        }

        private void updatePaths()
        {
            if (data == null)
                return;

            distributionPath.ClearVertices();
            cumulativePath.ClearVertices();

            foreach (var point in data)
                distributionPath.AddVertex(pointOnGraph(point.x, point.y) * pathContainer.DrawSize);

            int currentCount = 0;
            int totalCount = data.Sum(d => d.y);

            foreach (var point in data)
            {
                currentCount += point.y;
                float p = (float)currentCount / totalCount;
                cumulativePath.AddVertex(new Vector2(pointOnGraph(point.x, 0).X, 1 - p) * (pathContainer.DrawSize - new Vector2(0, 2)));
            }
        }

        private Vector2 pointOnGraph(int x, int y)
        {
            float xPos = ((float)x - xRange.min) / (xRange.max - xRange.min);
            float yPos = 1 - ((float)y - yRange.min) / (yRange.max - yRange.min);
            return new Vector2(xPos, yPos);
        }

        private static double roundToSignificant(double value)
        {
            if (value == 0)
                return 0;

            double scale = Math.Pow(10, Math.Floor(Math.Log10(value)));
            return Math.Ceiling(value / scale) * scale;
        }

        protected override bool OnHover(HoverEvent e)
        {
            hoverMarker.FadeTo(1f, 200);
            return true;
        }

        protected override void OnHoverLost(HoverLostEvent e)
        {
            hoverMarker.FadeTo(0f, 200);
        }

        protected override bool OnMouseMove(MouseMoveEvent e)
        {
            var content = TooltipContent;

            hoverMarker.Position = grid.ToLocalSpace(content.Position);
            hoverMarkerFill.Colour = content.Colour;

            return true;
        }

        public ITooltip<RatingDistributionGraphTooltipData> GetCustomTooltip() => new RatingDistributionGraphTooltip();

        public RatingDistributionGraphTooltipData TooltipContent
        {
            get
            {
                if (data == null)
                    return new RatingDistributionGraphTooltipData();

                Vector2 mousePos = GetContainingInputManager()!.CurrentState.Mouse.Position;

                float minDistToCursor = float.MaxValue;
                Vector2 closestPointToCursor = Vector2.Zero;
                Color4 closestColourToCursor = Color4.White;
                int closestRatingToCursor = 0;
                string closestValueToCursor = string.Empty;

                if (userRating != null)
                {
                    Vector2 userRatingPos1 = grid.ToScreenSpace(pointOnGraph(userRating.Value, 0) * grid.DrawSize);
                    Vector2 userRatingPos2 = grid.ToScreenSpace(pointOnGraph(userRating.Value, yRange.max) * grid.DrawSize);

                    minDistToCursor = Vector2.Distance(mousePos, userRatingPos1);
                    closestPointToCursor = userRatingPos1;
                    closestColourToCursor = Color4.Green;
                    closestRatingToCursor = userRating.Value;
                    closestValueToCursor = $"Your rating ({userRating})";

                    float d = Vector2.Distance(mousePos, userRatingPos2);

                    if (d < minDistToCursor)
                    {
                        minDistToCursor = d;
                        closestPointToCursor = userRatingPos2;
                    }
                }

                for (int i = 0; i < distributionPath.Vertices.Count; i++)
                {
                    Vector2 pos = distributionPath.ToScreenSpace(distributionPath.Vertices[i] + new Vector2(2, 0));
                    float d = Vector2.Distance(mousePos, pos);

                    if (d < minDistToCursor)
                    {
                        minDistToCursor = d;
                        closestPointToCursor = pos;
                        closestColourToCursor = Color4.SlateGray;
                        closestRatingToCursor = data![i].x;
                        closestValueToCursor = $"Players: {data![i].y}";
                    }
                }

                int currentCount = 0;
                int totalCount = data!.Sum(p => p.y);

                for (int i = 0; i < cumulativePath.Vertices.Count; i++)
                {
                    currentCount += data![i].y;

                    Vector2 pos = distributionPath.ToScreenSpace(cumulativePath.Vertices[i] + new Vector2(2));
                    float d = Vector2.Distance(mousePos, pos);

                    if (d < minDistToCursor)
                    {
                        minDistToCursor = d;
                        closestPointToCursor = pos;
                        closestColourToCursor = Color4.Yellow;
                        closestRatingToCursor = data![i].x;
                        closestValueToCursor = $"Cumulative: {(float)currentCount / totalCount:P1}";
                    }
                }

                if (float.IsNaN(minDistToCursor) || minDistToCursor == float.MaxValue)
                    return new RatingDistributionGraphTooltipData();

                return new RatingDistributionGraphTooltipData
                {
                    Colour = closestColourToCursor,
                    Position = closestPointToCursor,
                    Rating = closestRatingToCursor,
                    Value = closestValueToCursor,
                };
            }
        }

        /// <summary>
        /// A simple vertical line that always remains 1px in size.
        /// </summary>
        private class VerticalLine : Box
        {
            protected override void Update()
            {
                base.Update();
                Width = Parent!.DrawWidth / Parent.ScreenSpaceDrawQuad.Width;
            }
        }

        /// <summary>
        /// A simple horizontal line that always remains 1px in size.
        /// </summary>
        private class HorizontalLine : Box
        {
            protected override void Update()
            {
                base.Update();
                Height = Parent!.DrawHeight / Parent.ScreenSpaceDrawQuad.Height;
            }
        }

        private class UserRatingLine : CompositeDrawable
        {
            public UserRatingLine(int rating)
            {
                InternalChildren = new Drawable[]
                {
                    new Box
                    {
                        Anchor = Anchor.TopCentre,
                        Origin = Anchor.TopCentre,
                        RelativeSizeAxes = Axes.Y,
                        Width = 2,
                    },
                    new Circle
                    {
                        Anchor = Anchor.TopCentre,
                        Origin = Anchor.Centre,
                        Size = new Vector2(8),
                    },
                    new Circle
                    {
                        Anchor = Anchor.BottomCentre,
                        Origin = Anchor.Centre,
                        Size = new Vector2(8),
                    },
                    new OsuSpriteText
                    {
                        Anchor = Anchor.TopCentre,
                        Origin = Anchor.BottomCentre,
                        Y = -4,
                        Text = $"Your rating ({rating})",
                        Font = OsuFont.Torus.With(size: 12),
                    }
                };
            }
        }

        private class RatingDistributionGraphTooltip : VisibilityContainer, ITooltip<RatingDistributionGraphTooltipData>
        {
            private readonly OsuSpriteText ratingText;
            private readonly Drawable valueColour;
            private readonly OsuSpriteText valueText;

            private RatingDistributionGraphTooltipData content = new RatingDistributionGraphTooltipData();
            private bool instantMove = true;

            public RatingDistributionGraphTooltip()
            {
                AutoSizeAxes = Axes.Both;

                InternalChild = new Container
                {
                    AutoSizeAxes = Axes.Both,
                    Masking = true,
                    CornerRadius = 3,
                    Children = new Drawable[]
                    {
                        new Box
                        {
                            RelativeSizeAxes = Axes.Both,
                            Colour = Color4.Black,
                            Alpha = 0.7f
                        },
                        new FillFlowContainer
                        {
                            AutoSizeAxes = Axes.Both,
                            Padding = new MarginPadding(8),
                            Direction = FillDirection.Vertical,
                            Spacing = new Vector2(3),
                            Children = new Drawable[]
                            {
                                ratingText = new OsuSpriteText
                                {
                                    Font = OsuFont.Torus.With(weight: FontWeight.SemiBold)
                                },
                                new FillFlowContainer
                                {
                                    AutoSizeAxes = Axes.Both,
                                    Direction = FillDirection.Horizontal,
                                    Spacing = new Vector2(3),
                                    Children = new[]
                                    {
                                        valueColour = new Box
                                        {
                                            Size = new Vector2(12)
                                        },
                                        valueText = new OsuSpriteText
                                        {
                                            Font = OsuFont.Torus.With(size: 12)
                                        }
                                    }
                                }
                            }
                        }
                    }
                };
            }

            public void SetContent(RatingDistributionGraphTooltipData content)
            {
                this.content = content;

                ratingText.Text = content.Rating.ToString();
                valueColour.Colour = content.Colour;
                valueText.Text = content.Value;
            }

            public void Move(Vector2 pos)
            {
                pos = Parent!.ToLocalSpace(content.Position) - new Vector2(DrawWidth + 10, 0);

                if (instantMove)
                {
                    Position = pos;
                    instantMove = false;
                }
                else
                    this.MoveTo(pos, 200, Easing.OutQuint);
            }

            protected override void PopIn()
            {
                instantMove |= !IsPresent;
                this.FadeIn(200, Easing.OutQuint);
            }

            protected override void PopOut()
            {
                this.FadeOut(200, Easing.OutQuint);
            }
        }

        public class RatingDistributionGraphTooltipData
        {
            public Color4 Colour;
            public Vector2 Position;

            public int Rating;
            public string Value = string.Empty;
        }
    }
}
