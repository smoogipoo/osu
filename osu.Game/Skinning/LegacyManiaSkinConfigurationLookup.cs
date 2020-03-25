// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

namespace osu.Game.Skinning
{
    public class LegacyManiaSkinConfigurationLookup
    {
        public readonly LegacyManiaSkinConfigurationLookups Lookup;
        public readonly int? TargetColumn;

        public LegacyManiaSkinConfigurationLookup(LegacyManiaSkinConfigurationLookups lookup, int? targetColumn = null)
        {
            Lookup = lookup;
            TargetColumn = targetColumn;
        }
    }

    public enum LegacyManiaSkinConfigurationLookups
    {
        ColumnWidth,
        KeyImage,
        KeyImageDown,
        LightImage,
        HitTargetImage,
        LeftLineWidth,
        RightLineWidth,
        HoldNoteBodyImage,
        HoldNoteTailImage,
        HoldNoteHeadImage,
        NoteImage
    }
}
