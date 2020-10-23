// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Game.Rulesets.Objects.Drawables;
using osu.Game.Rulesets.Objects.Types;

namespace osu.Game.Rulesets.Osu.Objects.Drawables
{
    public class DrawableSliderHead : DrawableHitCircle
    {
        private readonly IBindable<int> pathVersion = new Bindable<int>();

        protected override OsuSkinComponents CirclePieceComponent => OsuSkinComponents.SliderHeadHitCircle;

        private DrawableSlider drawableSlider;

        public DrawableSliderHead()
        {
        }

        public DrawableSliderHead(SliderHeadCircle h)
            : base(h)
        {
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            PositionBindable.BindValueChanged(_ => updatePosition());
            pathVersion.BindValueChanged(_ => updatePosition());
        }

        public override void ApplyParent(DrawableHitObject parent)
        {
            if (drawableSlider != null)
                pathVersion.UnbindFrom(drawableSlider.PathVersion);

            base.ApplyParent(parent);

            drawableSlider = (DrawableSlider)parent;

            pathVersion.BindTo(drawableSlider.PathVersion);

            OnShake = drawableSlider.Shake;
            CheckHittable = (d, t) => drawableSlider.CheckHittable?.Invoke(d, t) ?? true;
        }

        protected override void Update()
        {
            base.Update();

            double completionProgress = Math.Clamp((Time.Current - drawableSlider.HitObject.StartTime) / drawableSlider.HitObject.Duration, 0, 1);

            //todo: we probably want to reconsider this before adding scoring, but it looks and feels nice.
            if (!IsHit)
                Position = drawableSlider.HitObject.CurvePositionAt(completionProgress);
        }

        public Action<double> OnShake;

        protected override void Shake(double maximumLength) => OnShake?.Invoke(maximumLength);

        private void updatePosition()
        {
            if (drawableSlider != null)
                Position = HitObject.Position - drawableSlider.HitObject.Position;
        }
    }
}
