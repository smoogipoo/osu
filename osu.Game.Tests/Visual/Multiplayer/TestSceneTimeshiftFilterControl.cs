// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Graphics;
using osu.Game.Screens.Multi.Timeshift;

namespace osu.Game.Tests.Visual.Multiplayer
{
    public class TestSceneTimeshiftFilterControl : OsuTestScene
    {
        public TestSceneTimeshiftFilterControl()
        {
            Add(new TimeshiftFilterControl
            {
                RelativeSizeAxes = Axes.X,
                Width = 0.7f,
                Height = 80,
            });
        }
    }
}
