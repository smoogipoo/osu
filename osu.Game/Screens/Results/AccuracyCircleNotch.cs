// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Game.Graphics;
using osuTK;

namespace osu.Game.Screens.Results
{
    public class AccuracyCircleNotch : CompositeDrawable
    {
        public AccuracyCircleNotch(float position)
        {
            RelativeSizeAxes = Axes.Both;

            InternalChild = new Container
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                RelativeSizeAxes = Axes.Both,
                Rotation = position * 360f,
                Child = new Box
                {
                    Anchor = Anchor.TopCentre,
                    Origin = Anchor.TopCentre,
                    RelativeSizeAxes = Axes.Y,
                    Height = AccuracyCircle.RANK_CIRCLE_RADIUS,
                    Width = 1f,
                    Colour = OsuColour.Gray(0.3f),
                    EdgeSmoothness = new Vector2(1f)
                }
            };
        }
    }
}
