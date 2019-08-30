// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using System.Linq;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Rulesets.Objects;
using osuTK;

namespace osu.Game.Screens.Edit.Compose.Components.Grids
{
    public class GridLayer : CompositeDrawable
    {
        private readonly List<DrawableGrid> activeGrids = new List<DrawableGrid>();

        public GridLayer()
        {
            RelativeSizeAxes = Axes.Both;
            Masking = true;
        }

        public void ShowFor(IEnumerable<HitObject> hitObjects)
        {
            activeGrids.Clear();
            ClearInternal();

            activeGrids.AddRange(CreateGrids(hitObjects));

            foreach (var g in activeGrids)
                AddInternal(g);
        }

        protected virtual IEnumerable<DrawableGrid> CreateGrids(IEnumerable<HitObject> hitObjects) => Enumerable.Empty<DrawableGrid>();

        public Vector2 GetSnappedPosition(Vector2 screenSpacePosition)
        {
            if (activeGrids.Count == 0)
                return screenSpacePosition;

            Vector2 localSnappedPosition = ToLocalSpace(screenSpacePosition);

            foreach (var g in activeGrids)
                localSnappedPosition = g.GetSnappedPosition(localSnappedPosition);

            return ToScreenSpace(localSnappedPosition);
        }
    }
}
