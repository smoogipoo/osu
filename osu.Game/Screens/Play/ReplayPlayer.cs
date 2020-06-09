﻿// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Game.Scoring;
using osu.Game.Screens.Ranking;

namespace osu.Game.Screens.Play
{
    public class ReplayPlayer : Player
    {
        private readonly Score score;

        // Disallow replays from failing. (see https://github.com/ppy/osu/issues/6108)
        protected override bool CheckModsAllowFailure() => false;

        public ReplayPlayer(Score score, bool allowPause = true, bool showResults = true)
            : base(allowPause, showResults)
        {
            this.score = score;
        }

        protected override void PrepareReplay()
        {
            DrawableRuleset?.SetReplayScore(score);
        }

        protected override Score CreateScore() => DrawableRuleset.ReplayScore;

        protected override ResultsScreen CreateResults(ScoreInfo score) => new SoloResultsScreen(score, false);
    }
}
