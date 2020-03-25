// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;
using osu.Game.Rulesets.UI.Scrolling;
using osu.Game.Skinning;
using osuTK;

namespace osu.Game.Rulesets.Mania.Skinning
{
    public class LegacyHitTarget : CompositeDrawable
    {
        private readonly IBindable<ScrollingDirection> direction = new Bindable<ScrollingDirection>();

        private Sprite sprite;

        public LegacyHitTarget()
        {
            RelativeSizeAxes = Axes.Both;
        }

        [BackgroundDependencyLoader]
        private void load(ISkinSource skin, IScrollingInfo scrollingInfo)
        {
            string targetImage = skin.GetConfig<LegacyManiaSkinConfigurationLookup, string>(new LegacyManiaSkinConfigurationLookup(LegacyManiaSkinConfigurationLookups.HitTargetImage))?.Value
                                 ?? "mania-stage-hint";

            InternalChild = sprite = new Sprite
            {
                Origin = Anchor.BottomCentre,
                Texture = skin.GetTexture(targetImage),
                RelativeSizeAxes = Axes.X,
                Width = 1
            };

            direction.BindTo(scrollingInfo.Direction);
            direction.BindValueChanged(onDirectionChanged, true);
        }

        private void onDirectionChanged(ValueChangedEvent<ScrollingDirection> direction)
        {
            if (direction.NewValue == ScrollingDirection.Up)
            {
                sprite.Anchor = Anchor.TopCentre;
                sprite.Scale = new Vector2(1, -1);
            }
            else
            {
                sprite.Anchor = Anchor.BottomCentre;
                sprite.Scale = Vector2.One;
            }
        }
    }
}
