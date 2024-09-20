// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Audio.Sample;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Textures;
using osu.Game.Audio;

namespace osu.Game.Skinning
{
    public interface IComponentProvider;

    public interface IDrawableProvider<in TComponent> : IComponentProvider
        where TComponent : ISkinComponentLookup
    {
        Drawable? Get(TComponent lookup);
    }

    public interface ITextureProvider : IComponentProvider
    {
        Texture? Get(string lookup, WrapMode wrapModeS, WrapMode wrapModeT);
    }

    public interface ISampleProvider : IComponentProvider
    {
        ISample? Get(ISampleInfo lookup);
    }

    public interface IConfigProvider<in TKey> : IComponentProvider
        where TKey : notnull
    {
        IBindable<TValue>? Get<TValue>(TKey lookup) where TValue : notnull;
    }

    public interface ILegacyComponentProvider : IComponentProvider
    {
        bool IsProvidingLegacyResources { get; }
    }
}
