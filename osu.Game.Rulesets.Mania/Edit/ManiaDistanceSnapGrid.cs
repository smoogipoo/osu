// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Rulesets.Mania.UI;
using osu.Game.Rulesets.UI.Scrolling;
using osu.Game.Screens.Edit;
using osu.Game.Screens.Edit.Compose.Components;
using osuTK;

namespace osu.Game.Rulesets.Mania.Edit
{
    public class ManiaDistanceSnapGrid : DistanceSnapGrid
    {
        [Resolved]
        private IScrollingInfo scrollingInfo { get; set; }

        [Resolved]
        private EditorClock editorClock { get; set; }

        private readonly IBindable<ScrollingDirection> direction = new Bindable<ScrollingDirection>();
        private readonly IBindable<double> timeRange = new BindableDouble();

        private Container gridContainer;

        public ManiaDistanceSnapGrid()
            : base(Vector2.Zero, 0)
        {
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            direction.BindTo(scrollingInfo.Direction);
            timeRange.BindTo(scrollingInfo.TimeRange);

            direction.BindValueChanged(onDirectionChanged, true);
            timeRange.BindValueChanged(onTimeRangeChanged, true);
        }

        private void onDirectionChanged(ValueChangedEvent<ScrollingDirection> direction)
        {
            if (direction.NewValue == ScrollingDirection.Up)
            {
                gridContainer.Anchor = Anchor.TopLeft;
                gridContainer.Origin = Anchor.TopLeft;

                foreach (var line in gridContainer)
                {
                    line.Anchor = Anchor.TopLeft;
                    line.Origin = Anchor.TopLeft;
                }
            }
            else
            {
                gridContainer.Anchor = Anchor.BottomLeft;
                gridContainer.Origin = Anchor.BottomLeft;

                foreach (var line in gridContainer)
                {
                    line.Anchor = Anchor.BottomLeft;
                    line.Origin = Anchor.BottomLeft;
                }
            }
        }

        private void onTimeRangeChanged(ValueChangedEvent<double> timeRange) => RelativeChildSize = new Vector2(1, (float)timeRange.NewValue);

        protected override void CreateContent()
        {
        }

        protected override void Update()
        {
            base.Update();

        }

        public override (Vector2 position, double time) GetSnappedPosition(Vector2 position)
        {
            return (position, 0);
        }

        private class Grid : CompositeDrawable
        {
            private readonly Stage stage;

            public Grid(Stage stage)
            {
                this.stage = stage;
            }
        }
    }
}
