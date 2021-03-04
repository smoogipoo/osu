// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Input.Events;
using osu.Game.Database;
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

        private Container paddingContainer;
        private FillFlowContainer<PlayerFacade> facades;
        private PlayerFacade maximisedFacade;

        // A depth value that gets decremented every time a new instance is maximised in order to reduce underlaps.
        private float maximisedInstanceDepth = 1;

        public MultiplayerSpectateScreen(PlaylistItem playlistItem, int[] userIds)
        {
            this.playlistItem = playlistItem;
            this.userIds = userIds;
        }

        [BackgroundDependencyLoader]
        private void load(UserLookupCache userLookupCache)
        {
            InternalChild = paddingContainer = new Container
            {
                RelativeSizeAxes = Axes.Both,
                Padding = new MarginPadding(player_spacing),
                Children = new Drawable[]
                {
                    new Container
                    {
                        RelativeSizeAxes = Axes.Both,
                        Child = facades = new FillFlowContainer<PlayerFacade>
                        {
                            RelativeSizeAxes = Axes.X,
                            AutoSizeAxes = Axes.Y,
                            Spacing = new Vector2(player_spacing),
                        }
                    },
                    maximisedFacade = new PlayerFacade { RelativeSizeAxes = Axes.Both }
                }
            };

            for (int i = 0; i < Math.Min(16, userIds.Length); i++)
            {
                var facade = new PlayerFacade();
                var player = new PlayerInstance(userLookupCache.GetUserAsync(userIds[i]).Result, facade)
                {
                    Depth = maximisedInstanceDepth,
                    ToggleMaximisationState = toggleMaximisationState
                };

                facades.Add(facade);
                facades.SetLayoutPosition(facade, i);

                instances.Add(player);

                AddInternal(player);
            }
        }

        protected override void Update()
        {
            base.Update();

            Vector2 cellsPerDimension;

            switch (facades.Count)
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

            Vector2 fullSize = paddingContainer.ChildSize - totalCellSpacing;
            Vector2 cellSize = Vector2.Divide(fullSize, new Vector2(cellsPerDimension.X, cellsPerDimension.Y));

            foreach (var facade in facades)
            {
                facade.FullSize = fullSize;
                facade.Size = cellSize;
            }
        }

        private void toggleMaximisationState(PlayerInstance target)
        {
            // Iterate through all instances to ensure only one is maximised at any time.
            foreach (var i in instances)
            {
                if (i == target)
                    i.IsMaximised = !i.IsMaximised;
                else
                    i.IsMaximised = false;

                if (i.IsMaximised)
                {
                    i.SetFacade(maximisedFacade);
                    ChangeInternalChildDepth(i, maximisedInstanceDepth -= 0.001f);
                }
                else
                    i.SetFacade(facades[Array.IndexOf(userIds, i.User.Id)]);
            }
        }

        public class PlayerFacade : Drawable
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

        public class PlayerInstance : CompositeDrawable
        {
            public Action<PlayerInstance> ToggleMaximisationState;

            public bool PlayerLoaded => stack.CurrentScreen is Player;
            public bool IsMaximised;

            public readonly User User;
            private readonly OsuScreenStack stack;

            private PlayerFacade facade;
            private bool isTracking = true;

            public PlayerInstance(User user, PlayerFacade facade)
            {
                User = user;
                this.facade = facade;

                Origin = Anchor.Centre;
                Masking = true;

                InternalChild = new Container
                {
                    RelativeSizeAxes = Axes.Both,
                    Child = new DrawSizePreservingFillContainer
                    {
                        RelativeSizeAxes = Axes.Both,
                        Child = stack = new OsuScreenStack()
                    }
                };

                stack.Push(new Spectator(user));
            }

            protected override void Update()
            {
                base.Update();

                if (isTracking)
                {
                    Position = getFinalPosition();
                    Size = getFinalSize();
                }
            }

            public void SetFacade([NotNull] PlayerFacade newFacade)
            {
                PlayerFacade lastFacade = facade;
                facade = newFacade;

                if (lastFacade == null || lastFacade == newFacade)
                    return;

                isTracking = false;

                this.MoveTo(getFinalPosition(), 400, Easing.OutQuint).ResizeTo(getFinalSize(), 400, Easing.OutQuint)
                    .Then()
                    .OnComplete(_ =>
                    {
                        if (facade == newFacade)
                            isTracking = true;
                    });
            }

            private Vector2 getFinalPosition()
            {
                var topLeft = Parent.ToLocalSpace(facade.ToScreenSpace(Vector2.Zero));
                return topLeft + facade.DrawSize / 2;
            }

            private Vector2 getFinalSize() => facade.DrawSize;

            // Todo: Temporary?
            protected override bool ShouldBeConsideredForInput(Drawable child) => false;

            protected override bool OnClick(ClickEvent e)
            {
                ToggleMaximisationState(this);
                return true;
            }
        }
    }
}
