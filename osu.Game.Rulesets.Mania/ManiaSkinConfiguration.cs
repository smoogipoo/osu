// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

namespace osu.Game.Rulesets.Mania
{
    public class ManiaSkinConfiguration
    {
        public readonly ManiaSkinConfigurations Lookup;
        public readonly int? Column;

        public ManiaSkinConfiguration(ManiaSkinConfigurations lookup, int? column = null)
        {
            Lookup = lookup;
            Column = column;
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
