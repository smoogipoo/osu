// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Colour;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Input.Events;
using osu.Game.Beatmaps;
using osu.Game.Beatmaps.Drawables;
using osu.Game.Graphics;
using osu.Game.Graphics.Containers;
using osu.Game.Online.Chat;
using osu.Game.Online.Multiplayer;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Screens.Multi.Match.Components
{
    public class DrawablePlaylistItem : RearrangeableListItem<PlaylistItem>
    {
        public Action RequestSelection;

        private Container maskingContainer;
        private ItemHandle handle;

        private readonly PlaylistItem item;

        public DrawablePlaylistItem(PlaylistItem item)
            : base(item)
        {
            this.item = item;

            RelativeSizeAxes = Axes.X;
            Height = 50;
        }

        [BackgroundDependencyLoader]
        private void load(OsuColour colours)
        {
            InternalChild = new GridContainer
            {
                RelativeSizeAxes = Axes.Both,
                Content = new[]
                {
                    new Drawable[]
                    {
                        handle = new ItemHandle
                        {
                            Anchor = Anchor.Centre,
                            Origin = Anchor.Centre,
                            Size = new Vector2(12),
                            Alpha = 0,
                        },
                        maskingContainer = new Container
                        {
                            RelativeSizeAxes = Axes.Both,
                            Masking = true,
                            CornerRadius = 10,
                            BorderColour = colours.Yellow,
                            Children = new Drawable[]
                            {
                                new PanelBackground(item.Beatmap) { RelativeSizeAxes = Axes.Both },
                                new FillFlowContainer
                                {
                                    RelativeSizeAxes = Axes.Both,
                                    Padding = new MarginPadding { Left = 8 },
                                    Spacing = new Vector2(8, 0),
                                    Direction = FillDirection.Horizontal,
                                    Children = new Drawable[]
                                    {
                                        new DifficultyIcon(item.Beatmap, item.Ruleset)
                                        {
                                            Anchor = Anchor.CentreLeft,
                                            Origin = Anchor.CentreLeft,
                                            Size = new Vector2(32)
                                        },
                                        new FillFlowContainer
                                        {
                                            Anchor = Anchor.CentreLeft,
                                            Origin = Anchor.CentreLeft,
                                            AutoSizeAxes = Axes.Both,
                                            Direction = FillDirection.Vertical,
                                            Children = new Drawable[]
                                            {
                                                new LinkFlowContainer { AutoSizeAxes = Axes.Both }.With(d =>
                                                {
                                                    d.AddLink(item.Beatmap.ToString(), LinkAction.OpenBeatmap, item.Beatmap.OnlineBeatmapID.ToString());
                                                }),
                                                new LinkFlowContainer { AutoSizeAxes = Axes.Both }.With(d =>
                                                {
                                                    if (item.Beatmap?.Metadata?.Author != null)
                                                    {
                                                        d.AddText("mapped by ");
                                                        d.AddUserLink(item.Beatmap.Metadata.Author);
                                                    }
                                                })
                                            }
                                        }
                                    }
                                },
                                new SpriteIcon
                                {
                                    Anchor = Anchor.CentreRight,
                                    Origin = Anchor.CentreRight,
                                    X = -18,
                                    Size = new Vector2(14),
                                    Icon = FontAwesome.Solid.MinusSquare
                                }
                            }
                        }
                    },
                },
                ColumnDimensions = new[] { new Dimension(GridSizeMode.AutoSize) },
            };
        }

        protected override bool IsDraggableAt(Vector2 screenSpacePos) => handle.HandlingDrag;

        protected override bool OnHover(HoverEvent e)
        {
            handle.UpdateHoverState(true);
            return base.OnHover(e);
        }

        protected override void OnHoverLost(HoverLostEvent e) => handle.UpdateHoverState(false);

        protected override bool OnClick(ClickEvent e)
        {
            RequestSelection?.Invoke();
            return true;
        }

        public void Select() => maskingContainer.BorderThickness = 5;

        public void Deselect() => maskingContainer.BorderThickness = 0;

        // For now, this is the same implementation as in PanelBackground, but supports a beatmap info rather than a working beatmap
        private class PanelBackground : Container // todo: should be a buffered container (https://github.com/ppy/osu-framework/issues/3222)
        {
            public PanelBackground(BeatmapInfo beatmapInfo)
            {
                InternalChildren = new Drawable[]
                {
                    new UpdateableBeatmapBackgroundSprite
                    {
                        RelativeSizeAxes = Axes.Both,
                        FillMode = FillMode.Fill,
                        Beatmap = { Value = beatmapInfo }
                    },
                    new Container
                    {
                        Depth = -1,
                        RelativeSizeAxes = Axes.Both,
                        // This makes the gradient not be perfectly horizontal, but diagonal at a ~40° angle
                        Shear = new Vector2(0.8f, 0),
                        Alpha = 0.5f,
                        Children = new[]
                        {
                            // The left half with no gradient applied
                            new Box
                            {
                                RelativeSizeAxes = Axes.Both,
                                RelativePositionAxes = Axes.Both,
                                Colour = Color4.Black,
                                Width = 0.4f,
                            },
                            // Piecewise-linear gradient with 3 segments to make it appear smoother
                            new Box
                            {
                                RelativeSizeAxes = Axes.Both,
                                RelativePositionAxes = Axes.Both,
                                Colour = ColourInfo.GradientHorizontal(Color4.Black, new Color4(0f, 0f, 0f, 0.9f)),
                                Width = 0.05f,
                                X = 0.4f,
                            },
                            new Box
                            {
                                RelativeSizeAxes = Axes.Both,
                                RelativePositionAxes = Axes.Both,
                                Colour = ColourInfo.GradientHorizontal(new Color4(0f, 0f, 0f, 0.9f), new Color4(0f, 0f, 0f, 0.1f)),
                                Width = 0.2f,
                                X = 0.45f,
                            },
                            new Box
                            {
                                RelativeSizeAxes = Axes.Both,
                                RelativePositionAxes = Axes.Both,
                                Colour = ColourInfo.GradientHorizontal(new Color4(0f, 0f, 0f, 0.1f), new Color4(0, 0, 0, 0)),
                                Width = 0.05f,
                                X = 0.65f,
                            },
                        }
                    }
                };
            }
        }

        private class ItemHandle : SpriteIcon
        {
            public bool HandlingDrag { get; private set; }
            private bool isHovering;

            public ItemHandle()
            {
                Margin = new MarginPadding { Horizontal = 5 };

                Icon = FontAwesome.Solid.Bars;
            }

            protected override bool OnMouseDown(MouseDownEvent e)
            {
                base.OnMouseDown(e);

                HandlingDrag = true;
                UpdateHoverState(isHovering);

                return false;
            }

            protected override void OnMouseUp(MouseUpEvent e)
            {
                base.OnMouseUp(e);

                HandlingDrag = false;
                UpdateHoverState(isHovering);
            }

            public void UpdateHoverState(bool hovering)
            {
                isHovering = hovering;

                if (isHovering || HandlingDrag)
                    this.FadeIn(100);
                else
                    this.FadeOut(100);
            }
        }
    }
}
