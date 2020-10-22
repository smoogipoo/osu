// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Graphics;
using osu.Game.Rulesets.Objects;

namespace osu.Game.Rulesets.UI
{
    public class HitObjectLifetimeEntry : LifetimeEntry
    {
        public readonly HitObject HitObject;

        public HitObjectLifetimeEntry(HitObject hitObject)
        {
            HitObject = hitObject;
            UpdateLifetimeStart();
        }

        private double realLifetimeStart;

        public new double LifetimeStart
        {
            get => realLifetimeStart;
            set => setLifetime(realLifetimeStart = value, LifetimeEnd);
        }

        private double realLifetimeEnd;

        public new double LifetimeEnd
        {
            get => realLifetimeEnd;
            set => setLifetime(LifetimeStart, realLifetimeEnd = value);
        }

        private void setLifetime(double start, double end)
        {
            if (keepAlive)
            {
                start = double.MinValue;
                end = double.MaxValue;
            }

            base.LifetimeStart = start;
            base.LifetimeEnd = end;
        }

        private bool keepAlive;

        internal bool KeepAlive
        {
            set
            {
                if (keepAlive == value)
                    return;

                keepAlive = value;
                setLifetime(realLifetimeStart, realLifetimeEnd);
            }
        }

        protected virtual double InitialLifetimeOffset => 10000;

        internal void UpdateLifetimeStart() => LifetimeStart = HitObject.StartTime - InitialLifetimeOffset;
    }
}
