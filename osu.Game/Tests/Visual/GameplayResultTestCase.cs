// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Extensions.IEnumerableExtensions;
using osu.Framework.Timing;
using osu.Game.Beatmaps;
using osu.Game.Rulesets;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Replays;
using osu.Game.Rulesets.Scoring;
using osu.Game.Rulesets.UI;
using osu.Game.Screens.Play;
using osu.Game.Tests.Beatmaps;

namespace osu.Game.Tests.Visual
{
    public abstract class GameplayResultTestCase : ScreenTestCase
    {
        private TestPlayer player;

        private readonly Ruleset ruleset;

        protected GameplayResultTestCase(Ruleset ruleset)
        {
            this.ruleset = ruleset;
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            Ruleset.Value = ruleset.RulesetInfo;

            AddStep("load beatmap", loadBeatmap);

            AddStep("load passing test", () => loadPlayer(CreatePassingReplay(Beatmap.Value.GetPlayableBeatmap(ruleset.RulesetInfo))));
            AddUntilStep(() => player.IsLoaded, "player loaded");
            restartGameplay();
            restartGameplay();

            AddStep("load failing test", () => loadPlayer(CreateFailingReplay(Beatmap.Value.GetPlayableBeatmap(ruleset.RulesetInfo))));
            AddUntilStep(() => player.IsLoaded, "player loaded");
            restartGameplay();
            restartGameplay();
        }

        private void loadBeatmap()
        {
            var beatmap = CreateBeatmap(ruleset);
            var working = new TestWorkingBeatmap(beatmap);

            Beatmap.Value = working;
            Beatmap.Value.Mods.Value = ruleset.GetAllMods().First(m => m is ModNoFail).Yield();
        }

        private void restartGameplay()
        {
            double? lastTime = null;

            AddStep("seek to start", () =>
            {
                player.AdjustableClock.Stop();
                player.AdjustableClock.Seek(0);
            });

            AddUntilStep(() => player.RulesetContainer.Playfield.Clock.CurrentTime == 0, "wait for completion");
            AddAssert("judgements revered", () => player.ScoreProcessor.JudgedHits == 0);

            AddStep("seek to end", () => player.AdjustableClock.Seek(Beatmap.Value.Track.Length));

            AddUntilStep(() =>
            {
                double lastFrameTime = player.Replay.Frames[player.Replay.Frames.Count - 1].Time;
                double currentTime = player.RulesetContainer.Playfield.Clock.CurrentTime;

                if (currentTime < lastFrameTime)
                    return false;

                player.AdjustableClock.Stop();

                // Wait until the time becomes invariant
                if (currentTime != lastTime)
                {
                    lastTime = currentTime;
                    return false;
                }

                return true;
            }, "wait for completion");

            AddAssert("has completed", () => player.ScoreProcessor.HasCompleted);
        }

        private void loadPlayer(Replay replay)
        {
            player = new TestPlayer(replay)
            {
                AllowPause = false,
                AllowLeadIn = false,
                AllowResults = false,
            };

            LoadScreen(player);
        }

        protected abstract Replay CreatePassingReplay(IBeatmap beatmap);

        protected abstract Replay CreateFailingReplay(IBeatmap beatmap);

        protected virtual IBeatmap CreateBeatmap(Ruleset ruleset) => new TestBeatmap(ruleset.RulesetInfo);

        private class TestPlayer : ReplayPlayer
        {
            public new ScoreProcessor ScoreProcessor => base.ScoreProcessor;
            public new IAdjustableClock AdjustableClock => base.AdjustableClock;
            public new RulesetContainer RulesetContainer => base.RulesetContainer;

            public TestPlayer(Replay replay)
                : base(replay)
            {
            }
        }
    }
}
