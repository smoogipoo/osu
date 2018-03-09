// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Game.Rulesets.Edit.Layers.HitObjectSelection;
using osu.Game.Rulesets.Objects.Drawables;
using osu.Game.Rulesets.Osu.Edit.Layers.HitObjectSelection.Overlays;
using osu.Game.Rulesets.Osu.Objects.Drawables;

namespace osu.Game.Rulesets.Osu.Edit.Layers.HitObjectSelection
{
    public class OsuHitObjectSelectionLayer : HitObjectSelectionLayer
    {
        protected override HitObjectOverlay CreateOverlayFor(DrawableHitObject hitObject)
        {
            switch (hitObject)
            {
                case DrawableHitCircle circle:
                    return new HitCircleOverlay(circle);
                case DrawableSlider slider:
                    return new SliderOverlay(slider);
            }

            return base.CreateOverlayFor(hitObject);
        }
    }
}
