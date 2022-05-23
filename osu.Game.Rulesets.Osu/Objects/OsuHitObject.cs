// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Linq;
using osu.Framework.Bindables;
using osu.Game.Beatmaps;
using osu.Game.Rulesets.Objects;
using osuTK;
using osu.Game.Rulesets.Objects.Types;
using osu.Game.Beatmaps.ControlPoints;
using osu.Game.Rulesets.Osu.Scoring;
using osu.Game.Rulesets.Scoring;

namespace osu.Game.Rulesets.Osu.Objects
{
    public abstract class OsuHitObject : HitObject, IHasComboInformation, IHasPosition
    {
        /// <summary>
        /// The radius of hit objects (ie. the radius of a <see cref="HitCircle"/>).
        /// </summary>
        public const float OBJECT_RADIUS = 64;

        /// <summary>
        /// Scoring distance with a speed-adjusted beat length of 1 second (ie. the speed slider balls move through their track).
        /// </summary>
        internal const float BASE_SCORING_DISTANCE = 100;

        /// <summary>
        /// Minimum preempt time at AR=10.
        /// </summary>
        public const double PREEMPT_MIN = 450;

        public double TimePreempt = 600;
        public double TimeFadeIn = 400;

        private BindableSlim<Vector2> positionBindable;
        private BindableSlim<int> stackHeightBindable;
        private BindableSlim<float> scaleBindable = new BindableSlim<float> { Default = 1, Value = 1 };
        private BindableSlim<int> comboOffsetBindable;
        private BindableSlim<int> indexInCurrentComboBindable;
        private BindableSlim<int> comboIndexBindable;
        private BindableSlim<int> comboIndexWithOffsetsBindable;
        private BindableSlim<bool> lastInComboBindable;

        public ref BindableSlim<Vector2> PositionBindable => ref positionBindable;
        public ref BindableSlim<int> StackHeightBindable => ref stackHeightBindable;
        public ref BindableSlim<float> ScaleBindable => ref scaleBindable;
        public ref BindableSlim<int> ComboOffsetBindable => ref comboOffsetBindable;
        public ref BindableSlim<int> IndexInCurrentComboBindable => ref indexInCurrentComboBindable;
        public ref BindableSlim<int> ComboIndexBindable => ref comboIndexBindable;
        public ref BindableSlim<int> ComboIndexWithOffsetsBindable => ref comboIndexWithOffsetsBindable;
        public ref BindableSlim<bool> LastInComboBindable => ref lastInComboBindable;

        public virtual Vector2 Position
        {
            get => positionBindable.Value;
            set => positionBindable.Value = value;
        }

        public float X => Position.X;
        public float Y => Position.Y;

        public Vector2 StackedPosition => Position + StackOffset;

        public virtual Vector2 EndPosition => Position;

        public Vector2 StackedEndPosition => EndPosition + StackOffset;

        public int StackHeight
        {
            get => stackHeightBindable.Value;
            set => stackHeightBindable.Value = value;
        }

        public virtual Vector2 StackOffset => new Vector2(StackHeight * Scale * -6.4f);

        public double Radius => OBJECT_RADIUS * Scale;

        public float Scale
        {
            get => scaleBindable.Value;
            set => scaleBindable.Value = value;
        }

        public virtual bool NewCombo { get; set; }

        public int ComboOffset
        {
            get => comboOffsetBindable.Value;
            set => comboOffsetBindable.Value = value;
        }

        public virtual int IndexInCurrentCombo
        {
            get => indexInCurrentComboBindable.Value;
            set => indexInCurrentComboBindable.Value = value;
        }

        public virtual int ComboIndex
        {
            get => comboIndexBindable.Value;
            set => comboIndexBindable.Value = value;
        }

        public int ComboIndexWithOffsets
        {
            get => comboIndexWithOffsetsBindable.Value;
            set => comboIndexWithOffsetsBindable.Value = value;
        }

        public bool LastInCombo
        {
            get => lastInComboBindable.Value;
            set => lastInComboBindable.Value = value;
        }

        IBindable<int> IHasComboInformation.IndexInCurrentComboBindable => indexInCurrentComboBindable;
        IBindable<int> IHasComboInformation.ComboIndexBindable => comboIndexBindable;
        IBindable<int> IHasComboInformation.ComboIndexWithOffsetsBindable => comboIndexWithOffsetsBindable;
        IBindable<bool> IHasComboInformation.LastInComboBindable => lastInComboBindable;

        protected OsuHitObject()
        {
            stackHeightBindable.BindValueChanged(height =>
            {
                foreach (var nested in NestedHitObjects.OfType<OsuHitObject>())
                    nested.StackHeight = height.NewValue;
            });
        }

        protected override void ApplyDefaultsToSelf(ControlPointInfo controlPointInfo, IBeatmapDifficultyInfo difficulty)
        {
            base.ApplyDefaultsToSelf(controlPointInfo, difficulty);

            TimePreempt = (float)IBeatmapDifficultyInfo.DifficultyRange(difficulty.ApproachRate, 1800, 1200, PREEMPT_MIN);

            // Preempt time can go below 450ms. Normally, this is achieved via the DT mod which uniformly speeds up all animations game wide regardless of AR.
            // This uniform speedup is hard to match 1:1, however we can at least make AR>10 (via mods) feel good by extending the upper linear function above.
            // Note that this doesn't exactly match the AR>10 visuals as they're classically known, but it feels good.
            // This adjustment is necessary for AR>10, otherwise TimePreempt can become smaller leading to hitcircles not fully fading in.
            TimeFadeIn = 400 * Math.Min(1, TimePreempt / PREEMPT_MIN);

            Scale = (1.0f - 0.7f * (difficulty.CircleSize - 5) / 5) / 2;
        }

        protected override HitWindows CreateHitWindows() => new OsuHitWindows();
    }
}
