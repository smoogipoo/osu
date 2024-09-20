// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using JetBrains.Annotations;
using osu.Game.Extensions;
using osu.Game.IO;

namespace osu.Game.Skinning
{
    public partial class ArgonProSkin : ArgonSkin
    {
        public new static SkinInfo CreateInfo() => new SkinInfo
        {
            ID = Skinning.SkinInfo.ARGON_PRO_SKIN,
            Name = "osu! \"argon\" pro (2022)",
            Creator = "team osu!",
            Protected = true,
            InstantiationInfo = typeof(ArgonProSkin).GetInvariantInstantiationInfo()
        };

        public ArgonProSkin(IStorageResourceProvider resources)
            : this(CreateInfo(), resources)
        {
        }

        [UsedImplicitly(ImplicitUseKindFlags.InstantiatedWithFixedConstructorSignature)]
        public ArgonProSkin(SkinInfo skin, IStorageResourceProvider resources)
            : base(skin, resources)
        {
        }
    }
}
