// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Game.Graphics;
using osu.Game.Graphics.UserInterface;
using osuTK;

namespace osu.Game.Screens.Multi.Match.Components
{
    public class Footer : CompositeDrawable
    {
        public const float HEIGHT = 100;

        public Action OnStart;
        public readonly BindableBool AllowStart = new BindableBool();

        private readonly Drawable background;
        private readonly OsuButton startButton;

        public Footer()
        {
            RelativeSizeAxes = Axes.X;
            Height = HEIGHT;

            InternalChildren = new[]
            {
                background = new Box { RelativeSizeAxes = Axes.Both },
                startButton = new OsuButton
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    Size = new Vector2(600, 50),
                    Text = "Start",
                    Enabled = { BindTarget = AllowStart },
                    Action = () => OnStart?.Invoke()
                }
            };
        }

        [BackgroundDependencyLoader]
        private void load(OsuColour colours)
        {
            background.Colour = OsuColour.FromHex(@"28242d");
            startButton.BackgroundColour = colours.Green;
        }
    }
}
