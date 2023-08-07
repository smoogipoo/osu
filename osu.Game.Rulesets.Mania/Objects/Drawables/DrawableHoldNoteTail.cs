// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using System.Diagnostics;
using osu.Framework.Graphics;
using osu.Framework.Input.Events;
using osu.Game.Rulesets.Scoring;

namespace osu.Game.Rulesets.Mania.Objects.Drawables
{
    /// <summary>
    /// The tail of a <see cref="DrawableHoldNote"/>.
    /// </summary>
    public partial class DrawableHoldNoteTail : DrawableNote
    {
        protected override ManiaSkinComponents Component => ManiaSkinComponents.HoldNoteTail;

        protected internal DrawableHoldNote HoldNote => (DrawableHoldNote)ParentHitObject;

        public DrawableHoldNoteTail()
            : this(null)
        {
        }

        public DrawableHoldNoteTail(TailNote tailNote)
            : base(tailNote)
        {
            Anchor = Anchor.TopCentre;
            Origin = Anchor.TopCentre;
        }

        public void UpdateResult() => base.UpdateResult(true);

        protected override void CheckForResult(bool userTriggered, double timeOffset)
        {
            Debug.Assert(HitObject.HitWindows != null);

            // Factor in the release lenience
            timeOffset /= TailNote.RELEASE_WINDOW_LENIENCE;

            if (!userTriggered)
            {
                if (!HitObject.HitWindows.CanBeHit(timeOffset))
                    ApplyResult(r => r.Type = r.Judgement.MinResult);

                return;
            }

            var result = HitObject.HitWindows.ResultFor(timeOffset);
            if (result == HitResult.None)
                return;

            ApplyResult(r =>
            {
                // If the user hasn't hit the head note, or the holding key was released at some point,
                // then the user's score is to be capped to a maximum of a "meh".
                bool hasHitCorrectly = HoldNote.Head.IsHit && HoldNote.HoldBrokenTime == null;

                if (result > HitResult.Meh && !hasHitCorrectly)
                    result = HitResult.Meh;

                r.Type = result;
            });
        }

        public override bool OnPressed(KeyBindingPressEvent<ManiaAction> e) => false; // Handled by the hold note

        public override void OnReleased(KeyBindingReleaseEvent<ManiaAction> e)
        {
        }
    }
}
