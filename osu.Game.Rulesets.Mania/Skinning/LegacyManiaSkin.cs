// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Audio.Sample;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Textures;
using osu.Game.Audio;
using osu.Game.Skinning;

namespace osu.Game.Rulesets.Mania.Skinning
{
    public class LegacyManiaSkin : ISkin
    {
        private readonly ISkin fallback;

        public LegacyManiaSkin(ISkin fallback)
        {
            this.fallback = fallback;
        }

        public Drawable GetDrawableComponent(ISkinComponent component) => fallback.GetDrawableComponent(component);

        public Texture GetTexture(string componentName) => fallback.GetTexture(componentName);

        public SampleChannel GetSample(ISampleInfo sampleInfo) => fallback.GetSample(sampleInfo);

        public IBindable<TValue> GetConfig<TLookup, TValue>(TLookup lookup)
        {
            // Todo:
            throw new System.NotImplementedException();
        }
    }
}
