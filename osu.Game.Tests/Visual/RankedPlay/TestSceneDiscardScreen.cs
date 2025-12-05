// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using osu.Game.Online.Rooms;
using osu.Game.Screens.OnlinePlay.Matchmaking.RankedPlay;
using osu.Game.Tests.Visual.Matchmaking;

namespace osu.Game.Tests.Visual.RankedPlay
{
    public partial class TestSceneDiscardScreen : MatchmakingTestScene
    {
        public override void SetUpSteps()
        {
            base.SetUpSteps();

            MultiplayerPlaylistItem[] items = Enumerable.Range(0, 5).Select(id => new MultiplayerPlaylistItem { ID = id, BeatmapID = id }).ToArray();

            AddStep("add screen", () => Child = new DiscardScreen(items));
        }
    }
}
