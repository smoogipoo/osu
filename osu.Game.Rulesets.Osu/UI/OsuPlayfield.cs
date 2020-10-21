// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Pooling;
using osu.Game.Rulesets.Judgements;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Objects.Drawables;
using osu.Game.Rulesets.Osu.Objects;
using osu.Game.Rulesets.Osu.Objects.Drawables;
using osu.Game.Rulesets.Osu.Objects.Drawables.Connections;
using osu.Game.Rulesets.Osu.Scoring;
using osu.Game.Rulesets.Osu.UI.Cursor;
using osu.Game.Rulesets.Scoring;
using osu.Game.Rulesets.UI;
using osu.Game.Skinning;
using osuTK;

namespace osu.Game.Rulesets.Osu.UI
{
    public class OsuPlayfield : Playfield
    {
        private readonly ProxyContainer approachCircles;
        private readonly ProxyContainer spinnerProxies;
        private readonly JudgementContainer<DrawableOsuJudgement> judgementLayer;
        private readonly FollowPointRenderer followPoints;
        private readonly OrderedHitPolicy hitPolicy;

        public static readonly Vector2 BASE_SIZE = new Vector2(512, 384);

        protected override GameplayCursorContainer CreateCursor() => new OsuCursorContainer();

        private readonly IDictionary<HitResult, DrawablePool<DrawableOsuJudgement>> poolDictionary = new Dictionary<HitResult, DrawablePool<DrawableOsuJudgement>>();

        public OsuPlayfield()
        {
            InternalChildren = new Drawable[]
            {
                spinnerProxies = new ProxyContainer
                {
                    RelativeSizeAxes = Axes.Both
                },
                followPoints = new FollowPointRenderer
                {
                    RelativeSizeAxes = Axes.Both,
                    Depth = 2,
                },
                judgementLayer = new JudgementContainer<DrawableOsuJudgement>
                {
                    RelativeSizeAxes = Axes.Both,
                    Depth = 1,
                },
                // Todo: This should not exist, but currently helps to reduce LOH allocations due to unbinding skin source events on judgement disposal
                // Todo: Remove when hitobjects are properly pooled
                new SkinProvidingContainer(null)
                {
                    Child = HitObjectContainer,
                },
                approachCircles = new ProxyContainer
                {
                    RelativeSizeAxes = Axes.Both,
                    Depth = -1,
                },
            };

            hitPolicy = new OrderedHitPolicy(HitObjectContainer);

            var hitWindows = new OsuHitWindows();

            foreach (var result in Enum.GetValues(typeof(HitResult)).OfType<HitResult>().Where(r => r > HitResult.None && hitWindows.IsHitResultAllowed(r)))
                poolDictionary.Add(result, new DrawableJudgementPool(result));

            AddRangeInternal(poolDictionary.Values);

            OnNewResult += onNewResult;
        }

        public override void Add(HitObject h)
        {
            var osuObject = (OsuHitObject)h;

            HitObjectContainer.Add(new OsuHitObjectLifetimeEntry(h));
            followPoints.AddFollowPoints(osuObject);
        }

        // Todo:
        // public override void Remove(HitObject h)
        // {
        // }

        private void onNewResult(DrawableHitObject judgedObject, JudgementResult result)
        {
            // Hitobjects that block future hits should miss previous hitobjects if they're hit out-of-order.
            hitPolicy.HandleHit(judgedObject);

            if (!judgedObject.DisplayResult || !DisplayJudgements.Value)
                return;

            DrawableOsuJudgement explosion = poolDictionary[result.Type].Get(doj => doj.Apply(result, judgedObject));

            judgementLayer.Add(explosion);
        }

        public override bool ReceivePositionalInputAt(Vector2 screenSpacePos) => HitObjectContainer.ReceivePositionalInputAt(screenSpacePos);

        private class ProxyContainer : LifetimeManagementContainer
        {
            public void Add(Drawable proxy) => AddInternal(proxy);
        }

        private class DrawableJudgementPool : DrawablePool<DrawableOsuJudgement>
        {
            private readonly HitResult result;

            public DrawableJudgementPool(HitResult result)
                : base(10)
            {
                this.result = result;
            }

            protected override DrawableOsuJudgement CreateNewDrawable()
            {
                var judgement = base.CreateNewDrawable();

                // just a placeholder to initialise the correct drawable hierarchy for this pool.
                judgement.Apply(new JudgementResult(new HitObject(), new Judgement()) { Type = result }, null);

                return judgement;
            }
        }

        protected override HitObjectContainer CreateHitObjectContainer() => new OsuHitObjectContainer(this);

        private class OsuHitObjectLifetimeEntry : HitObjectLifetimeEntry
        {
            public OsuHitObjectLifetimeEntry(HitObject hitObject)
                : base(hitObject)
            {
                LifetimeStart = hitObject.StartTime - ((OsuHitObject)hitObject).TimePreempt;
            }
        }

        private class OsuHitObjectContainer : HitObjectContainer
        {
            private readonly DrawablePool<DrawableHitCircle> hitCirclePool;
            private readonly DrawablePool<DrawableSlider> sliderPool;
            private readonly DrawablePool<DrawableSpinner> spinnerPool;

            public OsuHitObjectContainer(OsuPlayfield playfield)
            {
                hitCirclePool = new OsuDrawablePool<DrawableHitCircle>(playfield, 10, 100);
                sliderPool = new OsuDrawablePool<DrawableSlider>(playfield, 10, 100);
                spinnerPool = new OsuDrawablePool<DrawableSpinner>(playfield, 2, 20);
            }

            protected override void LoadComplete()
            {
                base.LoadComplete();

                AddInternalAlwaysAlive(hitCirclePool);
                AddInternalAlwaysAlive(sliderPool);
                AddInternalAlwaysAlive(spinnerPool);
            }

            protected override DrawableHitObject GetDrawableFor(HitObject hitObject)
            {
                switch (hitObject)
                {
                    case HitCircle _:
                        return hitCirclePool.Get(d => d.Apply(hitObject));

                    case Slider _:
                        return sliderPool.Get(d => d.Apply(hitObject));

                    case Spinner _:
                        return spinnerPool.Get(d => d.Apply(hitObject));
                }

                throw new InvalidOperationException("Invalid hitobject type!");
            }
        }

        private class OsuDrawablePool<T> : DrawablePool<T>
            where T : PoolableDrawable, new()
        {
            private readonly OsuPlayfield playfield;

            public OsuDrawablePool(OsuPlayfield playfield, int initialSize, int? maximumSize = null)
                : base(initialSize, maximumSize)
            {
                this.playfield = playfield;
            }

            protected override T CreateNewDrawable() => base.CreateNewDrawable().With(o =>
            {
                var osuObject = (DrawableOsuHitObject)(object)o;

                osuObject.CheckHittable = playfield.hitPolicy.IsHittable;
                osuObject.OnLoadComplete += d =>
                {
                    switch (d)
                    {
                        case DrawableSpinner _:
                            playfield.spinnerProxies.Add(d.CreateProxy());
                            break;

                        case IDrawableHitObjectWithProxiedApproach approach:
                            playfield.approachCircles.Add(approach.ProxiedLayer.CreateProxy());
                            break;
                    }
                };
            });
        }
    }
}
