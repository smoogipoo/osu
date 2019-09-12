// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using osu.Framework.Allocation;
using osu.Game.Beatmaps;
using osu.Game.Rulesets.Edit;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Objects.Types;
using osu.Game.Rulesets.Osu.Edit;
using osu.Game.Rulesets.Osu.Edit.Blueprints.HitCircles;
using osu.Game.Rulesets.Osu.Edit.Blueprints.HitCircles.Components;
using osu.Game.Rulesets.Osu.Objects;
using osu.Game.Screens.Edit.Compose.Components;
using osu.Game.Tests.Visual;
using osuTK;

namespace osu.Game.Rulesets.Osu.Tests
{
    public class TestSceneOsuHitObjectComposer : EditorClockTestScene
    {
        public override IReadOnlyList<Type> RequiredTypes => new[]
        {
            typeof(SelectionHandler),
            typeof(DragBox),
            typeof(HitObjectComposer),
            typeof(BlueprintContainer),
            typeof(NotNullAttribute),
            typeof(HitCirclePiece),
            typeof(HitCircleSelectionBlueprint),
            typeof(HitCirclePlacementBlueprint),
        };

        [BackgroundDependencyLoader]
        private void load()
        {
            BeatDivisor.Value = 8;

            Beatmap.Value = CreateWorkingBeatmap(new Beatmap
            {
                HitObjects = new List<HitObject>
                {
                    new HitCircle { Position = new Vector2(256, 192), Scale = 0.5f },
                    new HitCircle { Position = new Vector2(344, 148), Scale = 0.5f },
                    new Slider
                    {
                        Position = new Vector2(128, 256),
                        Path = new SliderPath(PathType.Linear, new[]
                        {
                            Vector2.Zero,
                            new Vector2(216, 0),
                        }),
                        Scale = 0.5f,
                    }
                },
            });

            Child = new OsuHitObjectComposer(new OsuRuleset());
        }
    }
}
