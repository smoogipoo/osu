// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Logging;
using osu.Framework.Screens;
using osu.Game.Online.API;
using osu.Game.Online.Multiplayer;
using osu.Game.Rulesets;
using osu.Game.Scoring;
using osu.Game.Screens.Multi.Ranking;
using osu.Game.Screens.Play;
using osu.Game.Screens.Ranking;

namespace osu.Game.Screens.Multi.Play
{
    public class TimeshiftPlayer : Player
    {
        public Action Exited;

        protected int? Token { get; private set; }

        [Resolved(typeof(Room), nameof(Room.RoomID))]
        protected Bindable<int?> RoomId { get; private set; }

        protected readonly PlaylistItem PlaylistItem;

        [Resolved]
        private IAPIProvider api { get; set; }

        [Resolved]
        private IBindable<RulesetInfo> ruleset { get; set; }

        public TimeshiftPlayer(PlaylistItem playlistItem, bool allowPause = true, bool showResults = true)
            : base(allowPause, showResults)
        {
            PlaylistItem = playlistItem;
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            Token = null;

            bool failed = false;

            // Sanity checks to ensure that TimeshiftPlayer matches the settings for the current PlaylistItem
            if (Beatmap.Value.BeatmapInfo.OnlineBeatmapID != PlaylistItem.Beatmap.Value.OnlineBeatmapID)
                throw new InvalidOperationException("Current Beatmap does not match PlaylistItem's Beatmap");

            if (ruleset.Value.ID != PlaylistItem.Ruleset.Value.ID)
                throw new InvalidOperationException("Current Ruleset does not match PlaylistItem's Ruleset");

            if (!PlaylistItem.RequiredMods.All(m => Mods.Value.Any(m.Equals)))
                throw new InvalidOperationException("Current Mods do not match PlaylistItem's RequiredMods");

            var req = new CreateRoomScoreRequest(RoomId.Value ?? 0, PlaylistItem.ID, Game.VersionHash);
            req.Success += r => Token = r.ID;
            req.Failure += e =>
            {
                failed = true;

                Logger.Error(e, "Failed to retrieve a score submission token.\n\nThis may happen if you are running an old or non-official release of osu! (ie. you are self-compiling).");

                Schedule(() =>
                {
                    ValidForResume = false;
                    this.Exit();
                });
            };

            api.Queue(req);

            while (!failed && !Token.HasValue)
                Thread.Sleep(1000);
        }

        public override bool OnExiting(IScreen next)
        {
            if (base.OnExiting(next))
                return true;

            Exited?.Invoke();

            return false;
        }

        protected override ResultsScreen CreateResults(ScoreInfo score)
        {
            Debug.Assert(RoomId.Value != null);
            return new TimeshiftResultsScreen(score, RoomId.Value.Value, PlaylistItem, true);
        }

        protected override async Task<ScoreInfo> CreateScore()
        {
            var score = await base.CreateScore();
            score.TotalScore = (int)Math.Round(ScoreProcessor.GetStandardisedScore());

            Debug.Assert(Token != null);

            bool completed = false;

            var request = new SubmitRoomScoreRequest(Token.Value, RoomId.Value ?? 0, PlaylistItem.ID, score);

            request.Success += s =>
            {
                score.OnlineScoreID = s.ID;
                completed = true;
            };

            request.Failure += e =>
            {
                Logger.Error(e, "Failed to submit score");
                completed = true;
            };

            api.Queue(request);

            while (!completed)
                await Task.Delay(100);

            return score;
        }

        protected override void Dispose(bool isDisposing)
        {
            base.Dispose(isDisposing);

            Exited = null;
        }
    }
}
