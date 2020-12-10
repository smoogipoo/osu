// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Diagnostics;
using System.Linq;
using JetBrains.Annotations;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Game.Graphics;
using osu.Game.Graphics.UserInterface;
using osu.Game.Online.API;
using osu.Game.Online.RealtimeMultiplayer;
using osuTK;

namespace osu.Game.Screens.Multi.Realtime
{
    public class ReadyButton : MatchComposite
    {
        [Resolved]
        private IAPIProvider api { get; set; }

        [CanBeNull]
        private MultiplayerRoomUser localUser;

        [Resolved]
        private OsuColour colours { get; set; }

        private OsuButton button;

        [BackgroundDependencyLoader]
        private void load()
        {
            InternalChild = button = new OsuButton
            {
                RelativeSizeAxes = Axes.Both,
                Size = Vector2.One,
                Action = onClick
            };
        }

        protected override void OnRoomChanged()
        {
            base.OnRoomChanged();

            localUser = Room?.Users.Single(u => u.User?.Id == api.LocalUser.Value.Id);
            updateState();
        }

        private void updateState()
        {
            if (localUser == null)
                return;

            Debug.Assert(Room != null);

            switch (localUser.State)
            {
                case MultiplayerUserState.Idle:
                    button.Text = "Ready";
                    button.BackgroundColour = colours.Green;
                    break;

                case MultiplayerUserState.Ready:
                    if (Room?.Host?.Equals(localUser) == true)
                    {
                        button.Text = "Let's go!";

                        button.BackgroundColour = Room.Users.All(u => u.State == MultiplayerUserState.Ready)
                            ? colours.Green
                            : colours.YellowDark;
                    }
                    else
                    {
                        button.Text = "Waiting for host...";
                        button.BackgroundColour = colours.YellowDark;
                    }

                    break;
            }
        }

        private void onClick()
        {
            if (localUser == null)
                return;

            if (localUser.State == MultiplayerUserState.Idle)
                Client.ChangeState(MultiplayerUserState.Ready);
            else
            {
                if (Room?.Host?.Equals(localUser) == true)
                    Client.StartMatch();
                else
                    Client.ChangeState(MultiplayerUserState.Idle);
            }
        }
    }
}
