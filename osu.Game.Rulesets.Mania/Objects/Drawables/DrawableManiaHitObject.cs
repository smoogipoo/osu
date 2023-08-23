// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using System;
using JetBrains.Annotations;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Game.Rulesets.Objects.Drawables;
using osu.Game.Rulesets.UI.Scrolling;
using osu.Game.Rulesets.Mania.UI;
using osu.Game.Rulesets.Scoring;
using osu.Game.Screens.Play;

namespace osu.Game.Rulesets.Mania.Objects.Drawables
{
    public abstract partial class DrawableManiaHitObject : DrawableHitObject<ManiaHitObject>
    {
        /// <summary>
        /// The <see cref="ManiaAction"/> which causes this <see cref="DrawableManiaHitObject{TObject}"/> to be hit.
        /// </summary>
        protected readonly IBindable<ManiaAction> Action = new Bindable<ManiaAction>();

        protected readonly IBindable<ScrollingDirection> Direction = new Bindable<ScrollingDirection>();

        [Resolved(canBeNull: true)]
        private ManiaPlayfield playfield { get; set; }

        [Resolved(CanBeNull = true)]
        private IGameplayClock gameplayClock { get; set; }

        protected override float SamplePlaybackPosition
        {
            get
            {
                if (playfield == null)
                    return base.SamplePlaybackPosition;

                return (float)HitObject.Column / playfield.TotalColumns;
            }
        }

        /// <summary>
        /// Whether this <see cref="DrawableManiaHitObject"/> can be hit, given a time value.
        /// If non-null, judgements will be ignored whilst the function returns false.
        /// </summary>
        public Func<DrawableHitObject, double, bool> CheckHittable;

        protected DrawableManiaHitObject(ManiaHitObject hitObject)
            : base(hitObject)
        {
            RelativeSizeAxes = Axes.X;
        }

        [BackgroundDependencyLoader(true)]
        private void load([CanBeNull] IBindable<ManiaAction> action, [NotNull] IScrollingInfo scrollingInfo)
        {
            if (action != null)
                Action.BindTo(action);

            Direction.BindTo(scrollingInfo.Direction);
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            Direction.BindValueChanged(OnDirectionChanged, true);
        }

        protected virtual void OnDirectionChanged(ValueChangedEvent<ScrollingDirection> e)
        {
            Anchor = Origin = e.NewValue == ScrollingDirection.Up ? Anchor.TopCentre : Anchor.BottomCentre;
        }

        protected override void UpdateHitStateTransforms(ArmedState state)
        {
            switch (state)
            {
                case ArmedState.Miss:
                    this.FadeOut(150, Easing.In);
                    break;

                case ArmedState.Hit:
                    this.FadeOut();
                    break;
            }
        }

        /// <summary>
        /// Causes this <see cref="DrawableManiaHitObject"/> to get missed, disregarding all conditions in implementations of <see cref="DrawableHitObject.CheckForResult"/>.
        /// </summary>
        public virtual void MissForcefully() => ApplyResult(r => r.Type = r.Judgement.MinResult);

        #region HitWindows overrides

        /// <inheritdoc cref="HitWindows.CanBeHit"/>
        /// <remarks>
        /// Accounts for the gameplay rate.
        /// </remarks>
        public bool CanBeHit(double timeOffset) => HitObject.HitWindows.CanBeHit(timeOffset / gameplayClock?.GetTrueGameplayRate() ?? 1);

        /// <inheritdoc cref="HitWindows.ResultFor"/>
        /// <remarks>
        /// Accounts for the gameplay rate.
        /// </remarks>
        public HitResult ResultFor(double timeOffset) => HitObject.HitWindows.ResultFor(timeOffset / gameplayClock?.GetTrueGameplayRate() ?? 1);

        /// <inheritdoc cref="HitWindows.WindowFor"/>
        /// <remarks>
        /// Accounts for the gameplay rate.
        /// </remarks>
        public double WindowFor(HitResult result) => HitObject.HitWindows.WindowFor(result) * gameplayClock?.GetTrueGameplayRate() ?? 1;

        #endregion
    }

    public abstract partial class DrawableManiaHitObject<TObject> : DrawableManiaHitObject
        where TObject : ManiaHitObject
    {
        public new TObject HitObject => (TObject)base.HitObject;

        protected DrawableManiaHitObject(TObject hitObject)
            : base(hitObject)
        {
        }
    }
}
