// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using System.IO;
using JetBrains.Annotations;
using osu.Framework.Graphics.Textures;
using osu.Framework.IO.Stores;
using osu.Game.IO;

namespace osu.Game.Skinning
{
    public partial class LegacySkin : Skin
    {
        private readonly Dictionary<int, LegacyManiaSkinConfiguration> maniaConfigurations = new Dictionary<int, LegacyManiaSkinConfiguration>();

        [UsedImplicitly(ImplicitUseKindFlags.InstantiatedWithFixedConstructorSignature)]
        public LegacySkin(SkinInfo skin, IStorageResourceProvider resources)
            : this(skin, resources, null)
        {
        }

        /// <summary>
        /// Construct a new legacy skin instance.
        /// </summary>
        /// <param name="skin">The model for this skin.</param>
        /// <param name="resources">Access to raw game resources.</param>
        /// <param name="fallbackStore">An optional fallback store which will be used for file lookups that are not serviced by realm user storage.</param>
        /// <param name="configurationFilename">The user-facing filename of the configuration file to be parsed. Can accept an .osu or skin.ini file.</param>
        protected LegacySkin(SkinInfo skin, IStorageResourceProvider? resources, IResourceStore<byte[]>? fallbackStore, string configurationFilename = @"skin.ini")
            : base(skin, resources, fallbackStore, configurationFilename)
        {
        }

        protected override IResourceStore<TextureUpload> CreateTextureLoaderStore(IStorageResourceProvider resources, IResourceStore<byte[]> storage)
            => new LegacyTextureLoaderStore(base.CreateTextureLoaderStore(resources, storage));

        protected override void ParseConfigurationStream(Stream stream)
        {
            base.ParseConfigurationStream(stream);

            stream.Seek(0, SeekOrigin.Begin);

            using (LineBufferedReader reader = new LineBufferedReader(stream))
            {
                var maniaList = new LegacyManiaSkinDecoder().Decode(reader);

                foreach (var config in maniaList)
                    maniaConfigurations[config.Keys] = config;
            }
        }
    }
}
