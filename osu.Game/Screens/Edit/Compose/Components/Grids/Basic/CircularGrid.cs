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

            float radius = Spacing.Value / 2f;

            int count = (int)Math.Round(distance / radius);

            return Centre + direction / distance * count * radius;
        }
    }
}
