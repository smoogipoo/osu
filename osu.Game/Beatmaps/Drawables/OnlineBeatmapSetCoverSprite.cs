// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Allocation;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.Textures;

namespace osu.Game.Beatmaps.Drawables
{
    /// <summary>
    /// A <see cref="Sprite"/> that displays the cover image for a beatmap set.
    /// </summary>
    [LongRunningLoad]
    public partial class OnlineBeatmapSetCoverSprite : Sprite
    {
        private readonly IBeatmapSetOnlineInfo set;
        private readonly BeatmapSetCoverType type;

        /// <summary>
        /// Creates a new <see cref="OnlineBeatmapSetCoverSprite"/>.
        /// </summary>
        /// <param name="set">The beatmap set to display the cover image.</param>
        /// <param name="type">The type of cover image to display.</param>
        public OnlineBeatmapSetCoverSprite(IBeatmapSetOnlineInfo set, BeatmapSetCoverType type = BeatmapSetCoverType.Cover)
        {
            ArgumentNullException.ThrowIfNull(set);

            this.set = set;
            this.type = type;
        }

        [BackgroundDependencyLoader]
        private void load(LargeTextureStore textures)
        {
            string? resource = null;

            switch (type)
            {
                case BeatmapSetCoverType.Cover:
                    resource = set.Covers.Cover;
                    break;

                case BeatmapSetCoverType.Card:
                    resource = set.Covers.Card;
                    break;

                case BeatmapSetCoverType.List:
                    resource = set.Covers.List;
                    break;
            }

            if (resource != null)
                Texture = textures.Get(resource);
        }
    }

    public enum BeatmapSetCoverType
    {
        Cover,
        Card,
        List,
    }
}
