// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Audio.Sample;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Textures;
using osu.Game.Audio;
using osu.Game.Beatmaps;
using osu.Game.Rulesets;

namespace osu.Game.Skinning
{
    public sealed class RulesetTransformedSkin : ISkin, ILegacyComponentProvider
    {
        public readonly ISkin OriginalSkin;
        private readonly IComponentProvider components;

        public RulesetTransformedSkin(ISkin skin, IBeatmap beatmap, Ruleset ruleset)
        {
            OriginalSkin = skin;
            components = ruleset.CreateSkinComponentProvider(skin, beatmap);
        }

        bool ILegacyComponentProvider.IsProvidingLegacyResources
            => (components as ILegacyComponentProvider)?.IsProvidingLegacyResources
               ?? false;

        Drawable? ISkin.GetDrawableComponent<TComponent>(TComponent lookup)
            => (components as IDrawableProvider<TComponent>)?.Get(lookup)
               ?? OriginalSkin.GetDrawableComponent(lookup);

        Texture? ISkin.GetTexture(string componentName, WrapMode wrapModeS, WrapMode wrapModeT)
            => (components as ITextureProvider)?.Get(componentName, wrapModeS, wrapModeT)
               ?? OriginalSkin.GetTexture(componentName, wrapModeS, wrapModeT);

        ISample? ISkin.GetSample(ISampleInfo sampleInfo)
            => (components as ISampleProvider)?.Get(sampleInfo)
               ?? OriginalSkin.GetSample(sampleInfo);

        IBindable<TValue>? ISkin.GetConfig<TLookup, TValue>(TLookup lookup)
            => (components as IConfigProvider<TLookup>)?.Get<TValue>(lookup)
               ?? OriginalSkin.GetConfig<TLookup, TValue>(lookup);
    }
}
