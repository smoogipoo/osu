// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osuTK;

namespace osu.Game.Arcade.Screens
{
    public class ArcadeLeaderboard : CompositeDrawable
    {
        public int MaxPanels { get; set; } = int.MaxValue;

        [Resolved]
        private ArcadeClient arcadeClient { get; set; } = null!;

        private readonly CancellationTokenSource cancellationSource = new CancellationTokenSource();
        private readonly Dictionary<int, ArcadeLeaderboardPanel> panelMap = [];

        private FillFlowContainer<ArcadeLeaderboardPanel> panelFlow = [];

        public ArcadeLeaderboard()
        {
            AutoSizeAxes = Axes.Y;
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            InternalChild = panelFlow = new FillFlowContainer<ArcadeLeaderboardPanel>
            {
                RelativeSizeAxes = Axes.X,
                AutoSizeAxes = Axes.Y,
                Spacing = new Vector2(5),
                LayoutDuration = 200,
                LayoutEasing = Easing.OutQuint
            };
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            Task.Run(runLoop);
        }

        private async Task runLoop()
        {
            while (!cancellationSource.IsCancellationRequested)
            {
                try
                {
                    await Fetch().ConfigureAwait(false);
                }
                finally
                {
                    await Task.Delay(2000, cancellationSource.Token).ConfigureAwait(false);
                }
            }
        }

        public async Task Fetch()
        {
            ArcadeUserStats[] stats = await arcadeClient.FetchLeaderboard().ConfigureAwait(false);

            stats = stats
                    .OrderByDescending(s => s.Victories)
                    .ThenBy(s => s.Username)
                    .ToArray();

            Schedule(() =>
            {
                for (int i = 0; i < stats.Length; i++)
                {
                    ArcadeUserStats item = stats[i];

                    if (!panelMap.TryGetValue(item.UserId, out ArcadeLeaderboardPanel? panel))
                        panelFlow.Add(panelMap[item.UserId] = panel = new ArcadeLeaderboardPanel(item.UserId, item.Username));

                    panel.Count = item.Victories;
                    panel.Rank = i + 1;

                    panelFlow.SetLayoutPosition(panel, i);
                }

                foreach (var panel in panelFlow.ToArray())
                {
                    if (panelFlow.GetLayoutPosition(panel) >= MaxPanels)
                    {
                        panelFlow.Remove(panel, true);
                        panelMap.Remove(panel.UserId);
                    }
                }
            });
        }

        protected override void Dispose(bool isDisposing)
        {
            base.Dispose(isDisposing);

            cancellationSource.Cancel();
        }
    }
}
