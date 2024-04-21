// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Graphics;
using osu.Game.Rulesets.Scoring;
using osu.Game.Skinning;

namespace osu.Game.Rulesets.Taiko.Skinning.Argon
{
    public class TaikoArgonSkinTransformer : SkinTransformer
    {
        public TaikoArgonSkinTransformer(ISkin skin)
            : base(skin)
        {
        }

        public override T? GetDrawableComponent<T>(ISkinComponentLookup lookup)
            where T : class
        {
            switch (lookup)
            {
                case GameplaySkinComponentLookup<HitResult> resultComponent:
                    // This should eventually be moved to a skin setting, when supported.
                    if (Skin is ArgonProSkin && resultComponent.Component >= HitResult.Great)
                        return SkinUtils.As<T>(Drawable.Empty());

                    return SkinUtils.As<T>(new ArgonJudgementPiece(resultComponent.Component));

                case TaikoSkinComponentLookup taikoComponent:
                    // TODO: Once everything is finalised, consider throwing UnsupportedSkinComponentException on missing entries.
                    switch (taikoComponent.Component)
                    {
                        case TaikoSkinComponents.CentreHit:
                            return SkinUtils.As<T>(base.GetDrawableComponent<ArgonCentreCirclePiece>(lookup) ?? new ArgonCentreCirclePiece());

                        case TaikoSkinComponents.RimHit:
                            return SkinUtils.As<T>(base.GetDrawableComponent<ArgonRimCirclePiece>(lookup) ?? new ArgonRimCirclePiece());

                        case TaikoSkinComponents.PlayfieldBackgroundLeft:
                            return SkinUtils.As<T>(base.GetDrawableComponent<ArgonPlayfieldBackgroundLeft>(lookup) ?? new ArgonPlayfieldBackgroundLeft());

                        case TaikoSkinComponents.PlayfieldBackgroundRight:
                            return SkinUtils.As<T>(base.GetDrawableComponent<ArgonPlayfieldBackgroundRight>(lookup) ?? new ArgonPlayfieldBackgroundRight());

                        case TaikoSkinComponents.InputDrum:
                            return SkinUtils.As<T>(base.GetDrawableComponent<ArgonInputDrum>(lookup) ?? new ArgonInputDrum());

                        case TaikoSkinComponents.HitTarget:
                            return SkinUtils.As<T>(base.GetDrawableComponent<ArgonHitTarget>(lookup) ?? new ArgonHitTarget());

                        case TaikoSkinComponents.BarLine:
                            return SkinUtils.As<T>(base.GetDrawableComponent<ArgonBarLine>(lookup) ?? new ArgonBarLine());

                        case TaikoSkinComponents.DrumRollBody:
                            return SkinUtils.As<T>(base.GetDrawableComponent<ArgonElongatedCirclePiece>(lookup) ?? new ArgonElongatedCirclePiece());

                        case TaikoSkinComponents.DrumRollTick:
                            return SkinUtils.As<T>(base.GetDrawableComponent<ArgonTickPiece>(lookup) ?? new ArgonTickPiece());

                        case TaikoSkinComponents.TaikoExplosionKiai:
                            // the drawable needs to expire as soon as possible to avoid accumulating empty drawables on the playfield.
                            return SkinUtils.As<T>(Drawable.Empty().With(d => d.Expire()));

                        case TaikoSkinComponents.DrumSamplePlayer:
                            return SkinUtils.As<T>(base.GetDrawableComponent<ArgonDrumSamplePlayer>(lookup) ?? new ArgonDrumSamplePlayer());

                        case TaikoSkinComponents.TaikoExplosionGreat:
                        case TaikoSkinComponents.TaikoExplosionMiss:
                        case TaikoSkinComponents.TaikoExplosionOk:
                            return SkinUtils.As<T>(base.GetDrawableComponent<ArgonHitExplosion>(lookup) ?? new ArgonHitExplosion(taikoComponent.Component));

                        case TaikoSkinComponents.Swell:
                            return SkinUtils.As<T>(base.GetDrawableComponent<ArgonSwellCirclePiece>(lookup) ?? new ArgonSwellCirclePiece());
                    }

                    break;
            }

            return base.GetDrawableComponent<T>(lookup);
        }
    }
}
