// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Game.Screens.Edit.Compose.Components;
using osuTK;

namespace osu.Game.Rulesets.Mania.Edit
{
    public class ManiaDistanceSnapGrid : DistanceSnapGrid
    {
        public ManiaDistanceSnapGrid()
            : base(Vector2.Zero, 0)
        {
        }

        protected override void CreateContent()
        {
        }

        public override (Vector2 position, double time) GetSnappedPosition(Vector2 position)
        {
        }
    }
}
