// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Colour;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Input.Events;
using osu.Framework.Threading;
using osu.Game.Beatmaps;
using osu.Game.Beatmaps.Drawables;
using osu.Game.Graphics;
using osu.Game.Graphics.Containers;
using osu.Game.Online.Chat;
using osu.Game.Online.Multiplayer;
using osu.Game.Rulesets;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Screens.Multi.Match.Components
{
    public class DrawablePlaylistItem : RearrangeableListItem<PlaylistItem>
    {
        public Action RequestSelection;

        private Container maskingContainer;
        private Container difficultyIconContainer;
        private LinkFlowContainer beatmapText;
        private LinkFlowContainer authorText;
        private ItemHandle handle;

        private readonly Bindable<BeatmapInfo> beatmap = new Bindable<BeatmapInfo>();
        private readonly Bindable<RulesetInfo> ruleset = new Bindable<RulesetInfo>();

        private readonly PlaylistItem item;

        public DrawablePlaylistItem(PlaylistItem item)
            : base(item)
        {
            this.item = item;

            RelativeSizeAxes = Axes.X;
            Height = 50;

            beatmap.BindTo(item.Beatmap);
            ruleset.BindTo(item.Ruleset);
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
                                new PanelBackground
                                {
                                    RelativeSizeAxes = Axes.Both,
                                    Beatmap = { BindTarget = beatmap }
                                },
                                new FillFlowContainer
                                {
                                    RelativeSizeAxes = Axes.Both,
                                    Padding = new MarginPadding { Left = 8 },
                                    Spacing = new Vector2(8, 0),
                                    Direction = FillDirection.Horizontal,
                                    Children = new Drawable[]
                                    {
                                        difficultyIconContainer = new Container
                                        {
                                            Anchor = Anchor.CentreLeft,
                                            Origin = Anchor.CentreLeft,
                                            AutoSizeAxes = Axes.Both,
                                        },
                                        new FillFlowContainer
                                        {
                                            Anchor = Anchor.CentreLeft,
                                            Origin = Anchor.CentreLeft,
                                            AutoSizeAxes = Axes.Both,
                                            Direction = FillDirection.Vertical,
                                            Children = new Drawable[]
                                            {
                                                beatmapText = new LinkFlowContainer { AutoSizeAxes = Axes.Both },
                                                authorText = new LinkFlowContainer { AutoSizeAxes = Axes.Both }
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

        protected override void LoadComplete()
        {
            base.LoadComplete();

            beatmap.BindValueChanged(_ => scheduleRefresh());
            ruleset.BindValueChanged(_ => scheduleRefresh());

            refresh();
        }

        private ScheduledDelegate scheduledRefresh;

        private void scheduleRefresh()
        {
            scheduledRefresh?.Cancel();
            scheduledRefresh = Schedule(refresh);
        }

        private void refresh()
        {
            difficultyIconContainer.Child = new DifficultyIcon(beatmap.Value, ruleset.Value) { Size = new Vector2(32) };

            beatmapText.Clear();
            beatmapText.AddLink(item.Beatmap.ToString(), LinkAction.OpenBeatmap, item.Beatmap.Value.OnlineBeatmapID.ToString());

            authorText.Clear();

            if (item.Beatmap?.Value?.Metadata?.Author != null)
            {
                authorText.AddText("mapped by ");
                authorText.AddUserLink(item.Beatmap.Value?.Metadata.Author);
            }
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
            public readonly Bindable<BeatmapInfo> Beatmap = new Bindable<BeatmapInfo>();

            public PanelBackground()
            {
                InternalChildren = new Drawable[]
                {
                    new UpdateableBeatmapBackgroundSprite
                    {
                        RelativeSizeAxes = Axes.Both,
                        FillMode = FillMode.Fill,
                        Beatmap = { BindTarget = Beatmap }
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
