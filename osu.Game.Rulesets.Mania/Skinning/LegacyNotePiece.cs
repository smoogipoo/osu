// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.Textures;
using osu.Game.Rulesets.Mania.Objects.Drawables;
using osu.Game.Rulesets.Mania.UI;
using osu.Game.Rulesets.Objects.Drawables;
using osu.Game.Skinning;

namespace osu.Game.Rulesets.Mania.Skinning
{
    public class LegacyNotePiece : CompositeDrawable
    {
        [Resolved]
        private Column column { get; set; }

        public LegacyNotePiece()
        {
            RelativeSizeAxes = Axes.X;
            Height = 20;
        }

        [BackgroundDependencyLoader]
        private void load(ISkinSource skin, DrawableHitObject drawableObject)
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

            InternalChild = new Sprite
            {
                RelativeSizeAxes = Axes.Both,
                FillMode = FillMode.Stretch,
                Texture = noteTexture
            };
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
