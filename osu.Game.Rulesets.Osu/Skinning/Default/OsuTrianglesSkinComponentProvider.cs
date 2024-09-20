// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Graphics;
using osu.Game.Rulesets.Scoring;
using osu.Game.Skinning;

namespace osu.Game.Rulesets.Osu.Skinning.Default
{
    public class OsuTrianglesSkinComponentProvider : IDrawableProvider<SkinComponentLookup<HitResult>>
    {
        public Drawable? Get(SkinComponentLookup<HitResult> lookup)
        {
            HitResult result = lookup.Component;

            switch (result)
            {
                case HitResult.IgnoreMiss:
                case HitResult.LargeTickMiss:
                    // use argon judgement piece for new tick misses because i don't want to design another one for triangles.
                    return new DefaultJudgementPieceSliderTickMiss(result);
            }

            return null;
        }
    }
}
