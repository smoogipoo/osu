// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Game.Beatmaps;
using osu.Game.Online.Multiplayer;
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

        [Resolved]
        private BeatmapManager beatmapManager { get; set; }

        private TestMatchSongSelect songSelect;

        [SetUp]
        public void Setup() => Schedule(() =>
        {
            Room.Playlist.Clear();
        });

        [Test]
        public void TestEnterWithEmptyPlaylist()
        {
            createSongSelect();

            AddUntilStep("playlist has one item", () => Room.Playlist.Count == 1);
        }

        [Test]
        public void TestEnterWithExistingPlaylist()
        {
            AddStep("add beatmap to playlist", () =>
            {
                var beatmap = beatmapManager.GetAllUsableBeatmapSets()[0].Beatmaps[0];

                Room.Playlist.Add(new PlaylistItem
                {
                    Beatmap = { Value = beatmap },
                    Ruleset = { Value = beatmap.Ruleset }
                });
            });

            createSongSelect();

            AddUntilStep("playlist has one item", () => Room.Playlist.Count == 1);
        }

        private void createSongSelect() => AddStep("create song select", () =>
        {
            LoadScreen(songSelect = new TestMatchSongSelect());
        });

        private class TestMatchSongSelect : MatchSongSelect
        {
        }
    }
}
