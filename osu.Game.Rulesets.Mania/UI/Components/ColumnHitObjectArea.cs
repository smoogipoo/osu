// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Rulesets.Mania.Skinning;
using osu.Game.Rulesets.UI;
using osu.Game.Rulesets.UI.Scrolling;
using osu.Game.Skinning;

namespace osu.Game.Rulesets.Mania.UI.Components
{
    public class ColumnHitObjectArea : CompositeDrawable
    {
        private readonly IBindable<ScrollingDirection> direction = new Bindable<ScrollingDirection>();

        private readonly Drawable hitTarget;

        public ColumnHitObjectArea(HitObjectContainer hitObjectContainer)
        {
            InternalChildren = new[]
            {
                hitTarget = new SkinnableDrawable(new ManiaSkinComponent(ManiaSkinComponents.HitTarget), _ => new DefaultHitTarget())
                {
                    RelativeSizeAxes = Axes.Both
                },
                hitObjectContainer
            };
        }

        [BackgroundDependencyLoader]
        private void load(IScrollingInfo scrollingInfo)
        {
            direction.BindTo(scrollingInfo.Direction);
            direction.BindValueChanged(onDirectionChanged, true);
        }

        private void onDirectionChanged(ValueChangedEvent<ScrollingDirection> direction)
        {
            if (direction.NewValue == ScrollingDirection.Up)
                hitTarget.Anchor = hitTarget.Origin = Anchor.TopLeft;
            else
                hitTarget.Anchor = hitTarget.Origin = Anchor.BottomLeft;
        }
    }
}
