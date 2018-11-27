// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System.Collections.Generic;
using osu.Game.Beatmaps;
using osu.Game.Rulesets.Objects.Types;
using osu.Game.Rulesets.Replays;
using osu.Game.Rulesets.Taiko.Beatmaps;
using osu.Game.Rulesets.Taiko.Objects;
using osu.Game.Rulesets.Taiko.Replays;
using osu.Game.Tests.Visual;
using osu.Game.Users;

namespace osu.Game.Rulesets.Taiko.Tests
{
    public class TestCaseTaikoGameplayResult : GameplayResultTestCase
    {
        public TestCaseTaikoGameplayResult()
            : base(new TaikoRuleset())
        {
        }

        protected override Replay CreatePassingReplay(IBeatmap beatmap)
            => new TaikoAutoGenerator((TaikoBeatmap)beatmap).Generate();

        protected override Replay CreateFailingReplay(IBeatmap beatmap)
            => new FailingAutoGenerator((TaikoBeatmap)beatmap).Generate();

        private class FailingAutoGenerator : AutoGenerator<TaikoHitObject>
        {
            protected readonly Replay Replay;
            protected List<ReplayFrame> Frames => Replay.Frames;

            public FailingAutoGenerator(Beatmap<TaikoHitObject> beatmap)
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
                Frames.Add(new TaikoReplayFrame(-100000));
                Frames.Add(new TaikoReplayFrame(Beatmap.HitObjects[0].StartTime - 1000));

                foreach (var h in Beatmap.HitObjects)
                    Frames.Add(new TaikoReplayFrame(((h as IHasEndTime)?.EndTime ?? h.StartTime) + h.HitWindows.HalfHittableWindow + 1));

                Frames.Sort((f1, f2) => f1.Time.CompareTo(f2.Time));

                return Replay;
            }
        }
    }
}
