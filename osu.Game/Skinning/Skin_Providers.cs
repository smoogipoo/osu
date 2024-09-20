// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;

namespace osu.Game.Skinning
{
    public partial class Skin : IDrawableProvider<SkinnableSprite.SpriteComponentLookup>,
                                IDrawableProvider<UserSkinComponentLookup>
    {
        internal virtual Drawable? Get(SkinnableSprite.SpriteComponentLookup lookup)
            => this.GetAnimation(lookup.LookupName, false, false, maxSize: lookup.MaxSize);

        internal virtual Drawable? Get(UserSkinComponentLookup lookup)
        {
            switch (lookup.Component)
            {
                case GlobalSkinnableContainerLookup containerLookup:
                    // It is important to return null if the user has not configured this yet.
                    // This allows skin transformers the opportunity to provide default components.
                    if (!LayoutInfos.TryGetValue(containerLookup.Lookup, out var layoutInfo)) return null;
                    if (!layoutInfo.TryGetDrawableInfo(containerLookup.Ruleset, out var drawableInfos)) return null;

                    return new Container
                    {
                        RelativeSizeAxes = Axes.Both,
                        ChildrenEnumerable = drawableInfos.Select(i => i.CreateInstance())
                    };
            }

            return null;
        }

        Drawable? IDrawableProvider<SkinnableSprite.SpriteComponentLookup>.Get(SkinnableSprite.SpriteComponentLookup lookup)
            => Get(lookup);

        Drawable? IDrawableProvider<UserSkinComponentLookup>.Get(UserSkinComponentLookup lookup)
            => Get(lookup);
    }
}
