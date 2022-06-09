// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable enable

using System.Collections.Generic;
using osu.Game.Rulesets.Objects;

namespace osu.Game.Rulesets.Difficulty.Preprocessing
{
    /// <summary>
    /// Wraps a <see cref="HitObject"/> and provides additional information to be used for difficulty calculation.
    /// </summary>
    public class DifficultyHitObject
    {
        /// <summary>
        /// The index of this <see cref="DifficultyHitObject"/> in the beatmap.
        /// </summary>
        public int Index;

        /// <summary>
        /// The <see cref="HitObject"/> this <see cref="DifficultyHitObject"/> wraps.
        /// </summary>
        public readonly HitObject BaseObject;

        /// <summary>
        /// Amount of time elapsed between this <see cref="DifficultyHitObject"/> and the last one, or 0 if there was no previous <see cref="DifficultyHitObject"/>.
        /// </summary>
        public readonly double DeltaTime;

        /// <summary>
        /// Clockrate adjusted start time of <see cref="BaseObject"/>.
        /// </summary>
        public readonly double StartTime;

        /// <summary>
        /// Clockrate adjusted end time of <see cref="BaseObject"/>.
        /// </summary>
        public readonly double EndTime;

        private readonly IReadOnlyList<DifficultyHitObject> allObjects;

        /// <summary>
        /// Creates a new <see cref="DifficultyHitObject"/>.
        /// </summary>
        /// <param name="index">The index of this <see cref="DifficultyHitObject"/> in <paramref name="allObjects"/> list.</param>
        /// <param name="hitObject">The <see cref="HitObject"/> which this <see cref="DifficultyHitObject"/> wraps.</param>
        /// <param name="clockRate">The rate at which the gameplay clock is run at.</param>
        /// <param name="allObjects">The list of <see cref="DifficultyHitObject"/>s in the current beatmap.</param>
        public DifficultyHitObject(int index, HitObject hitObject, double clockRate, IReadOnlyList<DifficultyHitObject> allObjects)
        {
            this.allObjects = allObjects;

            Index = index;
            BaseObject = hitObject;
            StartTime = hitObject.StartTime / clockRate;
            EndTime = hitObject.GetEndTime() / clockRate;

            if (Index > 0)
                DeltaTime = (hitObject.StartTime - Previous(0).BaseObject.StartTime) / clockRate;
        }

        public DifficultyHitObject Previous(int backwardsIndex) => allObjects[Index - (backwardsIndex + 1)];

        public DifficultyHitObject Next(int forwardsIndex) => allObjects[Index + (forwardsIndex + 1)];
    }
}
