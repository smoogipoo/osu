﻿// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Linq;
using osuTK;
using osu.Framework.Graphics;
using osu.Game.Rulesets.Objects.Drawables;
using osu.Game.Rulesets.Osu.Objects.Drawables.Pieces;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics.Containers;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Osu.Skinning;
using osu.Game.Rulesets.Osu.UI;
using osu.Game.Rulesets.UI;
using osuTK.Graphics;
using osu.Game.Skinning;

namespace osu.Game.Rulesets.Osu.Objects.Drawables
{
    public class DrawableSlider : DrawableOsuHitObject, IDrawableHitObjectWithProxiedApproach
    {
        public DrawableSliderHead HeadCircle => headContainer.Child;
        public DrawableSliderTail TailCircle => tailContainer.Child;

        public new Slider HitObject => (Slider)base.HitObject;

        public event Action<DrawableSlider> AccentChanged;

        public IBindable<int> PathVersion => pathVersion;
        private readonly Bindable<int> pathVersion = new Bindable<int>();

        public SliderBall Ball { get; private set; }
        public SkinnableDrawable Body { get; private set; }

        public override bool DisplayResult => false;

        private PlaySliderBody sliderBody => Body.Drawable as PlaySliderBody;

        private Container<DrawableSliderHead> headContainer;
        private Container<DrawableSliderTail> tailContainer;
        private Container<DrawableSliderTick> tickContainer;
        private Container<DrawableSliderRepeat> repeatContainer;
        private Container sampleContainer;

        public DrawableSlider()
        {
        }

        public DrawableSlider(Slider s)
            : base(s)
        {
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            InternalChildren = new Drawable[]
            {
                Body = new SkinnableDrawable(new OsuSkinComponent(OsuSkinComponents.SliderBody), _ => new DefaultSliderBody(), confineMode: ConfineMode.NoScaling),
                tailContainer = new Container<DrawableSliderTail> { RelativeSizeAxes = Axes.Both },
                tickContainer = new Container<DrawableSliderTick> { RelativeSizeAxes = Axes.Both },
                repeatContainer = new Container<DrawableSliderRepeat> { RelativeSizeAxes = Axes.Both },
                Ball = new SliderBall(this)
                {
                    GetInitialHitAction = () => HeadCircle.HitAction,
                    BypassAutoSizeAxes = Axes.Both,
                    AlwaysPresent = true,
                    Alpha = 0
                },
                headContainer = new Container<DrawableSliderHead> { RelativeSizeAxes = Axes.Both },
                sampleContainer = new Container { RelativeSizeAxes = Axes.Both }
            };

            PositionBindable.BindValueChanged(_ => Position = HitObject.StackedPosition);
            StackHeightBindable.BindValueChanged(_ => Position = HitObject.StackedPosition);
            ScaleBindable.BindValueChanged(scale => Ball.Scale = new Vector2(scale.NewValue));
            AccentColour.BindValueChanged(colour =>
            {
                foreach (var drawableHitObject in NestedHitObjects)
                    drawableHitObject.AccentColour.Value = colour.NewValue;

                AccentChanged?.Invoke(this);
            }, true);

            Tracking.BindValueChanged(updateSlidingSample);
        }

        protected override void FreeAfterUse()
        {
            PathVersion.UnbindFrom(HitObject.Path.Version);

            base.FreeAfterUse();
        }

        public override void Apply(HitObject hitObject, HitObjectLifetimeEntry lifetime)
        {
            base.Apply(hitObject, lifetime);

            // Ensure that the version will change after the upcoming BindTo().
            pathVersion.Value = int.MinValue;
            PathVersion.BindTo(HitObject.Path.Version);
        }

        private PausableSkinnableSound slidingSample;

        protected override void LoadSamples()
        {
            base.LoadSamples();

            sampleContainer.Clear();
            slidingSample = null;

            var firstSample = HitObject.Samples.FirstOrDefault();

            if (firstSample != null)
            {
                var clone = HitObject.SampleControlPoint.ApplyTo(firstSample);
                clone.Name = "sliderslide";

                sampleContainer.Add(slidingSample = new PausableSkinnableSound(clone)
                {
                    Looping = true
                });
            }
        }

        public override void StopAllSamples()
        {
            base.StopAllSamples();
            slidingSample?.Stop();
        }

        private void updateSlidingSample(ValueChangedEvent<bool> tracking)
        {
            if (tracking.NewValue)
                slidingSample?.Play();
            else
                slidingSample?.Stop();
        }

