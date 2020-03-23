// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Game.Rulesets.Mania.UI;
using osu.Game.Rulesets.Mania.UI.Components;
using osu.Game.Rulesets.UI.Scrolling;
using osu.Game.Tests.Visual;

namespace osu.Game.Rulesets.Mania.Tests
{
    public class TestSceneManiaKeyArea : SkinnableTestScene
    {
        [Cached]
        private readonly Column column = new Column(0) { Action = { Value = ManiaAction.Key1 } };

        [BackgroundDependencyLoader]
        private void load()
        {
            SetContents(() => new ScrollingTestContainer(ScrollingDirection.Down)
            {
                RelativeSizeAxes = Axes.Both,
                Child = new ManiaInputManager(new ManiaRuleset().RulesetInfo, 4)
                {
                    RelativeSizeAxes = Axes.Both,
                    Child = new ColumnKeyArea { RelativeSizeAxes = Axes.Both }
                }
            });
        }
    }
}
