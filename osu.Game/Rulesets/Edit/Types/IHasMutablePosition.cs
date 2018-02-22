// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using OpenTK;

namespace osu.Game.Rulesets.Edit.Types
{
    public interface IHasMutablePosition
    {
        event Action<Vector2> OnPositionChanged;

        Vector2 Position { get; set; }
    }
}
