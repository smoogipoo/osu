// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Graphics;
using osu.Game.Rulesets.Scoring;
using osu.Game.Skinning;

namespace osu.Game.Rulesets.Osu.Skinning.Argon
{
    public class OsuArgonSkinTransformer : SkinTransformer
    {
        public OsuArgonSkinTransformer(ISkin skin)
            : base(skin)
        {
        }

        public override T? GetDrawableComponent<T>(ISkinComponentLookup lookup)
            where T : class
        {
            switch (lookup)
            {
                case GameplaySkinComponentLookup<HitResult> resultComponent:
                    HitResult result = resultComponent.Component;

                    // This should eventually be moved to a skin setting, when supported.
                    if (Skin is ArgonProSkin && (result == HitResult.Great || result == HitResult.Perfect))
                        return SkinUtils.As<T>(Drawable.Empty());

                    switch (result)
                    {
                        case HitResult.IgnoreMiss:
                        case HitResult.LargeTickMiss:
                            return SkinUtils.As<T>(new ArgonJudgementPieceSliderTickMiss(result));

                        default:
                            return SkinUtils.As<T>(new ArgonJudgementPiece(result));
                    }

                case OsuSkinComponentLookup osuComponent:
                    // TODO: Once everything is finalised, consider throwing UnsupportedSkinComponentException on missing entries.
                    switch (osuComponent.Component)
                    {
                        case OsuSkinComponents.HitCircle:
                            return SkinUtils.As<T>(base.GetDrawableComponent<ArgonMainCirclePiece>(lookup) ?? new ArgonMainCirclePiece(true));

                        case OsuSkinComponents.SliderHeadHitCircle:
                            return SkinUtils.As<T>(base.GetDrawableComponent<ArgonMainCirclePiece>(lookup) ?? new ArgonMainCirclePiece(false));

                        case OsuSkinComponents.SliderBody:
                            return SkinUtils.As<T>(base.GetDrawableComponent<ArgonSliderBody>(lookup) ?? new ArgonSliderBody());

                        case OsuSkinComponents.SliderBall:
                            return SkinUtils.As<T>(base.GetDrawableComponent<ArgonSliderBall>(lookup) ?? new ArgonSliderBall());

                        case OsuSkinComponents.SliderFollowCircle:
                            return SkinUtils.As<T>(base.GetDrawableComponent<ArgonFollowCircle>(lookup) ?? new ArgonFollowCircle());

                        case OsuSkinComponents.SliderScorePoint:
                            return SkinUtils.As<T>(base.GetDrawableComponent<ArgonSliderScorePoint>(lookup) ?? new ArgonSliderScorePoint());

                        case OsuSkinComponents.SpinnerBody:
                            return SkinUtils.As<T>(base.GetDrawableComponent<ArgonSpinner>(lookup) ?? new ArgonSpinner());

                        case OsuSkinComponents.ReverseArrow:
                            return SkinUtils.As<T>(base.GetDrawableComponent<ArgonReverseArrow>(lookup) ?? new ArgonReverseArrow());

                        case OsuSkinComponents.FollowPoint:
                            return SkinUtils.As<T>(base.GetDrawableComponent<ArgonFollowPoint>(lookup) ?? new ArgonFollowPoint());

                        case OsuSkinComponents.Cursor:
                            return SkinUtils.As<T>(base.GetDrawableComponent<ArgonCursor>(lookup) ?? new ArgonCursor());

                        case OsuSkinComponents.CursorTrail:
                            return SkinUtils.As<T>(base.GetDrawableComponent<ArgonCursorTrail>(lookup) ?? new ArgonCursorTrail());
                    }

                    break;
            }

            return base.GetDrawableComponent<T>(lookup);
        }
    }
}
