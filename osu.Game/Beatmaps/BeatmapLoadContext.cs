// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;

namespace osu.Game.Beatmaps
{
    public abstract class BeatmapLoadContext
    {
        public static BeatmapLoadContext Current => current.Value!;
        private static readonly ThreadLocal<BeatmapLoadContext> current = new ThreadLocal<BeatmapLoadContext>(() => new DefaultBeatmapLoadContext());

        public IDisposable Begin()
            => new BeatmapLoadContextUsage(this);

        public abstract T Rent<T>() where T : notnull, new();

        public abstract void Return<T>(T obj) where T : notnull, new();

        protected abstract void UsageStarted();

        protected abstract void UsageFinished();

        private class BeatmapLoadContextUsage : IDisposable
        {
            private readonly BeatmapLoadContext context;
            private readonly BeatmapLoadContext oldContext;
            private bool isDisposed;

            public BeatmapLoadContextUsage(BeatmapLoadContext context)
            {
                this.context = context;
                oldContext = current.Value!;
                current.Value = context;

                context.UsageStarted();
            }

            public void Dispose()
            {
                if (isDisposed)
                    return;

                context.UsageFinished();
                current.Value = oldContext;
                isDisposed = true;
            }
        }
    }

    public class DefaultBeatmapLoadContext : BeatmapLoadContext
    {
        public override T Rent<T>()
            => new T();

        public override void Return<T>(T obj)
        {
        }

        protected override void UsageStarted()
        {
        }

        protected override void UsageFinished()
        {
        }
    }

    public class PooledBeatmapLoadContext : BeatmapLoadContext
    {
        private readonly Dictionary<Type, Stack<object>> pools = new Dictionary<Type, Stack<object>>();
        private readonly List<object> objectsInUse = new List<object>();
        private uint isTracking;

        public override T Rent<T>()
        {
            if (isTracking == 0)
                return new T();

            if (!getPool(typeof(T)).TryPop(out object? obj))
                obj = new T();

            objectsInUse.Add(obj);
            return (T)obj;
        }

        public override void Return<T>(T obj)
            => getPool(typeof(T)).Push(obj);

        protected override void UsageStarted()
            => isTracking++;

        protected override void UsageFinished()
        {
            Trace.Assert(isTracking > 0);
            if (--isTracking > 0)
                return;

            foreach (object obj in objectsInUse)
                getPool(obj.GetType()).Push(obj);
            objectsInUse.Clear();
        }

        private Stack<object> getPool(Type type)
        {
            if (pools.TryGetValue(type, out Stack<object>? pool))
                return pool;

            return pools[type] = [];
        }
    }
}
