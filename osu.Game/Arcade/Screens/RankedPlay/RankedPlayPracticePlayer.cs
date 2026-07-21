// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Threading.Tasks;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Screens;
using osu.Game.Scoring;
using osu.Game.Screens.Play;
using osu.Game.Screens.Play.Leaderboards;
using osu.Game.Screens.Ranking;

namespace osu.Game.Arcade.Screens.RankedPlay
{
    public class RankedPlayPracticePlayer : Player
    {
        [Cached(typeof(IGameplayLeaderboardProvider))]
        private readonly DummyLeaderboardProvider leaderboardProvider = new DummyLeaderboardProvider();

        protected override async Task PrepareScoreForResultsAsync(Score score)
        {
            await base.PrepareScoreForResultsAsync(score).ConfigureAwait(false);

            Scheduler.Add(() =>
            {
                if (this.IsCurrentScreen())
                    this.Exit();
            });
        }

        protected override ResultsScreen CreateResults(ScoreInfo score)
            => new SoloResultsScreen(score);

        private class DummyLeaderboardProvider : IGameplayLeaderboardProvider
        {
            public IBindableList<GameplayLeaderboardScore> Scores { get; } = new BindableList<GameplayLeaderboardScore>();
        }
    }
}
