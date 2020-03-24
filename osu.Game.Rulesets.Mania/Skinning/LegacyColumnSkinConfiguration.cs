// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

namespace osu.Game.Rulesets.Mania.Skinning
{
    public class LegacyColumnSkinConfiguration
    {
        public readonly int Column;
        public readonly LegacyColumnSkinConfigurations Lookup;

        public LegacyColumnSkinConfiguration(int column, LegacyColumnSkinConfigurations lookup)
        {
            Column = column;
            Lookup = lookup;
        }
    }

    public enum LegacyColumnSkinConfigurations
    {
        KeyImage,
        KeyImageDown,
        LightImage,
        LeftLineWidth,
        RightLineWidth
    }
}
