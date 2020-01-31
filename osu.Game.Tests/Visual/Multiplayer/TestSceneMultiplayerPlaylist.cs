// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Colour;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Input.Events;
using osu.Framework.Utils;
using osu.Game.Beatmaps;
using osu.Game.Beatmaps.Drawables;
using osu.Game.Graphics.Containers;
using osu.Game.Online.Chat;
using osu.Game.Online.Multiplayer;
using osu.Game.Rulesets;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Tests.Visual.Multiplayer
{
    public class TestSceneMultiplayerPlaylist : OsuTestScene
    {
        [Resolved]
        private RulesetStore rulesets { get; set; }

        [Resolved]
        private BeatmapManager beatmaps { get; set; }

        [SetUp]
        public void Setup() => Schedule(() =>
        {
            MultiplayerPlaylist list;
            Add(list = new MultiplayerPlaylist
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                Size = new Vector2(500)
            });

            List<BeatmapSetInfo> beatmapSets = beatmaps.GetAllUsableBeatmapSets().ToList();

            for (int i = 0; i < 25; i++)
            {
                list.Items.Add(new PlaylistItem
                {
                    Beatmap = beatmapSets[RNG.Next(0, beatmapSets.Count)].Beatmaps[0],
                    Ruleset = rulesets.GetRuleset(RNG.Next(0, 4))
                });
            }
        });

        private class MultiplayerPlaylist : RearrangeableListContainer<PlaylistItem>
        {
            protected override ScrollContainer<Drawable> CreateScrollContainer() => new OsuScrollContainer
            {
                ScrollbarVisible = false
            };

            protected override FillFlowContainer<RearrangeableListItem<PlaylistItem>> CreateListFillFlowContainer() => new FillFlowContainer<RearrangeableListItem<PlaylistItem>>
            {
                LayoutDuration = 200,
                LayoutEasing = Easing.OutQuint,
                Spacing = new Vector2(0, 2)
            };

            protected override RearrangeableListItem<PlaylistItem> CreateDrawable(PlaylistItem item) => new DrawableMultiplayerPlaylistItem(item);
        }

        private class DrawableMultiplayerPlaylistItem : RearrangeableListItem<PlaylistItem>
        {
            private ItemHandle handle;

            public DrawableMultiplayerPlaylistItem(PlaylistItem item)
                : base(item)
            {
                RelativeSizeAxes = Axes.X;
                Height = 50;

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
                            new Container
                            {
                                RelativeSizeAxes = Axes.Both,
                                Masking = true,
                                CornerRadius = 10,
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
