// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Graphics;
using osu.Game.Skinning;

namespace osu.Game.Rulesets.Catch.Skinning.Argon
{
    public class CatchArgonSkinComponentProvider : IDrawableProvider<CatchSkinComponentLookup>
    {
        public Drawable? Get(CatchSkinComponentLookup lookup)
        {
            // TODO: Once everything is finalised, consider throwing UnsupportedSkinComponentException on missing entries.
            switch (lookup.Component)
            {
                case CatchSkinComponents.HitExplosion:
                    return new ArgonHitExplosion();

                case CatchSkinComponents.Catcher:
                    return new ArgonCatcher();

                case CatchSkinComponents.Fruit:
                    return new ArgonFruitPiece();

                case CatchSkinComponents.Banana:
                    return new ArgonBananaPiece();

                case CatchSkinComponents.Droplet:
                    return new ArgonDropletPiece();
            }

            return null;
        }
    }
}
