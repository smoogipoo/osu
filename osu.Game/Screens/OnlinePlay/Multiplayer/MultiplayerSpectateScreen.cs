// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Extensions;
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
        // Isolates beatmap/ruleset to this screen.
        public override bool DisallowExternalBeatmapRulesetChanges => true;

        private readonly List<Drawable> instances = new List<Drawable>();

        private readonly PlaylistItem playlistItem;
        private readonly int[] userIds;

        [Resolved]
        private UserLookupCache userLookupCache { get; set; }

        private GridContainer grid;

        public MultiplayerSpectateScreen(PlaylistItem playlistItem, int[] userIds)
        {
            this.playlistItem = playlistItem;
            this.userIds = userIds;
        }

        [BackgroundDependencyLoader]
        private void load(UserLookupCache userLookupCache)
        {
            instances.AddRange(userIds.Select(u => new PlayerCell(userLookupCache.GetUserAsync(u).Result)));

            InternalChild = new OsuScrollContainer
            {
                RelativeSizeAxes = Axes.Both,
                Child = grid = new GridContainer
                {
                    RelativeSizeAxes = Axes.X,
                    AutoSizeAxes = Axes.Y
                }
            };

            recreateGrid();
        }

        private void recreateGrid()
        {
            // Todo:
            var cells = new Drawable[4, 4];

            for (int x = 0; x < cells.GetLength(0); x++)
            {
                for (int y = 0; y < cells.GetLength(1); y++)
                    cells[x, y] = instances[y * cells.GetLength(1) + x];
            }

            grid.Content = cells.ToJagged();
        }

        private class PlayerCell : OsuScreenStack
        {
            public PlayerCell(User user)
            {
                RelativeSizeAxes = Axes.None;
                Push(new Spectator(user));
            }

            protected override void Update()
            {
                base.Update();

                Size = Parent.Parent.ChildSize;
                Scale = Vector2.Divide(Parent.ChildSize, Size);
            }
        }
    }
}
