// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using System.Collections.Generic;
using NUnit.Framework;
using osu.Framework.Allocation;
using OpenTK;
using osu.Game.Beatmaps;
using osu.Game.Rulesets.Edit;
using osu.Game.Rulesets.Edit.Layers.DragSelection;
using osu.Game.Rulesets.Edit.Layers.HitObjectSelection;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Osu;
using osu.Game.Rulesets.Osu.Edit;
using osu.Game.Rulesets.Osu.Edit.Layers.HitObjectSelection;
using osu.Game.Rulesets.Osu.Edit.Layers.HitObjectSelection.Overlays;
using osu.Game.Rulesets.Osu.Objects;
using osu.Game.Tests.Beatmaps;

namespace osu.Game.Tests.Visual
{
    [TestFixture]
    public class TestCaseEditorSelectionLayer : OsuTestCase
    {
        public override IReadOnlyList<Type> RequiredTypes => new[]
        {
            typeof(DragSelectionBox),
            typeof(DragSelectionLayer),
            typeof(SelectionOverlay),
            typeof(HitObjectComposer),
            typeof(OsuHitObjectComposer),
            typeof(HitObjectSelectionLayer),
            typeof(OsuHitObjectSelectionLayer),
            typeof(HitObjectOverlay),
            typeof(HitCircleOverlay),
            typeof(SliderOverlay),
            typeof(SliderCircleOverlay)
        };

        [BackgroundDependencyLoader]
        private void load(OsuGameBase osuGame)
        {
            osuGame.Beatmap.Value = new TestWorkingBeatmap(new Beatmap
            {
                HitObjects = new List<HitObject>
                {
                    new HitCircle { Position = new Vector2(256, 192), Scale = 0.5f },
                    new HitCircle { Position = new Vector2(344, 148), Scale = 0.5f },
                    new Slider
                    {
                        Position = new Vector2(128, 256),
                        ControlPoints = new List<Vector2>
                        {
                            Vector2.Zero,
                            new Vector2(216, 0),
                        },
                        Distance = 400,
                        Velocity = 1,
                        TickDistance = 100,
                        Scale = 0.5f,
                    }
                },
            });

            Child = new OsuHitObjectComposer(new OsuRuleset());
        }
    }
}
