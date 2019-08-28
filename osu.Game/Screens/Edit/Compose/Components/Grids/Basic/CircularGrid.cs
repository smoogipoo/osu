// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Bindables;
using osuTK;

namespace osu.Game.Screens.Edit.Compose.Components.Grids.Basic
{
    public class CircularGrid : IGrid
    {
        public Bindable<int> Spacing { get; } = new Bindable<int>(20);

        public Bindable<int> SnapDistance { get; } = new Bindable<int>(20);

        /// <summary>
        /// The radius around <see cref="Centre"/> for which the grid should remain empty.
        /// </summary>
        public float CentreRadius = 50;

        /// <summary>
        /// The centre point of the grid.
        /// </summary>
        public readonly Vector2 Centre;

        public CircularGrid(Vector2 centre)
        {
            Centre = centre;
        }

        public DrawableGrid CreateVisualRepresentation() => new DrawableCircularGrid(this);

        public Vector2 GetSnappedPosition(Vector2 position)
        {
            Vector2 direction = position - Centre;
            float distance = direction.Length;

            float radius = Spacing.Value;
            int radialCount = (int)Math.Round((distance - CentreRadius) / radius);

            if (radialCount <= 0)
                return position;

            Vector2 normalisedDirection = direction * new Vector2(1f / distance);

            return Centre +
                   normalisedDirection * CentreRadius +
                   normalisedDirection * radialCount * radius;
        }
    }
}
