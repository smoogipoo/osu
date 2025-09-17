// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Extensions.ObjectExtensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Online.Multiplayer;
using osu.Game.Online.Multiplayer.MatchTypes.Matchmaking;

namespace osu.Game.Screens.OnlinePlay.Matchmaking.Screens.Idle
{
    public partial class PlayerPanelList : CompositeDrawable
    {
        [Resolved]
        private MultiplayerClient client { get; set; } = null!;

        public bool Horizontal { get; init; }

        private Container<PlayerPanel> panels = null!;
        private PlayerPanelLayoutContainer? layout;

        [BackgroundDependencyLoader]
        private void load()
        {
            InternalChildren = new Drawable[]
            {
                layout = new GridPlayerPanelLayoutContainer
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    RelativeSizeAxes = Axes.X
                },
                panels = new Container<PlayerPanel>
                {
                    RelativeSizeAxes = Axes.Both
                }
            };
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            client.MatchRoomStateChanged += onRoomStateChanged;
            client.UserJoined += onUserJoined;
            client.UserLeft += onUserLeft;

            if (client.Room != null)
            {
                onRoomStateChanged(client.Room.MatchState);
                foreach (var user in client.Room.Users)
                    onUserJoined(user);
            }
        }

        public void SetLayout(PlayerPanelLayoutContainer layout)
        {
            this.layout = layout;
            updatePanelPositions();
        }

        private void onUserJoined(MultiplayerRoomUser user) => Scheduler.Add(() =>
        {
            panels.Add(new PlayerPanel(user)
            {
                Horizontal = Horizontal
            });

            updatePanelPositions();
        });

        private void onUserLeft(MultiplayerRoomUser user) => Scheduler.Add(() =>
        {
            panels.Single(p => p.RoomUser.Equals(user)).Expire();
            updatePanelPositions();
        });

        private void onRoomStateChanged(MatchRoomState? state)
            => Scheduler.Add(updatePanelPositions);

        private void updatePanelPositions()
        {
            if (client.Room?.MatchState is not MatchmakingRoomState matchmakingState)
                return;

            if (layout == null)
                return;

            PlayerPanel[] orderedPanels = panels.OrderBy(p => matchmakingState.Users[p.RoomUser.UserID].Placement).ToArray();

            for (int i = 0; i < orderedPanels.Length; i++)
                orderedPanels[i].SetFacade(layout.GetFacade(i, orderedPanels.Length));
        }

        protected override void Dispose(bool isDisposing)
        {
            base.Dispose(isDisposing);

            if (client.IsNotNull())
            {
                client.MatchRoomStateChanged -= onRoomStateChanged;
                client.UserJoined -= onUserJoined;
                client.UserLeft -= onUserLeft;
            }
        }
    }
}
