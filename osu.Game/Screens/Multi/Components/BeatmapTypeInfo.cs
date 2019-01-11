﻿// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Graphics;
using osu.Game.Graphics.Containers;
using osu.Game.Online.Chat;
using osuTK;

namespace osu.Game.Screens.Multi.Components
{
    public class BeatmapTypeInfo : MultiplayerComposite
    {
        public BeatmapTypeInfo()
        {
            AutoSizeAxes = Axes.Both;

            LinkFlowContainer beatmapAuthor;

            InternalChild = new FillFlowContainer
            {
                AutoSizeAxes = Axes.Both,
                Direction = FillDirection.Horizontal,
                LayoutDuration = 100,
                Spacing = new Vector2(5, 0),
                Children = new Drawable[]
                {
                    new ModeTypeInfo(),
                    new Container
                    {
                        AutoSizeAxes = Axes.X,
                        Height = 30,
                        Margin = new MarginPadding { Left = 5 },
                        Children = new Drawable[]
                        {
                            new BeatmapTitle(),
                            beatmapAuthor = new LinkFlowContainer(s => s.TextSize = 14)
                            {
                                Anchor = Anchor.BottomLeft,
                                Origin = Anchor.BottomLeft,
                                AutoSizeAxes = Axes.Both
                            },
                        },
                    },
                }
            };

            CurrentBeatmap.BindValueChanged(v =>
            {
                beatmapAuthor.Clear();

                if (v != null)
                {
                    beatmapAuthor.AddText("mapped by ", s => s.Colour = OsuColour.Gray(0.8f));
                    beatmapAuthor.AddLink(v.Metadata.Author.Username, null, LinkAction.OpenUserProfile, v.Metadata.Author.Id.ToString(), "View Profile");
                }
            });
        }
    }
}
