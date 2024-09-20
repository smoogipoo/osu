// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Linq;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Game.Rulesets.Osu.Objects;
using osu.Game.Skinning;
using osuTK;

namespace osu.Game.Rulesets.Osu.Skinning.Legacy
{
    public class OsuLegacySkinComponentProvider : ILegacyComponentProvider,
                                                  IDrawableProvider<GlobalSkinnableContainerLookup>,
                                                  IDrawableProvider<OsuSkinComponentLookup>,
                                                  IConfigProvider<OsuSkinColour>,
                                                  IConfigProvider<OsuSkinConfiguration>,
                                                  ISkinWithExternalComponents
    {
        /// <summary>
        /// On osu-stable, hitcircles have 5 pixels of transparent padding on each side to allow for shadows etc.
        /// Their hittable area is 128px, but the actual circle portion is 118px.
        /// We must account for some gameplay elements such as slider bodies, where this padding is not present.
        /// </summary>
        public const float LEGACY_CIRCLE_RADIUS = OsuHitObject.OBJECT_RADIUS - 5;

        /// <summary>
        /// The maximum allowed size of sprites that reside in the follow circle area of a slider.
        /// </summary>
        /// <remarks>
        /// The reason this is extracted out to a constant, rather than be inlined in the follow circle sprite retrieval,
        /// is that some skins will use `sliderb` elements to emulate a slider follow circle with slightly different visual effects applied
        /// (`sliderb` is always shown and doesn't pulsate; `sliderfollowcircle` isn't always shown and pulsates).
        /// </remarks>
        public static readonly Vector2 MAX_FOLLOW_CIRCLE_AREA_SIZE = OsuHitObject.OBJECT_DIMENSIONS * 3;

        private readonly ISkin skin;
        private readonly Lazy<bool> hasHitCircle;

        public OsuLegacySkinComponentProvider(ISkin skin)
        {
            this.skin = skin;
            hasHitCircle = new Lazy<bool>(() => skin.GetTexture("hitcircle") != null);
        }

        public bool IsProvidingLegacyResources => skin.HasFont(LegacyFont.Combo) || hasHitCircle.Value;

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

                        var combo = container.OfType<LegacyDefaultComboCounter>().FirstOrDefault();

                        if (combo != null)
                        {
                            combo.Anchor = Anchor.BottomLeft;
                            combo.Origin = Anchor.BottomLeft;
                            combo.Scale = new Vector2(1.28f);
                        }
                    })
                    {
                        Children = new Drawable[]
                        {
                            new LegacyDefaultComboCounter(),
                            new LegacyKeyCounterDisplay(),
                        }
                    };
            }

            return null;
        }

        public Drawable? Get(OsuSkinComponentLookup lookup)
        {
            switch (lookup.Component)
            {
                case OsuSkinComponents.FollowPoint:
                    return skin.GetAnimation("followpoint", true, true, true, startAtCurrentTime: false,
                        maxSize: new Vector2(OsuHitObject.OBJECT_RADIUS * 2, OsuHitObject.OBJECT_RADIUS));

                case OsuSkinComponents.SliderScorePoint:
                    return skin.GetAnimation("sliderscorepoint", false, false, maxSize: OsuHitObject.OBJECT_DIMENSIONS);

                case OsuSkinComponents.SliderFollowCircle:
                    var followCircleContent = skin.GetAnimation("sliderfollowcircle", true, true, true, maxSize: MAX_FOLLOW_CIRCLE_AREA_SIZE);
                    if (followCircleContent != null)
                        return new LegacyFollowCircle(followCircleContent);

                    return null;

                case OsuSkinComponents.SliderBall:
                    if (skin.GetTexture("sliderb") != null || skin.GetTexture("sliderb0") != null)
                        return new LegacySliderBall(this);

                    return null;

                case OsuSkinComponents.SliderBody:
                    if (hasHitCircle.Value)
                        return new LegacySliderBody();

                    return null;

                case OsuSkinComponents.SliderTailHitCircle:
                    if (hasHitCircle.Value)
                        return new LegacyMainCirclePiece("sliderendcircle", false);

                    return null;

                case OsuSkinComponents.SliderHeadHitCircle:
                    if (hasHitCircle.Value)
                        return new LegacySliderHeadHitCircle();

                    return null;

                case OsuSkinComponents.ReverseArrow:
                    if (hasHitCircle.Value)
                        return new LegacyReverseArrow();

                    return null;

                case OsuSkinComponents.HitCircle:
                    if (hasHitCircle.Value)
                        return new LegacyMainCirclePiece();

                    return null;

                case OsuSkinComponents.Cursor:
                    if (skin.GetTexture("cursor") != null)
                        return new LegacyCursor(this);

                    return null;

                case OsuSkinComponents.CursorTrail:
                    if (skin.GetTexture("cursortrail") != null)
                        return new LegacyCursorTrail(this);

                    return null;

                case OsuSkinComponents.CursorRipple:
                    if (skin.GetTexture("cursor-ripple") != null)
                    {
                        var ripple = skin.GetAnimation("cursor-ripple", false, false);

                        // In stable this element was scaled down to 50% and opacity 20%, but this makes the elements WAY too big and inflexible.
                        // If anyone complains about these not being applied, this can be uncommented.
                        //
                        // But if no one complains I'd rather fix this in lazer. Wiki documentation doesn't mention size,
                        // so we might be okay.
                        //
                        // if (ripple != null)
                        // {
                        //     ripple.Scale = new Vector2(0.5f);
                        //     ripple.Alpha = 0.2f;
                        // }

                        return ripple;
                    }

                    return null;

                case OsuSkinComponents.CursorParticles:
                    if (skin.GetTexture("star2") != null)
                        return new LegacyCursorParticles();

                    return null;

                case OsuSkinComponents.CursorSmoke:
                    if (skin.GetTexture("cursor-smoke") != null)
                        return new LegacySmokeSegment();

                    return null;

                case OsuSkinComponents.HitCircleText:
                    if (!skin.HasFont(LegacyFont.HitCircle))
                        return null;

                    const float hitcircle_text_scale = 0.8f;
                    return new LegacySpriteText(LegacyFont.HitCircle)
                    {
                        // stable applies a blanket 0.8x scale to hitcircle fonts
                        Scale = new Vector2(hitcircle_text_scale),
                        MaxSizePerGlyph = OsuHitObject.OBJECT_DIMENSIONS * 2 / hitcircle_text_scale,
                    };

                case OsuSkinComponents.SpinnerBody:
                    bool hasBackground = skin.GetTexture("spinner-background") != null;

                    if (skin.GetTexture("spinner-top") != null && !hasBackground)
                        return new LegacyNewStyleSpinner();

                    if (hasBackground)
                        return new LegacyOldStyleSpinner();

                    return null;

                case OsuSkinComponents.ApproachCircle:
                    if (skin.GetTexture(@"approachcircle") != null)
                        return new LegacyApproachCircle();

                    return null;

                default:
                    throw new UnsupportedSkinComponentException(lookup);
            }
        }

        public IBindable<TValue>? Get<TValue>(OsuSkinColour lookup) where TValue : notnull
        {
            return skin.GetConfig<SkinCustomColourLookup, TValue>(new SkinCustomColourLookup(lookup));
        }

        public IBindable<TValue>? Get<TValue>(OsuSkinConfiguration lookup) where TValue : notnull
        {
            switch (lookup)
            {
                case OsuSkinConfiguration.SliderPathRadius:
                    if (hasHitCircle.Value)
                        return SkinUtils.As<TValue>(new BindableFloat(LEGACY_CIRCLE_RADIUS));

                    break;

                case OsuSkinConfiguration.HitCircleOverlayAboveNumber:
                    // See https://osu.ppy.sh/help/wiki/Skinning/skin.ini#%5Bgeneral%5D
                    // HitCircleOverlayAboveNumer (with typo) should still be supported for now.
                    return skin.GetConfig<OsuSkinConfiguration, TValue>(OsuSkinConfiguration.HitCircleOverlayAboveNumber) ??
                           skin.GetConfig<OsuSkinConfiguration, TValue>(OsuSkinConfiguration.HitCircleOverlayAboveNumer);
            }

            return null;
        }

        public IComponentProvider Components => skin;
    }
}
