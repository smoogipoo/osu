// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Extensions.ObjectExtensions;
using osu.Framework.IO.Stores;
using osu.Game.Beatmaps;
using osu.Game.Database;
using osu.Game.IO;

namespace osu.Game.Skinning
{
    public partial class LegacyBeatmapSkin : LegacySkin
    {
        /// <summary>
        /// Construct a new legacy beatmap skin instance.
        /// </summary>
        /// <param name="beatmapInfo">The model for this beatmap.</param>
        /// <param name="resources">Access to raw game resources.</param>
        public LegacyBeatmapSkin(BeatmapInfo beatmapInfo, IStorageResourceProvider? resources)
            : base(createSkinInfo(beatmapInfo), resources, createRealmBackedStore(beatmapInfo, resources), beatmapInfo.Path.AsNonNull())
        {
            // Disallow default colours fallback on beatmap skins to allow using parent skin combo colours. (via SkinProvidingContainer)
            Configuration.AllowDefaultComboColoursFallback = false;
        }

        private static IResourceStore<byte[]> createRealmBackedStore(BeatmapInfo beatmapInfo, IStorageResourceProvider? resources)
        {
            if (resources == null || beatmapInfo.BeatmapSet == null)
                // should only ever be used in tests.
                return new ResourceStore<byte[]>();

            return new RealmBackedResourceStore<BeatmapSetInfo>(beatmapInfo.BeatmapSet.ToLive(resources.RealmAccess), resources.Files, resources.RealmAccess);
        }

        private static SkinInfo createSkinInfo(BeatmapInfo beatmapInfo) =>
            new SkinInfo
            {
                Name = beatmapInfo.ToString(),
                Creator = beatmapInfo.Metadata.Author.Username
            };
    }
}
