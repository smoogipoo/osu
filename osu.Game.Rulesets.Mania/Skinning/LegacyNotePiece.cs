// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;
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
            int fallbackColumn = column.Index % 2 + 1;
            string noteImage;

            switch (drawableObject)
            {
                case DrawableHoldNoteTail _:
                    noteImage = skin.GetConfig<ManiaColumnSkinComponent, string>(new ManiaColumnSkinComponent(ManiaColumnSkinComponents.Note, column.Index))?.Value ?? $"mania-note{fallbackColumn}T";
                    if (skin.GetTexture(noteImage) == null)
                        goto default;
                    break;

                case DrawableHoldNoteHead _:
                    noteImage = skin.GetConfig<ManiaColumnSkinComponent, string>(new ManiaColumnSkinComponent(ManiaColumnSkinComponents.Note, column.Index))?.Value ?? $"mania-note{fallbackColumn}H";
                    break;

                default:
                    noteImage = skin.GetConfig<ManiaColumnSkinComponent, string>(new ManiaColumnSkinComponent(ManiaColumnSkinComponents.Note, column.Index))?.Value ?? $"mania-note{fallbackColumn}";
                    break;
            }

            InternalChild = new Sprite
            {
                RelativeSizeAxes = Axes.Both,
                FillMode = FillMode.Stretch,
                Texture = skin.GetTexture(noteImage),
            };
        }
    }
}
