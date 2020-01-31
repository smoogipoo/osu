// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

namespace osu.Game.Screens.Select
{
    public class BeatmapDetailsAreaLeaderboardTabItem<TScope> : BeatmapDetailsAreaTabItem
    {
        public override string Name => Scope.ToString();

        public readonly TScope Scope;

        public BeatmapDetailsAreaLeaderboardTabItem(TScope scope)
        {
            Scope = scope;
        }
    }
}
