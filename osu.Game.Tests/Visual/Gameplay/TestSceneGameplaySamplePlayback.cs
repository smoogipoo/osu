// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using NUnit.Framework;
using osu.Framework.Graphics.Audio;
using osu.Framework.Testing;
using osu.Game.Rulesets;
using osu.Game.Rulesets.Osu;
using osu.Game.Rulesets.Osu.Objects.Drawables;
using osu.Game.Rulesets.UI;
using osu.Game.Screens.Play;
using osu.Game.Skinning;

namespace osu.Game.Tests.Visual.Gameplay
{
    public class TestSceneGameplaySamplePlayback : PlayerTestScene
    {
        [Test]
        public void TestAllSamplesStopDuringSeek()
        {
            DrawableSlider slider = null;
            DrawableSample[] samples = null;
            ISamplePlaybackDisabler gameplayClock = null;

            AddStep("get variables", () =>
            {
                gameplayClock = Player.ChildrenOfType<FrameStabilityContainer>().First().GameplayClock;
                slider = Player.ChildrenOfType<DrawableSlider>().OrderBy(s => s.HitObject.StartTime).First();
                samples = slider.ChildrenOfType<DrawableSample>().ToArray();
            });

            AddUntilStep("wait for slider sliding then seek", () =>
            {
                if (!slider.Tracking.Value)
                    return false;

                if (!samples.Any(s => s.Playing))
                    return false;

                Player.ChildrenOfType<GameplayClockContainer>().First().Seek(40000);
                return true;
            });

            AddAssert("sample playback disabled", () => gameplayClock.SamplePlaybackDisabled.Value);

            // because we are in frame stable context, it's quite likely that not all samples are "played" at this point.
            // the important thing is that at least one started, and that sample has since stopped.
            AddAssert("no samples are playing", () => Player.ChildrenOfType<PausableSkinnableSound>().All(s => !s.IsPlaying));

            AddAssert("sample playback still disabled", () => gameplayClock.SamplePlaybackDisabled.Value);

            AddUntilStep("seek finished, sample playback enabled", () => !gameplayClock.SamplePlaybackDisabled.Value);
            AddUntilStep("any sample is playing", () => Player.ChildrenOfType<PausableSkinnableSound>().Any(s => s.IsPlaying));
        }

        protected override bool Autoplay => true;

        protected override Ruleset CreatePlayerRuleset() => new OsuRuleset();
    }
}
