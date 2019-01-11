// Copyright (c) 2007-2019 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.Graphics;
using osu.Game.Beatmaps.Drawables;

namespace osu.Game.Screens.Multi.Components
{
    public class MultiplayerBackgroundSprite : MultiplayerComposite
    {
        public MultiplayerBackgroundSprite()
        {
            UpdateableBeatmapBackgroundSprite background;

            InternalChild = background = new UpdateableBeatmapBackgroundSprite { RelativeSizeAxes = Axes.Both };

            background.Beatmap.BindTo(CurrentBeatmap);
        }

        protected virtual UpdateableBeatmapBackgroundSprite CreateBackgroundSprite() => new UpdateableBeatmapBackgroundSprite { RelativeSizeAxes = Axes.Both };
    }
}
