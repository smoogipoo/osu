// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Threading;
using osu.Framework.Allocation;
using osu.Game.Online.Multiplayer;
using osu.Game.Online.RealtimeMultiplayer;
using osu.Game.Screens.Multi.Play;

namespace osu.Game.Screens.Multi.Realtime
{
    public class RealtimePlayer : TimeshiftPlayer
    {
        [Resolved]
        private StatefulMultiplayerClient client { get; set; }

        private bool started;

        public RealtimePlayer(PlaylistItem playlistItem)
            : base(playlistItem)
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

        private void onResultsReady()
        {
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
