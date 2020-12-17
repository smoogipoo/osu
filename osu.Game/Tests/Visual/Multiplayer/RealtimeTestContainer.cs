// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Screens.Multi.Lounge.Components;
using osu.Game.Screens.Multi.Realtime;

namespace osu.Game.Tests.Visual.Multiplayer
{
    public class RealtimeTestContainer : Container
    {
        protected override Container<Drawable> Content => content;
        private readonly Container content;

        [Cached(typeof(StatefulMultiplayerClient))]
        public readonly RealtimeTestMultiplayerClient Client;

        [Cached(typeof(RealtimeRoomManager))]
        public readonly RealtimeTestRoomManager RoomManager;

        [Cached]
        public readonly Bindable<FilterCriteria> Filter = new Bindable<FilterCriteria>(new FilterCriteria());

        public RealtimeTestContainer()
        {
            RelativeSizeAxes = Axes.Both;

            AddRangeInternal(new Drawable[]
            {
                Client = new RealtimeTestMultiplayerClient(),
                RoomManager = new RealtimeTestRoomManager(),
                content = new Container { RelativeSizeAxes = Axes.Both }
            });
        }
    }
}
