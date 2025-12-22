// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Graphics;
using osu.Game.Screens.OnlinePlay.Matchmaking.RankedPlay;
using osu.Game.Screens.OnlinePlay.Matchmaking.RankedPlay.Components;
using osu.Game.Tests.Visual.Multiplayer;

namespace osu.Game.Tests.Visual.RankedPlay
{
    public partial class TestSceneRankedPlayCornerPiece : MultiplayerTestScene
    {
        public override void SetUpSteps()
        {
            base.SetUpSteps();

            AddStep("add children", () => Children =
            [
                new RankedPlayCornerPiece(RankedPlayColourScheme.Blue, Anchor.BottomLeft)
                {
                    Child = new RankedPlayUserDisplay(1, Anchor.BottomLeft, RankedPlayColourScheme.Blue)
                    {
                        RelativeSizeAxes = Axes.Both,
                    }
                },
                new RankedPlayCornerPiece(RankedPlayColourScheme.Red, Anchor.TopRight)
                {
                    Child = new RankedPlayUserDisplay(2, Anchor.TopRight, RankedPlayColourScheme.Red)
                    {
                        RelativeSizeAxes = Axes.Both,
                    }
                },
            ]);
        }
    }
}
