// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Graphics;

namespace osu.Game.Screens.Multi.Match.Components
{
    public class OverlinedPlaylist : OverlinedDisplay
    {
        private readonly Playlist playlist;

        public OverlinedPlaylist()
            : base("Playlist")
        {
            Content.Add(playlist = new Playlist(false, true)
            {
                RelativeSizeAxes = Axes.Both
            });
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            playlist.Items.BindTo(Playlist);
        }
    }
}
