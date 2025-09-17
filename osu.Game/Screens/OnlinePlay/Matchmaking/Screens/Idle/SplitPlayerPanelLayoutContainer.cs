// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;

namespace osu.Game.Screens.OnlinePlay.Matchmaking.Screens.Idle
{
    public partial class SplitPlayerPanelLayoutContainer : PlayerPanelLayoutContainer
    {
        private readonly FillFlowContainer<PlayerPanelFacade> leftPanels;
        private readonly FillFlowContainer<PlayerPanelFacade> rightPanels;

        public SplitPlayerPanelLayoutContainer()
        {
            InternalChildren =
            [
                leftPanels = new FillFlowContainer<PlayerPanelFacade>
                {
                    Anchor = Anchor.CentreLeft,
                    Origin = Anchor.CentreLeft,
                    AutoSizeAxes = Axes.Both,
                    Direction = FillDirection.Vertical
                },
                rightPanels = new FillFlowContainer<PlayerPanelFacade>
                {
                    Anchor = Anchor.CentreRight,
                    Origin = Anchor.CentreRight,
                    AutoSizeAxes = Axes.Both,
                    Direction = FillDirection.Vertical
                }
            ];
        }

        public override PlayerPanelFacade GetFacade(int index, int count)
        {
            if (index < count / 2)
            {
                while (index >= leftPanels.Count)
                    leftPanels.Add(new PlayerPanelFacade(true));
                return leftPanels[index];
            }

            index -= count / 2;

            while (index >= rightPanels.Count)
                rightPanels.Add(new PlayerPanelFacade(true));
            return rightPanels[index];
        }
    }
}
