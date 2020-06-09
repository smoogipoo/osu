// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;
using osu.Game.Beatmaps;
using osu.Game.Rulesets;
using osu.Game.Rulesets.Osu;
using osu.Game.Rulesets.Osu.Objects;
using osu.Game.Scoring;
using osu.Game.Screens.Play;
using osu.Game.Screens.Ranking;
using osu.Game.Tests.Beatmaps;
using osuTK;

namespace osu.Game.Tests.Visual.Gameplay
{
    public class TestScenePlayerScoreSubmission : PlayerTestScene
    {
        protected new DelayedSubmissionPlayer Player => (DelayedSubmissionPlayer)base.Player;

        public TestScenePlayerScoreSubmission()
            : base(new OsuRuleset())
        {
        }

        protected override IBeatmap CreateBeatmap(RulesetInfo ruleset)
        {
            var beatmap = new TestBeatmap(Ruleset.Value);

            beatmap.HitObjects.Clear();
            beatmap.HitObjects.Add(new HitCircle { Position = new Vector2(256, 192) });

            return beatmap;
        }

        /// <summary>
        /// Tests that the score is submitted immediately upon completion.
        /// </summary>
        [Test]
        public void TestUserScoreSubmitsOnCompletion()
        {
            AddUntilStep("wait for completion", () => Player.ScoreProcessor.HasCompleted.Value);

            AddAssert("submission triggered", () => Player.Submitting);

            AddStep("complete submission", () => Player.CompleteSubmission());
        }

        /// <summary>
        /// Tests that the results screen isn't shown until the submission completes.
        /// </summary>
        [Test]
        public void TestResultsScreenNotShownUntilScoreSubmissionFinished()
        {
            AddUntilStep("wait for completion", () => Player.ScoreProcessor.HasCompleted.Value);

            AddWaitStep("wait for results screen to potentially show", 10);
            AddAssert("results screen not shown", () => Stack.CurrentScreen is Player);

            AddStep("complete submission", () => Player.CompleteSubmission());
            AddUntilStep("results screen shown", () => Stack.CurrentScreen is ResultsScreen);
        }

        protected override TestPlayer CreatePlayer(Ruleset ruleset) => new DelayedSubmissionPlayer();

        protected class DelayedSubmissionPlayer : TestPlayer
        {
            public bool Submitting { get; private set; }

            private readonly SemaphoreSlim submissionSemaphore = new SemaphoreSlim(0, 1);

            public DelayedSubmissionPlayer()
                : base(false)
            {
            }

            /// <summary>
            /// Allows submission to complete.
            /// </summary>
            public void CompleteSubmission() => submissionSemaphore.Release();

            protected override async Task SubmitScoreAsync(Score score)
            {
                Submitting = true;

                await submissionSemaphore.WaitAsync(new CancellationTokenSource(10000).Token);
                await base.SubmitScoreAsync(score);
            }
        }
    }
}
