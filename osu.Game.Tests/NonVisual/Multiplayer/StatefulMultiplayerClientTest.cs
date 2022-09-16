// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using System;
using System.Linq;
using Humanizer;
using NUnit.Framework;
using osu.Framework.Extensions;
using osu.Framework.Extensions.ObjectExtensions;
using osu.Framework.Testing;
using osu.Game.Online.API.Requests.Responses;
using osu.Game.Online.Multiplayer;
using osu.Game.Online.Multiplayer.Countdown;
using osu.Game.Online.Rooms;
using osu.Game.Tests.Visual.Multiplayer;

namespace osu.Game.Tests.NonVisual.Multiplayer
{
    [HeadlessTest]
    public class StatefulMultiplayerClientTest : MultiplayerTestScene
    {
        [Test]
        public void TestUserAddedOnJoin()
        {
            var user = new APIUser { Id = 33 };

            AddRepeatStep("add user multiple times", () => MultiplayerClient.AddUser(user), 3);
            AddUntilStep("room has 2 users", () => MultiplayerClient.ClientRoom?.Users.Count == 2);
        }

        [Test]
        public void TestUserRemovedOnLeave()
        {
            var user = new APIUser { Id = 44 };

            AddStep("add user", () => MultiplayerClient.AddUser(user));
            AddUntilStep("room has 2 users", () => MultiplayerClient.ClientRoom?.Users.Count == 2);

            AddStep("remove user", () => MultiplayerClient.RemoveUser(user));
            AddUntilStep("room has 1 user", () => MultiplayerClient.ClientRoom?.Users.Count == 1);
        }

        [Test]
        public void TestPlayingUserTracking()
        {
            int id = 2000;

            AddRepeatStep("add some users", () => MultiplayerClient.AddUser(new APIUser { Id = id++ }), 5);
            checkPlayingUserCount(0);

            changeState(3, MultiplayerUserState.WaitingForLoad);
            checkPlayingUserCount(3);

            changeState(3, MultiplayerUserState.Playing);
            checkPlayingUserCount(3);

            changeState(3, MultiplayerUserState.Results);
            checkPlayingUserCount(0);

            changeState(6, MultiplayerUserState.WaitingForLoad);
            checkPlayingUserCount(6);

            AddStep("another user left", () => MultiplayerClient.RemoveUser((MultiplayerClient.ServerRoom?.Users.Last().User).AsNonNull()));
            checkPlayingUserCount(5);

            AddStep("leave room", () => MultiplayerClient.LeaveRoom());
            checkPlayingUserCount(0);
        }

        [Test]
        public void TestPlayingUsersUpdatedOnJoin()
        {
            AddStep("leave room", () => MultiplayerClient.LeaveRoom());
            AddUntilStep("wait for room part", () => !RoomJoined);

            AddStep("create room initially in gameplay", () =>
            {
                var newRoom = new Room();
                newRoom.CopyFrom(SelectedRoom.Value);

                newRoom.RoomID.Value = null;
                MultiplayerClient.RoomSetupAction = room =>
                {
                    room.State = MultiplayerRoomState.Playing;
                    room.Users.Add(new MultiplayerRoomUser(PLAYER_1_ID)
                    {
                        User = new APIUser { Id = PLAYER_1_ID },
                        State = MultiplayerUserState.Playing
                    });
                };

                RoomManager.CreateRoom(newRoom);
            });

            AddUntilStep("wait for room join", () => RoomJoined);
            checkPlayingUserCount(1);
        }

        [Test]
        public void TestMultipleInstancesOfNonExclusiveCountdowns()
        {
            AddStep("start countdown", () => MultiplayerClient.MatchEvent(new CountdownStartedEvent(new TestNonExclusiveCountdown
            {
                ID = 1,
                TimeRemaining = TimeSpan.FromMinutes(10)
            })).WaitSafely());

            AddAssert("one active countdown", () => MultiplayerClient.ClientRoom?.ActiveCountdowns, () => Has.One.Items);

            AddStep("start another countdown", () => MultiplayerClient.MatchEvent(new CountdownStartedEvent(new TestNonExclusiveCountdown
            {
                ID = 2,
                TimeRemaining = TimeSpan.FromMinutes(10)
            })).WaitSafely());

            AddAssert("two active countdows", () => MultiplayerClient.ClientRoom?.ActiveCountdowns, () => Has.Count.EqualTo(2));
            AddAssert("one countdown with id 1", () => MultiplayerClient.ClientRoom?.ActiveCountdowns, () => Has.One.Items.With.Property(nameof(MultiplayerCountdown.ID)).EqualTo(1));
            AddAssert("one countdown with id 2", () => MultiplayerClient.ClientRoom?.ActiveCountdowns, () => Has.One.Items.With.Property(nameof(MultiplayerCountdown.ID)).EqualTo(2));
        }

        [Test]
        public void TestSingleInstanceOfExclusiveCountdowns()
        {
            AddStep("start countdown", () => MultiplayerClient.MatchEvent(new CountdownStartedEvent(new TestExclusiveCountdown
            {
                ID = 1,
                TimeRemaining = TimeSpan.FromMinutes(10)
            })).WaitSafely());

            AddAssert("one active countdown", () => MultiplayerClient.ClientRoom?.ActiveCountdowns, () => Has.One.Items);

            AddStep("start another countdown", () => MultiplayerClient.MatchEvent(new CountdownStartedEvent(new TestExclusiveCountdown
            {
                ID = 2,
                TimeRemaining = TimeSpan.FromMinutes(10)
            })).WaitSafely());

            AddAssert("one active countdown", () => MultiplayerClient.ClientRoom?.ActiveCountdowns, () => Has.One.Items);
            AddAssert("active countdown has id 2", () => MultiplayerClient.ClientRoom?.ActiveCountdowns.Single().ID, () => Is.EqualTo(2));
        }

        private void checkPlayingUserCount(int expectedCount)
            => AddAssert($"{"user".ToQuantity(expectedCount)} playing", () => MultiplayerClient.CurrentMatchPlayingUserIds.Count == expectedCount);

        private void changeState(int userCount, MultiplayerUserState state)
            => AddStep($"{"user".ToQuantity(userCount)} in {state}", () =>
            {
                for (int i = 0; i < userCount; ++i)
                {
                    int userId = MultiplayerClient.ServerRoom?.Users[i].UserID ?? throw new AssertionException("Room cannot be null!");
                    MultiplayerClient.ChangeUserState(userId, state);
                }
            });

        private class TestNonExclusiveCountdown : MultiplayerCountdown
        {
            public override bool IsExclusive => false;
        }

        private class TestExclusiveCountdown : MultiplayerCountdown
        {
        }
    }
}
