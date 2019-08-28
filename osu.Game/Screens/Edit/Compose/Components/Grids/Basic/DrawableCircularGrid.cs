// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Graphics;
using osu.Framework.Graphics.UserInterface;
using osuTK;

namespace osu.Game.Screens.Edit.Compose.Components.Grids.Basic
{
    public class DrawableCircularGrid : DrawableGrid
    {
        private readonly CircularGrid grid;

        public DrawableCircularGrid(CircularGrid grid)
            : base(grid)
        {
            this.grid = grid;
        }

        protected override void CreateGrid()
        {
            float maxDistance = Math.Max(
                Vector2.Distance(grid.Centre, Vector2.Zero),
                Math.Max(
                    Vector2.Distance(grid.Centre, new Vector2(DrawWidth, 0)),
                    Math.Max(
                        Vector2.Distance(grid.Centre, new Vector2(0, DrawHeight)),
                        Vector2.Distance(grid.Centre, DrawSize))));

            int requiredCircles = (int)((maxDistance - grid.CentreRadius) / grid.Spacing.Value);

            for (int i = 0; i < requiredCircles; i++)
            {
                float radius = grid.CentreRadius * 2 + (i + 1) * grid.Spacing.Value * 2;

                AddInternal(new CircularProgress
                {
                    Origin = Anchor.Centre,
                    Position = grid.Centre,
                    Current = { Value = 1 },
                    Size = new Vector2(radius),
                    InnerRadius = 2 * 1f / radius,
                    Alpha = 0.5f
                });
            }
        }
    }
}
