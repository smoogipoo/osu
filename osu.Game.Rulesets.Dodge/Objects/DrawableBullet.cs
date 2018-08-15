// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Game.Rulesets.Objects.Drawables;

namespace osu.Game.Rulesets.Dodge.Objects
{
    public class DrawableBullet : DrawableHitObject<Bullet>
    {
        public DrawableBullet(Bullet hitObject)
            : base(hitObject)
        {
            Anchor = Anchor.Centre;
            Origin = Anchor.Centre;

            Size = hitObject.Size;

            InternalChild = new CircularContainer
            {
                RelativeSizeAxes = Axes.Both,
                Masking = true,
                Child = new Box { RelativeSizeAxes = Axes.Both }
            };
        }

        protected override void UpdateState(ArmedState state)
        {
        }
    }
}
