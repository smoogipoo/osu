// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osuTK;

namespace osu.Game.Screens.Edit.Compose.Components.Grids
{
    public class GridLayer : CompositeDrawable
    {
        public IGrid ActiveGrid { get; private set; }

        public GridLayer()
        {
            RelativeSizeAxes = Axes.Both;
        }

        public void ShowGrid(IGrid grid)
        {
            ActiveGrid = grid;
            InternalChild = grid.CreateVisualRepresentation().With(d => d.RelativeSizeAxes = Axes.Both);
        }

        public Vector2 GetSnappedPosition(Vector2 screenSpacePosition)
        {
            if (ActiveGrid == null)
                return screenSpacePosition;

            return ToScreenSpace(ActiveGrid.GetSnappedPosition(ToLocalSpace(screenSpacePosition)));
        }
    }
}
