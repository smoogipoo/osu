// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using JetBrains.Annotations;
using osu.Framework.Input.StateChanges;
using osu.Framework.Logging;
using osu.Game.Input.Handlers;
using osu.Game.Replays;
using osuTK;

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

        public TFrame StartFrame
        {
            get
            {
                if (!HasFrames)
                    return null;

                if (startFrameIndex == -1)
                    return null;

                return (TFrame)Frames[MathHelper.Clamp(startFrameIndex, 0, Frames.Count - 1)];
            }
        }

        public TFrame EndFrame
        {
            get
            {
                if (!HasFrames)
                    return null;

                if (endFrameIndex == -1)
                    return null;

                return (TFrame)Frames[MathHelper.Clamp(endFrameIndex, 0, Frames.Count - 1)];
            }
        }

        private int startFrameIndex = -1;

        private int endFrameIndex = -1;

        protected FramedReplayInputHandler(Replay replay)
        {
            this.replay = replay;
        }

        public override List<IInput> GetPendingInputs() => new List<IInput>();

        private const double sixty_frame_time = 1000.0 / 60;

        protected virtual double AllowedImportantTimeSpan => sixty_frame_time * 1.2;

        protected double? CurrentTime { get; private set; }

        /// <summary>
        /// When set, we will ensure frames executed by nested drawables are frame-accurate to replay data.
        /// Disabling this can make replay playback smoother (useful for autoplay, currently).
        /// </summary>
        public bool FrameAccuratePlayback = true;

        protected bool HasFrames => Frames.Count > 0;

        private bool inImportantSection
        {
            get
            {
                if (!HasFrames || !FrameAccuratePlayback)
                    return false;

                if (StartFrame == null)
                    return false;

                Debug.Assert(EndFrame != null);

                return IsImportant(StartFrame) && //a button is in a pressed state
                       Math.Abs(CurrentTime - EndFrame.Time ?? 0) <= AllowedImportantTimeSpan; //the next frame is within an allowable time span
            }
        }

        protected virtual bool IsImportant([NotNull] TFrame frame) => false;

        /// <summary>
        /// Update the current frame based on an incoming time value.
        /// There are cases where we return a "must-use" time value that is different from the input.
        /// This is to ensure accurate playback of replay data.
        /// </summary>
        /// <param name="time">The time which we should use for finding the current frame.</param>
        /// <returns>The usable time value. If null, we should not advance time as we do not have enough data.</returns>
        public override double? SetFrameFromTime(double time)
        {
            Logger.Log($"{nameof(SetFrameFromTime)}({time}) | CurrentTime: {CurrentTime} | StartFrame: {startFrameIndex} | EndFrame: {endFrameIndex}");

            try
            {
                // If there are no frames, always use the given time.
                if (!HasFrames)
                    return CurrentTime = time;

                // If the current time hasn't been set, this is the first invocation of this method, and we need to find the corresponding start/end frame indices.
                if (!CurrentTime.HasValue)
                {
                    Debug.Assert(startFrameIndex == -1);

                    for (int i = Frames.Count - 1; i >= 0; i--)
                    {
                        if (Frames[i].Time <= time)
                        {
                            startFrameIndex = i;
                            break;
                        }
                    }

                    endFrameIndex = startFrameIndex + 1;

                    // The start frame may be null if the given time value occurs before any frames
                    return CurrentTime = StartFrame?.Time ?? time;
                }

                Debug.Assert(startFrameIndex >= -1);
                Debug.Assert(endFrameIndex >= 0);

                switch (time.CompareTo(CurrentTime))
                {
                    case 0:
                        return CurrentTime = time;
                    case 1 when tryAdvance(time):
                        return CurrentTime;
                    case -1 when tryRewind(time):
                        return CurrentTime;
                }

                // Don't allow interpolation if inside an important section.
                if (inImportantSection)
                {
                    Logger.Log("Important section!");
                    return null;
                }

                return CurrentTime = time;
            }
            finally
            {
                Logger.Log($"CurrentTime = {CurrentTime}");
            }
        }

        private bool tryAdvance(double newTime)
        {
            // Use the given time if the current start frame is the last frame
            if (startFrameIndex == Frames.Count - 1)
            {
                CurrentTime = newTime;
                return true;
            }

            // Check if we've passed the frame boundary
            if (newTime >= EndFrame.Time)
            {
                startFrameIndex = endFrameIndex;
                endFrameIndex = startFrameIndex + 1;

                // Use the new frame boundary's current
                CurrentTime = StartFrame.Time;
                return true;
            }

            // Use the given time if the frame boundary was not passed and current start frame is null
            // It's important that this occurs _after_ the boundary is checked above
            if (startFrameIndex == -1)
            {
                CurrentTime = newTime;
                return true;
            }

            return false;
        }

        private bool tryRewind(double newTime)
        {
            // The frame boundary remains current until we're rewound _past_ the frame boundary
            // This can only occur if the current time is already at the frame boundary
            if (CurrentTime == StartFrame?.Time)
            {
                endFrameIndex = startFrameIndex;
                startFrameIndex = endFrameIndex - 1;
            }

            // Use the given time if the current start frame is null
            if (startFrameIndex == -1)
            {
                CurrentTime = newTime;
                return true;
            }

            // Guaranteed to have a start frame
            Debug.Assert(StartFrame != null);

            // Check if we've passed the frame boundary
            if (newTime <= StartFrame.Time)
            {
                CurrentTime = StartFrame.Time;
                return true;
            }

            // Use the given time if the frame boundary was not passed and the current start frame is the last frame
            // It's important that this occurs _after_ the boundary is checked above
            if (startFrameIndex == Frames.Count - 1)
            {
                CurrentTime = newTime;
                return true;
            }

            return false;
        }
    }
}
