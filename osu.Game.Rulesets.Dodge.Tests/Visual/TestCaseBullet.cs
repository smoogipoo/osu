// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.Allocation;
using osu.Game.Rulesets.Dodge.Objects;
using osu.Game.Tests.Visual;

namespace osu.Game.Rulesets.Dodge.Tests.Visual
{
    public class TestCaseBullet : OsuTestCase
    {
        [BackgroundDependencyLoader]
        private void load()
        {
            Child = new DrawableBullet(new Bullet());
        }
    }
}