        protected override void AddNestedHitObject(DrawableHitObject hitObject)
        {
            base.AddNestedHitObject(hitObject);

            switch (hitObject)
            {
                case DrawableSliderHead head:
                    headContainer.Child = head;
                    break;

                case DrawableSliderTail tail:
                    tailContainer.Child = tail;
                    break;

                case DrawableSliderTick tick:
                    tickContainer.Add(tick);
                    break;

                case DrawableSliderRepeat repeat:
                    repeatContainer.Add(repeat);
                    break;
            }
        }

        protected override void ClearNestedHitObjects()
        {
            base.ClearNestedHitObjects();

            headContainer.Clear(false);
            tailContainer.Clear(false);
            repeatContainer.Clear(false);
            tickContainer.Clear(false);
        }

        protected override void UpdateInitialTransforms()
        {
            base.UpdateInitialTransforms();

            Body.FadeInFromZero(HitObject.TimeFadeIn);
        }

        public new void Shake(double maximumLength) => base.Shake(maximumLength);

        public readonly Bindable<bool> Tracking = new Bindable<bool>();

        protected override void Update()
        {
            base.Update();

            Tracking.Value = Ball.Tracking;

            if (Tracking.Value && slidingSample != null)
                // keep the sliding sample playing at the current tracking position
                slidingSample.Balance.Value = CalculateSamplePlaybackBalance(Ball.X / OsuPlayfield.BASE_SIZE.X);

            double completionProgress = Math.Clamp((Time.Current - HitObject.StartTime) / HitObject.Duration, 0, 1);

            Ball.UpdateProgress(completionProgress);
            sliderBody?.UpdateProgress(completionProgress);

            foreach (DrawableHitObject hitObject in NestedHitObjects)
            {
                if (hitObject is ITrackSnaking s) s.UpdateSnakingPosition(HitObject.Path.PositionAt(sliderBody?.SnakedStart ?? 0), HitObject.Path.PositionAt(sliderBody?.SnakedEnd ?? 0));
                if (hitObject is IRequireTracking t) t.Tracking = Ball.Tracking;
            }

            Size = sliderBody?.Size ?? Vector2.Zero;
            OriginPosition = sliderBody?.PathOffset ?? Vector2.Zero;

            if (DrawSize != Vector2.Zero)
            {
                var childAnchorPosition = Vector2.Divide(OriginPosition, DrawSize);
                foreach (var obj in NestedHitObjects)
                    obj.RelativeAnchorPosition = childAnchorPosition;
                Ball.RelativeAnchorPosition = childAnchorPosition;
            }
        }

        public override void OnKilled()
        {
            base.OnKilled();
            sliderBody?.RecyclePath();
        }

        protected override void ApplySkin(ISkinSource skin, bool allowFallback)
        {
            base.ApplySkin(skin, allowFallback);

            bool allowBallTint = skin.GetConfig<OsuSkinConfiguration, bool>(OsuSkinConfiguration.AllowSliderBallTint)?.Value ?? false;
            Ball.AccentColour = allowBallTint ? AccentColour.Value : Color4.White;
        }

        protected override void CheckForResult(bool userTriggered, double timeOffset)
        {
            if (userTriggered || Time.Current < HitObject.EndTime)
                return;

            ApplyResult(r => r.Type = r.Judgement.MaxResult);
        }

        public override void PlaySamples()
        {
            // rather than doing it this way, we should probably attach the sample to the tail circle.
            // this can only be done after we stop using LegacyLastTick.
            if (TailCircle.IsHit)
                base.PlaySamples();
        }

        protected override void UpdateStateTransforms(ArmedState state)
        {
            base.UpdateStateTransforms(state);

            Ball.FadeIn();
            Ball.ScaleTo(HitObject.Scale);

            using (BeginDelayedSequence(HitObject.Duration, true))
            {
                const float fade_out_time = 450;

                // intentionally pile on an extra FadeOut to make it happen much faster.
                Ball.FadeOut(fade_out_time / 4, Easing.Out);

                switch (state)
                {
                    case ArmedState.Hit:
                        Ball.ScaleTo(HitObject.Scale * 1.4f, fade_out_time, Easing.Out);
                        break;
                }

                this.FadeOut(fade_out_time, Easing.OutQuint);
            }
        }

        public Drawable ProxiedLayer => HeadCircle.ProxiedLayer;

        public override bool ReceivePositionalInputAt(Vector2 screenSpacePos) => sliderBody?.ReceivePositionalInputAt(screenSpacePos) ?? base.ReceivePositionalInputAt(screenSpacePos);

        private class DefaultSliderBody : PlaySliderBody
        {
        }
    }
}
