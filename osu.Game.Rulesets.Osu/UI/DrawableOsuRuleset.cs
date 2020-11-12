﻿// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Graphics.Pooling;
using osu.Framework.Input;
using osu.Game.Beatmaps;
using osu.Game.Input.Handlers;
using osu.Game.Replays;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Objects.Drawables;
using osu.Game.Rulesets.Osu.Configuration;
using osu.Game.Rulesets.Osu.Objects;
using osu.Game.Rulesets.Osu.Objects.Drawables;
using osu.Game.Rulesets.Osu.Replays;
using osu.Game.Rulesets.UI;
using osu.Game.Screens.Play;
using osuTK;

namespace osu.Game.Rulesets.Osu.UI
{
    public class DrawableOsuRuleset : DrawableRuleset<OsuHitObject>
    {
        protected new OsuRulesetConfigManager Config => (OsuRulesetConfigManager)base.Config;

        public new OsuPlayfield Playfield => (OsuPlayfield)base.Playfield;

        public DrawableOsuRuleset(Ruleset ruleset, IBeatmap beatmap, IReadOnlyList<Mod> mods = null)
            : base(ruleset, beatmap, mods)
        {
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            registerPool<HitCircle, DrawableHitCircle>(10, 100);

            registerPool<Slider, DrawableSlider>(10, 100);
            registerPool<SliderHeadCircle, DrawableSliderHead>(10, 100);
            registerPool<SliderTailCircle, DrawableSliderTail>(10, 100);
            registerPool<SliderTick, DrawableSliderTick>(10, 100);
            registerPool<SliderRepeat, DrawableSliderRepeat>(5, 50);

            registerPool<Spinner, DrawableSpinner>(2, 20);
            registerPool<SpinnerTick, DrawableSpinnerTick>(10, 100);
            registerPool<SpinnerBonusTick, DrawableSpinnerBonusTick>(10, 100);
        }

        private void registerPool<TObject, TDrawable>(int initialSize, int? maximumSize = null)
            where TObject : HitObject
            where TDrawable : DrawableHitObject, new()
            => RegisterPool<TObject, TDrawable>(CreatePool<TDrawable>(initialSize, maximumSize));

        protected virtual DrawablePool<TDrawable> CreatePool<TDrawable>(int initialSize, int? maximumSize = null)
            where TDrawable : DrawableHitObject, new()
            => new OsuDrawablePool<TDrawable>(Playfield.CheckHittable, Playfield.OnHitObjectLoaded, initialSize, maximumSize);

        protected override HitObjectLifetimeEntry CreateLifetimeEntry(OsuHitObject hitObject) => new OsuHitObjectLifetimeEntry(hitObject);

        public override bool ReceivePositionalInputAt(Vector2 screenSpacePos) => true; // always show the gameplay cursor

        protected override Playfield CreatePlayfield() => new OsuPlayfield();

        protected override PassThroughInputManager CreateInputManager() => new OsuInputManager(Ruleset.RulesetInfo);

        public override PlayfieldAdjustmentContainer CreatePlayfieldAdjustmentContainer() => new OsuPlayfieldAdjustmentContainer { AlignWithStoryboard = true };

        protected override ResumeOverlay CreateResumeOverlay() => new OsuResumeOverlay();

        protected override ReplayInputHandler CreateReplayInputHandler(Replay replay) => new OsuFramedReplayInputHandler(replay);

        protected override ReplayRecorder CreateReplayRecorder(Replay replay) => new OsuReplayRecorder(replay);

        public override double GameplayStartTime
        {
            get
            {
                if (Objects.FirstOrDefault() is OsuHitObject first)
                    return first.StartTime - Math.Max(2000, first.TimePreempt);

                return 0;
            }
        }

        private class OsuHitObjectLifetimeEntry : HitObjectLifetimeEntry
        {
            public OsuHitObjectLifetimeEntry(HitObject hitObject)
                : base(hitObject)
            {
            }

            protected override double InitialLifetimeOffset => ((OsuHitObject)HitObject).TimePreempt;
        }
    }
}
