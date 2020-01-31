// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Game.Online.Multiplayer;
using osu.Game.Screens.Multi.Match;

namespace osu.Game.Screens.Select
{
    public class MatchBeatmapDetailArea : BeatmapDetailArea
    {
        [Resolved(typeof(Room))]
        protected BindableList<PlaylistItem> Playlist { get; private set; }

        private readonly Playlist playlist;

        public MatchBeatmapDetailArea()
        {
            Add(playlist = new Playlist
            {
                RelativeSizeAxes = Axes.Both
            });
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            playlist.Items.BindTo(Playlist);
        }

        protected override void OnFilter(BeatmapDetailsAreaTabItem tab, bool selectedMods)
        {
            base.OnFilter(tab, selectedMods);

            switch (tab)
            {
                case BeatmapDetailsAreaDetailsTabItem _:
                    playlist.Hide();
                    break;

                default:
                    playlist.Show();
                    break;
            }
        }

        protected override BeatmapDetailsAreaTabItem[] CreateTabItems() => new BeatmapDetailsAreaTabItem[]
        {
            new BeatmapDetailsAreaPlaylistTabItem(),
        };
    }
}
