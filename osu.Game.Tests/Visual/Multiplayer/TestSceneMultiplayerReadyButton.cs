// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Linq;
using System.Threading.Tasks;
using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Audio;
using osu.Framework.Bindables;
using osu.Framework.Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Cursor;
using osu.Framework.Platform;
using osu.Framework.Testing;
using osu.Framework.Utils;
using osu.Game.Beatmaps;
using osu.Game.Graphics.UserInterface;
using osu.Game.Online.API.Requests.Responses;
using osu.Game.Online.Multiplayer;
using osu.Game.Online.Multiplayer.Countdown;
using osu.Game.Online.Rooms;
using osu.Game.Rulesets;
using osu.Game.Screens.OnlinePlay.Multiplayer.Match;
using osu.Game.Tests.Resources;
using osuTK;
using osuTK.Input;

namespace osu.Game.Tests.Visual.Multiplayer
{
    public class TestSceneMultiplayerReadyButton : MultiplayerTestScene
    {
        private MultiplayerReadyButton button;
        private BeatmapSetInfo importedSet;

        private readonly Bindable<PlaylistItem> selectedItem = new Bindable<PlaylistItem>();

        private BeatmapManager beatmaps;
        private RulesetStore rulesets;

        private IDisposable readyClickOperation;

        [BackgroundDependencyLoader]
        private void load(GameHost host, AudioManager audio)
        {
            Dependencies.Cache(rulesets = new RealmRulesetStore(Realm));
            Dependencies.Cache(beatmaps = new BeatmapManager(LocalStorage, Realm, rulesets, null, audio, Resources, host, Beatmap.Default));
            Dependencies.Cache(Realm);
        }

        [SetUp]
        public new void Setup() => Schedule(() =>
        {
            AvailabilityTracker.SelectedItem.BindTo(selectedItem);

            beatmaps.Import(TestResources.GetQuickTestBeatmapForImport()).WaitSafely();
            importedSet = beatmaps.GetAllUsableBeatmapSets().First();
            Beatmap.Value = beatmaps.GetWorkingBeatmap(importedSet.Beatmaps.First());

            selectedItem.Value = new PlaylistItem(Beatmap.Value.BeatmapInfo)
            {
                RulesetID = Beatmap.Value.BeatmapInfo.Ruleset.OnlineID
            };

            Child = new PopoverContainer
            {
                RelativeSizeAxes = Axes.Both,
                Child = button = new MultiplayerReadyButton
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    Size = new Vector2(200, 50),
                    OnReadyClick = delay =>
                    {
                        readyClickOperation = OngoingOperationTracker.BeginOperation();

                        Task.Run(async () =>
                        {
                            if (MultiplayerClient.IsHost && MultiplayerClient.LocalUser?.State == MultiplayerUserState.Ready)
                            {
                                if (delay == null)
                                    await MultiplayerClient.StartMatch();
                                else
                                    await MultiplayerClient.SendMatchRequest(new MatchStartCountdownRequest { Delay = delay.Value });
                            }
                            else
                                await MultiplayerClient.ToggleReady();

                            readyClickOperation.Dispose();
                        });
                    },
                    OnCancelCountdown = () =>
                    {
                        readyClickOperation = OngoingOperationTracker.BeginOperation();

                        Task.Run(async () =>
                        {
                            await MultiplayerClient.SendMatchRequest(new StopCountdownRequest());
                            readyClickOperation.Dispose();
                        });
                    }
                }
            };
        });

        [Test]
        public void TestStartWithCountDown()
        {
            ClickButtonWhenEnabled<MultiplayerReadyButton>();
            AddUntilStep("countdown button shown", () => this.ChildrenOfType<MultiplayerReadyButton.CountdownButton>().SingleOrDefault()?.IsPresent == true);
            ClickButtonWhenEnabled<MultiplayerReadyButton.CountdownButton>();
            AddStep("click the first countdown button", () =>
            {
                var popoverButton = this.ChildrenOfType<MultiplayerReadyButton.CountdownButton.PopoverButton>().First();
                InputManager.MoveMouseTo(popoverButton);
                InputManager.Click(MouseButton.Left);
            });

            AddAssert("countdown button not visible", () => !this.ChildrenOfType<MultiplayerReadyButton.CountdownButton>().Single().IsPresent);
            AddStep("finish countdown", () => MultiplayerClient.FinishCountDown());
            AddUntilStep("match started", () => MultiplayerClient.LocalUser?.State == MultiplayerUserState.WaitingForLoad);
        }

