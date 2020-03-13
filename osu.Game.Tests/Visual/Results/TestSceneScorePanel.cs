// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using NUnit.Framework;
using osu.Framework.Graphics;
using osu.Game.Screens.Results;

namespace osu.Game.Tests.Visual.Results
{
    public class TestSceneScorePanel : OsuTestScene
    {
        public override IReadOnlyList<Type> RequiredTypes => new[]
        {
            typeof(ScorePanel),
            typeof(PanelState),
        };

        private ScorePanel panel;

        [SetUp]
        public void Setup() => Schedule(() =>
        {
            Child = panel = new ScorePanel
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
