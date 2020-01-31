// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Graphics;
using osu.Game.Beatmaps;
using osu.Game.Screens.Select.Leaderboards;

namespace osu.Game.Screens.Select
{
    public class PlayBeatmapDetailArea : BeatmapDetailArea
    {
        public readonly BeatmapLeaderboard Leaderboard;

        public override WorkingBeatmap Beatmap
        {
            get => base.Beatmap;
            set
            {
                base.Beatmap = value;
                Leaderboard.Beatmap = value is DummyWorkingBeatmap ? null : value?.BeatmapInfo;
            }
        }

        public PlayBeatmapDetailArea()
        {
            Add(Leaderboard = new BeatmapLeaderboard
            {
                RelativeSizeAxes = Axes.Both,
            });
        }

        protected override void OnFilter(BeatmapDetailsAreaTabItem tab, bool selectedMods)
        {
            base.OnFilter(tab, selectedMods);

            Leaderboard.FilterMods = selectedMods;

            switch (tab)
            {
                case BeatmapDetailsAreaLeaderboardTabItem<BeatmapLeaderboardScope> leaderboardTab:
                    Leaderboard.Scope = leaderboardTab.Scope;
                    Leaderboard.Show();
                    break;

                default:
                    Leaderboard.Show();
                    break;
            }
        }

        protected override BeatmapDetailsAreaTabItem[] CreateTabItems() => new BeatmapDetailsAreaTabItem[]
        {
            new BeatmapDetailsAreaLeaderboardTabItem<BeatmapLeaderboardScope>(BeatmapLeaderboardScope.Local),
            new BeatmapDetailsAreaLeaderboardTabItem<BeatmapLeaderboardScope>(BeatmapLeaderboardScope.Country),
            new BeatmapDetailsAreaLeaderboardTabItem<BeatmapLeaderboardScope>(BeatmapLeaderboardScope.Global),
            new BeatmapDetailsAreaLeaderboardTabItem<BeatmapLeaderboardScope>(BeatmapLeaderboardScope.Friend),
        };
    }
}
