// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using NUnit.Framework;
using osu.Framework.Extensions.ObjectExtensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Testing;
using osu.Framework.Utils;
using osu.Game.Graphics.UserInterface;
using osu.Game.Online;
using osu.Game.Online.Multiplayer;
using osu.Game.Online.Rooms;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Osu.Mods;
using osu.Game.Screens.OnlinePlay.Multiplayer.Participants;
using osuTK;

namespace osu.Game.Tests.Visual.Multiplayer
{
    public class TestSceneMultiplayerParticipantsList : MultiplayerTestScene
    {
        [SetUpSteps]
        public void SetupSteps()
        {
            createNewParticipantsList();
        }

        [Test]
        public void TestAddUser()
        {
            AddAssert("one unique panel", () => this.ChildrenOfType<ParticipantPanel>().Select(p => p.User).Distinct().Count() == 1);

            AddStep("add user", () => MultiplayerServer.AddUser(3));

            AddAssert("two unique panels", () => this.ChildrenOfType<ParticipantPanel>().Select(p => p.User).Distinct().Count() == 2);
        }

        [Test]
        public void TestAddUnresolvedUser()
        {
            AddAssert("one unique panel", () => this.ChildrenOfType<ParticipantPanel>().Select(p => p.User).Distinct().Count() == 1);

            AddStep("add non-resolvable user", () => MultiplayerServer.TestAddUnresolvedUser());
            AddAssert("null user added", () => MultiplayerClient.Room.AsNonNull().Users.Count(u => u.User == null) == 1);

            AddUntilStep("two unique panels", () => this.ChildrenOfType<ParticipantPanel>().Select(p => p.User).Distinct().Count() == 2);

            AddStep("kick null user", () => this.ChildrenOfType<ParticipantPanel>().Single(p => p.User.User == null)
                                                .ChildrenOfType<ParticipantPanel.KickButton>().Single().TriggerClick());

            AddAssert("null user kicked", () => MultiplayerClient.Room.AsNonNull().Users.Count == 1);
        }

        [Test]
        public void TestRemoveUser()
        {
            AddStep("add a user", () => MultiplayerServer.AddUser(3));

            AddStep("remove host", () => MultiplayerServer.RemoveUser(API.LocalUser.Value.Id));

            AddAssert("single panel is for second user", () => this.ChildrenOfType<ParticipantPanel>().Single().User.UserID == 3);
        }

        [Test]
        public void TestGameStateHasPriorityOverDownloadState()
        {
            AddStep("set to downloading map", () => MultiplayerServer.ChangeBeatmapAvailability(BeatmapAvailability.Downloading(0)));
            checkProgressBarVisibility(true);

            AddStep("make user ready", () => MultiplayerServer.ChangeState(MultiplayerUserState.Results));
            checkProgressBarVisibility(false);
            AddUntilStep("ready mark visible", () => this.ChildrenOfType<StateDisplay>().Single().IsPresent);

            AddStep("make user ready", () => MultiplayerServer.ChangeState(MultiplayerUserState.Idle));
            checkProgressBarVisibility(true);
        }

        [Test]
        public void TestCorrectInitialState()
        {
            AddStep("set to downloading map", () => MultiplayerServer.ChangeBeatmapAvailability(BeatmapAvailability.Downloading(0)));
            createNewParticipantsList();
            checkProgressBarVisibility(true);
        }

        [Test]
        public void TestBeatmapDownloadingStates()
        {
            AddStep("set to no map", () => MultiplayerServer.ChangeBeatmapAvailability(BeatmapAvailability.NotDownloaded()));
            AddStep("set to downloading map", () => MultiplayerServer.ChangeBeatmapAvailability(BeatmapAvailability.Downloading(0)));

            checkProgressBarVisibility(true);

            AddRepeatStep("increment progress", () =>
            {
                float progress = this.ChildrenOfType<ParticipantPanel>().Single().User.BeatmapAvailability.DownloadProgress ?? 0;
                MultiplayerServer.ChangeBeatmapAvailability(BeatmapAvailability.Downloading(progress + RNG.NextSingle(0.1f)));
            }, 25);

            AddAssert("progress bar increased", () => this.ChildrenOfType<ProgressBar>().Single().Current.Value > 0);

            AddStep("set to importing map", () => MultiplayerServer.ChangeBeatmapAvailability(BeatmapAvailability.Importing()));
            checkProgressBarVisibility(false);

            AddStep("set to available", () => MultiplayerServer.ChangeBeatmapAvailability(BeatmapAvailability.LocallyAvailable()));
        }

