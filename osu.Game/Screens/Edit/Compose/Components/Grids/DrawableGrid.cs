// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Bindables;
using osu.Framework.Caching;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osuTK;

namespace osu.Game.Screens.Edit.Compose.Components.Grids
{
    public abstract class DrawableGrid : CompositeDrawable
    {
        public Bindable<float> Spacing { get; } = new Bindable<float>(20);

        public Bindable<float> SnapDistance { get; } = new Bindable<float>(20);

        public override bool Invalidate(Invalidation invalidation = Invalidation.All, Drawable source = null, bool shallPropagate = true)
        {
            if ((invalidation & Invalidation.RequiredParentSizeToFit) > 0)
                gridCache.Invalidate();

            return base.Invalidate(invalidation, source, shallPropagate);
        }

        private readonly Cached gridCache = new Cached();

        protected override void Update()
        {
            base.Update();

            if (!gridCache.IsValid)
            {
                ClearInternal();
                CreateGrid();
                gridCache.Validate();
            }
        }

        protected abstract void CreateGrid();

        /// <summary>
        /// Snaps a position to this grid.
        /// </summary>
        /// <param name="screenSpacePosition">The original screen-space position.</param>
        /// <returns>The snapped position.</returns>
        public abstract Vector2 GetSnappedPosition(Vector2 screenSpacePosition);
    }
}
