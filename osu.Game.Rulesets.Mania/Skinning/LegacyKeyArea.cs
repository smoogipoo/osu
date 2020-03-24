// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Input.Bindings;
using osu.Game.Rulesets.Mania.UI;
using osu.Game.Skinning;

namespace osu.Game.Rulesets.Mania.Skinning
{
    public class LegacyKeyArea : CompositeDrawable, IKeyBindingHandler<ManiaAction>
    {
        private readonly int columnIndex;

        private Sprite upSprite;
        private Sprite downSprite;

        [Resolved]
        private Column column { get; set; }

        public LegacyKeyArea(int columnIndex)
        {
            this.columnIndex = columnIndex;

            RelativeSizeAxes = Axes.Both;
        }

        [BackgroundDependencyLoader]
        private void load(ISkinSource skin)
        {
            int fallbackColumn = columnIndex % 2 + 1;

            string upImage = skin.GetConfig<LegacyColumnSkinConfiguration, string>(new LegacyColumnSkinConfiguration(columnIndex, LegacyColumnSkinConfigurations.KeyImage))?.Value
                             ?? $"mania-key{fallbackColumn}";

            string downImage = skin.GetConfig<LegacyColumnSkinConfiguration, string>(new LegacyColumnSkinConfiguration(columnIndex, LegacyColumnSkinConfigurations.KeyImage))?.Value
                               ?? $"mania-key{fallbackColumn}D";

            InternalChildren = new Drawable[]
            {
                upSprite = new Sprite
                {
                    Anchor = Anchor.TopCentre,
                    Origin = Anchor.TopCentre,
                    RelativeSizeAxes = Axes.Both,
                    FillMode = FillMode.Fit,
                    Texture = skin.GetTexture(upImage)
                },
                downSprite = new Sprite
                {
                    Anchor = Anchor.TopCentre,
                    Origin = Anchor.TopCentre,
                    RelativeSizeAxes = Axes.Both,
                    FillMode = FillMode.Fit,
                    Texture = skin.GetTexture(downImage),
                    Alpha = 0
                }
            };
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
