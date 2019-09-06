// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Extensions.IEnumerableExtensions;
using osu.Framework.Timing;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Osu.Edit.Grids;
using osu.Game.Rulesets.Osu.Objects;
using osu.Game.Screens.Edit;
using osu.Game.Screens.Edit.Compose.Components.Grids;

namespace osu.Game.Rulesets.Osu.Edit
{
    public class OsuGridLayer : GridLayer
    {
        [Resolved]
        private IEditorBeatmap<OsuHitObject> beatmap { get; set; }

        [Resolved]
        private IFrameBasedClock clock { get; set; }

        [Resolved]
        private BindableBeatDivisor beatDivisor { get; set; }

        protected override IEnumerable<DrawableGrid> CreateGrids(IEnumerable<HitObject> hitObjects)
        {
            var objects = hitObjects?.ToList();

            if (objects == null || objects.Count == 0)
            {
                var lastObject = beatmap.HitObjects.LastOrDefault(h => h.StartTime < clock.CurrentTime);

                if (lastObject == null)
                    return Enumerable.Empty<DrawableGrid>();

                return new DrawableCircularOsuGrid(lastObject).Yield();
            }
            else
            {
                double minTime = objects.Min(h => h.StartTime);

                var lastObject = beatmap.HitObjects.LastOrDefault(h => h.StartTime < minTime);

                if (lastObject == null)
                    return Enumerable.Empty<DrawableGrid>();

                return new DrawableCircularOsuGrid(lastObject).Yield();
            }
        }
    }
}
