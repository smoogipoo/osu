// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Game.Rulesets.Scoring;

namespace osu.Game.Rulesets.Mania.Objects.Drawables
{
    /// <summary>
    /// The tail note of a hold.
    /// </summary>
    public class DrawableTailNote : DrawableNote
    {
        /// <summary>
        /// Lenience of release hit windows. This is to make cases where the hold note release
        /// is timed alongside presses of other hit objects less awkward.
        /// Todo: This shouldn't exist for non-LegacyBeatmapDecoder beatmaps
        /// </summary>
        private const double release_window_lenience = 1.5;

        private readonly DrawableHoldNote holdNote;

        public DrawableTailNote(DrawableHoldNote holdNote)
            : base(holdNote.HitObject.Tail)
        {
            this.holdNote = holdNote;
        }

        protected override void CheckForResult(bool userTriggered, double timeOffset)
        {
            // Factor in the release lenience
            timeOffset /= release_window_lenience;

            if (!userTriggered)
            {
                if (!HitObject.HitWindows.CanBeHit(timeOffset))
                    ApplyResult(r => r.Type = HitResult.Miss);

                return;
            }

            var result = HitObject.HitWindows.ResultFor(timeOffset);
            if (result == HitResult.None)
                return;

            ApplyResult(r =>
            {
                if (holdNote.hasBroken && (result == HitResult.Perfect || result == HitResult.Perfect))
                    result = HitResult.Good;

                r.Type = result;
            });
        }

        public override bool OnPressed(ManiaAction action) => false; // Tail doesn't handle key down

        public override bool OnReleased(ManiaAction action)
        {
            // Make sure that the user started holding the key during the hold note
            if (!holdNote.holdStartTime.HasValue)
                return false;

            if (action != Action.Value)
                return false;

            UpdateResult(true);

            // Handled by the hold note, which will set holding = false
            return false;
        }
    }
}
