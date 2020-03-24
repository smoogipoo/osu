// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Game.Rulesets.UI.Scrolling;
using osu.Game.Rulesets.UI.Scrolling.Algorithms;
using osu.Game.Tests.Visual;

namespace osu.Game.Rulesets.Mania.Tests
{
    public abstract class ManiaSkinnableTestScene : SkinnableTestScene
    {
        [Cached(Type = typeof(IScrollingInfo))]
        private readonly TestScrollingInfo scrollingInfo = new TestScrollingInfo();

        protected ManiaSkinnableTestScene()
        {
            scrollingInfo.Direction.Value = ScrollingDirection.Down;
        }

        [Test]
        public void TestScrollingDown()
        {
            AddStep("change direction to down", () => scrollingInfo.Direction.Value = ScrollingDirection.Down);
        }

        [Test]
        public void TestScrollingUp()
        {
            AddStep("change direction to up", () => scrollingInfo.Direction.Value = ScrollingDirection.Up);
        }

        private class TestScrollingInfo : IScrollingInfo
        {
            public readonly Bindable<ScrollingDirection> Direction = new Bindable<ScrollingDirection>();

            IBindable<ScrollingDirection> IScrollingInfo.Direction => Direction;
            IBindable<double> IScrollingInfo.TimeRange => throw new NotImplementedException();
            IScrollAlgorithm IScrollingInfo.Algorithm => throw new NotImplementedException();
        }
    }
}
