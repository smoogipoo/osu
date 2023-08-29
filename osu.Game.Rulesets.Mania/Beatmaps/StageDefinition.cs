﻿// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Game.Rulesets.Mania.UI;

namespace osu.Game.Rulesets.Mania.Beatmaps
{
    /// <summary>
    /// Defines properties for each stage in a <see cref="ManiaPlayfield"/>.
    /// </summary>
    public class StageDefinition
    {
        /// <summary>
        /// The number of <see cref="Column"/>s which this stage contains.
        /// </summary>
        public readonly int Columns;

        public readonly int? ScratchColumn;

        public StageDefinition(int index, int columns, int? scratchColumn = null)
        {
            if (columns < 1)
                throw new ArgumentException("Column count must be above zero.", nameof(columns));

            Columns = columns;
            ScratchColumn = scratchColumn;
        }

        /// <summary>
        /// Whether the column index is a special column for this stage.
        /// </summary>
        /// <param name="column">The 0-based column index.</param>
        /// <returns>Whether the column is a special column.</returns>
        public bool IsSpecialColumn(int column)
        {
            if (Columns % 2 == 1)
                return column == Columns / 2;

            return HasScratchColumn && column == 0;
        }
    }
}
