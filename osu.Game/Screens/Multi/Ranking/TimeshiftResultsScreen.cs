// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using osu.Game.Online.API;
using osu.Game.Online.API.Requests;
using osu.Game.Scoring;
using osu.Game.Screens.Ranking;

namespace osu.Game.Screens.Multi.Ranking
{
    public class TimeshiftResultsScreen : ResultsScreen
    {
        private readonly int roomId;
        private readonly int playlistItemId;

        public TimeshiftResultsScreen(ScoreInfo score, int roomId, int playlistItemId, bool allowRetry = true)
            : base(score, allowRetry)
        {
            this.roomId = roomId;
            this.playlistItemId = playlistItemId;
        }

        protected override APIRequest FetchScores(Action<IEnumerable<ScoreInfo>> scoresCallback)
        {
            var req = new GetRoomPlaylistScoresRequest(roomId, playlistItemId);
            req.Success += result => result.First().CreateScoreInfo()
            throw new NotImplementedException();
        }
    }
}
