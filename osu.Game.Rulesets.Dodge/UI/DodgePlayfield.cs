// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Game.Rulesets.UI;
using OpenTK;
using OpenTK.Graphics;

namespace osu.Game.Rulesets.Dodge.UI
{
    public class DodgePlayfield : Playfield
    {
        protected override Container<Drawable> Content => content;
        private readonly Container content;

        public DodgePlayfield()
        {
            base.Content.Add(new CircularContainer
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                RelativeSizeAxes = Axes.Both,
                Size = new Vector2(0.95f),
                FillMode = FillMode.Fit,
                Masking = true,
                EdgeEffect = new EdgeEffectParameters
                {
                    Colour = Color4.Black.Opacity(0.5f),
                    Type = EdgeEffectType.Shadow,
                    Radius = 300,
                    Hollow = true,
                },
                Children = new Drawable[]
                {
                    new Box
                    {
                        RelativeSizeAxes = Axes.Both,
                        Alpha = 0,
                        AlwaysPresent = true,
                    },
                    content = new Container { RelativeSizeAxes = Axes.Both }
                }
            });
        }
    }
}
