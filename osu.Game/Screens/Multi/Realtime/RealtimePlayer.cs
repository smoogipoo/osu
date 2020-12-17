// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using osu.Framework.Allocation;
using osu.Game.Online.Multiplayer;
using osu.Game.Online.RealtimeMultiplayer;
using osu.Game.Scoring;
using osu.Game.Screens.Multi.Play;
using osu.Game.Screens.Ranking;

namespace osu.Game.Screens.Multi.Realtime
{
    public class RealtimePlayer : TimeshiftPlayer
    {
        protected override bool PauseOnFocusLost => false;

        // Disallow fails in multiplayer for now.
        protected override bool CheckModsAllowFailure() => false;

        [Resolved]
        private StatefulMultiplayerClient client { get; set; }

        private bool started;
        private bool resultsReady;

        public RealtimePlayer(PlaylistItem playlistItem, bool showResults = true)
            : base(playlistItem, false, showResults)
        {
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            if (Token == null)
                return; // Todo: Somehow handle token retrieval failure.

            client.MatchStarted += onMatchStarted;
            client.ResultsReady += onResultsReady;
            client.ChangeState(MultiplayerUserState.Loaded);

            while (!started)
                Thread.Sleep(100);
        }

        private void onMatchStarted() => started = true;

        private void onResultsReady() => resultsReady = true;

        protected override async Task<ScoreInfo> CreateScore()
        {
            var score = await base.CreateScore();

            await client.ChangeState(MultiplayerUserState.FinishedPlay);
            while (!resultsReady)
                await Task.Delay(100);

            return score;
        }

        protected override ResultsScreen CreateResults(ScoreInfo score)
        {
            Debug.Assert(RoomId.Value != null);
            return new RealtimeResultsScreen(score, RoomId.Value.Value, PlaylistItem);
        }

        protected override void Dispose(bool isDisposing)
        {
            base.Dispose(isDisposing);

            if (client != null)
            {
                client.MatchStarted -= onMatchStarted;
                client.ResultsReady -= onResultsReady;
            }
        }
    }
}
