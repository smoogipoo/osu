// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.ComponentModel;
using System.Linq;
using osu.Game.Rulesets;
using osu.Game.Rulesets.Scoring;
using osu.Game.Screens.Play;

namespace osu.Game.Tests.Visual.Gameplay
{
    [Description("Player instantiated with an autoplay mod.")]
    public class TestSceneAutoplay : TestSceneAllRulesetPlayers
    {
        protected override void LoadComplete()
        {
            base.LoadComplete();
            RunAllSteps();
        }

        protected override Player CreatePlayer(Ruleset ruleset)
        {
            SelectedMods.Value = SelectedMods.Value.Concat(new[] { ruleset.GetAutoplayMod() }).ToArray();
            return new ScoreAccessiblePlayer();
        }

        private class ScoreAccessiblePlayer : TestPlayer
        {
            public new ScoreProcessor ScoreProcessor => base.ScoreProcessor;
            public new HUDOverlay HUDOverlay => base.HUDOverlay;

            public new GameplayClockContainer GameplayClockContainer => base.GameplayClockContainer;

            public ScoreAccessiblePlayer()
                : base(false, false)
            {
            }
        }
    }
}
