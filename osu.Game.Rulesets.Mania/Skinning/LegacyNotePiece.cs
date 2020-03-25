// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.Textures;
using osu.Game.Rulesets.Mania.Objects.Drawables;
using osu.Game.Rulesets.Mania.UI;
using osu.Game.Rulesets.Objects.Drawables;
using osu.Game.Rulesets.UI.Scrolling;
using osu.Game.Skinning;
using osuTK;

namespace osu.Game.Rulesets.Mania.Skinning
{
    // Todo: Consider splitting this out for head/tail
    public class LegacyNotePiece : CompositeDrawable
    {
        private readonly IBindable<ScrollingDirection> direction = new Bindable<ScrollingDirection>();

        private Sprite sprite;

        [Resolved]
        private Column column { get; set; }

        public LegacyNotePiece()
        {
            RelativeSizeAxes = Axes.X;
            Height = 20;
        }

        [BackgroundDependencyLoader]
        private void load(ISkinSource skin, IScrollingInfo scrollingInfo, DrawableHitObject drawableObject)
        {
            Texture noteTexture;

            switch (drawableObject)
            {
                case DrawableHoldNoteTail _:
                    noteTexture = loadTexture(skin, ManiaColumnSkinComponents.TailNote)
                                  ?? loadTexture(skin, ManiaColumnSkinComponents.HeadNote);
                    break;

                case DrawableHoldNoteHead _:
                    noteTexture = loadTexture(skin, ManiaColumnSkinComponents.HeadNote);
                    break;

                default:
                    noteTexture = loadTexture(skin, ManiaColumnSkinComponents.Note);
                    break;
            }

            InternalChild = sprite = new Sprite
            {
                Anchor = Anchor.TopCentre,
                RelativeSizeAxes = Axes.Both,
                FillMode = FillMode.Stretch,
                Texture = noteTexture
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

        private Texture loadTexture(ISkinSource skin, ManiaColumnSkinComponents component)
        {
            int fallbackColumn = column.Index % 2 + 1;
            string suffix = string.Empty;

            switch (component)
            {
                case ManiaColumnSkinComponents.HeadNote:
                    suffix = "H";
                    break;

                case ManiaColumnSkinComponents.TailNote:
                    suffix = "T";
                    break;
            }

            string noteImage = skin.GetConfig<ManiaColumnSkinComponent, string>(new ManiaColumnSkinComponent(component, column.Index))?.Value
                               ?? $"mania-note{fallbackColumn}{suffix}";

            return skin.GetTexture(noteImage);
        }
    }
}
