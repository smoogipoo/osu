// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Overlays;
using osu.Game.Overlays.Mods;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Osu.Mods;

namespace osu.Game.Tests.Visual.UserInterface
{
    public class TestSceneModPresetColumn : OsuTestScene
    {
        [Cached]
        private OverlayColourProvider colourProvider = new OverlayColourProvider(OverlayColourScheme.Green);

        [Test]
        public void TestBasicAppearance()
        {
            ModPresetColumn modPresetColumn = null!;

            AddStep("create content", () => Child = new Container
            {
                RelativeSizeAxes = Axes.Both,
                Padding = new MarginPadding(30),
                Child = modPresetColumn = new ModPresetColumn
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    Presets = createTestPresets().ToArray()
                }
            });
            AddStep("change presets", () => modPresetColumn.Presets = createTestPresets().Skip(1).ToArray());
        }

        private static IEnumerable<ModPreset> createTestPresets() => new[]
        {
            new ModPreset
            {
                Name = "First preset",
                Description = "Please ignore",
                Mods = new Mod[]
                {
                    new OsuModHardRock(),
                    new OsuModDoubleTime()
                }
            },
            new ModPreset
            {
                Name = "AR0",
                Description = "For good readers",
                Mods = new Mod[]
                {
                    new OsuModDifficultyAdjust
                    {
                        ApproachRate = { Value = 0 }
                    }
                }
            },
            new ModPreset
            {
                Name = "This preset is going to have an extraordinarily long name",
                Description = "This is done so that the capability to truncate overlong texts may be demonstrated",
                Mods = new Mod[]
                {
                    new OsuModFlashlight(),
                    new OsuModSpinIn()
                }
            }
        };
    }
}
