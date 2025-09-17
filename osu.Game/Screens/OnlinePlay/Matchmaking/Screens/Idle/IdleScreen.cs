// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Screens;

namespace osu.Game.Screens.OnlinePlay.Matchmaking.Screens.Idle
{
    public partial class IdleScreen : MatchmakingSubScreen
    {
        [Resolved]
        private PlayerPanelList? panelList { get; set; }

        private GridPlayerPanelLayoutContainer panelLayout = null!;

        [BackgroundDependencyLoader]
        private void load()
        {
            InternalChild = panelLayout = new GridPlayerPanelLayoutContainer
            {
                RelativeSizeAxes = Axes.Both
            };
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();
            panelList?.SetLayout(panelLayout);
        }

        public override void OnEntering(ScreenTransitionEvent e)
        {
            base.OnEntering(e);
            this.MoveToX(0);
        }
    }
}
