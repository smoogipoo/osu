// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Screens;
using osu.Game.Scoring;
using osu.Game.Screens.Backgrounds;

namespace osu.Game.Screens.Results
{
    public class ResultsScreen : OsuScreen
    {
        public override bool DisallowExternalBeatmapRulesetChanges => true;

        protected override BackgroundScreen CreateBackground() => new BackgroundScreenBeatmap(Beatmap.Value);

        private readonly ScoreInfo score;

        public ResultsScreen(ScoreInfo score)
        {
            this.score = score;
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            InternalChild = new ScorePanel(score)
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                State = PanelState.Expanded
            };
        }

        public override void OnEntering(IScreen last)
        {
            base.OnEntering(last);

            Background.FadeTo(0.5f, 250);
        }

        public override bool OnExiting(IScreen next)
        {
            Background.FadeTo(1, 250);

            return base.OnExiting(next);
        }
    }
}
