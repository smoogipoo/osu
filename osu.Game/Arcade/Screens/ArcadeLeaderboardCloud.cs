// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Pooling;
using osu.Framework.Utils;
using osuTK;

namespace osu.Game.Arcade.Screens
{
    public class ArcadeLeaderboardCloud : CompositeDrawable
    {
        [Resolved]
        private ArcadeClient arcadeClient { get; set; } = null!;

        private readonly CancellationTokenSource cancellationSource = new CancellationTokenSource();
        private ArcadeUserStats[] arcadeUserStats = [];

        private DrawablePool<PanelWrapper> pool = null!;
        private Container<PanelWrapper> panels = null!;

        [BackgroundDependencyLoader]
        private void load()
        {
            InternalChildren = new Drawable[]
            {
                pool = new DrawablePool<PanelWrapper>(50),
                panels = new Container<PanelWrapper>
                {
                    RelativeSizeAxes = Axes.Both
                }
            };
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            Task.Run(runLoop);
        }

        protected override void Update()
        {
            base.Update();

            foreach (var panel in panels)
            {
                if (panel.DrawPosition.X > panels.DrawWidth + 10)
                    panel.Expire();
            }

            if (arcadeUserStats.Length > 0)
            {
                int maxVictories = arcadeUserStats.Select(u => u.Victories).Max();
                bool isInitialPopulation = panels.Count == 0;

                const float min_scale = 0.4f;
                const float max_scale = 1f;

                while (panels.Count < 50)
                {
                    int index = RNG.Next(arcadeUserStats.Length);
                    ArcadeUserStats stats = arcadeUserStats[index];
                    float scale = maxVictories == 0 ? 1 : min_scale + (max_scale - min_scale) * stats.Victories / maxVictories;

                    panels.Add(pool.Get(w =>
                    {
                        if (isInitialPopulation)
                            w.Position = new Vector2(RNG.NextSingle(-200, DrawWidth), RNG.NextSingle(60, DrawHeight - ArcadeLeaderboardPanel.HEIGHT - 10));
                        else
                            w.Position = new Vector2(-200, RNG.NextSingle(60, DrawHeight - ArcadeLeaderboardPanel.HEIGHT - 10));
                        w.Scale = new Vector2(scale);
                        w.Alpha = scale;
                        w.Speed = scale;
                        w.SetUser(stats.UserId, stats.Username);
                        w.Count = stats.Victories;
                        w.Rank = index + 1;
                    }));
                }
            }

            float distance = (float)(Time.Elapsed / 100);
            foreach (var panel in panels)
                panel.X += distance / panel.Speed;
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

            arcadeUserStats = stats;
        }

        protected override void Dispose(bool isDisposing)
        {
            base.Dispose(isDisposing);

            cancellationSource.Cancel();
        }

        private class PanelWrapper : PoolableDrawable
        {
            public float Speed { get; set; } = 1;

            private ArcadeLeaderboardPanel? panel;

            public PanelWrapper()
            {
                Width = 250;
                AutoSizeAxes = Axes.Y;
            }

            public void SetUser(int userId, string username)
            {
                InternalChild = panel = new ArcadeLeaderboardPanel(userId, username)
                {
                    RelativeSizeAxes = Axes.X,
                    Count = Count,
                    Rank = Rank
                };
            }

            private int count;

            public int Count
            {
                get => count;
                set
                {
                    count = value;

                    if (panel != null)
                        panel.Count = value;
                }
            }

            private int rank;

            public int Rank
            {
                get => rank;
                set
                {
                    rank = value;

                    if (panel != null)
                        panel.Rank = value;
                }
            }
        }
    }
}
