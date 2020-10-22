// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Rulesets.Judgements;
using osu.Game.Rulesets.Objects.Drawables;

namespace osu.Game.Rulesets.UI
{
    public class HitObjectContainer : LifetimeManagementContainer
    {
        public IEnumerable<DrawableHitObject> Objects => InternalChildren.OfType<DrawableHitObject>().OrderBy(h => h.HitObject.StartTime);
        public IEnumerable<DrawableHitObject> AliveObjects => AliveInternalChildren.OfType<DrawableHitObject>().OrderBy(h => h.HitObject.StartTime);

        public event Action<DrawableHitObject, JudgementResult> OnNewResult;
        public event Action<DrawableHitObject, JudgementResult> OnRevertResult;

        /// <summary>
        /// Invoked when a <see cref="DrawableHitObject"/> becomes "current".
        /// </summary>
        /// <remarks>
        /// If this <see cref="HitObjectContainer"/> uses pooled objects, this represents the time when the <see cref="DrawableHitObject"/>s become alive.
        /// </remarks>
        public event Action<DrawableHitObject> HitObjectEnteredCurrent;

        /// <summary>
        /// Invoked when a <see cref="DrawableHitObject"/> becomes "not current".
        /// </summary>
        /// <remarks>
        /// If this <see cref="HitObjectContainer"/> uses pooled objects, this represents the time when the <see cref="DrawableHitObject"/>s become dead.
        /// </remarks>
        public event Action<DrawableHitObject> HitObjectExitedCurrent;

        /// <summary>
        /// The list of all "current" <see cref="DrawableHitObject"/>s. See <see cref="HitObjectEnteredCurrent"/> for more information.
        /// </summary>
        public IEnumerable<DrawableHitObject> CurrentObjects => InternalChildren.OfType<DrawableHitObject>().Reverse();

        private readonly Dictionary<DrawableHitObject, IBindable> startTimeMap = new Dictionary<DrawableHitObject, IBindable>();
        private readonly Dictionary<HitObjectLifetimeEntry, DrawableHitObject> drawableMap = new Dictionary<HitObjectLifetimeEntry, DrawableHitObject>();
        private readonly LifetimeManager lifetimeManager = new LifetimeManager();

        [Resolved(CanBeNull = true)]
        private DrawableRuleset drawableRuleset { get; set; }

        public HitObjectContainer()
        {
            RelativeSizeAxes = Axes.Both;

            lifetimeManager.OnBecomeAlive += onBecomeAlive;
            lifetimeManager.OnBecomeDead += onBecomeDead;
        }

        public void Add(HitObjectLifetimeEntry entry) => lifetimeManager.AddEntry(entry);

        public void Remove(HitObjectLifetimeEntry entry)
        {
            lifetimeManager.RemoveEntry(entry);

            // If the entry is already associated with a DHO, we also need to remove it.
            if (drawableMap.TryGetValue(entry, out _))
                removeDrawable(entry);
        }

        public virtual void Clear(bool disposeChildren = true)
        {
            ClearInternal(disposeChildren);

            foreach (var (entry, _) in drawableMap)
                removeDrawable(entry);

            unbindAllStartTimes();
        }

        protected override bool CheckChildrenLife() => base.CheckChildrenLife() | lifetimeManager.Update(Time.Current - PastLifetimeExtension, Time.Current + FutureLifetimeExtension);

        #region Pooling support

        private void onBecomeAlive(LifetimeEntry entry) => addDrawable((HitObjectLifetimeEntry)entry);

        private void onBecomeDead(LifetimeEntry entry) => removeDrawable((HitObjectLifetimeEntry)entry);

        private void addDrawable(HitObjectLifetimeEntry entry)
        {
            Debug.Assert(!drawableMap.ContainsKey(entry));

            var drawable = drawableRuleset.CreateDrawableRepresentation(entry.HitObject);
            Debug.Assert(drawable.LifetimeEntry == null);

            drawable.LifetimeEntry = entry;
            drawable.OnNewResult += onNewResult;
            drawable.OnRevertResult += onRevertResult;

            bindStartTime(drawable);
            AddInternalAlwaysAlive(drawableMap[entry] = drawable);

            HitObjectEnteredCurrent?.Invoke(drawable);
        }

