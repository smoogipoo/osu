// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Bindables;
using osu.Game.Beatmaps;
using osu.Game.Rulesets.Mania.Beatmaps;
using osu.Game.Skinning;
using osuTK.Graphics;

namespace osu.Game.Rulesets.Mania.Skinning.Default
{
    public class ManiaTrianglesSkinComponentProvider : IConfigProvider<ManiaSkinConfigurationLookup>
    {
        private readonly ManiaBeatmap beatmap;

        public ManiaTrianglesSkinComponentProvider(IBeatmap beatmap)
        {
            this.beatmap = (ManiaBeatmap)beatmap;
        }

        private readonly Color4 colourEven = new Color4(6, 84, 0, 255);
        private readonly Color4 colourOdd = new Color4(94, 0, 57, 255);
        private readonly Color4 colourSpecial = new Color4(0, 48, 63, 255);

        public IBindable<TValue>? Get<TValue>(ManiaSkinConfigurationLookup lookup) where TValue : notnull
        {
            switch (lookup.Lookup)
            {
                case LegacyManiaSkinConfigurationLookups.ColumnBackgroundColour:
                    int column = lookup.ColumnIndex ?? 0;
                    var stage = beatmap.GetStageForColumnIndex(column);
                    int columnInStage = column % stage.Columns;

                    if (stage.IsSpecialColumn(columnInStage))
                        return SkinUtils.As<TValue>(new Bindable<Color4>(colourSpecial));

                    int distanceToEdge = Math.Min(columnInStage, (stage.Columns - 1) - columnInStage);
                    return SkinUtils.As<TValue>(new Bindable<Color4>(distanceToEdge % 2 == 0 ? colourOdd : colourEven));
            }

            return null;
        }
    }
}