        [Test]
        public void TestCancelCountdown()
        {
            ClickButtonWhenEnabled<MultiplayerReadyButton>();
            AddUntilStep("countdown button shown", () => this.ChildrenOfType<MultiplayerReadyButton.CountdownButton>().SingleOrDefault()?.IsPresent == true);
            ClickButtonWhenEnabled<MultiplayerReadyButton.CountdownButton>();
            AddStep("click the first countdown button", () =>
            {
                var popoverButton = this.ChildrenOfType<MultiplayerReadyButton.CountdownButton.PopoverButton>().First();
                InputManager.MoveMouseTo(popoverButton);
                InputManager.Click(MouseButton.Left);
            });

            ClickButtonWhenEnabled<MultiplayerReadyButton>();

            AddStep("finish countdown", () => MultiplayerClient.FinishCountDown());
            AddUntilStep("match not started", () => MultiplayerClient.LocalUser?.State == MultiplayerUserState.Ready);
        }

        [Test]
        public void TestReadyAndUnReadyDuringCountdown()
        {
            AddStep("add second user as host", () =>
            {
                MultiplayerClient.AddUser(new APIUser { Id = 2, Username = "Another user" });
                MultiplayerClient.TransferHost(2);
            });

            AddStep("start with countdown", () => MultiplayerClient.SendMatchRequest(new MatchStartCountdownRequest { Delay = TimeSpan.FromMinutes(2) }));

            ClickButtonWhenEnabled<MultiplayerReadyButton>();
            AddUntilStep("user is ready", () => MultiplayerClient.Room?.Users[0].State == MultiplayerUserState.Ready);

            ClickButtonWhenEnabled<MultiplayerReadyButton>();
            AddUntilStep("user is idle", () => MultiplayerClient.Room?.Users[0].State == MultiplayerUserState.Idle);
        }

        [Test]
        public void TestCountdownButtonEnablementAndVisibilityWhileSpectating()
        {
            AddStep("set spectating", () => MultiplayerClient.ChangeUserState(API.LocalUser.Value.OnlineID, MultiplayerUserState.Spectating));
            AddUntilStep("local user is spectating", () => MultiplayerClient.LocalUser?.State == MultiplayerUserState.Spectating);

            AddAssert("countdown button is visible", () => this.ChildrenOfType<MultiplayerReadyButton.CountdownButton>().Single().IsPresent);
            AddAssert("countdown button disabled", () => !this.ChildrenOfType<MultiplayerReadyButton.CountdownButton>().Single().Enabled.Value);

            AddStep("add second user", () => MultiplayerClient.AddUser(new APIUser { Id = 2, Username = "Another user" }));
            AddAssert("countdown button disabled", () => !this.ChildrenOfType<MultiplayerReadyButton.CountdownButton>().Single().Enabled.Value);

            AddStep("set second user ready", () => MultiplayerClient.ChangeUserState(2, MultiplayerUserState.Ready));
            AddAssert("countdown button enabled", () => this.ChildrenOfType<MultiplayerReadyButton.CountdownButton>().Single().Enabled.Value);
        }

        [Test]
        public void TestSpectatingDuringCountdownWithNoReadyUsersCancelsCountdown()
        {
            ClickButtonWhenEnabled<MultiplayerReadyButton>();
            AddUntilStep("countdown button shown", () => this.ChildrenOfType<MultiplayerReadyButton.CountdownButton>().SingleOrDefault()?.IsPresent == true);
            ClickButtonWhenEnabled<MultiplayerReadyButton.CountdownButton>();
            AddStep("click the first countdown button", () =>
            {
                var popoverButton = this.ChildrenOfType<MultiplayerReadyButton.CountdownButton.PopoverButton>().First();
                InputManager.MoveMouseTo(popoverButton);
                InputManager.Click(MouseButton.Left);
            });

            AddStep("set spectating", () => MultiplayerClient.ChangeUserState(API.LocalUser.Value.OnlineID, MultiplayerUserState.Spectating));
            AddUntilStep("local user is spectating", () => MultiplayerClient.LocalUser?.State == MultiplayerUserState.Spectating);

            AddStep("finish countdown", () => MultiplayerClient.FinishCountDown());
            AddUntilStep("match not started", () => MultiplayerClient.Room?.State == MultiplayerRoomState.Open);
        }

