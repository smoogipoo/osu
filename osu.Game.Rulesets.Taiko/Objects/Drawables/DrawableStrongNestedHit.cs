// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using osu.Game.Rulesets.Judgements;
using osu.Game.Rulesets.Objects.Drawables;
using osu.Game.Rulesets.Taiko.Judgements;

namespace osu.Game.Rulesets.Taiko.Objects.Drawables
{
    /// <summary>
    /// Used as a nested hitobject to provide <see cref="TaikoStrongJudgement"/>s for <see cref="DrawableTaikoHitObject"/>s.
    /// </summary>
    public class DrawableStrongNestedHit : DrawableTaikoHitObject
    {
        public readonly DrawableHitObject MainObject;

        public DrawableStrongNestedHit(StrongHitObject strong, DrawableHitObject mainObject)
            : base(strong)
        {
            MainObject = mainObject;
        }

        public new void ApplyResult(Action<JudgementResult> application) => base.ApplyResult(application);

        protected override void UpdateState(ArmedState state)
        {
        }

        public override bool OnPressed(TaikoAction action) => false;
    }
}
