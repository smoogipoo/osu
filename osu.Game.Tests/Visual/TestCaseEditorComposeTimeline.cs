// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using System.Collections.Generic;
using NUnit.Framework;
using OpenTK;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Overlays;
using osu.Game.Screens.Edit.Screens.Compose.Timeline;

namespace osu.Game.Tests.Visual
{
    [TestFixture]
    public class TestCaseEditorComposeTimeline : EditorClockTestCase
    {
        public override IReadOnlyList<Type> RequiredTypes => new[] { typeof(TimelineArea), typeof(Timeline), typeof(BeatmapWaveformGraph), typeof(TimelineButton) };

        [BackgroundDependencyLoader]
        private void load(OsuGameBase osuGame)
        {
            TimelineArea timelineArea;
            Children = new Drawable[]
            {
                new MusicController
                {
                    Anchor = Anchor.TopCentre,
                    Origin = Anchor.TopCentre,
                    State = Visibility.Visible
                },
                timelineArea = new TimelineArea(Clock)
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    RelativeSizeAxes = Axes.X,
                    Size = new Vector2(0.8f, 100)
                }
            };

            timelineArea.Beatmap.BindTo(osuGame.Beatmap);
        }
    }
}
