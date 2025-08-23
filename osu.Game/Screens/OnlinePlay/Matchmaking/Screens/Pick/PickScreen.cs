// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Extensions.ObjectExtensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Online.API;
using osu.Game.Online.Multiplayer;
using osu.Game.Online.Rooms;

namespace osu.Game.Screens.OnlinePlay.Matchmaking.Screens.Pick
{
    public partial class PickScreen : OsuScreen
    {
        private BeatmapSelectionGrid selectionGrid = null!;

        [Resolved]
        private MultiplayerClient client { get; set; } = null!;

        [Resolved]
        private IAPIProvider api { get; set; } = null!;

        [BackgroundDependencyLoader]
        private void load()
        {
            InternalChild = new Container
            {
                RelativeSizeAxes = Axes.Both,
                Child = selectionGrid = new BeatmapSelectionGrid
                {
                    RelativeSizeAxes = Axes.Both,
                },
            };
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            client.ItemAdded += onItemAdded;
            client.ItemChanged += onItemChanged;
            client.ItemRemoved += onItemRemoved;

            foreach (var item in client.Room!.Playlist)
                onItemAdded(item);

            selectionGrid.ItemSelected += item => client.MatchmakingToggleSelection(item.ID);

            client.MatchmakingItemSelected += onItemSelected;
            client.MatchmakingItemDeselected += onItemDeselected;
        }

        private void onItemAdded(MultiplayerPlaylistItem item) => Scheduler.Add(() => selectionGrid.AddItem(item));

        private void onItemChanged(MultiplayerPlaylistItem item) => Scheduler.Add(() =>
        {
            if (item.Expired)
                selectionGrid.RemoveItem(item.ID);
        });

        private void onItemSelected(int userId, long itemId)
        {
            var user = client.Room!.Users.First(it => it.UserID == userId).User!;
            selectionGrid.SetUserSelection(user, itemId, true);
        }

        private void onItemDeselected(int userId, long itemId)
        {
            var user = client.Room!.Users.First(it => it.UserID == userId).User!;
            selectionGrid.SetUserSelection(user, itemId, false);
        }

        private void onItemRemoved(long itemId) => Scheduler.Add(() => selectionGrid.RemoveItem(itemId));

        public void RollFinalBeatmap(long[] candidateItems, long finalItem) => selectionGrid.RollAndDisplayFinalBeatmap(candidateItems, finalItem);

        protected override void Dispose(bool isDisposing)
        {
            base.Dispose(isDisposing);

            if (client.IsNotNull())
            {
                client.ItemAdded -= onItemAdded;
                client.ItemChanged -= onItemChanged;
                client.ItemRemoved -= onItemRemoved;
            }
        }
    }
}