        [Test]
        public void TestToggleReadyState()
        {
            AddAssert("ready mark invisible", () => !this.ChildrenOfType<StateDisplay>().Single().IsPresent);

            AddStep("make user ready", () => MultiplayerServer.ChangeState(MultiplayerUserState.Ready));
            AddUntilStep("ready mark visible", () => this.ChildrenOfType<StateDisplay>().Single().IsPresent);

            AddStep("make user idle", () => MultiplayerServer.ChangeState(MultiplayerUserState.Idle));
            AddUntilStep("ready mark invisible", () => !this.ChildrenOfType<StateDisplay>().Single().IsPresent);
        }

        [Test]
        public void TestToggleSpectateState()
        {
            AddStep("make user spectating", () => MultiplayerServer.ChangeState(MultiplayerUserState.Spectating));
            AddStep("make user idle", () => MultiplayerServer.ChangeState(MultiplayerUserState.Idle));
        }

        [Test]
        public void TestCrownChangesStateWhenHostTransferred()
        {
            AddStep("add user", () => MultiplayerServer.AddUser(3));

            AddUntilStep("first user crown visible", () => this.ChildrenOfType<ParticipantPanel>().ElementAt(0).ChildrenOfType<SpriteIcon>().First().Alpha == 1);
            AddUntilStep("second user crown hidden", () => this.ChildrenOfType<ParticipantPanel>().ElementAt(1).ChildrenOfType<SpriteIcon>().First().Alpha == 0);

            AddStep("make second user host", () => MultiplayerServer.TransferHost(3));

            AddUntilStep("first user crown hidden", () => this.ChildrenOfType<ParticipantPanel>().ElementAt(0).ChildrenOfType<SpriteIcon>().First().Alpha == 0);
            AddUntilStep("second user crown visible", () => this.ChildrenOfType<ParticipantPanel>().ElementAt(1).ChildrenOfType<SpriteIcon>().First().Alpha == 1);
        }

        [Test]
        public void TestHostGetsPinnedToTop()
        {
            AddStep("add user", () => MultiplayerServer.AddUser(3));

            AddStep("make second user host", () => MultiplayerServer.TransferHost(3));
            AddAssert("second user above first", () =>
            {
                var first = this.ChildrenOfType<ParticipantPanel>().ElementAt(0);
                var second = this.ChildrenOfType<ParticipantPanel>().ElementAt(1);
                return second.Y < first.Y;
            });
        }

        [Test]
        public void TestKickButtonOnlyPresentWhenHost()
        {
            AddStep("add user", () => MultiplayerServer.AddUser(3));

            AddUntilStep("kick buttons visible", () => this.ChildrenOfType<ParticipantPanel.KickButton>().Count(d => d.IsPresent) == 1);

            AddStep("make second user host", () => MultiplayerServer.TransferHost(3));

            AddUntilStep("kick buttons not visible", () => this.ChildrenOfType<ParticipantPanel.KickButton>().Count(d => d.IsPresent) == 0);

            AddStep("make local user host again", () => MultiplayerServer.TransferHost(API.LocalUser.Value.Id));

            AddUntilStep("kick buttons visible", () => this.ChildrenOfType<ParticipantPanel.KickButton>().Count(d => d.IsPresent) == 1);
        }

        [Test]
        public void TestKickButtonKicks()
        {
            AddStep("add user", () => MultiplayerServer.AddUser(3));

            AddStep("kick second user", () => this.ChildrenOfType<ParticipantPanel.KickButton>().Single(d => d.IsPresent).TriggerClick());

            AddAssert("second user kicked", () => MultiplayerClient.Room?.Users.Single().UserID == API.LocalUser.Value.Id);
        }

