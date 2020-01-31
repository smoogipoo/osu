// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

namespace osu.Game.Screens.Select
{
    public abstract class BeatmapDetailsAreaTabItem
    {
        public abstract string Name { get; }

        public override string ToString() => Name;
    }
}
