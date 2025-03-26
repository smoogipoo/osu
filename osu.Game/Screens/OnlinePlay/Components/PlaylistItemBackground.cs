// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Graphics.Sprites;
using osu.Game.Beatmaps;
using osu.Game.Beatmaps.Drawables;
using osu.Game.Graphics.Backgrounds;
using osu.Game.Online.Rooms;

namespace osu.Game.Screens.OnlinePlay.Components
{
    public partial class PlaylistItemBackground : Background
    {
        public readonly IBeatmapInfo? Beatmap;

        public PlaylistItemBackground(PlaylistItem? playlistItem)
        {
            Beatmap = playlistItem?.Beatmap;
        }

        protected override Sprite CreateSprite()
        {
            if (Beatmap == null)
                return new DefaultBeatmapBackgroundSprite();

            return new OnlineBeatmapCoverSprite(Beatmap.OnlineID);
        }

        public override bool Equals(Background? other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;

            return other.GetType() == GetType()
                   && ((PlaylistItemBackground)other).Beatmap == Beatmap;
        }
    }
}
