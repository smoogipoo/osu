// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Graphics;
using osu.Game.Scoring;

namespace osu.Game.Screens.Results
{
    public class ResultsScreen : OsuScreen
    {
        public override bool DisallowExternalBeatmapRulesetChanges => true;

        public ResultsScreen(ScoreInfo score)
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
