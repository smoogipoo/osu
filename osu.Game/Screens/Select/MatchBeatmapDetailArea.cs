// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Graphics.UserInterface;
using osu.Game.Online.Multiplayer;
using osu.Game.Screens.Multi.Match;
using osuTK;

namespace osu.Game.Screens.Select
{
    public class MatchBeatmapDetailArea : BeatmapDetailArea
    {
        public Action CreateNewItem;

        public readonly Bindable<PlaylistItem> SelectedItem = new Bindable<PlaylistItem>();

        [Resolved(typeof(Room))]
        protected BindableList<PlaylistItem> Playlist { get; private set; }

        private readonly Drawable playlistArea;
        private readonly Playlist playlist;

        public MatchBeatmapDetailArea()
        {
            Add(playlistArea = new Container
            {
                RelativeSizeAxes = Axes.Both,
                Padding = new MarginPadding { Vertical = 10 },
                Child = new GridContainer
                {
                    RelativeSizeAxes = Axes.Both,
                    Content = new[]
                    {
                        new Drawable[]
                        {
                            new Container
                            {
                                RelativeSizeAxes = Axes.Both,
                                Padding = new MarginPadding { Bottom = 10 },
                                Child = playlist = new Playlist(true, true)
                                {
                                    RelativeSizeAxes = Axes.Both
                                }
                            }
                        },
                        new Drawable[]
                        {
                            new TriangleButton
                            {
                                Text = "create new item",
                                RelativeSizeAxes = Axes.Both,
                                Size = Vector2.One,
                                Action = () => CreateNewItem?.Invoke()
                            }
                        },
                    },
                    RowDimensions = new[]
                    {
                        new Dimension(),
                        new Dimension(GridSizeMode.Absolute, 50),
                    }
                }
            });
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            playlist.Items.BindTo(Playlist);
            playlist.SelectedItem.BindTo(SelectedItem);
        }

        protected override void OnFilter(BeatmapDetailsAreaTabItem tab, bool selectedMods)
        {
            base.OnFilter(tab, selectedMods);

            switch (tab)
            {
                case BeatmapDetailsAreaDetailsTabItem _:
                    playlistArea.Hide();
                    break;

                default:
                    playlistArea.Show();
                    break;
            }
        }

        protected override BeatmapDetailsAreaTabItem[] CreateTabItems() => new BeatmapDetailsAreaTabItem[]
        {
            new BeatmapDetailsAreaPlaylistTabItem(),
        };
    }
}
