// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Game.Skinning;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Rulesets.Catch.Skinning.Legacy
{
    public class CatchLegacySkinComponentProvider : ILegacyComponentProvider,
                                                    IDrawableProvider<GlobalSkinnableContainerLookup>,
                                                    IDrawableProvider<CatchSkinComponentLookup>,
                                                    IConfigProvider<CatchSkinColour>
    {
        private readonly ISkin skin;

        public CatchLegacySkinComponentProvider(ISkin skin)
        {
            this.skin = skin;
        }

        public bool IsProvidingLegacyResources => skin.HasFont(LegacyFont.Combo) || hasPear;

        public Drawable? Get(GlobalSkinnableContainerLookup lookup)
        {
            // Only handle per ruleset defaults here.
            if (lookup.Ruleset == null)
                return null;

            // we don't have enough assets to display these components (this is especially the case on a "beatmap" skin).
            if (!IsProvidingLegacyResources)
                return null;

            // Our own ruleset components default.
            switch (lookup.Lookup)
            {
                case GlobalSkinnableContainers.MainHUDComponents:
                    // todo: remove CatchSkinComponents.CatchComboCounter and refactor LegacyCatchComboCounter to be added here instead.
                    return new DefaultSkinComponentsContainer(container =>
                    {
                        var keyCounter = container.OfType<LegacyKeyCounterDisplay>().FirstOrDefault();

                        if (keyCounter != null)
                        {
                            // set the anchor to top right so that it won't squash to the return button to the top
                            keyCounter.Anchor = Anchor.CentreRight;
                            keyCounter.Origin = Anchor.TopRight;
                            keyCounter.Position = new Vector2(0, -40) * 1.6f;
                        }
                    })
                    {
                        Children = new Drawable[]
                        {
                            new LegacyKeyCounterDisplay(),
                        }
                    };
            }

            return null;
        }

        public Drawable? Get(CatchSkinComponentLookup lookup)
        {
            switch (lookup.Component)
            {
                case CatchSkinComponents.Fruit:
                    if (hasPear)
                        return new LegacyFruitPiece();

                    return null;

                case CatchSkinComponents.Banana:
                    if (skin.GetTexture("fruit-bananas") != null)
                        return new LegacyBananaPiece();

                    return null;

                case CatchSkinComponents.Droplet:
                    if (skin.GetTexture("fruit-drop") != null)
                        return new LegacyDropletPiece();

                    return null;

                case CatchSkinComponents.Catcher:
                    decimal version = skin.GetConfig<SkinConfiguration.LegacySetting, decimal>(SkinConfiguration.LegacySetting.Version)?.Value ?? 1;

                    if (version < 2.3m)
                    {
                        if (hasOldStyleCatcherSprite())
                            return new LegacyCatcherOld();
                    }

                    if (hasNewStyleCatcherSprite())
                        return new LegacyCatcherNew();

                    return null;

                case CatchSkinComponents.CatchComboCounter:
                    if (providesComboCounter)
                        return new LegacyCatchComboCounter();

                    return null;

                case CatchSkinComponents.HitExplosion:
                    if (hasOldStyleCatcherSprite() || hasNewStyleCatcherSprite())
                        return new LegacyHitExplosion();

                    return null;

                default:
                    throw new UnsupportedSkinComponentException(lookup);
            }
        }

        public IBindable<TValue>? Get<TValue>(CatchSkinColour lookup) where TValue : notnull
        {
            var result = (Bindable<Color4>?)skin.GetConfig<SkinCustomColourLookup, TValue>(new SkinCustomColourLookup(lookup));
            if (result == null)
                return null;

            result.Value = LegacyColourCompatibility.DisallowZeroAlpha(result.Value);
            return (IBindable<TValue>)result;
        }

        private bool hasPear => skin.GetTexture("fruit-pear") != null;

        /// <summary>
        /// For simplicity, let's use legacy combo font texture existence as a way to identify legacy skins from default.
        /// </summary>
        private bool providesComboCounter => skin.HasFont(LegacyFont.Combo);

        private bool hasOldStyleCatcherSprite() =>
            skin.GetTexture(@"fruit-ryuuta") != null
            || skin.GetTexture(@"fruit-ryuuta-0") != null;

        private bool hasNewStyleCatcherSprite() =>
            skin.GetTexture(@"fruit-catcher-idle") != null
            || skin.GetTexture(@"fruit-catcher-idle-0") != null;
    }
}
