// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using NUnit.Framework;
using osu.Game.Screens.Select;

namespace osu.Game.Tests.Visual.Multiplayer
{
    public class TestSceneMatchSongSelect : MultiplayerTestScene
    {
        [SetUp]
        public void Setup() => Schedule(() =>
        {
            LoadScreen(new MatchSongSelect());
        });
    }
}
