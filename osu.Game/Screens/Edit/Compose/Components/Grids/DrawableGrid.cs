// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Caching;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;

namespace osu.Game.Screens.Edit.Compose.Components.Grids
{
    public abstract class DrawableGrid : CompositeDrawable
    {
        protected readonly IGrid Grid;

        protected DrawableGrid(IGrid grid)
        {
            Grid = grid;
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            Grid.Spacing.BindValueChanged(_ => gridCache.Invalidate());
        }

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
    }
}
