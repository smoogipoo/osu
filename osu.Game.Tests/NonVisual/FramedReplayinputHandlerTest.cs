// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Globalization;
using NUnit.Framework;
using osu.Game.Replays;
using osu.Game.Rulesets.Replays;

namespace osu.Game.Tests.NonVisual
{
    [TestFixture]
    public class FramedReplayinputHandlerTest
    {
        private TestInputHandler handler;

        private TestReplayFrame[] scenarioFrames;
        private double[] scenarioTimes;
        private double scenarioTimeSpan;

        [SetUp]
        public void Setup()
        {
            scenarioFrames = null;
            scenarioTimes = null;
            scenarioTimeSpan = 1000;
        }

        [Test]
        public void TestDefaultState()
        {
            var localHandler = new TestInputHandler(new Replay
            {
                Frames = new List<ReplayFrame>
                {
                    new TestReplayFrame(0)
                }
            });

            Assert.That(localHandler.StartFrame, Is.Null);
            Assert.That(localHandler.EndFrame, Is.Null);
        }

        [Test]
        public void TestDefaultStateWithTimeBeforeFirstFrame()
        {
            setTestFrames(new TestReplayFrame(0));
            setTestTimes(-100);

            confirmFrameRanges((null, 0));
            confirmSequence(true, -100);
        }

        [Test]
        public void TestDefaultStateWithTimeAfterFirstFrame()
        {
            setTestFrames(
                new TestReplayFrame(0),
                new TestReplayFrame(1000)
            );

            setTestTimes(100);

            confirmFrameRanges((0, 1));
            confirmSequence(true, 0);
        }

        [TestCase(false)]
        [TestCase(true)]
        public void TestLeadIn(bool importantFrames)
        {
            setTestFrames(
                new TestReplayFrame(0, importantFrames),
                new TestReplayFrame(1000, importantFrames)
            );

            setTestTimes(
                -300,
                -200,
                -100
            );

            confirmFrameRanges(
                (null, 0),
                (null, 0),
                (null, 0)
            );

            confirmSequence(true,
                -300,
                -200,
                -100
            );

            confirmSequence(false,
                -100,
                -200,
                -300
            );
        }

        [TestCase(false)]
        [TestCase(true)]
        public void TestLeadOut(bool importantFrames)
        {
            setTestFrames(
                new TestReplayFrame(0, importantFrames),
                new TestReplayFrame(1000, importantFrames)
            );

            setTestTimes(
                1000,
                1200,
                1500
            );

            confirmFrameRanges(
                (1, 1),
                (1, 1),
                (1, 1)
            );

            confirmSequence(true,
                1000,
                1200,
                1500
            );

            confirmSequence(false,
                1500,
                1200,
                1000
            );
        }

        [TestCase(false)]
        [TestCase(true)]
        public void TestCrossFirstFrameBoundary(bool importantFrames)
        {
            setTestFrames(
                new TestReplayFrame(0, importantFrames),
                new TestReplayFrame(1000, importantFrames)
            );

            setTestTimes(
                -100,
                -50,
                150,
                200
            );

            confirmFrameRanges(
                (null, 0),
                (null, 0),
                (0, 1),
                (0, 1)
            );

            confirmSequence(true,
                -100,
                -50,
                0,
                importantFrames ? null : (double?)200
            );

            confirmSequence(false,
                importantFrames ? null : (double?)200,
                importantFrames ? null : (double?)150,
                0,
                -100
            );
        }

        [TestCase(false)]
        [TestCase(true)]
        public void TestCrossMiddleFrameBoundary(bool importantFrames)
        {
            setTestFrames(
                new TestReplayFrame(0, importantFrames),
                new TestReplayFrame(1000, importantFrames),
                new TestReplayFrame(2000, importantFrames)
            );

            setTestTimes(
                0,
                500,
                750,
                1250,
                1500
            );

            confirmFrameRanges(
                (0, 1),
                (0, 1),
                (0, 1),
                (1, 2),
                (1, 2)
            );

            confirmSequence(true,
                0,
                importantFrames ? null : (double?)500,
                importantFrames ? null : (double?)750,
                1000,
                importantFrames ? null : (double?)1500
            );

            confirmSequence(false,
                importantFrames ? null : (double?)1500,
                importantFrames ? null : (double?)1250,
                1000,
                importantFrames ? null : (double?)500,
                0
            );
        }

        [TestCase(false)]
        [TestCase(true)]
        public void TestCrossFinalFrameBoundary(bool importantFrames)
        {
            setTestFrames(
                new TestReplayFrame(0, importantFrames),
                new TestReplayFrame(1000, importantFrames)
            );

            setTestTimes(
                0,
                100,
                900,
                1100,
                1200
            );

            confirmFrameRanges(
                (0, 1),
                (0, 1),
                (0, 1),
                (1, 1),
                (1, 1)
            );

            confirmSequence(true,
                0,
                importantFrames ? null : (double?)100,
                importantFrames ? null : (double?)900,
                1000,
                1200
            );

            confirmSequence(false,
                1200,
                1100,
                1000,
                importantFrames ? null : (double?)100,
                0
            );
        }