        private void removeDrawable(HitObjectLifetimeEntry entry)
        {
            Debug.Assert(drawableMap.ContainsKey(entry));

            var drawable = drawableMap[entry];
            Debug.Assert(drawable.LifetimeEntry != null);

            drawable.LifetimeEntry = null;
            drawable.OnNewResult -= onNewResult;
            drawable.OnRevertResult -= onRevertResult;
            drawable.OnKilled();

            drawableMap.Remove(entry);

            unbindStartTime(drawable);
            RemoveInternal(drawable);

            HitObjectExitedCurrent?.Invoke(drawable);
        }

        #endregion

        private void onNewResult(DrawableHitObject d, JudgementResult r) => OnNewResult?.Invoke(d, r);
        private void onRevertResult(DrawableHitObject d, JudgementResult r) => OnRevertResult?.Invoke(d, r);

        #region Comparator + StartTime tracking

        private void bindStartTime(DrawableHitObject hitObject)
        {
            var bindable = hitObject.StartTimeBindable.GetBoundCopy();
            bindable.BindValueChanged(_ => onStartTimeChanged(hitObject));

            startTimeMap[hitObject] = bindable;
        }

        private void unbindStartTime(DrawableHitObject hitObject)
        {
            startTimeMap[hitObject].UnbindAll();
            startTimeMap.Remove(hitObject);
        }

        private void unbindAllStartTimes()
        {
            foreach (var kvp in startTimeMap)
                kvp.Value.UnbindAll();
            startTimeMap.Clear();
        }

        private void onStartTimeChanged(DrawableHitObject hitObject)
        {
            hitObject.LifetimeEntry?.UpdateLifetimeStart();
            SortInternal();
        }

        protected override int Compare(Drawable x, Drawable y)
        {
            if (!(x is DrawableHitObject xObj) || !(y is DrawableHitObject yObj))
                return base.Compare(x, y);

            // Put earlier hitobjects towards the end of the list, so they handle input first
            int i = yObj.HitObject.StartTime.CompareTo(xObj.HitObject.StartTime);
            return i == 0 ? CompareReverseChildID(x, y) : i;
        }

        #endregion

        #region Non-pooling support

        public virtual void Add(DrawableHitObject hitObject)
        {
            bindStartTime(hitObject);
            AddInternal(hitObject);

            hitObject.OnNewResult += onNewResult;
            hitObject.OnRevertResult += onRevertResult;
        }

        public virtual bool Remove(DrawableHitObject hitObject)
        {
            if (!RemoveInternal(hitObject))
                return false;

            hitObject.OnNewResult -= onNewResult;
            hitObject.OnRevertResult -= onRevertResult;

            unbindStartTime(hitObject);

            return true;
        }

        public int IndexOf(DrawableHitObject hitObject) => IndexOfInternal(hitObject);

        protected override void OnChildLifetimeBoundaryCrossed(LifetimeBoundaryCrossedEvent e)
        {
            if (!(e.Child is DrawableHitObject hitObject))
                return;

            switch (e.Kind)
            {
                case LifetimeBoundaryKind.Start when e.Direction == LifetimeBoundaryCrossingDirection.Forward:
                case LifetimeBoundaryKind.End when e.Direction == LifetimeBoundaryCrossingDirection.Backward:
                    HitObjectEnteredCurrent?.Invoke(hitObject);
                    break;

                case LifetimeBoundaryKind.End when e.Direction == LifetimeBoundaryCrossingDirection.Forward:
                case LifetimeBoundaryKind.Start when e.Direction == LifetimeBoundaryCrossingDirection.Backward:
                    hitObject.OnKilled();
                    HitObjectExitedCurrent?.Invoke(hitObject);
                    break;
            }
        }

        #endregion

        protected override void Dispose(bool isDisposing)
        {
            base.Dispose(isDisposing);
            unbindAllStartTimes();
        }
    }
}
