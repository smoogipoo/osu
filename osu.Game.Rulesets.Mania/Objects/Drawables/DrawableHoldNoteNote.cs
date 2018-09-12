// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using osu.Game.Rulesets.Judgements;

namespace osu.Game.Rulesets.Mania.Objects.Drawables
{
    /// <summary>
    /// A note visualising the head or tail of a <see cref="DrawableHoldNote"/>.
    /// <remarks>
    /// The <see cref="DrawableNote.Result"/> of this note is completely handled by the <see cref="DrawableHoldNote"/>.
    /// </remarks>
    /// </summary>
    public class DrawableHoldNoteNote : DrawableNote
    {
        public DrawableHoldNoteNote(Note hitObject)
            : base(hitObject)
        {
        }

        public new void ApplyResult(Action<JudgementResult> application) => base.ApplyResult(application);

        protected override void CheckForResult(bool userTriggered, double timeOffset)
        {
        }

        public override bool OnPressed(ManiaAction action) => false;
    }
}
