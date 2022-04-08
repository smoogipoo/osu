// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Allocation;
using osu.Framework.Screens;
using osu.Game.Online.Multiplayer;
using osu.Game.Screens.Play;

namespace osu.Game.Screens.OnlinePlay.Multiplayer
{
    public class MultiplayerPlayerLoader : PlayerLoader
    {
        public bool GameplayPassed => player?.GameplayState.HasPassed == true;

        [Resolved]
        private MultiplayerClient multiplayerClient { get; set; }

        private Player player;

        public MultiplayerPlayerLoader(Func<Player> createPlayer)
            : base(createPlayer)
        {
        }

        protected override bool ReadyForGameplay =>
            base.ReadyForGameplay
            // The server is forcefully transitioning this user to gameplay.
            || multiplayerClient.LocalUser?.State == MultiplayerUserState.Playing
            // The server is forcefully transitioning this user back to the menu.
            || multiplayerClient.LocalUser?.State == MultiplayerUserState.Idle;

        public override void OnSuspending(IScreen next)
        {
            base.OnSuspending(next);
            player = (Player)next;
        }
    }
}
