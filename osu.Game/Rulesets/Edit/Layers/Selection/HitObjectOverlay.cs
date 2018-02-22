// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.Graphics.Containers;
using osu.Framework.Input;
using osu.Game.Rulesets.Edit.Types;
using osu.Game.Rulesets.Objects.Drawables;

namespace osu.Game.Rulesets.Edit.Layers.Selection
{
    public class HitObjectOverlay : OverlayContainer
    {
        private readonly DrawableHitObject hitObject;

        public HitObjectOverlay(DrawableHitObject hitObject)
        {
            this.hitObject = hitObject;

            State = Visibility.Visible;
        }

        protected override bool OnDragStart(InputState state)
        {
            return true;
        }

        protected override bool OnDrag(InputState state)
        {
            var hasPosition = hitObject.HitObject as IHasMutablePosition;
            if (hasPosition != null)
                hasPosition.Position += state.Mouse.Delta;
            return true;
        }

        protected override bool OnDragEnd(InputState state) => true;

        protected override void PopIn() => Alpha = 1;
        protected override void PopOut() => Alpha = 0;
    }
}
