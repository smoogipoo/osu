// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using NUnit.Framework;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Rulesets.Scoring;
using osu.Game.Rulesets.Taiko.Objects;
using osu.Game.Rulesets.Taiko.Objects.Drawables;
using osu.Game.Rulesets.Taiko.UI;

namespace osu.Game.Rulesets.Taiko.Tests.Skinning
{
    [TestFixture]
    public class TestSceneHitExplosion : TaikoSkinnableTestScene
    {
        [Test]
        public void TestNormalHit()
        {
            AddStep("Great", () => SetContents(() => getContentFor(createHit(HitResult.Great))));
            AddStep("Good", () => SetContents(() => getContentFor(createHit(HitResult.Good))));
            AddStep("Miss", () => SetContents(() => getContentFor(createHit(HitResult.Miss))));
        }

        [Test]
        public void TestStrongHit([Values(false, true)] bool hitBoth)
        {
            AddStep("Great", () => SetContents(() => getContentFor(createStrongHit(HitResult.Great, hitBoth))));
            AddStep("Good", () => SetContents(() => getContentFor(createStrongHit(HitResult.Good, hitBoth))));
        }

        private Drawable getContentFor(DrawableTaikoHitObject hit)
        {
            return new Container
            {
                RelativeSizeAxes = Axes.Both,
                Children = new Drawable[]
                {
                    hit,
                    new HitExplosion(hit)
                    {
                        Anchor = Anchor.Centre,
                        Origin = Anchor.Centre,
                    }
                }
            };
        }

        private DrawableTaikoHitObject createHit(HitResult type) => new DrawableTestHit(new Hit { StartTime = Time.Current }, type);

        private DrawableTaikoHitObject createStrongHit(HitResult type, bool hitBoth)
            => new DrawableTestStrongHit(Time.Current, type, hitBoth);
    }
}
