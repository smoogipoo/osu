// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Animations;
using osu.Framework.Graphics.Containers;
using osu.Game.Rulesets.Mania.Objects.Drawables;
using osu.Game.Rulesets.Mania.UI;
using osu.Game.Rulesets.Objects.Drawables;
using osu.Game.Rulesets.UI.Scrolling;
using osu.Game.Skinning;
using osuTK;

namespace osu.Game.Rulesets.Mania.Skinning
{
    public class LegacyBodyPiece : CompositeDrawable
    {
        private readonly IBindable<ScrollingDirection> direction = new Bindable<ScrollingDirection>();
        private readonly IBindable<bool> isHitting = new Bindable<bool>();

        private Drawable sprite;

        [Resolved]
        private Column column { get; set; }

        public LegacyBodyPiece()
        {
            RelativeSizeAxes = Axes.Both;
        }

        [BackgroundDependencyLoader]
        private void load(ISkinSource skin, IScrollingInfo scrollingInfo, DrawableHitObject drawableObject)
        {
            int fallbackIndex = column.Index % 2 + 1;

            string imageName = skin.GetConfig<ManiaSkinConfiguration, string>(new ManiaSkinConfiguration(ManiaSkinConfigurations.HoldNoteBodyImage, column.Index))?.Value
                               ?? $"mania-note{fallbackIndex}L";

            sprite = skin.GetAnimation(imageName, true, true).With(d =>
            {
                if (d == null)
                    return;

                if (d is TextureAnimation animation)
                    animation.IsPlaying = false;

                d.Anchor = Anchor.TopCentre;
                d.RelativeSizeAxes = Axes.Both;
                d.Size = Vector2.One;
                d.FillMode = FillMode.Stretch;
                // Todo: Wrap
            });

            if (sprite != null)
                InternalChild = sprite;

            direction.BindTo(scrollingInfo.Direction);
            direction.BindValueChanged(onDirectionChanged, true);

            var holdNote = (DrawableHoldNote)drawableObject;
            isHitting.BindTo(holdNote.IsHitting);
            isHitting.BindValueChanged(onIsHittingChanged, true);
        }

        private void onIsHittingChanged(ValueChangedEvent<bool> isHitting)
        {
            if (!(sprite is TextureAnimation animation))
                return;

            animation.IsPlaying = isHitting.NewValue;
        }

        private void onDirectionChanged(ValueChangedEvent<ScrollingDirection> direction)
        {
            if (sprite == null)
                return;

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
