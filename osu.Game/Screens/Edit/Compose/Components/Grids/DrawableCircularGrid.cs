// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Graphics;
using osu.Framework.Graphics.UserInterface;
using osu.Game.Rulesets.Objects;
using osuTK;

namespace osu.Game.Screens.Edit.Compose.Components.Grids
{
    public abstract class DrawableCircularGrid : DrawableGrid
    {
        private readonly Vector2 centre;
        private readonly float centreRadius;

        /// <summary>
        /// Creates a new <see cref="DrawableCircularGrid"/>.
        /// </summary>
        /// <param name="hitObject"></param>
        /// <param name="centre">The centre point of the grid.</param>
        /// <param name="centreRadius">The radius around <see cref="centre"/> for which the grid should remain empty.</param>
        protected DrawableCircularGrid(HitObject hitObject, Vector2 centre, float centreRadius = 50)
            : base(hitObject, centre)
        {
            this.centre = centre;
            this.centreRadius = centreRadius;

            RelativeSizeAxes = Axes.Both;
        }

        protected override void DrawGrid()
        {
            float maxDistance = Math.Max(
                Vector2.Distance(centre, Vector2.Zero),
                Math.Max(
                    Vector2.Distance(centre, new Vector2(DrawWidth, 0)),
                    Math.Max(
                        Vector2.Distance(centre, new Vector2(0, DrawHeight)),
                        Vector2.Distance(centre, DrawSize))));

            int requiredCircles = (int)((maxDistance - centreRadius) / DistanceSpacing);

            for (int i = 0; i < requiredCircles; i++)
            {
                float radius = centreRadius * 2 + (i + 1) * DistanceSpacing * 2;

                AddInternal(new CircularProgress
                {
                    Origin = Anchor.Centre,
                    Position = centre,
                    Current = { Value = 1 },
                    Size = new Vector2(radius),
                    InnerRadius = 4 * 1f / radius,
                    Colour = GetColourForBeatIndex(i),
                });
            }
        }

        public override Vector2 GetSnapPosition(Vector2 position)
        {
            Vector2 direction = position - centre;
            float distance = direction.Length;

            float radius = DistanceSpacing;
            int radialCount = (int)Math.Round((distance - centreRadius) / radius);

            if (radialCount <= 0)
                return position;

            Vector2 normalisedDirection = direction * new Vector2(1f / distance);

            return centre +
                   normalisedDirection * centreRadius +
                   normalisedDirection * radialCount * radius;
        }
    }
}
