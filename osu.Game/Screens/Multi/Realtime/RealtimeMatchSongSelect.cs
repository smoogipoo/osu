// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using Humanizer;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Screens;
using osu.Game.Online.Multiplayer;
using osu.Game.Screens.Select;

namespace osu.Game.Screens.Multi.Realtime
{
    public class RealtimeMatchSongSelect : SongSelect, IMultiplayerSubScreen
    {
        public string ShortTitle => "song selection";

        public override string Title => ShortTitle.Humanize();

        [Resolved(typeof(Room), nameof(Room.Playlist))]
        private BindableList<PlaylistItem> playlist { get; set; }

        protected override bool OnStart()
        {
            var item = new PlaylistItem();

            item.Beatmap.Value = Beatmap.Value.BeatmapInfo;
            item.Ruleset.Value = Ruleset.Value;

            item.RequiredMods.Clear();
            item.RequiredMods.AddRange(Mods.Value.Select(m => m.CreateCopy()));

            if (playlist.Any())
                playlist[0] = item;
            else
                playlist.Add(item);

            this.Exit();
            return true;
        }

        protected override BeatmapDetailArea CreateBeatmapDetailArea() => new PlayBeatmapDetailArea();
    }
}
