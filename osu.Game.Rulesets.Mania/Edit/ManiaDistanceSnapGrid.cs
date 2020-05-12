// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Shapes;
using osu.Game.Beatmaps.ControlPoints;
using osu.Game.Rulesets.Mania.UI;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Objects.Drawables;
using osu.Game.Rulesets.UI.Scrolling;
using osu.Game.Screens.Edit;
using osu.Game.Screens.Edit.Compose.Components;
using osuTK;

namespace osu.Game.Rulesets.Mania.Edit
{
    public class ManiaDistanceSnapGrid : DistanceSnapGrid
    {
        [Resolved]
        private ManiaHitObjectComposer maniaComposer { get; set; }

        public ManiaDistanceSnapGrid()
            : base(Vector2.Zero, 0)
        {
        }

        protected override void CreateContent()
        {
            ClearInternal();

            foreach (var stage in maniaComposer.Playfield.Stages)
                AddInternal(new Grid(stage));
        }

        public override (Vector2 position, double time) GetSnappedPosition(Vector2 position)
        {
            return (position, 0);
        }

        private class Grid : ScrollingHitObjectContainer
        {
            [Resolved]
            private EditorBeatmap beatmap { get; set; }

            private readonly Stage stage;

            public Grid(Stage stage)
            {
                this.stage = stage;
            }

            [BackgroundDependencyLoader]
            private void load()
            {
                for (int i = 0; i < beatmap.ControlPointInfo.TimingPoints.Count; i++)
                {
                    TimingControlPoint current = beatmap.ControlPointInfo.TimingPoints[i];
                    TimingControlPoint next = i < beatmap.ControlPointInfo.TimingPoints.Count - 1 ? beatmap.ControlPointInfo.TimingPoints[i + 1] : null;

                    if (next == null)
                        break; // Todo:

                    for (double x = current.Time; x < next.Time; x += current.BeatLength / beatmap.BeatDivisor)
                        Add(new DrawableGridLine(x));
                }
            }

            protected override void Update()
            {
                base.Update();

                var parentQuad = Parent.ToLocalSpace(stage.ScreenSpaceDrawQuad);
                Position = parentQuad.TopLeft;
                Size = parentQuad.Size;
            }
        }

        private class DrawableGridLine : DrawableHitObject
        {
            public DrawableGridLine(double startTime)
                : base(new HitObject { StartTime = startTime })
            {
                RelativeSizeAxes = Axes.X;
                Height = 2;

                InternalChild = new Box
                {
                    RelativeSizeAxes = Axes.Both
                };
            }
        }
    }
}
