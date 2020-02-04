// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osuTK.Graphics;

namespace osu.Game.Screens.Multi.Match.Components
{
    public class OverlinedContainer : CompositeDrawable
    {
        private readonly Circle line;

        public Color4 LineColour
        {
            set => line.Colour = value;
        }

        public OverlinedContainer(bool big = false, int minimumWidth = 60)
        {
            AutoSizeAxes = Axes.Both;
            InternalChild = new FillFlowContainer
            {
                Direction = FillDirection.Vertical,
                AutoSizeAxes = Axes.Both,
                Children = new Drawable[]
                {
                    line = new Circle
                    {
                        RelativeSizeAxes = Axes.X,
                        Height = 2,
                        Margin = new MarginPadding { Bottom = 2 }
                    },
                    new Container //Add a minimum size to the FillFlowContainer
                    {
                        Width = minimumWidth,
                    }
                }
            };
        }
    }
}
