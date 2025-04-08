// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using Bogus;
using MessagePack;
using NUnit.Framework;
using osu.Game.Online.Multiplayer;
using osu.Game.Online.Rooms;

namespace osu.Game.Tests.OnlinePlay
{
    [TestFixture]
    public class MultiplayerRoomTest
    {
        private readonly Faker<MultiplayerRoom> faker;

        public MultiplayerRoomTest()
        {
            faker = new Faker<MultiplayerRoom>()
                    .StrictMode(true)
                    .CustomInstantiator(f => new MultiplayerRoom(f.Random.Long()))
                    .Ignore(o => o.RoomID)
                    .Ignore(o => o.State)
                    .RuleFor(o => o.Settings, f => new MultiplayerRoomSettings
                    {
                        Name = f.Random.String2(10),
                        AutoSkip = f.Random.Bool(),
                        AutoStartDuration = TimeSpan.FromSeconds(f.Random.UShort()),
                        MatchType = f.PickRandom<MatchType>(),
                        Password = f.Random.String2(10),
                        QueueMode = f.PickRandom<QueueMode>()
                    })
                    .Ignore(o => o.Users)
                    .RuleFor(o => o.Host, f => new MultiplayerRoomUser(f.Random.Int()))
                    .Ignore(o => o.MatchState)
                    .RuleFor(o => o.Playlist, f => f.Make(3, _ => new MultiplayerPlaylistItem
                    {
                        ID = f.Random.Long()
                    }))
                    .Ignore(o => o.ActiveCountdowns)
                    .RuleFor(o => o.ChannelID, f => f.Random.Int());
            faker.Generate();
        }

        [SetUp]
        public void Setup()
        {
            Randomizer.Seed = new Random(1337);
        }

        [Test]
        public void TestConstructFromAPIModel()
        {
            for (int i = 0; i < 100; i++)
            {
                MultiplayerRoom initialRoom = faker.Generate();
                MultiplayerRoom copiedRoom = new MultiplayerRoom(new Room(initialRoom));
                Assert.That(MessagePackSerializer.SerializeToJson(copiedRoom), Is.EqualTo(MessagePackSerializer.SerializeToJson(initialRoom)).NoClip);
            }
        }

        [Test]
        public void TestAPIModelCopy()
        {
            for (int i = 0; i < 100; i++)
            {
                MultiplayerRoom fakedRoom = faker.Generate();

                Room fakedApiRoom = new Room(fakedRoom);
                Room copiedApiRoom = new Room();
                copiedApiRoom.CopyFrom(fakedApiRoom);

                MultiplayerRoom copiedRoom = new MultiplayerRoom(copiedApiRoom);

                Assert.That(MessagePackSerializer.SerializeToJson(copiedRoom), Is.EqualTo(MessagePackSerializer.SerializeToJson(fakedRoom)).NoClip);
            }
        }
    }
}
