// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Audio;
using osu.Framework.Audio.Sample;
using osu.Framework.Graphics.Textures;
using osu.Framework.IO.Stores;
using osu.Framework.Platform;
using osu.Game.Audio;

namespace osu.Game.Skinning
{
    /// <summary>
    /// An <see cref="ISkin"/> that uses an underlying <see cref="IResourceStore{T}"/> with namespaces for resources retrieval.
    /// </summary>
    public class ResourceStoreBackedSkin : ISkinWithExternalComponents,
                                           IDisposable,
                                           ITextureProvider,
                                           ISampleProvider
    {
        private readonly TextureStore textures;
        private readonly ISampleStore samples;

        public ResourceStoreBackedSkin(IResourceStore<byte[]> resources, GameHost host, AudioManager audio)
        {
            textures = new TextureStore(host.Renderer, host.CreateTextureLoaderStore(new NamespacedResourceStore<byte[]>(resources, @"Textures")));
            samples = audio.GetSampleStore(new NamespacedResourceStore<byte[]>(resources, @"Samples"));
        }

        public Texture? Get(string lookup, WrapMode wrapModeS, WrapMode wrapModeT)
            => textures.Get(lookup, wrapModeS, wrapModeT);

        public ISample? Get(ISampleInfo lookup)
        {
            foreach (string? lookupName in lookup.LookupNames)
            {
                ISample? sample = samples.Get(lookupName);
                if (sample != null)
                    return sample;
            }

            return null;
        }

        public IComponentProvider Components => this;

        public void Dispose()
        {
            textures.Dispose();
            samples.Dispose();
        }
    }
}
