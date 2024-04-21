// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Game.Skinning;

namespace osu.Game.Rulesets.Catch.Skinning.Argon
{
    public class CatchArgonSkinTransformer : SkinTransformer
    {
        public CatchArgonSkinTransformer(ISkin skin)
            : base(skin)
        {
        }

        public override T? GetDrawableComponent<T>(ISkinComponentLookup lookup)
            where T : class
        {
            switch (lookup)
            {
                case CatchSkinComponentLookup catchComponent:
                    // TODO: Once everything is finalised, consider throwing UnsupportedSkinComponentException on missing entries.
                    switch (catchComponent.Component)
                    {
                        case CatchSkinComponents.HitExplosion:
                            return SkinUtils.As<T>(base.GetDrawableComponent<ArgonHitExplosion>(lookup) ?? new ArgonHitExplosion());

                        case CatchSkinComponents.Catcher:
                            return SkinUtils.As<T>(base.GetDrawableComponent<ArgonCatcher>(lookup) ?? new ArgonCatcher());

                        case CatchSkinComponents.Fruit:
                            return SkinUtils.As<T>(base.GetDrawableComponent<ArgonFruitPiece>(lookup) ?? new ArgonFruitPiece());

                        case CatchSkinComponents.Banana:
                            return SkinUtils.As<T>(base.GetDrawableComponent<ArgonBananaPiece>(lookup) ?? new ArgonBananaPiece());

                        case CatchSkinComponents.Droplet:
                            return SkinUtils.As<T>(base.GetDrawableComponent<ArgonDropletPiece>(lookup) ?? new ArgonDropletPiece());
                    }

                    break;
            }

            return base.GetDrawableComponent<T>(lookup);
        }
    }
}