        [Test]
        public void TestReadyButtonEnabledWhileSpectatingDuringCountdown()
        {
            AddStep("add second user", () => MultiplayerClient.AddUser(new APIUser { Id = 2, Username = "Another user" }));
            AddStep("set second user ready", () => MultiplayerClient.ChangeUserState(2, MultiplayerUserState.Ready));

            ClickButtonWhenEnabled<MultiplayerReadyButton>();
            AddUntilStep("countdown button shown", () => this.ChildrenOfType<MultiplayerReadyButton.CountdownButton>().SingleOrDefault()?.IsPresent == true);
            ClickButtonWhenEnabled<MultiplayerReadyButton.CountdownButton>();
            AddStep("click the first countdown button", () =>
            {
                var popoverButton = this.ChildrenOfType<MultiplayerReadyButton.CountdownButton.PopoverButton>().First();
                InputManager.MoveMouseTo(popoverButton);
                InputManager.Click(MouseButton.Left);
            });

            AddStep("set spectating", () => MultiplayerClient.ChangeUserState(API.LocalUser.Value.OnlineID, MultiplayerUserState.Spectating));
            AddUntilStep("local user is spectating", () => MultiplayerClient.LocalUser?.State == MultiplayerUserState.Spectating);

            AddAssert("ready button enabled", () => this.ChildrenOfType<MultiplayerReadyButton.ReadyButton>().Single().Enabled.Value);
        }

        [Test]
        public void TestBecomeHostDuringCountdownAndReady()
        {
            AddStep("add second user as host", () =>
            {
                MultiplayerClient.AddUser(new APIUser { Id = 2, Username = "Another user" });
                MultiplayerClient.TransferHost(2);
            });

            AddStep("start countdown", () => MultiplayerClient.SendMatchRequest(new MatchStartCountdownRequest { Delay = TimeSpan.FromMinutes(1) }));
            AddUntilStep("countdown started", () => MultiplayerClient.Room?.Countdown != null);

            AddStep("transfer host to local user", () => MultiplayerClient.TransferHost(API.LocalUser.Value.OnlineID));
            AddUntilStep("local user is host", () => MultiplayerClient.Room?.Host?.Equals(MultiplayerClient.LocalUser) == true);

            ClickButtonWhenEnabled<MultiplayerReadyButton>();
            AddUntilStep("local user became ready", () => MultiplayerClient.LocalUser?.State == MultiplayerUserState.Ready);
            AddAssert("countdown still active", () => MultiplayerClient.Room?.Countdown != null);
        }

        [Test]
        public void TestDeletedBeatmapDisableReady()
        {
            OsuButton readyButton = null;

            AddUntilStep("ensure ready button enabled", () =>
            {
                readyButton = button.ChildrenOfType<OsuButton>().Single();
                return readyButton.Enabled.Value;
            });

            AddStep("delete beatmap", () => beatmaps.Delete(importedSet));
            AddUntilStep("ready button disabled", () => !readyButton.Enabled.Value);
            AddStep("undelete beatmap", () => beatmaps.Undelete(importedSet));
            AddUntilStep("ready button enabled back", () => readyButton.Enabled.Value);
        }

        [Test]
        public void TestToggleStateWhenNotHost()
        {
            AddStep("add second user as host", () =>
            {
                MultiplayerClient.AddUser(new APIUser { Id = 2, Username = "Another user" });
                MultiplayerClient.TransferHost(2);
            });

            ClickButtonWhenEnabled<MultiplayerReadyButton>();
            AddUntilStep("user is ready", () => MultiplayerClient.Room?.Users[0].State == MultiplayerUserState.Ready);

            ClickButtonWhenEnabled<MultiplayerReadyButton>();
            AddUntilStep("user is idle", () => MultiplayerClient.Room?.Users[0].State == MultiplayerUserState.Idle);
        }

