// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.Textures;
using osu.Game.Rulesets.Mania.UI;
using osu.Game.Rulesets.UI.Scrolling;
using osu.Game.Skinning;
using osuTK;

namespace osu.Game.Rulesets.Mania.Skinning
{
    // Todo: Consider splitting this out for head/tail
    public class LegacyNotePiece : CompositeDrawable
    {
        private readonly IBindable<ScrollingDirection> direction = new Bindable<ScrollingDirection>();

        [Resolved(CanBeNull = true)]
        protected ManiaStage Stage { get; private set; }

        [Resolved]
        protected Column Column { get; private set; }

        private Sprite noteSprite;

        public LegacyNotePiece()
        {
            RelativeSizeAxes = Axes.X;
            Height = 20;
        }

        [BackgroundDependencyLoader]
        private void load(ISkinSource skin, IScrollingInfo scrollingInfo)
        {
            InternalChild = noteSprite = new Sprite
            {
                Anchor = Anchor.TopCentre,
                RelativeSizeAxes = Axes.Both,
                FillMode = FillMode.Stretch,
                Texture = GetTexture(skin)
            };

            direction.BindTo(scrollingInfo.Direction);
            direction.BindValueChanged(OnDirectionChanged, true);
        }

        protected virtual void OnDirectionChanged(ValueChangedEvent<ScrollingDirection> direction)
        {
            if (direction.NewValue == ScrollingDirection.Up)
            {
                noteSprite.Origin = Anchor.BottomCentre;
                noteSprite.Scale = new Vector2(1, -1);
            }
            else
            {
                noteSprite.Origin = Anchor.TopCentre;
                noteSprite.Scale = Vector2.One;
            }
        }

        protected virtual Texture GetTexture(ISkinSource skin) => GetTextureFromLookup(skin, LegacyManiaSkinConfigurationLookups.NoteImage);

        protected Texture GetTextureFromLookup(ISkin skin, LegacyManiaSkinConfigurationLookups lookup)
        {
            int fallbackColumn = Column.Index % 2 + 1;
            string suffix = string.Empty;

            switch (lookup)
            {
                case LegacyManiaSkinConfigurationLookups.HoldNoteHeadImage:
                    suffix = "H";
                    break;

                case LegacyManiaSkinConfigurationLookups.HoldNoteTailImage:
                    suffix = "T";
                    break;
            }

            string noteImage = skin.GetConfig<LegacyManiaSkinConfigurationLookup, string>(
                                   new LegacyManiaSkinConfigurationLookup(Stage?.Columns.Count ?? 4, lookup, Column.Index))?.Value
                               ?? $"mania-note{fallbackColumn}{suffix}";

            return skin.GetTexture(noteImage);
        }
    }
}
