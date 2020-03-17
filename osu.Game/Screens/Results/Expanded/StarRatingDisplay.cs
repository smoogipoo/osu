// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Globalization;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osu.Game.Beatmaps;
using osu.Game.Graphics;
using osu.Game.Graphics.Containers;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Screens.Results.Expanded
{
    public class StarRatingDisplay : CompositeDrawable
    {
        private readonly BeatmapInfo beatmap;

        public StarRatingDisplay(BeatmapInfo beatmap)
        {
            this.beatmap = beatmap;
            AutoSizeAxes = Axes.Both;
        }

        [BackgroundDependencyLoader]
        private void load(OsuColour colours)
        {
            var starRatingParts = beatmap.StarDifficulty.ToString("0.00", CultureInfo.InvariantCulture).Split('.');
            string wholePart = starRatingParts[0];
            string fractionPart = starRatingParts[1];
            string separator = CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator;

            InternalChildren = new Drawable[]
            {
                new CircularContainer
                {
                    RelativeSizeAxes = Axes.Both,
                    Masking = true,
                    Children = new Drawable[]
                    {
                        new Box
                        {
                            RelativeSizeAxes = Axes.Both,
                            Colour = colours.ForDifficultyRating(beatmap.DifficultyRating)
                        },
                    }
                },
                new FillFlowContainer
                {
                    AutoSizeAxes = Axes.Both,
                    Padding = new MarginPadding { Horizontal = 8, Vertical = 4 },
                    Direction = FillDirection.Horizontal,
                    Spacing = new Vector2(2, 0),
                    Children = new Drawable[]
                    {
                        new SpriteIcon
                        {
                            Anchor = Anchor.CentreLeft,
                            Origin = Anchor.CentreLeft,
                            Size = new Vector2(7),
                            Icon = FontAwesome.Solid.Star,
                            Colour = Color4.Black
                        },
                        new OsuTextFlowContainer(s => s.Font = OsuFont.Numeric.With(weight: FontWeight.Black))
                        {
                            Anchor = Anchor.CentreLeft,
                            Origin = Anchor.CentreLeft,
                            AutoSizeAxes = Axes.Both,
                            Direction = FillDirection.Horizontal,
                            TextAnchor = Anchor.BottomLeft,
                        }.With(t =>
                        {
                            t.AddText($"{wholePart}", s =>
                            {
                                s.Colour = Color4.Black;
                                s.Font = s.Font.With(size: 14);
                                s.UseFullGlyphHeight = false;
                            });

                            t.AddText($"{separator}{fractionPart}", s =>
                            {
                                s.Colour = Color4.Black;
                                s.Font = s.Font.With(size: 7);
                                s.UseFullGlyphHeight = false;
                            });
                        })
                    }
                }
            };
        }
    }
}
