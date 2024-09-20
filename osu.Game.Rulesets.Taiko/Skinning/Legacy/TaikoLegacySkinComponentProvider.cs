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
    public class TaikoLegacySkinComponentProvider : ILegacyComponentProvider,
                                                    ISampleProvider,
                                                    IDrawableProvider<SkinComponentLookup<HitResult>>,
                                                    IDrawableProvider<TaikoSkinComponentLookup>
    {
        public bool IsProvidingLegacyResources => skin.HasFont(LegacyFont.Combo) || hasHitCircle || hasBarLeft;

        private readonly ISkin skin;
        private readonly Lazy<bool> hasExplosion;

        public TaikoLegacySkinComponentProvider(ISkin skin)
        {
            this.skin = skin;
            hasExplosion = new Lazy<bool>(() => skin.GetTexture(getHitName(TaikoSkinComponents.TaikoExplosionGreat)) != null);
        }

        public ISample? Get(ISampleInfo lookup)
        {
            if (lookup is HitSampleInfo hitSampleInfo)
                return skin.GetSample(new LegacyTaikoSampleInfo(hitSampleInfo));

            return null;
        }

        public Drawable? Get(SkinComponentLookup<HitResult> lookup)
        {
            // if a taiko skin is providing explosion sprites, hide the judgements completely
            if (hasExplosion.Value)
                return Drawable.Empty().With(d => d.Expire());

            return null;
        }

        public Drawable? Get(TaikoSkinComponentLookup lookup)
        {
            switch (lookup.Component)
            {
                case TaikoSkinComponents.DrumRollBody:
                    if (skin.GetTexture("taiko-roll-middle") != null)
                        return new LegacyDrumRoll();

                    return null;

                case TaikoSkinComponents.InputDrum:
                    if (hasBarLeft)
                        return new LegacyInputDrum();

                    return null;

                case TaikoSkinComponents.DrumSamplePlayer:
                    return null;

                case TaikoSkinComponents.CentreHit:
                case TaikoSkinComponents.RimHit:
                    if (hasHitCircle)
                        return new LegacyHit(lookup.Component);

                    return null;

                case TaikoSkinComponents.DrumRollTick:
                    return skin.GetAnimation("sliderscorepoint", false, false);

                case TaikoSkinComponents.Swell:
                    // todo: support taiko legacy swell (https://github.com/ppy/osu/issues/13601).
                    return null;

                case TaikoSkinComponents.HitTarget:
                    if (skin.GetTexture("taikobigcircle") != null)
                        return new TaikoLegacyHitTarget();

                    return null;

                case TaikoSkinComponents.PlayfieldBackgroundRight:
                    if (skin.GetTexture("taiko-bar-right") != null)
                        return new TaikoLegacyPlayfieldBackgroundRight();

                    return null;

                case TaikoSkinComponents.PlayfieldBackgroundLeft:
                    // This is displayed inside LegacyInputDrum. It is required to be there for layout purposes (can be seen on legacy skins).
                    if (skin.GetTexture("taiko-bar-right") != null)
                        return Drawable.Empty();

                    return null;

                case TaikoSkinComponents.BarLine:
                    if (skin.GetTexture("taiko-barline") != null)
                        return new LegacyBarLine();

                    return null;

                case TaikoSkinComponents.TaikoExplosionMiss:
                    var missSprite = skin.GetAnimation(getHitName(lookup.Component), true, false);
                    if (missSprite != null)
                        return new LegacyHitExplosion(missSprite);

                    return null;

                case TaikoSkinComponents.TaikoExplosionOk:
                case TaikoSkinComponents.TaikoExplosionGreat:
                    string hitName = getHitName(lookup.Component);
                    var hitSprite = skin.GetAnimation(hitName, true, false);

                    if (hitSprite != null)
                    {
                        var strongHitSprite = skin.GetAnimation($"{hitName}k", true, false);

                        return new LegacyHitExplosion(hitSprite, strongHitSprite);
                    }

                    return null;

                case TaikoSkinComponents.TaikoExplosionKiai:
                    // suppress the default kiai explosion if the skin brings its own sprites.
                    // the drawable needs to expire as soon as possible to avoid accumulating empty drawables on the playfield.
                    if (hasExplosion.Value)
                        return Drawable.Empty().With(d => d.Expire());

                    return null;

                case TaikoSkinComponents.Scroller:
                    if (skin.GetTexture("taiko-slider") != null)
                        return new LegacyTaikoScroller();

                    return null;

                case TaikoSkinComponents.Mascot:
                    return new DrawableTaikoMascot();

                case TaikoSkinComponents.KiaiGlow:
                    if (skin.GetTexture("taiko-glow") != null)
                        return new LegacyKiaiGlow();

                    return null;

                default:
                    throw new UnsupportedSkinComponentException(lookup);
            }
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

        private bool hasHitCircle => skin.GetTexture("taikohitcircle") != null;

        private bool hasBarLeft => skin.GetTexture("taiko-bar-left") != null;

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
