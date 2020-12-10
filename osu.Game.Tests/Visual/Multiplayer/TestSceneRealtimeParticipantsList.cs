// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using NUnit.Framework;
using osu.Framework.Graphics;
using osu.Framework.Testing;
using osu.Game.Online.RealtimeMultiplayer;
using osu.Game.Screens.Multi.Realtime.Participants;
using osu.Game.Users;
using osuTK;

namespace osu.Game.Tests.Visual.Multiplayer
{
    public class TestSceneRealtimeParticipantsList : RealtimeMultiplayerTestScene
    {
        [SetUp]
        public new void Setup() => Schedule(() =>
        {
            Child = new ParticipantsList
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                RelativeSizeAxes = Axes.Y,
                Size = new Vector2(380, 0.7f)
            };
        });

        [Test]
        public void TestAddUser()
        {
            AddStep("add user", () => Client.AddUser(new User
            {
                Id = 2,
                Username = "First",
                CoverUrl = @"https://osu.ppy.sh/images/headers/profile-covers/c6.jpg",
                CurrentModeRank = 1234
            }));

            AddAssert("one unique panel", () => this.ChildrenOfType<ParticipantPanel>().Select(p => p.User).Distinct().Count() == 1);

            AddStep("add user", () => Client.AddUser(new User
            {
                Id = 3,
                Username = "Second",
                CoverUrl = @"https://osu.ppy.sh/images/headers/profile-covers/c3.jpg",
            }));

            AddAssert("two unique panels", () => this.ChildrenOfType<ParticipantPanel>().Select(p => p.User).Distinct().Count() == 2);
        }

        [Test]
        public void TestRemoveUser()
        {
            User firstUser = null;
            User secondUser = null;

            AddStep("add two users", () =>
            {
                Client.AddUser(firstUser = new User
                {
                    Id = 2,
                    Username = "First",
                    CoverUrl = @"https://osu.ppy.sh/images/headers/profile-covers/c6.jpg",
                    CurrentModeRank = 1234
                });

                Client.AddUser(secondUser = new User
                {
                    Id = 3,
                    Username = "Second",
                    CoverUrl = @"https://osu.ppy.sh/images/headers/profile-covers/c3.jpg",
                });
            });

            AddStep("remove first user", () => Client.RemoveUser(firstUser));

            AddAssert("single panel is for second user", () => this.ChildrenOfType<ParticipantPanel>().Single().User.User == secondUser);
        }

        [Test]
        public void TestToggleReadyState()
        {
            User user = null;

            AddStep("add user", () => Client.AddUser(user = new User
            {
                Id = 2,
                Username = "First",
                CoverUrl = @"https://osu.ppy.sh/images/headers/profile-covers/c6.jpg",
                CurrentModeRank = 1234
            }));

            AddAssert("ready mark invisible", () => !this.ChildrenOfType<ParticipantReadyMark>().Single().IsPresent);

            AddStep("make user ready", () => Client.ChangeUserState(user, MultiplayerUserState.Ready));
            AddUntilStep("ready mark visible", () => this.ChildrenOfType<ParticipantReadyMark>().Single().IsPresent);

            AddStep("make user idle", () => Client.ChangeUserState(user, MultiplayerUserState.Idle));
            AddUntilStep("ready mark invisible", () => !this.ChildrenOfType<ParticipantReadyMark>().Single().IsPresent);
        }
    }
}
