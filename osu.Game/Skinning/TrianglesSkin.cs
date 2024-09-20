// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using JetBrains.Annotations;
using osu.Game.Extensions;
using osu.Game.IO;

namespace osu.Game.Skinning
{
    public partial class TrianglesSkin : Skin
    {
        public static SkinInfo CreateInfo() => new SkinInfo
        {
            ID = osu.Game.Skinning.SkinInfo.TRIANGLES_SKIN,
            Name = "osu! \"triangles\" (2017)",
            Creator = "team osu!",
            Protected = true,
            InstantiationInfo = typeof(TrianglesSkin).GetInvariantInstantiationInfo()
        };

        private readonly IStorageResourceProvider resources;

        public TrianglesSkin(IStorageResourceProvider resources)
            : this(CreateInfo(), resources)
        {
        }

        [UsedImplicitly(ImplicitUseKindFlags.InstantiatedWithFixedConstructorSignature)]
        public TrianglesSkin(SkinInfo skin, IStorageResourceProvider resources)
            : base(skin, resources)
        {
            this.resources = resources;
        }
    }
}
