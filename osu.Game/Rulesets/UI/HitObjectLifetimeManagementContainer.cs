// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Rulesets.Judgements;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Objects.Drawables;

namespace osu.Game.Rulesets.UI
{
    // Todo: Temporary, should be merged into HOC.
    public class HitObjectLifetimeManagementContainer : LifetimeManagementContainer
    {
        public event Action<DrawableHitObject, JudgementResult> OnNewResult;
        public event Action<DrawableHitObject, JudgementResult> OnRevertResult;

        private readonly Dictionary<HitObjectLifetimeEntry, DrawableHitObject> drawableMap = new Dictionary<HitObjectLifetimeEntry, DrawableHitObject>();

        public void Add(HitObjectLifetimeEntry entry)
        {
            LifetimeManager.AddEntry(entry);
        }

        public void Remove(HitObjectLifetimeEntry entry)
        {
            LifetimeManager.RemoveEntry(entry);

            // If the entry is already associated with a DHO, we also need to remove it.
            if (drawableMap.TryGetValue(entry, out var dho))
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

            drawableMap.Remove(entry);

            RemoveInternal(drawable);
        }

        private void onRevertResult(DrawableHitObject d, JudgementResult r) => OnRevertResult?.Invoke(d, r);
        private void onNewResult(DrawableHitObject d, JudgementResult r) => OnNewResult?.Invoke(d, r);
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
