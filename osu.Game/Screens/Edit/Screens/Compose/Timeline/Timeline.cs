// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.Allocation;
using osu.Framework.Configuration;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Input;
using osu.Framework.Timing;
using osu.Game.Beatmaps;
using osu.Game.Graphics;

namespace osu.Game.Screens.Edit.Screens.Compose.Timeline
{
    public class Timeline : ZoomableScrollContainer
    {
        public readonly Bindable<bool> WaveformVisible = new Bindable<bool>();
        public readonly Bindable<WorkingBeatmap> Beatmap = new Bindable<WorkingBeatmap>();

        private readonly IAdjustableClock adjustableClock;

        public Timeline(IAdjustableClock adjustableClock)
        {
            this.adjustableClock = adjustableClock;

            BeatmapWaveformGraph waveform;
            Child = waveform = new BeatmapWaveformGraph
            {
                RelativeSizeAxes = Axes.Both,
                Colour = OsuColour.FromHex("222"),
                Depth = float.MaxValue
            };

            waveform.Beatmap.BindTo(Beatmap);

            WaveformVisible.ValueChanged += visible => waveform.FadeTo(visible ? 1 : 0, 200, Easing.OutQuint);
        }

        [BackgroundDependencyLoader]
        private void load(OsuColour colours)
        {
            AddInternal(new Container
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                RelativeSizeAxes = Axes.Y,
                Width = 2,
                Colour = colours.Red,
                Child = new Box { RelativeSizeAxes = Axes.Both }
            });
        }

        protected override void Update()
        {
            base.Update();

            // We want time = 0 to be at the centre of the container when scrolled to the start
            Content.Margin = new MarginPadding { Horizontal = DrawWidth / 2 };
        }

        protected override bool OnWheel(InputState state)
        {
            if (!state.Keyboard.ControlPressed)
                return base.OnWheel(state);

            if (adjustableClock.IsRunning)
            {
                // Bypass base to zoom while focusing on the centre point
                Zoom += state.Mouse.WheelDelta;
                return true;
            }

            return base.OnWheel(state);
        }
    }
}
