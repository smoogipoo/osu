// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using System.IO;
using System.Linq;
using osu.Game.IO.Archives;

namespace osu.Game.Database
{
    public class EmptyArchiveReader : ArchiveReader
    {
        public EmptyArchiveReader()
            : base(string.Empty)
        {
        }

        public override Stream GetStream(string name) => new MemoryStream();

        public override void Dispose()
        {
        }

        public override IEnumerable<string> Filenames => Enumerable.Empty<string>();

        public override Stream GetUnderlyingStream() => throw new System.NotImplementedException();
    }
}
