// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Game.Beatmaps;
using osu.Game.Rulesets.Mania.Beatmaps;
using osu.Game.Rulesets.Mania.UI;
using osuTK;

namespace osu.Game.Rulesets.Mania.Tests.Skinning
{
    public partial class TestSceneStage : ManiaSkinnableTestScene
    {
        private int columnCount;
        private bool scratchColumn;

        [BackgroundDependencyLoader]
        private void load()
        {
            Ruleset.Value = new ManiaRuleset().RulesetInfo;
        }

        [Test]
        public void Test4K()
        {
            createStage(4, false);
        }

        [Test]
        public void Test5K()
        {
            createStage(5, false);
        }

        [Test]
        public void Test6K()
        {
            createStage(6, false);
        }

        [Test]
        public void Test6KWithScratch()
        {
            createStage(6, true);
        }

        [Test]
        public void Test7K()
        {
            createStage(7, false);
        }

        [Test]
        public void Test8K()
        {
            createStage(8, false);
        }

        [Test]
        public void Test8KWithScratch()
        {
            createStage(8, true);
        }

        private void createStage(int columnCount, bool scratchColumn) => AddStep("create stage", () =>
        {
            this.columnCount = columnCount;
            this.scratchColumn = scratchColumn;

            SetContents(_ =>
            {
                ManiaAction normalAction = ManiaAction.Key1;
                ManiaAction specialAction = ManiaAction.Special1;

                return new ManiaInputManager(new ManiaRuleset().RulesetInfo, columnCount)
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    Scale = new Vector2(Math.Min(1, 5f / columnCount)),
                    Child = new Stage(0, new StageDefinition(columnCount), ref normalAction, ref specialAction)
                };
            });
        });

        protected override IBeatmap CreateBeatmap(RulesetInfo ruleset)
        {
            var beatmap = base.CreateBeatmap(ruleset);
            beatmap.BeatmapInfo.Difficulty.CircleSize = columnCount;
            beatmap.BeatmapInfo.SpecialStyle = scratchColumn;
            return beatmap;
        }
    }
}
