// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;
using osu.Game.Rulesets.Mania.UI;
using osu.Game.Rulesets.UI.Scrolling;
using osu.Game.Skinning;
using osuTK;

namespace osu.Game.Rulesets.Mania.Skinning
{
    public class LegacyBodyPiece : CompositeDrawable
    {
        private readonly IBindable<ScrollingDirection> direction = new Bindable<ScrollingDirection>();

        private Sprite sprite;

        [Resolved]
        private Column column { get; set; }

        public LegacyBodyPiece()
        {
            RelativeSizeAxes = Axes.Both;
        }

        [BackgroundDependencyLoader]
        private void load(ISkinSource skin, IScrollingInfo scrollingInfo)
        {
            int fallbackIndex = column.Index % 2 + 1;

            string imageName = skin.GetConfig<ManiaColumnSkinComponent, string>(new ManiaColumnSkinComponent(ManiaColumnSkinComponents.Body, column.Index))?.Value ?? $"mania-note{fallbackIndex}L-0";

            InternalChild = sprite = new Sprite
            {
                Anchor = Anchor.TopCentre,
                RelativeSizeAxes = Axes.Both,
                FillMode = FillMode.Stretch,
                // Todo: WrapMode,
                Texture = skin.GetTexture(imageName)
            };

            direction.BindTo(scrollingInfo.Direction);
            direction.BindValueChanged(onDirectionChanged, true);
        }

        private void onDirectionChanged(ValueChangedEvent<ScrollingDirection> direction)
        {
            if (direction.NewValue == ScrollingDirection.Up)
            {
                sprite.Origin = Anchor.BottomCentre;
                sprite.Scale = new Vector2(1, -1);
            }
            else
            {
                sprite.Origin = Anchor.TopCentre;
                sprite.Scale = Vector2.One;
            }
        }
    }
}
