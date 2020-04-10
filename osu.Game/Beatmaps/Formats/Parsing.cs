// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Globalization;

namespace osu.Game.Beatmaps.Formats
{
    /// <summary>
    /// Helper methods to parse from string to number and perform very basic validation.
    /// </summary>
    public static class Parsing
    {
        public const int MAX_COORDINATE_VALUE = 65536;

        public const double MAX_PARSE_VALUE = int.MaxValue;

        public static float ParseFloat(string input, float parseLimit = (float)MAX_PARSE_VALUE)
            => ParseFloat(input.AsSpan(), parseLimit);

        public static float ParseFloat(ReadOnlySpan<char> input, float parseLimit = (float)MAX_PARSE_VALUE)
        {
            var output = float.Parse(input, provider: CultureInfo.InvariantCulture);

            if (output < -parseLimit) throw new OverflowException("Value is too low");
            if (output > parseLimit) throw new OverflowException("Value is too high");

            if (float.IsNaN(output)) throw new FormatException("Not a number");

            return output;
        }

        public static double ParseDouble(string input, double parseLimit = MAX_PARSE_VALUE)
            => ParseDouble(input.AsSpan(), parseLimit);

        public static double ParseDouble(ReadOnlySpan<char> input, double parseLimit = MAX_PARSE_VALUE)
        {
            var output = double.Parse(input, provider: CultureInfo.InvariantCulture);

            if (output < -parseLimit) throw new OverflowException("Value is too low");
            if (output > parseLimit) throw new OverflowException("Value is too high");

            if (double.IsNaN(output)) throw new FormatException("Not a number");

            return output;
        }

        public static int ParseInt(string input, int parseLimit = (int)MAX_PARSE_VALUE)
            => ParseInt(input.AsSpan(), parseLimit);

        public static int ParseInt(ReadOnlySpan<char> input, int parseLimit = (int)MAX_PARSE_VALUE)
        {
            var output = int.Parse(input, provider: CultureInfo.InvariantCulture);

            if (output < -parseLimit) throw new OverflowException("Value is too low");
            if (output > parseLimit) throw new OverflowException("Value is too high");

            return output;
        }
    }
}
