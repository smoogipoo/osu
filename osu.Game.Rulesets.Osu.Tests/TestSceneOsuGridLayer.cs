// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Beatmaps;
using osu.Game.Beatmaps.ControlPoints;
using osu.Game.Rulesets.Osu.Edit;
using osu.Game.Rulesets.Osu.Edit.Grids;
using osu.Game.Rulesets.Osu.Objects;
using osu.Game.Rulesets.Osu.Objects.Drawables;
using osu.Game.Screens.Edit;
using osu.Game.Screens.Edit.Compose.Components.Grids;
using osu.Game.Tests.Visual;
using osuTK;

namespace osu.Game.Rulesets.Osu.Tests
{
    public class TestSceneOsuGridLayer : EditorClockTestScene
    {
        public override IReadOnlyList<Type> RequiredTypes => new[]
        {
            typeof(GridLayer),
            typeof(DrawableCircularGrid),
            typeof(DrawableGrid),
            typeof(DrawableCircularOsuGrid)
        };

        [Cached(typeof(IEditorBeatmap))]
        [Cached(typeof(IEditorBeatmap<OsuHitObject>))]
        private readonly EditorBeatmap<OsuHitObject> beatmap;

        private readonly GridLayer gridLayer;

        public TestSceneOsuGridLayer()
        {
            beatmap = new EditorBeatmap<OsuHitObject>(new Beatmap<OsuHitObject>());

            for (int i = 0; i < 100; i++)
            {
                var circle = new HitCircle
                {
                    StartTime = -10000 + 300 * i,
                    Position = new Vector2(100 + (10 * i) % 500),
                    Scale = 0.5f
                };

                circle.ApplyDefaults(new ControlPointInfo(), new BeatmapDifficulty());

                beatmap.Add(circle);
            }

            Add(new Container
            {
                RelativeSizeAxes = Axes.Both,
                Clock = Clock,
                ChildrenEnumerable = beatmap.HitObjects.OfType<HitCircle>().Select(h => new DrawableHitCircle(h))
            });

            Add(gridLayer = new OsuGridLayer());

            AddStep("increase beat divisor", () => BeatDivisor.Next());
            AddStep("decrease beat divisor", () => BeatDivisor.Previous());
        }

        protected override void Update()
        {
            base.Update();

            gridLayer.ShowFor(null);
        }
    }
}
