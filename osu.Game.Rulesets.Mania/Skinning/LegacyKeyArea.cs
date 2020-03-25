// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Input.Bindings;
using osu.Game.Rulesets.Mania.UI;
using osu.Game.Rulesets.UI.Scrolling;
using osu.Game.Skinning;
using osuTK;

namespace osu.Game.Rulesets.Mania.Skinning
{
    public class LegacyKeyArea : CompositeDrawable, IKeyBindingHandler<ManiaAction>
    {
        private readonly IBindable<ScrollingDirection> direction = new Bindable<ScrollingDirection>();

        private Sprite upSprite;
        private Sprite downSprite;

        [Resolved]
        private Column column { get; set; }

        public LegacyKeyArea()
        {
            RelativeSizeAxes = Axes.Both;
        }

        [BackgroundDependencyLoader]
        private void load(ISkinSource skin, IScrollingInfo scrollingInfo)
        {
            int fallbackColumn = column.Index % 2 + 1;

            string upImage = skin.GetConfig<ManiaSkinConfiguration, string>(new ManiaSkinConfiguration(ManiaSkinConfigurations.KeyImage, column.Index))?.Value
                             ?? $"mania-key{fallbackColumn}";

            string downImage = skin.GetConfig<ManiaSkinConfiguration, string>(new ManiaSkinConfiguration(ManiaSkinConfigurations.KeyImageDown, column.Index))?.Value
                               ?? $"mania-key{fallbackColumn}D";

            InternalChildren = new Drawable[]
            {
                upSprite = new Sprite
                {
                    Anchor = Anchor.TopCentre,
                    RelativeSizeAxes = Axes.Both,
                    FillMode = FillMode.Stretch,
                    Texture = skin.GetTexture(upImage)
                },
                downSprite = new Sprite
                {
                    Anchor = Anchor.TopCentre,
                    RelativeSizeAxes = Axes.Both,
                    FillMode = FillMode.Stretch,
                    Texture = skin.GetTexture(downImage),
                    Alpha = 0
                }
            };

            direction.BindTo(scrollingInfo.Direction);
            direction.BindValueChanged(onDirectionChanged, true);
        }

        private void onDirectionChanged(ValueChangedEvent<ScrollingDirection> direction)
        {
            if (direction.NewValue == ScrollingDirection.Up)
            {
                upSprite.Origin = downSprite.Origin = Anchor.BottomCentre;
                upSprite.Scale = downSprite.Scale = new Vector2(1, -1);
            }
            else
            {
                upSprite.Origin = downSprite.Origin = Anchor.TopCentre;
                upSprite.Scale = downSprite.Scale = Vector2.One;
            }
        }

        public bool OnPressed(ManiaAction action)
        {
            if (action == column.Action.Value)
            {
                upSprite.FadeTo(0);
                downSprite.FadeTo(1);
            }

            return false;
        }

        public void OnReleased(ManiaAction action)
        {
            if (action == column.Action.Value)
            {
                upSprite.FadeTo(1);
                downSprite.FadeTo(0);
            }
        }
    }
}
