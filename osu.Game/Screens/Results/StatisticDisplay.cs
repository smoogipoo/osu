// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;

namespace osu.Game.Screens.Results
{
    public abstract class StatisticDisplay : CompositeDrawable
    {
        private readonly string header;

        private Drawable content;

        protected StatisticDisplay(string header)
        {
            this.header = header;
            RelativeSizeAxes = Axes.X;
            AutoSizeAxes = Axes.Y;
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            InternalChild = new FillFlowContainer
            {
                RelativeSizeAxes = Axes.X,
                AutoSizeAxes = Axes.Y,
                Direction = FillDirection.Vertical,
                Children = new[]
                {
                    new CircularContainer
                    {
                        RelativeSizeAxes = Axes.X,
                        Height = 12,
                        Masking = true,
                        Children = new Drawable[]
                        {
                            new Box
                            {
                                RelativeSizeAxes = Axes.Both,
                                Colour = Color4Extensions.FromHex("#222")
                            },
                            new OsuSpriteText
                            {
                                Anchor = Anchor.Centre,
                                Origin = Anchor.Centre,
                                Font = OsuFont.Torus.With(size: 12, weight: FontWeight.SemiBold),
                                Text = header.ToUpperInvariant(),
                            }
                        }
                    },
                    new Container
                    {
                        Anchor = Anchor.TopCentre,
                        Origin = Anchor.TopCentre,
                        AutoSizeAxes = Axes.Both,
                        Children = new Drawable[]
                        {
                            content = CreateContent().With(d =>
                            {
                                d.Anchor = Anchor.TopCentre;
                                d.Origin = Anchor.TopCentre;
                                d.Alpha = 0;
                                d.AlwaysPresent = true;
                            }),
                        }
                    }
                }
            };
        }

        public virtual void Appear() => content.FadeIn(100);

        protected abstract Drawable CreateContent();
    }
}
