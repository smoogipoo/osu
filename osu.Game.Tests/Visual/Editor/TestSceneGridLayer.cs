// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Extensions.IEnumerableExtensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Input.Events;
using osu.Game.Beatmaps;
using osu.Game.Beatmaps.ControlPoints;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Osu.Beatmaps;
using osu.Game.Rulesets.Osu.Objects;
using osu.Game.Screens.Edit;
using osu.Game.Screens.Edit.Compose.Components.Grids;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Tests.Visual.Editor
{
    public class TestSceneGridLayer : EditorClockTestScene
    {
        public override IReadOnlyList<Type> RequiredTypes => new[]
        {
            typeof(DrawableGrid),
            typeof(DrawableCircularGrid)
        };

        [Cached(typeof(IEditorBeatmap))]
        [Cached(typeof(IEditorBeatmap<OsuHitObject>))]
        private readonly EditorBeatmap<OsuHitObject> beatmap = new EditorBeatmap<OsuHitObject>(new OsuBeatmap());

        private GridLayer gridLayer;
        private Drawable snapMarker;

        [SetUp]
        public void Setup() => Schedule(() =>
        {
            Child = new Container
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                RelativeSizeAxes = Axes.Both,
                Size = new Vector2(0.75f),
                Masking = true,
                Children = new[]
                {
                    new Box
                    {
                        RelativeSizeAxes = Axes.Both,
                        Alpha = 0.2f
                    },
                    gridLayer = new TestGridLayer(),
                    snapMarker = new CircularContainer
                    {
                        Origin = Anchor.Centre,
                        Size = new Vector2(10),
                        Masking = true,
                        Child = new Box
                        {
                            Colour = Color4.Red,
                            RelativeSizeAxes = Axes.Both,
                        },
                    }
                },
            };

            gridLayer.ShowFor(null);
        });

        [TestCase(1)]
        [TestCase(2)]
        [TestCase(3)]
        [TestCase(4)]
        [TestCase(6)]
        [TestCase(8)]
        [TestCase(12)]
        [TestCase(16)]
        public void TestBeatDivisor(int divisor)
        {
            AddStep("set beat divisor", () => BeatDivisor.Value = divisor);
        }

        protected override bool OnMouseMove(MouseMoveEvent e)
        {
            if (gridLayer == null)
                return false;

            snapMarker.Position = gridLayer.ToLocalSpace(gridLayer.GetSnappedPosition(e.ScreenSpaceMousePosition));

            return true;
        }

        private class TestGridLayer : GridLayer
        {
            protected override IEnumerable<DrawableGrid> CreateGrids(IEnumerable<HitObject> hitObjects) => new TestDrawableCircularGrid().Yield();
        }

        private class TestDrawableCircularGrid : DrawableCircularGrid
        {
            public TestDrawableCircularGrid()
                : base(new HitObject(), new Vector2(256), 25)
            {
            }

            protected override float GetDistanceSpacing(double time, ControlPointInfo controlPointInfo, BeatmapDifficulty difficulty) => 0.05f;
        }
    }
}
