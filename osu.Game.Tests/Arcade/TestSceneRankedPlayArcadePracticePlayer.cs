// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Game.Arcade.Screens.RankedPlay;
using osu.Game.Overlays;
using osu.Game.Rulesets.Osu;
using osu.Game.Tests.Visual.RankedPlay;

namespace osu.Game.Tests.Arcade
{
    public class TestSceneRankedPlayArcadePracticePlayer : RankedPlayTestScene
    {
        [Cached]
        private readonly SettingsOverlay settingsOverlay;

        public TestSceneRankedPlayArcadePracticePlayer()
        {
            Add(settingsOverlay = new SettingsOverlay());
        }

        public override void SetUpSteps()
        {
            base.SetUpSteps();

            AddStep("set beatmap", () => Beatmap.Value = CreateWorkingBeatmap(new OsuRuleset().RulesetInfo));

            RankedPlayPracticePlayer screen = null!;
            AddStep("load screen", () => LoadScreen(screen = new RankedPlayPracticePlayer()));
            AddUntilStep("wait for load", () => screen.IsLoaded);
        }
    }
}
