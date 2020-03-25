// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Rulesets.Mania.Skinning;
using osu.Game.Rulesets.UI.Scrolling;
using osu.Game.Skinning;

namespace osu.Game.Rulesets.Mania.UI.Components
{
    public class ColumnKeyArea : CompositeDrawable
    {
        private readonly IBindable<ScrollingDirection> direction = new Bindable<ScrollingDirection>();

        private Drawable keyArea;

        [Resolved]
        private Column column { get; set; }

        [BackgroundDependencyLoader]
        private void load(IScrollingInfo scrollingInfo)
        {
            InternalChild = keyArea = new SkinnableDrawable(new ManiaSkinComponent(ManiaSkinComponents.KeyArea), _ => new DefaultKeyArea())
            {
                RelativeSizeAxes = Axes.X,
                Height = ManiaStage.HIT_TARGET_POSITION,
            };

            direction.BindTo(scrollingInfo.Direction);
            direction.BindValueChanged(onDirectionChanged, true);
        }

        private void onDirectionChanged(ValueChangedEvent<ScrollingDirection> direction)
        {
            if (direction.NewValue == ScrollingDirection.Up)
                keyArea.Anchor = keyArea.Origin = Anchor.TopLeft;
            else
                keyArea.Anchor = keyArea.Origin = Anchor.BottomLeft;
        }
    }
}
