// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using JetBrains.Annotations;
using osu.Game.Extensions;
using osu.Game.IO;
using osuTK.Graphics;

namespace osu.Game.Skinning
{
    public partial class ArgonSkin : Skin
    {
        public static SkinInfo CreateInfo() => new SkinInfo
        {
            ID = Skinning.SkinInfo.ARGON_SKIN,
            Name = "osu! \"argon\" (2022)",
            Creator = "team osu!",
            Protected = true,
            InstantiationInfo = typeof(ArgonSkin).GetInvariantInstantiationInfo()
        };

        protected readonly IStorageResourceProvider Resources;

        public ArgonSkin(IStorageResourceProvider resources)
            : this(CreateInfo(), resources)
        {
        }

        [UsedImplicitly(ImplicitUseKindFlags.InstantiatedWithFixedConstructorSignature)]
        public ArgonSkin(SkinInfo skin, IStorageResourceProvider resources)
            : base(
                skin,
                resources
            )
        {
            Resources = resources;

            Configuration.CustomComboColours = new List<Color4>
            {
                // Standard combo progression order is green - blue - red - yellow.
                // But for whatever reason, this starts from index 1, not 0.
                //
                // We've added two new combo colours in argon, so to ensure the initial rotation matches,
                // this same progression is in slots 1 - 4.

                // Orange
                new Color4(241, 116, 0, 255),
                // Green
                new Color4(0, 241, 53, 255),
                // Blue
                new Color4(0, 82, 241, 255),
                // Red
                new Color4(241, 0, 0, 255),
                // Yellow
                new Color4(232, 235, 0, 255),
                // Purple
                new Color4(92, 0, 241, 255),
            };
        }
    }
}
