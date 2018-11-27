// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System.Collections.Generic;
using osu.Game.Beatmaps;
using osu.Game.Rulesets.Objects.Types;
using osu.Game.Rulesets.Osu.Beatmaps;
using osu.Game.Rulesets.Osu.Objects;
using osu.Game.Rulesets.Osu.Replays;
using osu.Game.Rulesets.Replays;
using osu.Game.Tests.Visual;
using osu.Game.Users;
using osuTK;

namespace osu.Game.Rulesets.Osu.Tests
{
    public class TestCaseOsuGameplayResult : GameplayResultTestCase
    {
        public TestCaseOsuGameplayResult()
            : base(new OsuRuleset())
        {
        }

        protected override Replay CreatePassingReplay(IBeatmap beatmap)
            => new OsuAutoGenerator((OsuBeatmap)beatmap).Generate();

        protected override Replay CreateFailingReplay(IBeatmap beatmap)
            => new FailingAutoGenerator((OsuBeatmap)beatmap).Generate();

        private class FailingAutoGenerator : AutoGenerator<OsuHitObject>
        {
            protected readonly Replay Replay;
            protected List<ReplayFrame> Frames => Replay.Frames;

            public FailingAutoGenerator(Beatmap<OsuHitObject> beatmap)
                : base(beatmap)
            {
                Replay = new Replay
                {
                    User = new User
                    {
                        Username = @"Autoplay",
                    }
                };
            }

            public override Replay Generate()
            {
                Frames.Add(new OsuReplayFrame(-100000, Vector2.Zero));
                Frames.Add(new OsuReplayFrame(Beatmap.HitObjects[0].StartTime - 1000, Vector2.Zero));

                foreach (var h in Beatmap.HitObjects)
                    Frames.Add(new OsuReplayFrame(((h as IHasEndTime)?.EndTime ?? h.StartTime) + h.HitWindows.HalfHittableWindow + 1, Vector2.Zero));

                Frames.Sort((f1, f2) => f1.Time.CompareTo(f2.Time));

                return Replay;
            }
        }
    }
}
