// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Bindables;
using osu.Framework.Extensions.IEnumerableExtensions;
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

        private readonly bool allowEdit;
        private readonly bool allowSelection;

        public Playlist(bool allowEdit, bool allowSelection)
        {
            this.allowEdit = allowEdit;
            this.allowSelection = allowSelection;
        }

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

        protected override RearrangeableListItem<PlaylistItem> CreateDrawable(PlaylistItem item) => new DrawablePlaylistItem(item, allowEdit, allowSelection)
        {
            RequestSelection = requestSelection,
            RequestDeletion = requestDeletion
        };

        private void requestSelection(PlaylistItem item)
        {
            SelectedItem.Value = item;
        }

        private void requestDeletion(PlaylistItem item)
        {
            if (Items.Count == 1)
            {
                // Don't allow deletion of the only item
                return;
            }

            if (SelectedItem.Value == item)
            {
                // Attempt to select the next item in the list, falling back to the second item from the end.
                // We are guaranteed to have at least 2 items here.
                SelectedItem.Value = Items.GetNext(item) ?? Items[^2];
            }

            Items.Remove(item);
        }
    }
}
