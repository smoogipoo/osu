// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Database;
using osu.Game.Graphics.Containers;
using osu.Game.Online.Rooms;
using osu.Game.Screens.Play;
using osu.Game.Users;
using osuTK;

namespace osu.Game.Screens.OnlinePlay.Multiplayer
{
    public class MultiplayerSpectateScreen : OsuScreen
    {
        private const float player_spacing = 5;

        // Isolates beatmap/ruleset to this screen.
        public override bool DisallowExternalBeatmapRulesetChanges => true;

        public bool AllPlayersLoaded => instances.All(p => p.PlayerLoaded);

        private readonly PlaylistItem playlistItem;
        private readonly int[] userIds;

        [Resolved]
        private UserLookupCache userLookupCache { get; set; }

        private readonly List<PlayerInstance> instances = new List<PlayerInstance>();

        private OsuScrollContainer scroll;
        private FillFlowContainer<PlayerFacade> flow;

        public MultiplayerSpectateScreen(PlaylistItem playlistItem, int[] userIds)
        {
            this.playlistItem = playlistItem;
            this.userIds = userIds;
        }

        [BackgroundDependencyLoader]
        private void load(UserLookupCache userLookupCache)
        {
            InternalChild = scroll = new OsuScrollContainer
            {
                RelativeSizeAxes = Axes.Both,
                Child = flow = new FillFlowContainer<PlayerFacade>
                {
                    RelativeSizeAxes = Axes.X,
                    AutoSizeAxes = Axes.Y,
                    Padding = new MarginPadding(player_spacing),
                    Spacing = new Vector2(player_spacing),
                }
            };

            for (int i = 0; i < Math.Min(16, userIds.Length); i++)
            {
                var facade = new PlayerFacade();
                var player = new PlayerInstance(userLookupCache.GetUserAsync(userIds[i]).Result, facade) { Depth = 1 };

                flow.Add(facade);
                instances.Add(player);

                AddInternal(player);
            }
        }

        protected override void Update()
        {
            base.Update();

            Vector2 cellsPerDimension;

            switch (flow.Count)
            {
                case 1:
                    cellsPerDimension = Vector2.One;
                    break;

                case 2:
                    cellsPerDimension = new Vector2(2, 1);
                    break;

                case 3:
                case 4:
                    cellsPerDimension = new Vector2(2);
                    break;

                case 5:
                case 6:
                    cellsPerDimension = new Vector2(3, 2);
                    break;

                case 7:
                case 8:
                case 9:
                    // 3 rows / 3 cols.
                    cellsPerDimension = new Vector2(3);
                    break;

                case 10:
                case 11:
                case 12:
                    // 3 rows / 4 cols.
                    cellsPerDimension = new Vector2(4, 3);
                    break;

                default:
                    // 4 rows / 4 cols.
                    cellsPerDimension = new Vector2(4);
                    break;
            }

            // Total spacing between cells
            Vector2 totalCellSpacing = player_spacing * (cellsPerDimension - Vector2.One);

            Vector2 fullSize = new Vector2(flow.ChildSize.X, scroll.ChildSize.Y) - totalCellSpacing;
            Vector2 cellSize = Vector2.Divide(fullSize, new Vector2(cellsPerDimension.X, cellsPerDimension.Y));

            foreach (var facade in flow)
            {
                facade.FullSize = fullSize;
                facade.Size = cellSize;
            }
        }

        private class PlayerFacade : Drawable
        {
            /// <summary>
            /// The size of the entire screen area.
            /// </summary>
            public Vector2 FullSize;

            public PlayerFacade()
            {
                Anchor = Anchor.Centre;
                Origin = Anchor.Centre;
            }
        }

        private class PlayerInstance : CompositeDrawable
        {
            public bool PlayerLoaded => stack.CurrentScreen is Player;

            private readonly PlayerFacade facade;
            private readonly OsuScreenStack stack;

            public PlayerInstance(User user, PlayerFacade facade)
            {
                this.facade = facade;

                Origin = Anchor.Centre;
                Masking = true;

                InternalChild = new Container
                {
                    RelativeSizeAxes = Axes.Both,
                    Child = stack = new OsuScreenStack()
                };

                stack.Push(new Spectator(user));
            }

            protected override void Update()
            {
                base.Update();

                var topLeft = Parent.ToLocalSpace(facade.ToScreenSpace(Vector2.Zero));
                Position = topLeft + facade.DrawSize / 2;

                Size = facade.FullSize;
                Scale = Vector2.Divide(facade.DrawSize, Size);
            }
        }
    }
}
