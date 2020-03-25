// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

namespace osu.Game.Rulesets.Mania
{
    public class ManiaSkinConfiguration
    {
        public readonly int Column;
        public readonly ManiaSkinConfigurations Lookup;

        public ManiaSkinConfiguration(int column, ManiaSkinConfigurations lookup)
        {
            Column = column;
            Lookup = lookup;
        }
    }

    public enum ManiaSkinConfigurations
    {
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
