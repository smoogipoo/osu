// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using NUnit.Framework;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Game.Rulesets.UI;
using OpenTK;
using OpenTK.Graphics;

namespace osu.Game.Tests.Visual
{
    [TestFixture]
    public class TestCaseScalableContainer : OsuTestCase
    {
        public TestCaseScalableContainer()
        {
            Child = new Container
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                RelativeSizeAxes = Axes.Both,
                Size = new Vector2(0.5f),
                Children = new Drawable[]
                {
                    new Box
                    {
                        RelativeSizeAxes = Axes.Both,
                        Colour = Color4.White.Opacity(0.25f),
                    },
                    new ScalableContainer(512)
                    {
                        RelativeSizeAxes = Axes.Both,
                        Children = new[]
                        {
                            new TestPoint(),
                            new TestPoint { Position = new Vector2(512, 0) },
                            new TestPoint { Position = new Vector2(0, 384) },
                            new TestPoint { Position = new Vector2(512, 384) },
                        },
                    }
                }
            };
        }

        private class TestPoint : CircularContainer
        {
            public TestPoint()
            {
                Origin = Anchor.Centre;

                Size = new Vector2(50);
                Masking = true;

                InternalChildren = new Drawable[]
                {
                    new Box
                    {
                        RelativeSizeAxes = Axes.Both,
                        Alpha = 0.5f
                    },
                    new CircularContainer
                    {
                        Anchor = Anchor.Centre,
                        Origin = Anchor.Centre,
                        Size = new Vector2(5f),
                        Colour = Color4.Red,
                        Masking = true,
                        Child = new Box { RelativeSizeAxes = Axes.Both }
                    }
                };
            }
        }
    }
}
