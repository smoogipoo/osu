// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using OpenTK;
using osu.Framework.Configuration;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Game.Beatmaps;
using osu.Game.Beatmaps.ControlPoints;

namespace osu.Game.Screens.Edit.Screens.Compose.Timeline
{
    public class TickGraph : CompositeDrawable
    {
        public readonly Bindable<WorkingBeatmap> Beatmap = new Bindable<WorkingBeatmap>();

        public readonly BindableInt BeatDivisor = new BindableInt(1)
        {
            MinValue = 1,
            MaxValue = 32
        };

        private readonly Container<Tick> ticks;

        public TickGraph()
        {
            AddInternal(ticks = new Container<Tick> { RelativeSizeAxes = Axes.Both });

            Beatmap.ValueChanged += generateTicks;
        }

        private void generateTicks(WorkingBeatmap beatmap)
        {
            ticks.Clear();

            if (!beatmap.Track.IsLoaded)
            {
                Schedule(() => generateTicks(beatmap));
                return;
            }

            if (beatmap.Track?.IsLoaded != true)
                return;

            ticks.RelativeChildSize = new Vector2((float)beatmap.Track.Length, 1);

            ControlPointInfo cpi = beatmap.Beatmap.ControlPointInfo;
            for (int i = 0; i < cpi.TimingPoints.Count; i++)
            {
                double startTime = cpi.TimingPoints[i].Time;
                double endTime = i == cpi.TimingPoints.Count - 1 ? beatmap.Track.Length : cpi.TimingPoints[i + 1].Time;
                double beatLength = cpi.TimingPoints[i].BeatLength;

                double currentTime = startTime;
                int currentPoint = 0;
                while (currentTime < endTime)
                {
                    ticks.Add(new Tick(currentTime));

                    currentPoint++;
                    currentTime += beatLength / BeatDivisor;
                }
            }
        }

        private class Tick : CompositeDrawable
        {
            public Tick(double startTime)
            {
                RelativePositionAxes = Axes.X;
                X = (float)startTime;

                RelativeSizeAxes = Axes.Y;
                Width = 1;

                InternalChildren = new Drawable[]
                {
                    new Box { RelativeSizeAxes = Axes.Both }
                };
            }
        }
    }
}