        [TestCase(true)]
        [TestCase(false)]
        public void TestToggleStateWhenHost(bool allReady)
        {
            AddStep("setup", () =>
            {
                MultiplayerClient.TransferHost(MultiplayerClient.Room?.Users[0].UserID ?? 0);

                if (!allReady)
                    MultiplayerClient.AddUser(new APIUser { Id = 2, Username = "Another user" });
            });

            ClickButtonWhenEnabled<MultiplayerReadyButton>();
            AddUntilStep("user is ready", () => MultiplayerClient.Room?.Users[0].State == MultiplayerUserState.Ready);

            verifyGameplayStartFlow();
        }

        [Test]
        public void TestBecomeHostWhileReady()
        {
            AddStep("add host", () =>
            {
                MultiplayerClient.AddUser(new APIUser { Id = 2, Username = "Another user" });
                MultiplayerClient.TransferHost(2);
            });

            ClickButtonWhenEnabled<MultiplayerReadyButton>();
            AddStep("make user host", () => MultiplayerClient.TransferHost(MultiplayerClient.Room?.Users[0].UserID ?? 0));

            verifyGameplayStartFlow();
        }

        [Test]
        public void TestLoseHostWhileReady()
        {
            AddStep("setup", () =>
            {
                MultiplayerClient.TransferHost(MultiplayerClient.Room?.Users[0].UserID ?? 0);
                MultiplayerClient.AddUser(new APIUser { Id = 2, Username = "Another user" });
            });

            ClickButtonWhenEnabled<MultiplayerReadyButton>();
            AddUntilStep("user is ready", () => MultiplayerClient.Room?.Users[0].State == MultiplayerUserState.Ready);

            AddStep("transfer host", () => MultiplayerClient.TransferHost(MultiplayerClient.Room?.Users[1].UserID ?? 0));

            ClickButtonWhenEnabled<MultiplayerReadyButton>();
            AddUntilStep("user is idle (match not started)", () => MultiplayerClient.Room?.Users[0].State == MultiplayerUserState.Idle);
            AddAssert("ready button enabled", () => button.ChildrenOfType<OsuButton>().Single().Enabled.Value);
        }

        [TestCase(true)]
        [TestCase(false)]
        public void TestManyUsersChangingState(bool isHost)
        {
            const int users = 10;
            AddStep("setup", () =>
            {
                MultiplayerClient.TransferHost(MultiplayerClient.Room?.Users[0].UserID ?? 0);
                for (int i = 0; i < users; i++)
                    MultiplayerClient.AddUser(new APIUser { Id = i, Username = "Another user" });
            });

            if (!isHost)
                AddStep("transfer host", () => MultiplayerClient.TransferHost(2));

            ClickButtonWhenEnabled<MultiplayerReadyButton>();

            AddRepeatStep("change user ready state", () =>
            {
                MultiplayerClient.ChangeUserState(RNG.Next(0, users), RNG.NextBool() ? MultiplayerUserState.Ready : MultiplayerUserState.Idle);
            }, 20);

            AddRepeatStep("ready all users", () =>
            {
                var nextUnready = MultiplayerClient.Room?.Users.FirstOrDefault(c => c.State == MultiplayerUserState.Idle);
                if (nextUnready != null)
                    MultiplayerClient.ChangeUserState(nextUnready.UserID, MultiplayerUserState.Ready);
            }, users);
        }

        private void verifyGameplayStartFlow()
        {
            AddUntilStep("user is ready", () => MultiplayerClient.Room?.Users[0].State == MultiplayerUserState.Ready);
            ClickButtonWhenEnabled<MultiplayerReadyButton>();
            AddUntilStep("user waiting for load", () => MultiplayerClient.Room?.Users[0].State == MultiplayerUserState.WaitingForLoad);

            AddAssert("ready button disabled", () => !button.ChildrenOfType<OsuButton>().Single().Enabled.Value);
            AddStep("transitioned to gameplay", () => readyClickOperation.Dispose());

            AddStep("finish gameplay", () =>
            {
                MultiplayerClient.ChangeUserState(MultiplayerClient.Room?.Users[0].UserID ?? 0, MultiplayerUserState.Loaded);
                MultiplayerClient.ChangeUserState(MultiplayerClient.Room?.Users[0].UserID ?? 0, MultiplayerUserState.FinishedPlay);
            });

            AddUntilStep("ready button enabled", () => button.ChildrenOfType<OsuButton>().Single().Enabled.Value);
        }
    }
}
