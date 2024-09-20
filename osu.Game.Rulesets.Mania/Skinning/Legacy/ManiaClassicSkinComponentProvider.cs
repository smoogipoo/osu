// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Bindables;
using osu.Game.Beatmaps;
using osu.Game.Skinning;
using osuTK.Graphics;

namespace osu.Game.Rulesets.Mania.Skinning.Legacy
{
    public class ManiaClassicSkinComponentProvider : ManiaLegacySkinComponentProvider
    {
        public ManiaClassicSkinComponentProvider(ISkin skin, IBeatmap beatmap)
            : base(skin, beatmap)
        {
        }

        public override IBindable<TValue>? Get<TValue>(ManiaSkinConfigurationLookup lookup)
        {
            if (base.Get<TValue>(lookup) is IBindable<TValue> baseLookup)
                return baseLookup;

            // default provisioning.
            switch (lookup.Lookup)
            {
                case LegacyManiaSkinConfigurationLookups.ColumnBackgroundColour:
                    return SkinUtils.As<TValue>(new Bindable<Color4>(Color4.Black));
            }

            return null;
        }
    }
}
