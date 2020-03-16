// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Colour;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Game.Scoring;
using osu.Game.Screens.Results;
using osuTK;

namespace osu.Game.Tests.Visual.Results
{
    public class TestSceneAccuracyCircle : OsuTestScene
    {
        public override IReadOnlyList<Type> RequiredTypes => new[]
        {
            typeof(AccuracyCircle),
            typeof(AccuracyCircleBadge),
            typeof(AccuracyCircleNotch),
            typeof(AccuracyCircleText),
            typeof(SmoothCircularProgress)
        };

        public TestSceneAccuracyCircle()
        {
            Child = new Container
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                Size = new Vector2(500, 700),
                Children = new Drawable[]
                {
                    new Box
                    {
                        RelativeSizeAxes = Axes.Both,
                        Colour = ColourInfo.GradientVertical(Color4Extensions.FromHex("#555"), Color4Extensions.FromHex("#333"))
                    }
                }
            };
            Add(new AccuracyCircle(new ScoreInfo { Accuracy = 0.95, Rank = ScoreRank.S })
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                Size = new Vector2(230)
            });
        }
    }
}
