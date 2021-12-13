// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using NUnit.Framework;
using osu.Framework.Testing;
using osu.Game.Rulesets.Mania.UI;
using osu.Game.Tests.Visual;
using osuTK;

namespace osu.Game.Rulesets.Mania.Tests
{
    public class TestSceneManiaPlayer : PlayerTestScene
    {
        protected override Ruleset CreatePlayerRuleset() => new ManiaRuleset();

        [Test]
        public void TestPerspective()
        {
            AddSliderStep("Perspective-Y", -4f, 4f, 0, y =>
            {
                var playfield = this.ChildrenOfType<ManiaPlayfield>().SingleOrDefault();
                if (playfield != null)
                    playfield.Perspective = new Vector2(playfield.Perspective.X, y / 100);
            });
            AddSliderStep("Scale", 0f, 1f, 1, scale =>
            {
                var playfield = this.ChildrenOfType<ManiaPlayfield>().SingleOrDefault();
                if (playfield != null)
                    playfield.Scale = new Vector2(scale);
            });
        }
    }
}
