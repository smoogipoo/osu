// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using Humanizer;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Screens;
using osu.Game.Beatmaps;
using osu.Game.Online.Multiplayer;
using osu.Game.Screens.Multi;

namespace osu.Game.Screens.Select
{
    public class MatchSongSelect : SongSelect, IMultiplayerSubScreen
    {
        public string ShortTitle => "song selection";
        public override string Title => ShortTitle.Humanize();

        [Resolved(typeof(Room))]
        protected BindableList<PlaylistItem> Playlist { get; private set; }

        private readonly Bindable<PlaylistItem> selectedItem = new Bindable<PlaylistItem>();

        public override bool AllowEditing => false;

        [Resolved]
        private BeatmapManager beatmaps { get; set; }

        public MatchSongSelect()
        {
            Padding = new MarginPadding { Horizontal = HORIZONTAL_OVERFLOW_PADDING };
        }

        protected override BeatmapDetailArea CreateBeatmapDetailArea() => new MatchBeatmapDetailArea
        {
            SelectedItem = { BindTarget = selectedItem },
            AddNewItem = addNewItem
        };

        private void addNewItem()
        {
            PlaylistItem item = populate(new PlaylistItem());

            Playlist.Add(item);
            selectedItem.Value = item;
        }

        private PlaylistItem populate(PlaylistItem item)
        {
            item.Beatmap = Beatmap.Value.BeatmapInfo;
            item.Ruleset = Ruleset.Value;

            item.RequiredMods.Clear();
            item.RequiredMods.AddRange(Mods.Value);

            return item;
        }

        protected override bool OnStart()
        {
            populate(selectedItem.Value);

            if (this.IsCurrentScreen())
                this.Exit();

            return true;
        }

        public override void OnEntering(IScreen last)
        {
            base.OnEntering(last);

            Beatmap.Disabled = false;
            Ruleset.Disabled = false;
            Mods.Disabled = false;
        }

        protected override void UpdateBeatmap(WorkingBeatmap beatmap)
        {
            base.UpdateBeatmap(beatmap);

            if (Playlist.Count == 0)
                addNewItem();

            selectedItem.Value = Playlist[^1];
        }

        public override bool OnExiting(IScreen next)
        {
            if (base.OnExiting(next))
                return true;

            Beatmap.Disabled = true;
            Ruleset.Disabled = true;
            Mods.Disabled = true;

            return false;
        }
    }
}
