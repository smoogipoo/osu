// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Game.Beatmaps;
using osu.Game.Rulesets;
using osu.Game.Scoring;

namespace osu.Game.Screens.Spectate
{
    public class GameplayState
    {
        public readonly Score Score;
        public readonly Ruleset Ruleset;
        public readonly WorkingBeatmap Beatmap;

        public GameplayState(Score score, Ruleset ruleset, WorkingBeatmap beatmap)
        {
            Score = score;
            Ruleset = ruleset;
            Beatmap = beatmap;
        }
    }
}
