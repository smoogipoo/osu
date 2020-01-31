// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using NUnit.Framework;
using osu.Framework.Screens;
using osu.Game.Screens.Multi.Match;
using osu.Game.Screens.Multi.Match.Components;
using osu.Game.Screens.Select;

namespace osu.Game.Tests.Visual.Multiplayer
{
    public class TestSceneMatchSongSelect : MultiplayerTestScene
    {
        public override IReadOnlyList<Type> RequiredTypes => new[]
        {
            typeof(MatchSongSelect),
            typeof(MatchBeatmapDetailArea),
            typeof(Playlist),
            typeof(DrawablePlaylistItem)
        };

        [SetUp]
        public void Setup() => Schedule(() =>
        {
            LoadScreen(new TestMatchSongSelect());
        });

        private class TestMatchSongSelect : MatchSongSelect
        {
            public override bool OnExiting(IScreen next) => true;
        }
    }
}
