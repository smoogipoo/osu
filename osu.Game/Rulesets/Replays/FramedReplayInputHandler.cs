// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using System.Collections.Generic;
using osu.Framework.Input.StateChanges;
using osu.Game.Input.Handlers;
using OpenTK;

namespace osu.Game.Rulesets.Replays
{
    /// <summary>
    /// The ReplayHandler will take a replay and handle the propagation of updates to the input stack.
    /// It handles logic of any frames which *must* be executed.
    /// </summary>
    public abstract class FramedReplayInputHandler<TFrame> : ReplayInputHandler
        where TFrame : ReplayFrame
    {
        private readonly Replay replay;

        protected List<ReplayFrame> Frames => replay.Frames;

        public TFrame CurrentFrame => !HasFrames ? null : (TFrame)Frames[currentFrameIndex];
        public TFrame NextFrame => !HasFrames ? null : (TFrame)Frames[nextFrameIndex];

        private int currentFrameIndex;

        private int nextFrameIndex => MathHelper.Clamp(currentFrameIndex + (currentDirection > 0 ? 1 : -1), 0, Frames.Count - 1);

        protected FramedReplayInputHandler(Replay replay)
        {
            this.replay = replay;
        }

        private bool advanceFrame()
        {
            int newFrame = nextFrameIndex;

            //ensure we aren't at an extent.
            if (newFrame == currentFrameIndex) return false;

            currentFrameIndex = newFrame;
            return true;
        }

        public override List<IInput> GetPendingInputs() => new List<IInput>();

        public bool AtLastFrame => currentFrameIndex == Frames.Count - 1;
        public bool AtFirstFrame => currentFrameIndex == 0;

        private const double max_skip_time = 1000.0 / 50;

        protected double CurrentTime { get; private set; }
        private int currentDirection;

        protected bool HasFrames => Frames.Count > 0;

        protected virtual bool IsImportant(TFrame frame) => false;

        /// <summary>
        /// Update the current frame based on an incoming time value.
        /// There are cases where we return a "must-use" time value that is different from the input.
        /// This is to ensure accurate playback of replay data.
        /// </summary>
        /// <param name="time">The time which we should use for finding the current frame.</param>
        /// <returns>The usable time value. If null, we should not advance time as we do not have enough data.</returns>
        public override double SetFrameFromTime(double time)
        {
            if (!HasFrames)
                return CurrentTime = time;

            currentDirection = time.CompareTo(CurrentTime);
            if (currentDirection == 0) currentDirection = 1;

            while (true)
            {
                // Will be the same value as the direction of playback if the next frame hasn't been reached
                int nextFrameOffset = NextFrame.Time.CompareTo(time);

                // If the direction and offset are equal, the frame will not change. Play at the requested time value.
                if (currentDirection == nextFrameOffset)
                    return CurrentTime = time;

                int currentFrameOffset = NextFrame.Time.CompareTo(CurrentTime);

                // From here, we _can_ advance the frame, but prior to doing so we need to make sure we're not skipping too far into the future
                // We can only do this if the next frame is IN-BETWEEN the current time and the requested time.
                double frameTime = currentDirection == -1 ? NextFrame.Time - 1 : NextFrame.Time;
                if (currentFrameOffset != nextFrameOffset && Math.Abs(CurrentTime - frameTime) >= max_skip_time)
                    return CurrentTime += currentDirection * max_skip_time;

                // Attempt to advance the frame, and play at the requested time value if there's no next frame
                if (!advanceFrame())
                    return CurrentTime = time;

                // If the frame is important, always play at the frame time
                if (IsImportant(CurrentFrame))
                    return CurrentTime = frameTime;
            }
        }
    }
}
