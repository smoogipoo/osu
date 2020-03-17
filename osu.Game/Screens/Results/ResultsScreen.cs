// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Screens;
using osu.Game.Graphics.UserInterface;
using osu.Game.Scoring;
using osu.Game.Screens.Backgrounds;
using osu.Game.Screens.Ranking.Pages;
using osuTK;

namespace osu.Game.Screens.Results
{
    public class ResultsScreen : OsuScreen
    {
        public override bool DisallowExternalBeatmapRulesetChanges => true;

        protected override BackgroundScreen CreateBackground() => new BackgroundScreenBeatmap(Beatmap.Value);

        private readonly ScoreInfo score;

        private Drawable bottomPanel;

        public ResultsScreen(ScoreInfo score)
        {
            this.score = score;
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            InternalChildren = new Drawable[]
            {
                new ScorePanel(score)
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    State = PanelState.Expanded
                },
                bottomPanel = new Container
                {
                    Anchor = Anchor.BottomLeft,
                    Origin = Anchor.BottomLeft,
                    RelativeSizeAxes = Axes.X,
                    Height = TwoLayerButton.SIZE_EXTENDED.Y,
                    Alpha = 0,
                    Children = new Drawable[]
                    {
                        new Box
                        {
                            RelativeSizeAxes = Axes.Both,
                            Colour = Color4Extensions.FromHex("#333")
                        },
                        new FillFlowContainer
                        {
                            Anchor = Anchor.Centre,
                            Origin = Anchor.Centre,
                            AutoSizeAxes = Axes.Both,
                            Spacing = new Vector2(5),
                            Direction = FillDirection.Horizontal,
                            Children = new Drawable[]
                            {
                                new ReplayDownloadButton(score),
                                new RetryButton()
                            }
                        }
                    }
                }
            };
        }

        public override void OnEntering(IScreen last)
        {
            base.OnEntering(last);

            Background.FadeTo(0.5f, 250);
            bottomPanel.FadeTo(1, 250);
        }

        public override bool OnExiting(IScreen next)
        {
            Background.FadeTo(1, 250);

            return base.OnExiting(next);
        }
    }
}
