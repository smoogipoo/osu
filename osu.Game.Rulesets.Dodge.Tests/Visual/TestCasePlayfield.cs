// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using System.Collections.Generic;
using osu.Framework.Allocation;
using osu.Game.Rulesets.Dodge.UI;
using osu.Game.Tests.Visual;

namespace osu.Game.Rulesets.Dodge.Tests.Visual
{
    public class TestCasePlayfield : OsuTestCase
    {
        public override IReadOnlyList<Type> RequiredTypes => new[] { typeof(DodgePlayfield) };

        [BackgroundDependencyLoader]
        private void load()
        {
            Child = new DodgePlayfield();
        }
    }
}
