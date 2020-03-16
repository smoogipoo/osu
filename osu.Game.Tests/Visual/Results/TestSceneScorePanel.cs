// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using NUnit.Framework;
using osu.Framework.Graphics;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Osu;
using osu.Game.Rulesets.Osu.Mods;
using osu.Game.Rulesets.Scoring;
using osu.Game.Scoring;
using osu.Game.Screens.Results;
using osu.Game.Tests.Beatmaps;
using osu.Game.Users;

namespace osu.Game.Tests.Visual.Results
{
    public class TestSceneScorePanel : OsuTestScene
    {
        public override IReadOnlyList<Type> RequiredTypes => new[]
        {
            typeof(ScorePanel),
            typeof(PanelState),
            typeof(ExpandedPanelMiddleContent),
            typeof(ExpandedPanelTopContent),
            typeof(AccuracyCircle),
            typeof(AccuracyCircleBadge)
        };

        private ScorePanel panel;

        [SetUp]
        public void Setup() => Schedule(() =>
        {
            Child = panel = new ScorePanel(new ScoreInfo
            {
                User = new User
                {
                    Id = 2,
                    Username = "peppy",
                },
                Beatmap = new TestBeatmap(new OsuRuleset().RulesetInfo).BeatmapInfo,
                Mods = new Mod[] { new OsuModHardRock(), new OsuModDoubleTime() },
                TotalScore = 2845370,
                Accuracy = 1,
                MaxCombo = 999,
                Rank = ScoreRank.S,
                Date = DateTimeOffset.Now,
                Statistics =
                {
                    { HitResult.Miss, 1 },
                    { HitResult.Meh, 50 },
                    { HitResult.Good, 100 },
                    { HitResult.Great, 300 },
                }
            })
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                State = PanelState.Expanded
            };
        });

        [Test]
        public void TestExpanded()
        {
            AddStep("set expanded state", () => panel.State = PanelState.Expanded);
        }

        [Test]
        public void TestContracted()
        {
            AddStep("set expanded state", () => panel.State = PanelState.Contracted);
        }
    }
}
