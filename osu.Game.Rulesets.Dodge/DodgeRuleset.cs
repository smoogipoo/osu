// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System.Collections.Generic;
using System.Linq;
using osu.Game.Beatmaps;
using osu.Game.Rulesets.Difficulty;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.UI;

namespace osu.Game.Rulesets.Dodge
{
    public class DodgeRuleset : Ruleset
    {
        public override IEnumerable<Mod> GetModsFor(ModType type) => Enumerable.Empty<Mod>();

        public override RulesetContainer CreateRulesetContainerWith(WorkingBeatmap beatmap)
        {
            throw new System.NotImplementedException();
        }

        public override IBeatmapConverter CreateBeatmapConverter(IBeatmap beatmap)
        {
            throw new System.NotImplementedException();
        }

        public override DifficultyCalculator CreateDifficultyCalculator(WorkingBeatmap beatmap)
        {
            throw new System.NotImplementedException();
        }

        public override string Description => "osu!dodge";

        public override string ShortName => "dodge";
    }
}
