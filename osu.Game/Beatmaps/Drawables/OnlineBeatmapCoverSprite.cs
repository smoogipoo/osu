// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Threading;
using osu.Framework.Allocation;
using osu.Framework.Extensions;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.Textures;
using osu.Game.Database;
using osu.Game.Online.API.Requests.Responses;

namespace osu.Game.Beatmaps.Drawables
{
    /// <summary>
    /// A <see cref="Sprite"/> that queries for a beatmap and displays its cover image.
    /// </summary>
    [LongRunningLoad]
    public partial class OnlineBeatmapCoverSprite : Sprite
    {
        private readonly int beatmapId;
        private readonly BeatmapSetCoverType type;

        /// <summary>
        /// Creates a new <see cref="OnlineBeatmapCoverSprite"/>.
        /// </summary>
        /// <param name="beatmapId">The beatmap to query the cover image for.</param>
        /// <param name="type">The type of cover image to display.</param>
        public OnlineBeatmapCoverSprite(int beatmapId, BeatmapSetCoverType type = BeatmapSetCoverType.Cover)
        {
            this.beatmapId = beatmapId;
            this.type = type;
        }

        [BackgroundDependencyLoader]
        private void load(BeatmapLookupCache lookupCache, LargeTextureStore textures, CancellationToken cancellationToken)
        {
            try
            {
                APIBeatmap? apiBeatmap = lookupCache.GetBeatmapAsync(beatmapId, cancellationToken).GetResultSafely();

                string? resource = null;

                switch (type)
                {
                    case BeatmapSetCoverType.Cover:
                        resource = apiBeatmap?.BeatmapSet?.Covers.Cover;
                        break;

                    case BeatmapSetCoverType.Card:
                        resource = apiBeatmap?.BeatmapSet?.Covers.Card;
                        break;

                    case BeatmapSetCoverType.List:
                        resource = apiBeatmap?.BeatmapSet?.Covers.List;
                        break;
                }

                if (resource != null)
                    Texture = textures.Get(resource);
            }
            catch
            {
            }
        }
    }
}
