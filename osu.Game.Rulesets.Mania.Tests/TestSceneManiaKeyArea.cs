// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Rulesets.Mania.Skinning;
using osu.Game.Rulesets.Mania.UI;
using osu.Game.Rulesets.Mania.UI.Components;
using osu.Game.Rulesets.UI.Scrolling;
using osu.Game.Tests.Visual;

namespace osu.Game.Rulesets.Mania.Tests
{
    public class TestSceneManiaKeyArea : SkinnableTestScene
    {
        public override IReadOnlyList<Type> RequiredTypes => new[]
        {
            typeof(ColumnKeyArea),
            typeof(DefaultKeyArea),
            typeof(LegacyKeyArea)
        };

        [BackgroundDependencyLoader]
        private void load()
        {
            SetContents(() => new FillFlowContainer
            {
                RelativeSizeAxes = Axes.Both,
                Direction = FillDirection.Horizontal,
                Children = new Drawable[]
                {
                    new ColumnTestContainer(0, ManiaAction.Key1)
                    {
                        RelativeSizeAxes = Axes.Both,
                        Width = 0.5f,
                        Child = new ColumnKeyArea { RelativeSizeAxes = Axes.Both }
                    },
                    new ColumnTestContainer(1, ManiaAction.Key2)
                    {
                        RelativeSizeAxes = Axes.Both,
                        Width = 0.5f,
                        Child = new ColumnKeyArea { RelativeSizeAxes = Axes.Both }
                    },
                }
            });
        }

        private class ColumnTestContainer : Container
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
}
