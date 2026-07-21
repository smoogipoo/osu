// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Game.Database;
using osu.Game.Graphics.Sprites;
using osu.Game.Screens;

namespace osu.Game.Arcade.Screens.RankedPlay
{
    public class RankedPlayArcadeQueueScreen : OsuScreen
    {
        [Resolved]
        private ArcadeClient arcadeClient { get; set; } = null!;

        [Resolved]
        private UserLookupCache userLookupCache { get; set; } = null!;

        private readonly BindableDictionary<int, ArcadeIdentity> connectedClients = [];

        public RankedPlayArcadeQueueScreen()
        {
            ValidForResume = false;
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            InternalChild = new OsuSpriteText
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                Text = "Waiting for other player to connect..."
            };
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            connectedClients.BindTo(arcadeClient.ConnectedClients);
            connectedClients.BindCollectionChanged(onConnectedClientsChanged, true);
        }

        private void onConnectedClientsChanged(object? sender, NotifyDictionaryChangedEventArgs<int, ArcadeIdentity> e) => Schedule(() =>
        {
            if (connectedClients.Count < 2)
                return;

            InternalChild = new OsuSpriteText
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                Text = "Continuing..."
            };
        });
    }
}
