// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Audio.Sample;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Textures;
using osu.Game.Audio;

namespace osu.Game.Skinning
{
    /// <summary>
    /// Provides access to various elements contained by a skin.
    /// </summary>
    public interface ISkin : IComponentProvider
    {
        /// <summary>
        /// Retrieve a <see cref="Drawable"/> component implementation.
        /// </summary>
        /// <param name="lookup">The requested component.</param>
        /// <returns>A drawable representation for the requested component, or null if unavailable.</returns>
        Drawable? GetDrawableComponent<TComponent>(TComponent lookup)
            where TComponent : ISkinComponentLookup
            => (this as IDrawableProvider<TComponent>)?.Get(lookup);

        /// <summary>
        /// Retrieve a <see cref="Texture"/>.
        /// </summary>
        /// <param name="componentName">The requested texture.</param>
        /// <param name="wrapModeS">The texture wrap mode in horizontal direction.</param>
        /// <param name="wrapModeT">The texture wrap mode in vertical direction.</param>
        /// <returns>A matching texture, or null if unavailable.</returns>
        Texture? GetTexture(string componentName, WrapMode wrapModeS = default, WrapMode wrapModeT = default)
            => (this as ITextureProvider)?.Get(componentName, wrapModeS, wrapModeT);

        /// <summary>
        /// Retrieve a <see cref="SampleChannel"/>.
        /// </summary>
        /// <param name="sampleInfo">The requested sample.</param>
        /// <returns>A matching sample channel, or null if unavailable.</returns>
        ISample? GetSample(ISampleInfo sampleInfo)
            => (this as ISampleProvider)?.Get(sampleInfo);

        /// <summary>
        /// Retrieve a configuration value.
        /// </summary>
        /// <remarks>
        /// Note that while this returns a bindable value, it is not actually updated.
        /// Until the API is fixed, just use the received bindable's <see cref="IBindable{TValue}.Value"/> immediately.</remarks>
        /// <param name="lookup">The requested configuration value.</param>
        /// <returns>A matching value boxed in an <see cref="IBindable{TValue}"/>, or null if unavailable.</returns>
        IBindable<TValue>? GetConfig<TLookup, TValue>(TLookup lookup)
            where TLookup : notnull
            where TValue : notnull
            => (this as IConfigProvider<TLookup>)?.Get<TValue>(lookup);
    }

    public interface ISkinWithExternalComponents : ISkin
    {
        /// <summary>
        /// The external provider of skin components.
        /// This is used if the component is not provided by this <see cref="ISkin"/> instance.
        /// </summary>
        IComponentProvider Components { get; }

        Drawable? ISkin.GetDrawableComponent<TComponent>(TComponent lookup)
            => (this as IDrawableProvider<TComponent>)?.Get(lookup)
               ?? (Components as IDrawableProvider<TComponent>)?.Get(lookup);

        Texture? ISkin.GetTexture(string componentName, WrapMode wrapModeS, WrapMode wrapModeT)
            => (this as ITextureProvider)?.Get(componentName, wrapModeS, wrapModeT)
               ?? (Components as ITextureProvider)?.Get(componentName, wrapModeS, wrapModeT);

        ISample? ISkin.GetSample(ISampleInfo sampleInfo)
            => (this as ISampleProvider)?.Get(sampleInfo)
               ?? (Components as ISampleProvider)?.Get(sampleInfo);

        IBindable<TValue>? ISkin.GetConfig<TLookup, TValue>(TLookup lookup)
            => (this as IConfigProvider<TLookup>)?.Get<TValue>(lookup)
               ?? (Components as IConfigProvider<TLookup>)?.Get<TValue>(lookup);
    }

    public static class SkinExtensions
    {
        /// <summary>
        /// Retrieve a <see cref="Drawable"/> component implementation.
        /// </summary>
        /// <param name="skin">The skin.</param>
        /// <param name="lookup">The requested component.</param>
        /// <returns>A drawable representation for the requested component, or null if unavailable.</returns>
        public static Drawable? GetDrawableComponent<TComponent>(this ISkin? skin, TComponent lookup)
            where TComponent : ISkinComponentLookup
            => skin?.GetDrawableComponent(lookup);

        /// <summary>
        /// Retrieve a <see cref="Texture"/>.
        /// </summary>
        /// <param name="skin">The skin.</param>
        /// <param name="componentName">The requested texture.</param>
        /// <param name="wrapModeS">The texture wrap mode in horizontal direction.</param>
        /// <param name="wrapModeT">The texture wrap mode in vertical direction.</param>
        /// <returns>A matching texture, or null if unavailable.</returns>
        public static Texture? GetTexture(this ISkin? skin, string componentName, WrapMode wrapModeS = default, WrapMode wrapModeT = default)
            => skin?.GetTexture(componentName, wrapModeS, wrapModeT);

        /// <summary>
        /// Retrieve a <see cref="SampleChannel"/>.
        /// </summary>
        /// <param name="skin">The skin.</param>
        /// <param name="sampleInfo">The requested sample.</param>
        /// <returns>A matching sample channel, or null if unavailable.</returns>
        public static ISample? GetSample(this ISkin? skin, ISampleInfo sampleInfo)
            => skin?.GetSample(sampleInfo);

        /// <summary>
        /// Retrieve a configuration value.
        /// </summary>
        /// <remarks>
        /// Note that while this returns a bindable value, it is not actually updated.
        /// Until the API is fixed, just use the received bindable's <see cref="IBindable{TValue}.Value"/> immediately.</remarks>
        /// <param name="skin">The skin.</param>
        /// <param name="lookup">The requested configuration value.</param>
        /// <returns>A matching value boxed in an <see cref="IBindable{TValue}"/>, or null if unavailable.</returns>
        public static IBindable<TValue>? GetConfig<TLookup, TValue>(this ISkin? skin, TLookup lookup)
            where TLookup : notnull
            where TValue : notnull
            => skin?.GetConfig<TLookup, TValue>(lookup);
    }
}
