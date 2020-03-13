// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Scoring;
using osuTK;

namespace osu.Game.Screens.Results
{
    public class ExpandedPanelMiddleContent : CompositeDrawable
    {
        private readonly ScoreInfo score;

        public ExpandedPanelMiddleContent(ScoreInfo score)
        {
            this.score = score;
            RelativeSizeAxes = Axes.Both;
            Masking = true;
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            InternalChild = new FillFlowContainer
            {
                RelativeSizeAxes = Axes.Both,
                Direction = FillDirection.Vertical,
                Y = 50,
                Children = new Drawable[]
                {
                    new AccuracyCircle(score)
                    {
                        Anchor = Anchor.TopCentre,
                        Origin = Anchor.TopCentre,
                        Size = new Vector2(320)
                    }
                }
            };
        }
    }
}
