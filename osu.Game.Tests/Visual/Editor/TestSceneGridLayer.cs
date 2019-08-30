// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using NUnit.Framework;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Input.Events;
using osu.Game.Screens.Edit.Compose.Components.Grids;
using osu.Game.Screens.Edit.Compose.Components.Grids.Basic;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Tests.Visual.Editor
{
    public class TestSceneGridLayer : OsuTestScene
    {
        public override IReadOnlyList<Type> RequiredTypes => new[]
        {
            typeof(DrawableGrid),
            typeof(DrawableCircularGrid)
        };

        private GridLayer gridLayer;
        private Drawable snapMarker;

        [SetUp]
        public void Setup() => Schedule(() =>
        {
            Child = new Container
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                RelativeSizeAxes = Axes.Both,
                Size = new Vector2(0.75f),
                Masking = true,
                Children = new[]
                {
                    new Box
                    {
                        RelativeSizeAxes = Axes.Both,
                        Alpha = 0.2f
                    },
                    gridLayer = new GridLayer(),
                    snapMarker = new CircularContainer
                    {
                        Origin = Anchor.Centre,
                        Size = new Vector2(10),
                        Masking = true,
                        Child = new Box
                        {
                            Colour = Color4.Red,
                            RelativeSizeAxes = Axes.Both,
                        },
                    }
                },
            };
        });

        protected override bool OnMouseMove(MouseMoveEvent e)
        {
            if (gridLayer == null)
                return false;

            snapMarker.Position = gridLayer.ToLocalSpace(gridLayer.GetSnappedPosition(e.ScreenSpaceMousePosition));

            return true;
        }
    }
}
