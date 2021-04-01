// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using osu.Game.Online.Spectator;
using osu.Game.Screens.Play.HUD;

namespace osu.Game.Screens.OnlinePlay.Multiplayer.Spectate
{
    public class MultiplayerSpectatorLeaderboard : MultiplayerGameplayLeaderboard
    {
        private readonly PlayerInstance[] instances;
        private readonly Dictionary<int, List<TimedFrameHeader>> framesByUserId = new Dictionary<int, List<TimedFrameHeader>>();
        private readonly object framesLock = new object();

        public MultiplayerSpectatorLeaderboard(PlayerInstance[] instances, int[] userIds)
            : base(userId => instances.SingleOrDefault(i => i.User.Id == userId)?.ScoreProcessor, userIds)
        {
            this.instances = instances;
        }

        protected override void OnIncomingFrames(int userId, FrameDataBundle bundle)
        {
            lock (framesLock)
            {
                if (!framesByUserId.TryGetValue(userId, out var frames))
                    framesByUserId[userId] = frames = new List<TimedFrameHeader>();

                frames.Add(new TimedFrameHeader(bundle.Frames.First().Time, bundle.Header));
            }
        }

        protected override void Update()
        {
            base.Update();

            foreach (var p in instances)
            {
                if (p?.PlayerLoaded != true)
                    continue;

                var targetTime = p.Beatmap.Track.CurrentTime;

                lock (framesLock)
                {
                    if (!framesByUserId.TryGetValue(p.User.Id, out var frames))
                        continue;

                    int frameIndex = frames.BinarySearch(new TimedFrameHeader(targetTime));
                    if (frameIndex < 0)
                        frameIndex = ~frameIndex;
                    frameIndex = Math.Clamp(frameIndex - 1, 0, frames.Count - 1);

                    SetCurrentFrame(p.User.Id, frames[frameIndex].Header);
                }
            }
        }

        private class TimedFrameHeader : IComparable<TimedFrameHeader>
        {
            public readonly double Time;
            public readonly FrameHeader Header;

            public TimedFrameHeader(double time)
            {
                Time = time;
            }

            public TimedFrameHeader(double time, FrameHeader header)
            {
                Time = time;
                Header = header;
            }

            public int CompareTo(TimedFrameHeader other) => Time.CompareTo(other.Time);
        }
    }
}
