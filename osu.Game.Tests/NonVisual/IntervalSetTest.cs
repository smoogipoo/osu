// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using NUnit.Framework;
using osu.Game.Rulesets.Osu.Replays;

namespace osu.Game.Tests.NonVisual
{
    [TestFixture]
    public class IntervalSetTest
    {
        [Test]
        public void TestEmptySet()
        {
            var set = new IntervalSet();
            set.AddInterval(0, 10);

            Assert.AreEqual(1, set.Count);
            Assert.AreEqual(new Interval { Start = 0, End = 10 }, set[0]);
        }

        [Test]
        public void TestAddAtEnd()
        {
            var set = new IntervalSet();
            set.AddInterval(0, 10);

            set.AddInterval(10, 20);

            Assert.AreEqual(1, set.Count);
            Assert.AreEqual(new Interval { Start = 0, End = 20 }, set[0]);
        }

        [Test]
        public void TestAddAfterEnd()
        {
            var set = new IntervalSet();
            set.AddInterval(0, 10);

            set.AddInterval(15, 20);

            Assert.AreEqual(2, set.Count);
            Assert.AreEqual(new Interval { Start = 0, End = 10 }, set[0]);
            Assert.AreEqual(new Interval { Start = 15, End = 20 }, set[1]);
        }

        [Test]
        public void TestAddBeforeStart()
        {
            var set = new IntervalSet();
            set.AddInterval(10, 20);

            set.AddInterval(0, 5);

            Assert.AreEqual(2, set.Count);
            Assert.AreEqual(new Interval { Start = 0, End = 5 }, set[0]);
            Assert.AreEqual(new Interval { Start = 10, End = 20 }, set[1]);
        }

        [Test]
        public void TestAddAtStart()
        {
            var set = new IntervalSet();
            set.AddInterval(10, 20);

            set.AddInterval(0, 10);

            Assert.AreEqual(1, set.Count);
            Assert.AreEqual(new Interval { Start = 0, End = 20 }, set[0]);
        }

        [Test]
        public void TestStartOverlapsWithLastInterval()
        {
            var set = new IntervalSet();
            set.AddInterval(0, 10);
            set.AddInterval(20, 30);

            set.AddInterval(25, 40);

            Assert.AreEqual(2, set.Count);
            Assert.AreEqual(new Interval { Start = 0, End = 10 }, set[0]);
            Assert.AreEqual(new Interval { Start = 20, End = 40 }, set[1]);
        }

        [Test]
        public void TestStartOverlapWithSecondLastInterval()
        {
            var set = new IntervalSet();
            set.AddInterval(0, 10);
            set.AddInterval(20, 30);
            set.AddInterval(40, 50);

            set.AddInterval(25, 60);

            Assert.AreEqual(2, set.Count);
            Assert.AreEqual(new Interval { Start = 0, End = 10 }, set[0]);
            Assert.AreEqual(new Interval { Start = 20, End = 60 }, set[1]);
        }

        [Test]
        public void TestStartAndEndOverlapWithLastInterval()
        {
            var set = new IntervalSet();
            set.AddInterval(0, 10);
            set.AddInterval(20, 30);

            set.AddInterval(24, 26);

            Assert.AreEqual(2, set.Count);
            Assert.AreEqual(new Interval { Start = 0, End = 10 }, set[0]);
            Assert.AreEqual(new Interval { Start = 20, End = 30 }, set[1]);
        }

        [Test]
        public void TestStartAndOverlapWithSecondLastIntervals()
        {
            var set = new IntervalSet();
            set.AddInterval(0, 10);
            set.AddInterval(20, 30);
            set.AddInterval(40, 50);

            set.AddInterval(25, 45);

            Assert.AreEqual(2, set.Count);
            Assert.AreEqual(new Interval { Start = 0, End = 10 }, set[0]);
            Assert.AreEqual(new Interval { Start = 20, End = 50 }, set[1]);
        }

        [Test]
        public void TestStartAndEndOverlapWithAllIntervals()
        {
            var set = new IntervalSet();
            set.AddInterval(0, 10);
            set.AddInterval(20, 30);
            set.AddInterval(40, 50);

            set.AddInterval(0, 50);

            Assert.AreEqual(1, set.Count);
            Assert.AreEqual(new Interval { Start = 0, End = 50 }, set[0]);
        }

        [Test]
        public void TestInternalStartOverlap()
        {
            var set = new IntervalSet();
            set.AddInterval(0, 10);
            set.AddInterval(20, 30);

            set.AddInterval(5, 15);

            Assert.AreEqual(2, set.Count);
            Assert.AreEqual(new Interval { Start = 0, End = 15 }, set[0]);
            Assert.AreEqual(new Interval { Start = 20, End = 30 }, set[1]);
        }

        [Test]
        public void TestInternalEndOverlap()
        {
            var set = new IntervalSet();
            set.AddInterval(0, 10);
            set.AddInterval(20, 30);

            set.AddInterval(15, 25);

            Assert.AreEqual(2, set.Count);
            Assert.AreEqual(new Interval { Start = 0, End = 10 }, set[0]);
            Assert.AreEqual(new Interval { Start = 15, End = 30 }, set[1]);
        }
    }
}
