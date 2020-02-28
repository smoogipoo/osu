// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Screens;
using osu.Game.Configuration;
using osu.Game.Rulesets;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Osu;
using osu.Game.Screens.Play;

namespace osu.Game.Tests.Visual.Gameplay
{
    public abstract class TestSceneAllRulesetPlayers : RateAdjustedBeatmapTestScene
    {
        protected Player Player;

        [BackgroundDependencyLoader]
        private void load(RulesetStore rulesets)
        {
            OsuConfigManager manager;
            Dependencies.Cache(manager = new OsuConfigManager(LocalStorage));
            manager.GetBindable<double>(OsuSetting.DimLevel).Value = 1.0;
        }

        [Test]
        public void TestOsu() => runForRuleset(new OsuRuleset().RulesetInfo);

        private void runForRuleset(RulesetInfo ruleset)
        {
            AddStep($"load {ruleset.Name} player", () => loadPlayerFor(ruleset));
        }

        protected override void Update()
        {
            base.Update();

            var p = (TestPlayer)Player;

            if (p?.IsLoaded == true)
            {
                if (p.DrawableRuleset.FrameStableClock.TimeInfo.Elapsed > 0)
                {
                    if (p.DrawableRuleset.FrameStableClock.CurrentTime >= 150000)
                        p.GameplayClockContainer.Seek(0);
                    else
                        p.GameplayClockContainer.Seek(200000);
                }
            }
        }

        private Player loadPlayerFor(RulesetInfo rulesetInfo)
        {
            Ruleset.Value = rulesetInfo;
            var ruleset = rulesetInfo.CreateInstance();

            var working = CreateWorkingBeatmap(rulesetInfo);

            Beatmap.Value = working;
            SelectedMods.Value = new[] { ruleset.GetAllMods().First(m => m is ModNoFail) };

            Player?.Exit();
            Player = null;

            Player = CreatePlayer(ruleset);

            LoadScreen(Player);

            return Player;
        }

        protected virtual Player CreatePlayer(Ruleset ruleset) => new TestPlayer(false, false);
    }
}
