// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using NUnit.Framework;
using osu.Framework.Graphics;
using osu.Game.Online.Multiplayer;
using osu.Game.Rulesets.Osu;
using osu.Game.Screens.Multi;
using osu.Game.Tests.Beatmaps;
using osuTK;

namespace osu.Game.Tests.Visual.Multiplayer
{
    public class TestSceneOverlinedPlaylist : MultiplayerTestScene
    {
        protected override bool UseOnlineAPI => true;

        public TestSceneOverlinedPlaylist()
        {
            Add(new DrawableRoomPlaylist(false, false)
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                Size = new Vector2(500),
                Items = { BindTarget = Room.Playlist }
            });
        }

        [SetUp]
        public new void Setup() => Schedule(() =>
        {
            Room.RoomID.Value = 7;

            for (int i = 0; i < 10; i++)
            {
                Room.Playlist.Add(new PlaylistItem
                {
                    ID = i,
                    Beatmap = { Value = new TestBeatmap(new OsuRuleset().RulesetInfo).BeatmapInfo },
                    Ruleset = { Value = new OsuRuleset().RulesetInfo }
                });
            }
        });
    }
}
