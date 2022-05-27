// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using osu.Framework.Graphics;
using osu.Game.Online.API;
using osu.Game.Online.Multiplayer;
using osu.Game.Rulesets.Osu.Mods;
using osu.Game.Rulesets.Scoring;
using osu.Game.Screens.Play.HUD;

namespace osu.Game.Tests.Visual.Multiplayer
{
    public class TestSceneMultiplayerGameplayLeaderboard : MultiplayerGameplayLeaderboardTestScene
    {
        protected override MultiplayerRoomUser CreateUser(int userId)
        {
            var user = base.CreateUser(userId);

            if (userId == TOTAL_USERS - 1)
                user.Mods = new[] { new APIMod(new OsuModNoFail()) };

            return user;
        }

        protected override MultiplayerGameplayLeaderboard CreateLeaderboard()
        {
            return new TestLeaderboard(MultiplayerUsers.ToArray())
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
            };
        }

        [Test]
        public void TestPerUserMods()
        {
            AddStep("first user has no mods", () => Assert.That(((TestLeaderboard)Leaderboard).ScoreProcessors[0].Mods.Value, Is.Empty));
            AddStep("last user has NF mod", () =>
            {
                Assert.That(((TestLeaderboard)Leaderboard).ScoreProcessors[TOTAL_USERS - 1].Mods.Value, Has.One.Items);
                Assert.That(((TestLeaderboard)Leaderboard).ScoreProcessors[TOTAL_USERS - 1].Mods.Value.Single(), Is.TypeOf<OsuModNoFail>());
            });
        }

        private class TestLeaderboard : MultiplayerGameplayLeaderboard
        {
            public readonly Dictionary<int, ScoreProcessor> ScoreProcessors = new Dictionary<int, ScoreProcessor>();

            public TestLeaderboard(MultiplayerRoomUser[] users)
                : base(users)
            {
            }
        }
    }
}
