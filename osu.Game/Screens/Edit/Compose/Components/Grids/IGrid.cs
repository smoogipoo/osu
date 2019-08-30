// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Bindables;
using osuTK;

namespace osu.Game.Screens.Edit.Compose.Components.Grids
{
    public interface IGrid
    {
        /// <summary>
        /// The spacing between lines of the grid.
        /// </summary>
        Bindable<float> Spacing { get; }

        /// <summary>
        /// The maximum distance from the lines of the grid at which snapping can occur.
        /// </summary>
        Bindable<float> SnapDistance { get; }

        /// <summary>
        /// Creates the visual representation for this grid.
        /// </summary>
        /// <returns>The visual representation.</returns>
        DrawableGrid CreateVisualRepresentation();

        /// <summary>
        /// Snaps a position to this grid.
        /// </summary>
        /// <param name="position"></param>
        /// <returns>The snapped position.</returns>
        Vector2 GetSnappedPosition(Vector2 position);
    }
}
