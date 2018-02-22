// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.Allocation;
using osu.Game.Rulesets.Edit.Layers.Selection;
using osu.Game.Rulesets.Osu.Objects.Drawables;

namespace osu.Game.Rulesets.Osu.Edit.Layers.Selection.Overlays
{
    public class OsuHitObjectOverlay : HitObjectOverlay
    {
        private readonly DrawableOsuHitObject hitObject;

        public OsuHitObjectOverlay(DrawableOsuHitObject hitObject)
            : base(hitObject)
        {
            this.hitObject = hitObject;

            hitObject.HitObject.OnPositionChanged += p => UpdatePosition();
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            UpdatePosition();
        }

        protected virtual void UpdatePosition() => Position = hitObject.HitObject.StackedPosition;
    }
}
