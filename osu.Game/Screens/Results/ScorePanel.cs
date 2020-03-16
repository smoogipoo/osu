// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework;
using osu.Framework.Allocation;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Colour;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Game.Scoring;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Screens.Results
{
    public class ScorePanel : CompositeDrawable, IStateful<PanelState>
    {
        private const float contracted_width = 160;
        private const float contracted_height = 320;
        private const float expanded_width = 360;
        private const float expanded_height = 560;

        private const float expanded_top_layer_height = 53;
        private const float contracted_top_layer_height = 40;

        private const double appear_delay = 300;

        private static readonly ColourInfo expanded_top_layer_colour = ColourInfo.GradientVertical(Color4Extensions.FromHex("#444"), Color4Extensions.FromHex("#333"));
        private static readonly ColourInfo expanded_middle_layer_colour = ColourInfo.GradientVertical(Color4Extensions.FromHex("#555"), Color4Extensions.FromHex("#333"));
        private static readonly Color4 contracted_top_layer_colour = Color4Extensions.FromHex("#353535");
        private static readonly Color4 contracted_middle_layer_colour = Color4Extensions.FromHex("#444");

        public event Action<PanelState> StateChanged;

        private readonly ScoreInfo score;

        private Container topLayer;
        private Container middleLayer;
        private Drawable topLayerBackground;
        private Drawable middleLayerBackground;
        private Container topLayerContent;
        private Container middleLayerContent;

        public ScorePanel(ScoreInfo score)
        {
            this.score = score;
        }

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

                StateChanged?.Invoke(value);
            }
        }

        private void updateState()
        {
            topLayer.MoveToY(0, 200, Easing.OutQuint);
            middleLayer.MoveToY(0, 200, Easing.OutQuint);

            foreach (var c in topLayerContent)
                c.FadeOut(50).Expire();
            foreach (var c in middleLayerContent)
                c.FadeOut(50).Expire();

            switch (state)
            {
                case PanelState.Expanded:
                    this.ResizeTo(new Vector2(expanded_width, expanded_height), 200, Easing.OutQuint);

                    topLayerBackground.FadeColour(expanded_top_layer_colour, 200, Easing.OutQuint);
                    middleLayerBackground.FadeColour(expanded_middle_layer_colour, 200, Easing.OutQuint);

                    topLayerContent.Add(new ExpandedPanelTopContent(score.User));
                    middleLayerContent.Add(new ExpandedPanelMiddleContent(score));

                    using (BeginDelayedSequence(appear_delay, true))
                    {
                        topLayer.MoveToY(-expanded_top_layer_height / 2, 200, Easing.OutQuint);
                        middleLayer.MoveToY(expanded_top_layer_height / 2, 200, Easing.OutQuint);
                    }

                    break;

                case PanelState.Contracted:
                    this.ResizeTo(new Vector2(contracted_width, contracted_height), 200, Easing.OutQuint);

                    topLayerBackground.FadeColour(contracted_top_layer_colour, 200, Easing.OutQuint);
                    middleLayerBackground.FadeColour(contracted_middle_layer_colour, 200, Easing.OutQuint);

                    using (BeginDelayedSequence(appear_delay, true))
                    {
                        topLayer.MoveToY(-contracted_top_layer_height / 2, 200, Easing.OutQuint);
                        middleLayer.MoveToY(contracted_top_layer_height / 2, 200, Easing.OutQuint);
                    }

                    break;
            }

            foreach (var c in topLayerContent)
                c.FadeOut().Delay(appear_delay).FadeIn(50);
            foreach (var c in middleLayerContent)
                c.FadeOut().Delay(appear_delay).FadeIn(50);
        }
    }
}
