// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Diagnostics;
using osu.Framework.Allocation;
using osu.Framework.Logging;
using osu.Framework.Screens;
using osu.Game.Online.Multiplayer;
using osu.Game.Screens.OnlinePlay.Components;
using osu.Game.Screens.OnlinePlay.Lounge;

namespace osu.Game.Screens.OnlinePlay.Multiplayer
{
    public class Multiplayer : OnlinePlayScreen
    {
        [Resolved]
        private MultiplayerClient client { get; set; }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            client.RoomUpdated += onRoomUpdated;
            client.GameplayLoadAborted += onGameplayLoadAborted;
            onRoomUpdated();
        }

        private void onRoomUpdated()
        {
            if (client.Room == null)
                return;

            Debug.Assert(client.LocalUser != null);

            // If the user exits gameplay before score submission completes, we'll transition to idle when results has been prepared.
            if (this.IsCurrentScreen() && client.LocalUser.State == MultiplayerUserState.Results)
                client.ChangeState(MultiplayerUserState.Idle);
        }

        private void onGameplayLoadAborted()
        {
            // If the server aborts gameplay for this user (due to loading too slow), exit gameplay screens.
            if (!this.IsCurrentScreen())
            {
                Logger.Log("Gameplay aborted because this client took too long to load.", LoggingTarget.Runtime, LogLevel.Important);
                this.MakeCurrent();
            }
        }

        public override void OnResuming(IScreen last)
        {
            base.OnResuming(last);

            if (client.Room == null)
                return;

            Debug.Assert(client.LocalUser != null);

            if (!(last is MultiplayerPlayerLoader playerLoader))
                return;

            // If the server moved the client to the idle state by itself.
            // This could happen if the server aborted gameplay load.
            if (client.LocalUser.State == MultiplayerUserState.Idle)
                return;

            // If gameplay was completed, then transition to the idle state by aborting gameplay.
            if (!playerLoader.GameplayPassed)
            {
                client.AbortGameplay().FireAndForget();
                return;
            }

            // If gameplay was completed and the user went all the way to results, transition to idle here. Otherwise, the transition will happen in onRoomUpdated().
            if (client.LocalUser.State == MultiplayerUserState.Results)
                client.ChangeState(MultiplayerUserState.Idle);
        }

        protected override string ScreenTitle => "Multiplayer";

        protected override RoomManager CreateRoomManager() => new MultiplayerRoomManager();

        protected override LoungeSubScreen CreateLounge() => new MultiplayerLoungeSubScreen();

        protected override void Dispose(bool isDisposing)
        {
            base.Dispose(isDisposing);

            if (client != null)
            {
                client.RoomUpdated -= onRoomUpdated;
                client.GameplayLoadAborted -= onGameplayLoadAborted;
            }
        }
    }
}