        [Test]
        public void TestManyUsers()
        {
            const int users_count = 20;

            AddStep("add many users", () =>
            {
                for (int i = 0; i < users_count; i++)
                {
                    MultiplayerServer.AddUser(i);

                    MultiplayerServer.ChangeUserState(i, (MultiplayerUserState)RNG.Next(0, (int)MultiplayerUserState.Results + 1));

                    if (RNG.NextBool())
                    {
                        var beatmapState = (DownloadState)RNG.Next(0, (int)DownloadState.LocallyAvailable + 1);

                        switch (beatmapState)
                        {
                            case DownloadState.NotDownloaded:
                                MultiplayerServer.ChangeUserBeatmapAvailability(i, BeatmapAvailability.NotDownloaded());
                                break;

                            case DownloadState.Downloading:
                                MultiplayerServer.ChangeUserBeatmapAvailability(i, BeatmapAvailability.Downloading(RNG.NextSingle()));
                                break;

                            case DownloadState.Importing:
                                MultiplayerServer.ChangeUserBeatmapAvailability(i, BeatmapAvailability.Importing());
                                break;
                        }
                    }
                }
            });

            AddRepeatStep("switch hosts", () => MultiplayerServer.TransferHost(RNG.Next(0, users_count)), 10);
            AddStep("give host back", () => MultiplayerServer.TransferHost(API.LocalUser.Value.Id));
        }

        [Test]
        public void TestUserWithMods()
        {
            AddStep("add user", () =>
            {
                MultiplayerServer.AddUser(0);

                MultiplayerServer.ChangeUserMods(0, new Mod[]
                {
                    new OsuModHardRock(),
                    new OsuModDifficultyAdjust { ApproachRate = { Value = 1 } }
                });
            });

            for (var i = MultiplayerUserState.Idle; i < MultiplayerUserState.Results; i++)
            {
                var state = i;
                AddStep($"set state: {state}", () => MultiplayerServer.ChangeUserState(0, state));
            }

            AddStep("set state: downloading", () => MultiplayerServer.ChangeUserBeatmapAvailability(0, BeatmapAvailability.Downloading(0)));

            AddStep("set state: locally available", () => MultiplayerServer.ChangeUserBeatmapAvailability(0, BeatmapAvailability.LocallyAvailable()));
        }

        [Test]
        public void TestModOverlap()
        {
            AddStep("add dummy mods", () =>
            {
                MultiplayerServer.ChangeUserMods(0, new Mod[]
                {
                    new OsuModNoFail(),
                    new OsuModDoubleTime()
                });
            });

            AddStep("add user with mods", () =>
            {
                MultiplayerServer.AddUser(0);
                MultiplayerServer.ChangeUserMods(0, new Mod[]
                {
                    new OsuModHardRock(),
                    new OsuModDoubleTime()
                });
            });

            AddStep("set 0 ready", () => MultiplayerServer.ChangeState(MultiplayerUserState.Ready));

            AddStep("set 1 spectate", () => MultiplayerServer.ChangeUserState(0, MultiplayerUserState.Spectating));

            // Have to set back to idle due to status priority.
            AddStep("set 0 no map, 1 ready", () =>
            {
                MultiplayerServer.ChangeState(MultiplayerUserState.Idle);
                MultiplayerServer.ChangeBeatmapAvailability(BeatmapAvailability.NotDownloaded());
                MultiplayerServer.ChangeUserState(0, MultiplayerUserState.Ready);
            });

            AddStep("set 0 downloading", () => MultiplayerServer.ChangeBeatmapAvailability(BeatmapAvailability.Downloading(0)));

            AddStep("set 0 spectate", () => MultiplayerServer.ChangeUserState(0, MultiplayerUserState.Spectating));

            AddStep("make both default", () =>
            {
                MultiplayerServer.ChangeBeatmapAvailability(BeatmapAvailability.LocallyAvailable());
                MultiplayerServer.ChangeUserState(0, MultiplayerUserState.Idle);
                MultiplayerServer.ChangeState(MultiplayerUserState.Idle);
            });
        }

        private void createNewParticipantsList()
        {
            ParticipantsList participantsList = null;

            AddStep("create new list", () => Child = participantsList = new ParticipantsList
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                RelativeSizeAxes = Axes.Y,
                Size = new Vector2(380, 0.7f)
            });

            AddUntilStep("wait for list to load", () => participantsList.IsLoaded);
        }

        private void checkProgressBarVisibility(bool visible) =>
            AddUntilStep($"progress bar {(visible ? "is" : "is not")}visible", () =>
                this.ChildrenOfType<ProgressBar>().Single().IsPresent == visible);
    }
}
