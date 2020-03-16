// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Game.Scoring;

namespace osu.Game.Screens.Results
{
    public class ResultsScreen : OsuScreen
    {
        public override bool DisallowExternalBeatmapRulesetChanges => true;

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
    }
}
