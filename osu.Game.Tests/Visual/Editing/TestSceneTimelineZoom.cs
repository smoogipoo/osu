// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using NUnit.Framework;
using osu.Framework.Graphics;

namespace osu.Game.Tests.Visual.Editing
{
    [Ignore("Timeline initialisation is kinda broken.")] // Initial work to rectify this was done in https://github.com/ppy/osu/pull/19297, but needs more massaging to work.
    public class TestSceneTimelineZoom : TimelineTestScene
    {
        public override Drawable CreateTestComponent() => Empty();

        [Test]
        [FlakyTest]
        /*
         * Fail rate around 0.3%
         *
         * TearDown : osu.Framework.Testing.Drawables.Steps.AssertButton+TracedException : range halved
         *   --TearDown
         *      at osu.Framework.Threading.ScheduledDelegate.RunTaskInternal()
         *      at osu.Framework.Threading.Scheduler.Update()
         *      at osu.Framework.Graphics.Drawable.UpdateSubTree()
         */
        public void TestVisibleRangeUpdatesOnZoomChange()
        {
            double initialVisibleRange = 0;

            AddStep("reset zoom", () => TimelineArea.Timeline.Zoom = 100);
            AddStep("get initial range", () => initialVisibleRange = TimelineArea.Timeline.VisibleRange);

            AddStep("scale zoom", () => TimelineArea.Timeline.Zoom = 200);
            AddStep("range halved", () => Assert.That(TimelineArea.Timeline.VisibleRange, Is.EqualTo(initialVisibleRange / 2).Within(1)));
            AddStep("descale zoom", () => TimelineArea.Timeline.Zoom = 50);
            AddStep("range doubled", () => Assert.That(TimelineArea.Timeline.VisibleRange, Is.EqualTo(initialVisibleRange * 2).Within(1)));

            AddStep("restore zoom", () => TimelineArea.Timeline.Zoom = 100);
            AddStep("range restored", () => Assert.That(TimelineArea.Timeline.VisibleRange, Is.EqualTo(initialVisibleRange).Within(1)));
        }

        [Test]
        public void TestVisibleRangeConstantOnSizeChange()
        {
            double initialVisibleRange = 0;

            AddStep("reset timeline size", () => TimelineArea.Timeline.Width = 1);
            AddStep("get initial range", () => initialVisibleRange = TimelineArea.Timeline.VisibleRange);

            AddStep("scale timeline size", () => TimelineArea.Timeline.Width = 2);
            AddAssert("same range", () => TimelineArea.Timeline.VisibleRange == initialVisibleRange);
            AddStep("descale timeline size", () => TimelineArea.Timeline.Width = 0.5f);
            AddAssert("same range", () => TimelineArea.Timeline.VisibleRange == initialVisibleRange);

            AddStep("restore timeline size", () => TimelineArea.Timeline.Width = 1);
            AddAssert("same range", () => TimelineArea.Timeline.VisibleRange == initialVisibleRange);
        }
    }
}
