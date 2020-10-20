// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Rulesets.Judgements;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Objects.Drawables;

namespace osu.Game.Rulesets.UI
{
    public class HitObjectContainer : LifetimeManagementContainer
    {
        public IEnumerable<DrawableHitObject> Objects => InternalChildren.OfType<DrawableHitObject>().OrderBy(h => h.HitObject.StartTime);
        public IEnumerable<DrawableHitObject> AliveObjects => AliveInternalChildren.OfType<DrawableHitObject>().OrderBy(h => h.HitObject.StartTime);

        public event Action<DrawableHitObject, JudgementResult> OnNewResult;
        public event Action<DrawableHitObject, JudgementResult> OnRevertResult;

        private readonly Dictionary<HitObjectLifetimeEntry, DrawableHitObject> drawableMap = new Dictionary<HitObjectLifetimeEntry, DrawableHitObject>();

        public HitObjectContainer()
        {
            RelativeSizeAxes = Axes.Both;
        }

        public virtual void Add(DrawableHitObject hitObject) => AddInternal(hitObject);

        public virtual bool Remove(DrawableHitObject hitObject) => RemoveInternal(hitObject);

        public int IndexOf(DrawableHitObject hitObject) => IndexOfInternal(hitObject);

        public void Add(HitObjectLifetimeEntry entry) => LifetimeManager.AddEntry(entry);

        public void Remove(HitObjectLifetimeEntry entry)
        {
            LifetimeManager.RemoveEntry(entry);

            // If the entry is already associated with a DHO, we also need to remove it.
            if (drawableMap.TryGetValue(entry, out var dho))
                removeDrawable(entry);
        }

        public virtual void Clear(bool disposeChildren = true)
        {
            ClearInternal(disposeChildren);

            foreach (var (entry, _) in drawableMap)
                removeDrawable(entry);
        }

        protected override void OnBecomeAlive(LifetimeEntry entry)
        {
            if (entry is HitObjectLifetimeEntry hEntry)
                addDrawable(hEntry);
            else
                base.OnBecomeAlive(entry);
        }

        protected override void OnBecomeDead(LifetimeEntry entry)
        {
            if (entry is HitObjectLifetimeEntry hEntry)
                removeDrawable(hEntry);
            else
                base.OnBecomeDead(entry);
        }

        protected override void OnBoundaryCrossed(LifetimeEntry entry, LifetimeBoundaryKind kind, LifetimeBoundaryCrossingDirection direction)
        {
            if (entry is HitObjectLifetimeEntry)
            {
                // We're managing the lifetime.
                return;
            }

            base.OnBoundaryCrossed(entry, kind, direction);
        }

        protected override void OnChildLifetimeBoundaryCrossed(LifetimeBoundaryCrossedEvent e)
        {
            if (!(e.Child is DrawableHitObject hitObject))
                return;

            if ((e.Kind == LifetimeBoundaryKind.End && e.Direction == LifetimeBoundaryCrossingDirection.Forward)
                || (e.Kind == LifetimeBoundaryKind.Start && e.Direction == LifetimeBoundaryCrossingDirection.Backward))
            {
                hitObject.OnKilled();
            }
        }

        protected override Drawable GetDrawableFor(LifetimeEntry entry)
        {
            if (entry is HitObjectLifetimeEntry hEntry)
            {
                if (drawableMap.TryGetValue(hEntry, out var drawable))
                    return drawable;

                return null;
            }

            return base.GetDrawableFor(entry);
        }

        /// <summary>
        /// Retrieves the <see cref="DrawableHitObject"/> corresponding to a <see cref="HitObject"/>.
        /// </summary>
        /// <param name="hitObject">The <see cref="HitObject"/> to retrieve the drawable form of.</param>
        /// <returns>The <see cref="DrawableHitObject"/>.</returns>
        protected virtual DrawableHitObject GetDrawableFor(HitObject hitObject) => null;

        private void addDrawable(HitObjectLifetimeEntry entry)
        {
            Debug.Assert(!drawableMap.ContainsKey(entry));

            var drawable = GetDrawableFor(entry.HitObject);
            Debug.Assert(drawable.LifetimeEntry == null);

            drawable.LifetimeEntry = entry;
            drawable.OnNewResult += onNewResult;
            drawable.OnRevertResult += onRevertResult;

            AddInternalAlwaysAlive(drawableMap[entry] = drawable);
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

            RemoveInternal(drawable);
        }

        private void onRevertResult(DrawableHitObject d, JudgementResult r) => OnRevertResult?.Invoke(d, r);
        private void onNewResult(DrawableHitObject d, JudgementResult r) => OnNewResult?.Invoke(d, r);

        protected override int Compare(Drawable x, Drawable y)
        {
            if (!(x is DrawableHitObject xObj) || !(y is DrawableHitObject yObj))
                return base.Compare(x, y);

            // Put earlier hitobjects towards the end of the list, so they handle input first
            int i = yObj.HitObject.StartTime.CompareTo(xObj.HitObject.StartTime);
            return i == 0 ? CompareReverseChildID(x, y) : i;
        }
    }

    public class HitObjectLifetimeEntry : LifetimeEntry
    {
        public readonly HitObject HitObject;

        public HitObjectLifetimeEntry(HitObject hitObject)
        {
            HitObject = hitObject;
            LifetimeStart = HitObject.StartTime;
        }
    }
}
