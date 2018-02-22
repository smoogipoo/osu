// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;

namespace osu.Game.Rulesets.Edit.Types
{
    public interface IHasMutableXPosition
    {
        event Action<float> OnXPositionChanged;

        float X { get; set; }
    }
}