        [TestCase(false)]
        [TestCase(true)]
        public void TestProgress(bool importantFrames)
        {
            setTestFrames(
                new TestReplayFrame(0, importantFrames),
                new TestReplayFrame(1000, importantFrames)
            );

            setTestTimes(
                0,
                200,
                800
            );

            confirmFrameRanges(
                (0, 1),
                (0, 1),
                (0, 1)
            );

            confirmSequence(true,
                0,
                200,
                800
            );

            confirmSequence(false,
                800,
                200,
                0
            );
        }

        [Test]
        public void TestEnterAndExitImportantSection()
        {
            setTestFrames(
                new TestReplayFrame(0),
                new TestReplayFrame(500, true),
                new TestReplayFrame(1000)
            );

            setTestTimes(
                0,
                400,
                600,
                900,
                1000
            );

            confirmFrameRanges(
                (0, 1),
                (0, 1),
                (1, 2),
                (1, 2),
                (2, 2)
            );

            confirmSequence(true,
                0,
                400,
                500,
                null,
                1000
            );

            confirmSequence(false,
                1000,
                null,
                null,
                500,
                0
            );
        }

        [Test]
        public void TestExceedAllowableImportantTimeSpan()
        {
            setTimeSpan(100);

            setTestFrames(
                new TestReplayFrame(0, true),
                new TestReplayFrame(1000)
            );

            setTestTimes(
                0,
                100,
                300,
                400,
                600
            );

            confirmFrameRanges(
                (0, 1),
                (0, 1),
                (0, 1),
                (0, 1),
                (0, 1)
            );

            confirmSequence(true,
                0,
                null,
                300,
                null,
                600
            );

            confirmSequence(false,
                null,
                400,
                null,
                100,
                null
            );
        }

        [TestCase(false)]
        [TestCase(true)]
        public void TestPivotDirection(bool importantFrames)
        {
            setTestFrames(
                new TestReplayFrame(0, importantFrames),
                new TestReplayFrame(1000, importantFrames)
            );

            setTestTimes(
                0,
                600,
                400,
                1000
            );

            confirmFrameRanges(
                (0, 1),
                (0, 1),
                (0, 1),
                (0, 1)
            );

            confirmSequence(true,
                0,
                importantFrames ? null : (double?)600,
                importantFrames ? null : (double?)400,
                1000
            );

            confirmSequence(false,
                1000,
                importantFrames ? null : (double?)40,
                importantFrames ? null : (double?)600,
                0
            );
        }

        private void setTestFrames(params TestReplayFrame[] frames) => scenarioFrames = frames;

        private void setTestTimes(params double[] times) => scenarioTimes = times;

        private void setTimeSpan(double timeSpan) => scenarioTimeSpan = timeSpan;

        private void confirmFrameRanges(params (int? start, int end)[] frameRanges)
        {
            runScenario(true, (i, _) => check(i));
            runScenario(false, (i, _) => check(frameRanges.Length - 1 - i));

            void check(int i)
            {
                var range = frameRanges[i];

                if (range.start == null)
                    Assert.That(handler.StartFrame, Is.Null);
                else
                    Assert.That(handler.StartFrame, Is.EqualTo(scenarioFrames[range.start.Value]));

                Assert.That(handler.EndFrame, Is.EqualTo(scenarioFrames[range.end]));
            }
        }

        private void confirmSequence(bool forwards, params double?[] frameTimes)
            => runScenario(forwards, (i, t) => Assert.That(t, Is.EqualTo(frameTimes[i])));

        private void runScenario(bool forwards, Action<int, double?> action)
        {
            handler = new TestInputHandler(new Replay { Frames = new List<ReplayFrame>(scenarioFrames) })
            {
                TimeSpan = scenarioTimeSpan
            };

            if (forwards)
            {
                for (int i = 0; i < scenarioTimes.Length; i++)
                    action(i, handler.SetFrameFromTime(scenarioTimes[i]));
            }
            else
            {
                handler.SetFrameFromTime(scenarioTimes[scenarioTimes.Length - 1]);

                for (int i = scenarioTimes.Length - 1; i >= 0; i--)
                    action(scenarioTimes.Length - 1 - i, handler.SetFrameFromTime(scenarioTimes[i]));
            }
        }

        private class TestReplayFrame : ReplayFrame
        {
            public readonly bool IsImportant;

            public TestReplayFrame(double time, bool isImportant = false)
                : base(time)
            {
                IsImportant = isImportant;
            }

            public override string ToString() => Time.ToString(CultureInfo.InvariantCulture);
        }

        private class TestInputHandler : FramedReplayInputHandler<TestReplayFrame>
        {
            private double timeSpan = 1000;

            public double TimeSpan
            {
                get => timeSpan;
                set => timeSpan = value;
            }

            public Replay Replay => replay;

            private readonly Replay replay;

            public TestInputHandler(Replay replay)
                : base(replay)
            {
                this.replay = replay;
            }

            protected override double AllowedImportantTimeSpan => TimeSpan;

            protected override bool IsImportant(TestReplayFrame frame) => frame.IsImportant;
        }
    }
}
