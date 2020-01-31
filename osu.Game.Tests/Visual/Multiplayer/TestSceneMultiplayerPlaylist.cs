// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Utils;
using osu.Game.Beatmaps;
using osu.Game.Online.Multiplayer;
using osu.Game.Rulesets;
using osu.Game.Screens.Multi.Match;
using osuTK;

namespace osu.Game.Tests.Visual.Multiplayer
{
    public class TestSceneMultiplayerPlaylist : OsuTestScene
    {
        [Resolved]
        private RulesetStore rulesets { get; set; }

        [Resolved]
        private BeatmapManager beatmaps { get; set; }

        [SetUp]
        public void Setup() => Schedule(() =>
        {
            Playlist list;
            Add(list = new Playlist
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                Size = new Vector2(500)
            });

            List<BeatmapSetInfo> beatmapSets = beatmaps.GetAllUsableBeatmapSets().ToList();

            for (int i = 0; i < 25; i++)
            {
                list.Items.Add(new PlaylistItem
                {
                    Beatmap = beatmapSets[RNG.Next(0, beatmapSets.Count)].Beatmaps[0],
                    Ruleset = rulesets.GetRuleset(RNG.Next(0, 4))
                });
            }
        });


    }
}
