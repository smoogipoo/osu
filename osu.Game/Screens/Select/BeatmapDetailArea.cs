// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Linq;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Beatmaps;
using osu.Game.Screens.Select.Details;

namespace osu.Game.Screens.Select
{
    public abstract class BeatmapDetailArea : Container
    {
        private const float details_padding = 10;

        private readonly Container content;
        protected override Container<Drawable> Content => content;

        public readonly BeatmapDetails Details;

        private WorkingBeatmap beatmap;

        public virtual WorkingBeatmap Beatmap
        {
            get => beatmap;
            set
            {
                beatmap = value;
                Details.Beatmap = beatmap?.BeatmapInfo;
            }
        }

        protected BeatmapDetailArea()
        {
            AddRangeInternal(new Drawable[]
            {
                content = new Container
                {
                    RelativeSizeAxes = Axes.Both,
                    Padding = new MarginPadding { Top = BeatmapDetailAreaTabControl.HEIGHT },
                },
                new BeatmapDetailAreaTabControl
                {
                    RelativeSizeAxes = Axes.X,
                    TabItems = CreateTabItems().Prepend(new BeatmapDetailsAreaDetailsTabItem()).ToArray(),
                    OnFilter = OnFilter
                },
            });

            Add(Details = new BeatmapDetails
            {
                RelativeSizeAxes = Axes.X,
                Alpha = 0,
                Margin = new MarginPadding { Top = details_padding },
            });
        }

        protected override void UpdateAfterChildren()
        {
            base.UpdateAfterChildren();

            Details.Height = Math.Min(DrawHeight - details_padding * 3 - BeatmapDetailAreaTabControl.HEIGHT, 450);
        }

        protected virtual void OnFilter(BeatmapDetailsAreaTabItem tab, bool selectedMods)
        {
            switch (tab)
            {
                case BeatmapDetailsAreaDetailsTabItem _:
                    Details.Show();
                    break;

                default:
                    Details.Hide();
                    break;
            }
        }

        protected abstract BeatmapDetailsAreaTabItem[] CreateTabItems();
    }
}
