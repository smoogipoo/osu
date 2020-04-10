// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Globalization;
using osu.Game.Beatmaps.Formats;

namespace osu.Game.Skinning
{
    public class LegacySkinDecoder : LegacyDecoder<LegacySkinConfiguration>
    {
        public LegacySkinDecoder()
            : base(1)
        {
        }

        protected override void ParseLine(LegacySkinConfiguration skin, Section section, ReadOnlySpan<char> line)
        {
            if (section != Section.Colours)
            {
                StripComments(ref line);
                SplitKeyVal(line, out var key, out var value);

                switch (section)
                {
                    case Section.General:
                        switch (key)
                        {
                            case @"Name":
                                skin.SkinInfo.Name = value.ToString();
                                return;

                            case @"Author":
                                skin.SkinInfo.Creator = value.ToString();
                                return;

                            case @"Version":
                                if (value == "latest")
                                    skin.LegacyVersion = LegacySkinConfiguration.LATEST_VERSION;
                                else if (decimal.TryParse(value, NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out var version))
                                    skin.LegacyVersion = version;

                                return;
                        }

                        break;
                }

                if (!string.IsNullOrEmpty(key))
                    skin.ConfigDictionary[key] = value.ToString();
            }

            base.ParseLine(skin, section, line);
        }

        protected override LegacySkinConfiguration CreateTemplateObject()
        {
            var config = base.CreateTemplateObject();
            config.LegacyVersion = 1.0m;
            return config;
        }
    }
}
