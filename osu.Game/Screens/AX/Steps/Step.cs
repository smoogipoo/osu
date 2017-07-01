// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using OpenTK;

namespace osu.Game.Screens.AX.Steps
{
    public abstract class Step : Container
    {
        protected override Container<Drawable> Content => content;
        private readonly FillFlowContainer content;

        public Step()
        {
            Anchor = Anchor.Centre;
            Origin = Anchor.Centre;

            RelativeSizeAxes = Axes.X;
            AutoSizeAxes = Axes.Y;

            AddInternal(new Drawable[]
            {
                content = new FillFlowContainer
                {
                    RelativeSizeAxes = Axes.X,
                    AutoSizeAxes = Axes.Y,
                    Direction = FillDirection.Vertical,
                    Spacing = new Vector2(0, 5),
                    Masking = true
                },
            });
        }
    }
}
