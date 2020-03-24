// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Rulesets.Mania.UI;
using osu.Game.Rulesets.UI.Scrolling;
using osu.Game.Tests.Visual;

namespace osu.Game.Rulesets.Mania.Tests
{
    public class ColumnTestContainer : Container
    {
        protected override Container<Drawable> Content => content;

        private readonly Container content;

        [Cached]
        private readonly Column column;

        public ColumnTestContainer(int column, ManiaAction action)
        {
            this.column = new Column(column) { Action = { Value = action } };

            InternalChild = new ScrollingTestContainer(ScrollingDirection.Down)
            {
                RelativeSizeAxes = Axes.Both,
                Child = content = new ManiaInputManager(new ManiaRuleset().RulesetInfo, 4)
                {
                    RelativeSizeAxes = Axes.Both
                }
            };
        }
    }
}
