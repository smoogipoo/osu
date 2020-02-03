// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

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
        public override string Title => "SS";

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

        protected override void LoadComplete()
        {
            base.LoadComplete();

            if (Playlist.Count > 0)
                selectedItem.Value = Playlist[^1];
        }

        protected override BeatmapDetailArea CreateBeatmapDetailArea() => new MatchBeatmapDetailArea
        {
            SelectedItem = { BindTarget = selectedItem },
            CreateNewItem = createNewItem
        };

        protected override bool OnStart()
        {
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
                createNewItem();

            populate(selectedItem.Value);
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

        private void createNewItem()
        {
            PlaylistItem item = populate(new PlaylistItem());

            Playlist.Add(item);
            selectedItem.Value = item;
        }

        private PlaylistItem populate(PlaylistItem item)
        {
            item.Beatmap.Value = Beatmap.Value.BeatmapInfo;
            item.Ruleset.Value = Ruleset.Value;

            item.RequiredMods.Clear();
            item.RequiredMods.AddRange(Mods.Value);

            return item;
        }
    }
}
