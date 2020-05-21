// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using osu.Game.Online.API;
using osu.Game.Scoring;

namespace osu.Game.Screens.Ranking
{
    public class SoloResultsScreen : ResultsScreen
    {
        public SoloResultsScreen(ScoreInfo score, bool allowRetry = true)
            : base(score, allowRetry)
        {
        }

        protected override APIRequest FetchScores(Action<IEnumerable<ScoreInfo>> scoresCallback)
        {
            return null;
        }
    }
}
