// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using NUnit.Framework;
using osu.Framework;
using osu.Framework.Allocation;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Colour;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osu.Game.Scoring;
using osu.Game.Screens.Results;
using osu.Game.Users;
using osu.Game.Users.Drawables;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Tests.Visual.Results
{
    public class TestSceneThreeLayerPanel : OsuTestScene
    {
        private ThreeLayerPanel panel;

        [SetUp]
        public void Setup() => Schedule(() =>
        {
            Child = panel = new ThreeLayerPanel
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                State = PanelState.Expanded
            };
        });

        [Test]
        public void TestExpanded()
        {
            AddStep("set expanded state", () => panel.State = PanelState.Expanded);
        }

        [Test]
        public void TestContracted()
        {
            AddStep("set expanded state", () => panel.State = PanelState.Contracted);
        }

        private class ThreeLayerPanel : CompositeDrawable, IStateful<PanelState>
        {
            private const float contracted_width = 160;
            private const float contracted_height = 320;
            private const float expanded_width = 460;
            private const float expanded_height = 560;

            private const float expanded_top_layer_height = 70;
            private const float contracted_top_layer_height = 40;

            private static readonly ColourInfo expanded_top_layer_colour = ColourInfo.GradientVertical(Color4Extensions.FromHex("#444"), Color4Extensions.FromHex("#333"));
            private static readonly ColourInfo expanded_middle_layer_colour = ColourInfo.GradientVertical(Color4Extensions.FromHex("#555"), Color4Extensions.FromHex("#333"));
            private static readonly Color4 contracted_top_layer_colour = Color4Extensions.FromHex("#353535");
            private static readonly Color4 contracted_middle_layer_colour = Color4Extensions.FromHex("#444");

            public event Action<PanelState> StateChanged;

            private Container topLayer;
            private Container middleLayer;
            private Drawable topLayerBackground;
            private Drawable middleLayerBackground;
            private Container topLayerContent;
            private Container middleLayerContent;

            [BackgroundDependencyLoader]
            private void load()
            {
                InternalChildren = new Drawable[]
                {
                    topLayer = new Container
                    {
                        Name = "Top layer",
                        RelativeSizeAxes = Axes.X,
                        Height = 120,
                        Children = new Drawable[]
                        {
                            new Container
                            {
                                RelativeSizeAxes = Axes.Both,
                                CornerRadius = 20,
                                CornerExponent = 2.5f,
                                Masking = true,
                                Child = topLayerBackground = new Box { RelativeSizeAxes = Axes.Both }
                            },
                            topLayerContent = new Container { RelativeSizeAxes = Axes.Both }
                        }
                    },
                    middleLayer = new Container
                    {
                        Name = "Middle layer",
                        RelativeSizeAxes = Axes.Both,
                        Children = new Drawable[]
                        {
                            new Container
                            {
                                RelativeSizeAxes = Axes.Both,
                                CornerRadius = 20,
                                CornerExponent = 2.5f,
                                Masking = true,
                                Child = middleLayerBackground = new Box { RelativeSizeAxes = Axes.Both }
                            },
                            middleLayerContent = new Container { RelativeSizeAxes = Axes.Both }
                        }
                    }
                };
            }

            protected override void LoadComplete()
            {
                base.LoadComplete();

                if (state == PanelState.Expanded)
                {
                    topLayerBackground.FadeColour(expanded_top_layer_colour);
                    middleLayerBackground.FadeColour(expanded_middle_layer_colour);
                }
                else
                {
                    topLayerBackground.FadeColour(contracted_top_layer_colour);
                    middleLayerBackground.FadeColour(contracted_middle_layer_colour);
                }

                updateState();
            }

            private PanelState state = PanelState.Contracted;

            public PanelState State
            {
                get => state;
                set
                {
                    if (state == value)
                        return;

                    state = value;

                    if (LoadState >= LoadState.Ready)
                        updateState();
                }
            }

            private void updateState()
            {
                topLayer.MoveToY(0, 200, Easing.OutQuint);
                middleLayer.MoveToY(0, 200, Easing.OutQuint);

                foreach (var c in topLayerContent)
                    c.FadeOut(200).Expire();

                foreach (var c in middleLayerContent)
                    c.FadeOut(200).Expire();

                switch (state)
                {
                    case PanelState.Expanded:
                        this.ResizeTo(new Vector2(expanded_width, expanded_height), 200, Easing.OutQuint);

                        topLayerBackground.FadeColour(expanded_top_layer_colour, 200, Easing.OutQuint);
                        middleLayerBackground.FadeColour(expanded_middle_layer_colour, 200, Easing.OutQuint);

                        using (BeginDelayedSequence(300, true))
                        {
                            topLayer.MoveToY(-expanded_top_layer_height / 2, 200, Easing.OutQuint);
                            middleLayer.MoveToY(expanded_top_layer_height / 2, 200, Easing.OutQuint);
                        }

                        topLayerContent.Add(new ExpandedTopContent(new User { Id = 2, Username = "peppy" }));
                        middleLayerContent.Add(new ExpandedMiddleContent(new Score()));

                        break;

                    case PanelState.Contracted:
                        this.ResizeTo(new Vector2(contracted_width, contracted_height), 200, Easing.OutQuint);

                        topLayerBackground.FadeColour(contracted_top_layer_colour, 200, Easing.OutQuint);
                        middleLayerBackground.FadeColour(contracted_middle_layer_colour, 200, Easing.OutQuint);

                        using (BeginDelayedSequence(300, true))
                        {
                            topLayer.MoveToY(-contracted_top_layer_height / 2, 200, Easing.OutQuint);
                            middleLayer.MoveToY(contracted_top_layer_height / 2, 200, Easing.OutQuint);
                        }

                        break;
                }
            }
        }

        private class ExpandedTopContent : CompositeDrawable
        {
            private readonly User user;

            public ExpandedTopContent(User user)
            {
                this.user = user;
                Anchor = Anchor.TopCentre;
                Origin = Anchor.Centre;

                AutoSizeAxes = Axes.Both;
            }

            [BackgroundDependencyLoader]
            private void load()
            {
                InternalChild = new FillFlowContainer
                {
                    AutoSizeAxes = Axes.Both,
                    Direction = FillDirection.Vertical,
                    Children = new Drawable[]
                    {
                        new UpdateableAvatar(user)
                        {
                            Anchor = Anchor.TopCentre,
                            Origin = Anchor.TopCentre,
                            Size = new Vector2(95),
                            CornerRadius = 20,
                            CornerExponent = 2.5f,
                            Masking = true,
                        },
                        new OsuSpriteText
                        {
                            Anchor = Anchor.TopCentre,
                            Origin = Anchor.TopCentre,
                            Text = user.Username,
                            Font = OsuFont.Torus.With(size: 16, weight: FontWeight.SemiBold)
                        }
                    }
                };
            }
        }

        private class ExpandedMiddleContent : CompositeDrawable
        {
            public ExpandedMiddleContent(Score score)
            {
                RelativeSizeAxes = Axes.Both;
            }

            [BackgroundDependencyLoader]
            private void load()
            {
                InternalChild = new FillFlowContainer
                {
                    RelativeSizeAxes = Axes.Both,
                    Direction = FillDirection.Vertical,
                    Y = 50,
                    Children = new Drawable[]
                    {
                        new AccuracyCircle
                        {
                            Anchor = Anchor.TopCentre,
                            Origin = Anchor.TopCentre,
                            Size = new Vector2(320)
                        }
                    }
                };
            }
        }

        private enum PanelState
        {
            Expanded,
            Contracted
        }
    }
}
