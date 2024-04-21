// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using osu.Framework.Audio.Sample;
using osu.Framework.Graphics;
using osu.Game.Audio;
using osu.Game.Rulesets.Scoring;
using osu.Game.Rulesets.Taiko.UI;
using osu.Game.Skinning;

namespace osu.Game.Rulesets.Taiko.Skinning.Legacy
{
    public class TaikoLegacySkinTransformer : LegacySkinTransformer
    {
        public override bool IsProvidingLegacyResources => base.IsProvidingLegacyResources || hasHitCircle || hasBarLeft;

        private readonly Lazy<bool> hasExplosion;

        private bool hasHitCircle => GetTexture("taikohitcircle") != null;
        private bool hasBarLeft => GetTexture("taiko-bar-left") != null;

        public TaikoLegacySkinTransformer(ISkin skin)
            : base(skin)
        {
            hasExplosion = new Lazy<bool>(() => GetTexture(getHitName(TaikoSkinComponents.TaikoExplosionGreat)) != null);
        }

        public override T? GetDrawableComponent<T>(ISkinComponentLookup lookup)
            where T : class
        {
            if (lookup is GameplaySkinComponentLookup<HitResult>)
            {
                // if a taiko skin is providing explosion sprites, hide the judgements completely
                if (hasExplosion.Value)
                    return SkinUtils.As<T>(Drawable.Empty().With(d => d.Expire()));
            }

            if (lookup is TaikoSkinComponentLookup taikoComponent)
            {
                switch (taikoComponent.Component)
                {
                    case TaikoSkinComponents.DrumRollBody:
                        if (GetTexture("taiko-roll-middle") != null)
                            return SkinUtils.As<T>(base.GetDrawableComponent<LegacyDrumRoll>(lookup) ?? new LegacyDrumRoll());

                        return null;

                    case TaikoSkinComponents.InputDrum:
                        if (hasBarLeft)
                            return SkinUtils.As<T>(base.GetDrawableComponent<LegacyInputDrum>(lookup) ?? new LegacyInputDrum());

                        return null;

                    case TaikoSkinComponents.DrumSamplePlayer:
                        return null;

                    case TaikoSkinComponents.CentreHit:
                    case TaikoSkinComponents.RimHit:
                        if (hasHitCircle)
                            return SkinUtils.As<T>(base.GetDrawableComponent<LegacyHit>(lookup) ?? new LegacyHit(taikoComponent.Component));

                        return null;

                    case TaikoSkinComponents.DrumRollTick:
                        return SkinUtils.As<T>(this.GetAnimation("sliderscorepoint", false, false));

                    case TaikoSkinComponents.Swell:
                        // todo: support taiko legacy swell (https://github.com/ppy/osu/issues/13601).
                        return null;

                    case TaikoSkinComponents.HitTarget:
                        if (GetTexture("taikobigcircle") != null)
                            return SkinUtils.As<T>(base.GetDrawableComponent<TaikoLegacyHitTarget>(lookup) ?? new TaikoLegacyHitTarget());

                        return null;

                    case TaikoSkinComponents.PlayfieldBackgroundRight:
                        if (GetTexture("taiko-bar-right") != null)
                            return SkinUtils.As<T>(base.GetDrawableComponent<TaikoLegacyPlayfieldBackgroundRight>(lookup) ?? new TaikoLegacyPlayfieldBackgroundRight());

                        return null;

                    case TaikoSkinComponents.PlayfieldBackgroundLeft:
                        // This is displayed inside LegacyInputDrum. It is required to be there for layout purposes (can be seen on legacy skins).
                        if (GetTexture("taiko-bar-right") != null)
                            return SkinUtils.As<T>(Drawable.Empty());

                        return null;

                    case TaikoSkinComponents.BarLine:
                        if (GetTexture("taiko-barline") != null)
                            return SkinUtils.As<T>(base.GetDrawableComponent<LegacyBarLine>(lookup) ?? new LegacyBarLine());

                        return null;

                    case TaikoSkinComponents.TaikoExplosionMiss:
                        var missSprite = this.GetAnimation(getHitName(taikoComponent.Component), true, false);
                        if (missSprite != null)
                            return SkinUtils.As<T>(base.GetDrawableComponent<LegacyHitExplosion>(lookup) ?? new LegacyHitExplosion(missSprite));

                        return null;

                    case TaikoSkinComponents.TaikoExplosionOk:
                    case TaikoSkinComponents.TaikoExplosionGreat:
                        string hitName = getHitName(taikoComponent.Component);
                        var hitSprite = this.GetAnimation(hitName, true, false);

                        if (hitSprite != null)
                        {
                            var strongHitSprite = this.GetAnimation($"{hitName}k", true, false);

                            return SkinUtils.As<T>(base.GetDrawableComponent<LegacyHitExplosion>(lookup) ?? new LegacyHitExplosion(hitSprite, strongHitSprite));
                        }

                        return null;

                    case TaikoSkinComponents.TaikoExplosionKiai:
                        // suppress the default kiai explosion if the skin brings its own sprites.
                        // the drawable needs to expire as soon as possible to avoid accumulating empty drawables on the playfield.
                        if (hasExplosion.Value)
                            return SkinUtils.As<T>(Drawable.Empty().With(d => d.Expire()));

                        return null;

                    case TaikoSkinComponents.Scroller:
                        if (GetTexture("taiko-slider") != null)
                            return SkinUtils.As<T>(base.GetDrawableComponent<LegacyTaikoScroller>(lookup) ?? new LegacyTaikoScroller());

                        return null;

                    case TaikoSkinComponents.Mascot:
                        return SkinUtils.As<T>(base.GetDrawableComponent<DrawableTaikoMascot>(lookup) ?? new DrawableTaikoMascot());

                    case TaikoSkinComponents.KiaiGlow:
                        if (GetTexture("taiko-glow") != null)
                            return SkinUtils.As<T>(base.GetDrawableComponent<LegacyKiaiGlow>(lookup) ?? new LegacyKiaiGlow());

                        return null;

                    default:
                        throw new UnsupportedSkinComponentException(lookup);
                }
            }

            return base.GetDrawableComponent<T>(lookup);
        }

        private string getHitName(TaikoSkinComponents component)
        {
            switch (component)
            {
                case TaikoSkinComponents.TaikoExplosionMiss:
                    return "taiko-hit0";

                case TaikoSkinComponents.TaikoExplosionOk:
                    return "taiko-hit100";

                case TaikoSkinComponents.TaikoExplosionGreat:
                    return "taiko-hit300";
            }

            throw new ArgumentOutOfRangeException(nameof(component), $"Invalid component type: {component}");
        }

        public override ISample? GetSample(ISampleInfo sampleInfo)
        {
            if (sampleInfo is HitSampleInfo hitSampleInfo)
                return base.GetSample(new LegacyTaikoSampleInfo(hitSampleInfo));

            return base.GetSample(sampleInfo);
        }

        private class LegacyTaikoSampleInfo : HitSampleInfo
        {
            public LegacyTaikoSampleInfo(HitSampleInfo sampleInfo)
                : base(sampleInfo.Name, sampleInfo.Bank, sampleInfo.Suffix, sampleInfo.Volume)

            {
            }

            public override IEnumerable<string> LookupNames
            {
                get
                {
                    foreach (string name in base.LookupNames)
                        yield return name.Insert(name.LastIndexOf('/') + 1, "taiko-");
                }
            }
        }
    }
}
