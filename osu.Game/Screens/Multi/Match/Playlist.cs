// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Graphics.Containers;
using osu.Game.Online.Multiplayer;
using osu.Game.Screens.Multi.Match.Components;
using osuTK;

namespace osu.Game.Screens.Multi.Match
{
    public class Playlist : RearrangeableListContainer<PlaylistItem>
    {
        public readonly Bindable<PlaylistItem> SelectedItem = new Bindable<PlaylistItem>();

        protected override void LoadComplete()
        {
            base.LoadComplete();

            SelectedItem.BindValueChanged(item =>
            {
                if (item.OldValue != null)
                    ((DrawablePlaylistItem)ItemMap[item.OldValue]).Deselect();

                if (item.NewValue != null)
                    ((DrawablePlaylistItem)ItemMap[item.NewValue]).Select();
            });
        }

        protected override ScrollContainer<Drawable> CreateScrollContainer() => new OsuScrollContainer
        {
            ScrollbarVisible = false
        };

        protected override FillFlowContainer<RearrangeableListItem<PlaylistItem>> CreateListFillFlowContainer() => new FillFlowContainer<RearrangeableListItem<PlaylistItem>>
        {
            LayoutDuration = 200,
            LayoutEasing = Easing.OutQuint,
            Spacing = new Vector2(0, 2)
        };

        protected override RearrangeableListItem<PlaylistItem> CreateDrawable(PlaylistItem item) => new DrawablePlaylistItem(item)
        {
            RequestSelection = () => SelectedItem.Value = item
        };
    }
}
