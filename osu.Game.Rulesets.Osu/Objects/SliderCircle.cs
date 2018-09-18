﻿// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

namespace osu.Game.Rulesets.Osu.Objects
{
    public class SliderCircle : HitCircle
    {
        public SliderCircle(Slider slider)
        {
            PositionChanged += p => slider.Position = p;
        }
    }
}
