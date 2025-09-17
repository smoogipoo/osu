// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osuTK;

namespace osu.Game.Screens.OnlinePlay.Matchmaking.Screens.Idle
{
    public partial class GridPlayerPanelLayoutContainer : PlayerPanelLayoutContainer
    {
        private readonly FillFlowContainer<PlayerPanelFacade> panels;

        public GridPlayerPanelLayoutContainer()
        {
            AutoSizeAxes = Axes.Y;

            InternalChild = panels = new FillFlowContainer<PlayerPanelFacade>
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                RelativeSizeAxes = Axes.X,
                AutoSizeAxes = Axes.Y,
                Spacing = new Vector2(20, 5)
            };
        }

        public override PlayerPanelFacade GetFacade(int index, int count)
        {
            while (index >= panels.Count)
                panels.Add(new PlayerPanelFacade(false));
            return panels[index];
        }
    }
}
