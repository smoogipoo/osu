// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;

namespace osu.Game.Beatmaps.Formats
{
    public ref struct LegacyLineTokenizer
    {
        private readonly char separator;

        private ReadOnlySpan<char> line;

        public LegacyLineTokenizer(ReadOnlySpan<char> line, char separator = ',')
        {
            this.line = line;
            this.separator = separator;
        }

        public ReadOnlySpan<char> Read()
        {
            if (line.Length == 0)
                return line;

            int separatorIndex = line.IndexOf(separator);

            ReadOnlySpan<char> result;

            if (separatorIndex == -1)
            {
                result = line;
                line = ReadOnlySpan<char>.Empty;
            }
            else
            {
                result = line.Slice(0, separatorIndex);
                line = line.Slice(separatorIndex + 1);
            }

            return result;
        }

        public ReadOnlySpan<char> ReadToEnd()
        {
            var result = line;
            line = ReadOnlySpan<char>.Empty;

            return result;
        }

        public bool HasMore => !line.IsEmpty;
    }
}
