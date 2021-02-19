// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Allocation;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Game.Graphics;
using osuTK;

namespace osu.Game.Screens.OnlinePlay.Multiplayer.Match
{
    public class MultiplayerMatchFooter : CompositeDrawable
    {
        public const float HEIGHT = 50;
        private const float ready_button_width = 600;
        private const float spectate_button_width = 200;

        public Action OnReadyClick
        {
            set => readyButton.OnReadyClick = value;
        }

        public Action OnSpectateClick
        {
            set => spectateButton.OnSpectateClick = value;
        }

        private readonly Drawable background;
        private readonly MultiplayerSpectateButton spectateButton;
        private readonly MultiplayerReadyButton readyButton;

        public MultiplayerMatchFooter()
        {
            RelativeSizeAxes = Axes.X;
            Height = HEIGHT;

            InternalChildren = new[]
            {
                background = new Box { RelativeSizeAxes = Axes.Both },
                spectateButton = new MultiplayerSpectateButton
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    Size = new Vector2(spectate_button_width, 50),
                    X = -ready_button_width / 2f - spectate_button_width / 2f - 10,
                },
                readyButton = new MultiplayerReadyButton
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    Size = new Vector2(ready_button_width, 50),
                }
            };
        }

        [BackgroundDependencyLoader]
        private void load(OsuColour colours)
        {
            background.Colour = Color4Extensions.FromHex(@"28242d");
        }
    }
}
