// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Specialized;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Screens;
using osu.Game.Screens.Multi.Match.Components;

namespace osu.Game.Screens.Multi.Realtime
{
    public class BeatmapSelectionControl : MultiplayerComposite
    {
        [Resolved]
        private RealtimeMatchSubScreen matchSubScreen { get; set; }

        private Container beatmapPanelContainer;

        public BeatmapSelectionControl()
        {
            AutoSizeAxes = Axes.Y;
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            InternalChild = new FillFlowContainer
            {
                RelativeSizeAxes = Axes.X,
                AutoSizeAxes = Axes.Y,
                Direction = FillDirection.Vertical,
                Children = new Drawable[]
                {
                    beatmapPanelContainer = new Container
                    {
                        RelativeSizeAxes = Axes.X,
                        AutoSizeAxes = Axes.Y
                    },
                    new PurpleTriangleButton
                    {
                        RelativeSizeAxes = Axes.X,
                        Height = 40,
                        Text = "Select beatmap",
                        Action = () => matchSubScreen.Push(new RealtimeMatchSongSelect())
                    }
                }
            };
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            Playlist.BindCollectionChanged(onPlaylistChanged, true);
        }

        private void onPlaylistChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (Playlist.Any())
                beatmapPanelContainer.Child = new DrawableRoomPlaylistItem(Playlist.Single(), false, false);
            else
                beatmapPanelContainer.Clear();
        }
    }
}
