// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.UserInterface;
using osu.Game.Screens.OnlinePlay.Match.Components;

namespace osu.Game.Screens.OnlinePlay.Multiplayer.Match
{
    public class AddOrEditPlaylistButtons : MultiplayerRoomComposite
    {
        public Action AddBeatmap;
        public Action EditBeatmap;

        public Button AddButton { get; private set; }
        public Button EditButton { get; private set; }
        private GridContainer buttonGrid;

        public AddOrEditPlaylistButtons()
        {
            AutoSizeAxes = Axes.Y;
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            InternalChild = buttonGrid = new GridContainer
            {
                RelativeSizeAxes = Axes.X,
                Height = 40,
                ColumnDimensions = new[]
                {
                    new Dimension(),
                    new Dimension(GridSizeMode.Absolute, 5),
                    new Dimension(),
                },
                Content = new[]
                {
                    new Drawable[]
                    {
                        AddButton = new PurpleTriangleButton
                        {
                            RelativeSizeAxes = Axes.Both,
                            Height = 1,
                            Text = "Add item",
                            Action = () => AddBeatmap?.Invoke()
                        },
                        null,
                        EditButton = new PurpleTriangleButton
                        {
                            RelativeSizeAxes = Axes.Both,
                            Height = 1,
                            Text = "Edit item",
                            Action = () => EditBeatmap?.Invoke()
                        }
                    }
                }
            };
        }

        protected override void OnRoomUpdated()
        {
            base.OnRoomUpdated();

            if (Room == null)
                return;

            AddButton.Enabled.Value = Client.IsHost || QueueMode.Value != Online.Multiplayer.QueueMode.HostOnly;
            EditButton.Enabled.Value = Client.IsHost;

            buttonGrid.Alpha = AddButton.Enabled.Value || EditButton.Enabled.Value ? 1 : 0;
            buttonGrid.ColumnDimensions = new[]
            {
                new Dimension(AddButton.Enabled.Value ? GridSizeMode.Distributed : GridSizeMode.Absolute),
                new Dimension(GridSizeMode.Absolute, AddButton.Enabled.Value && EditButton.Enabled.Value ? 5 : 0),
                new Dimension(EditButton.Enabled.Value ? GridSizeMode.Distributed : GridSizeMode.Absolute)
            };
        }
    }
}
