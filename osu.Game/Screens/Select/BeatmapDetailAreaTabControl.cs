// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Linq;
using osuTK.Graphics;
using osu.Framework.Allocation;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Configuration;
using osu.Game.Graphics;
using osu.Game.Graphics.UserInterface;
using osu.Framework.Graphics.Shapes;

namespace osu.Game.Screens.Select
{
    public class BeatmapDetailAreaTabControl : Container
    {
        public const float HEIGHT = 24;

        public BeatmapDetailsAreaTabItem[] TabItems
        {
            set => tabs.Items = value;
        }

        private readonly OsuTabControlCheckbox modsCheckbox;
        private readonly OsuTabControl<BeatmapDetailsAreaTabItem> tabs;
        private readonly Container tabsContainer;

        public Action<BeatmapDetailsAreaTabItem, bool> OnFilter; //passed the selected tab and if mods is checked

        public BeatmapDetailAreaTabControl()
        {
            Height = HEIGHT;

            Children = new Drawable[]
            {
                new Box
                {
                    Anchor = Anchor.BottomLeft,
                    Origin = Anchor.BottomLeft,
                    RelativeSizeAxes = Axes.X,
                    Height = 1,
                    Colour = Color4.White.Opacity(0.2f),
                },
                tabsContainer = new Container
                {
                    RelativeSizeAxes = Axes.Both,
                    Child = tabs = new OsuTabControl<BeatmapDetailsAreaTabItem>
                    {
                        Anchor = Anchor.BottomLeft,
                        Origin = Anchor.BottomLeft,
                        RelativeSizeAxes = Axes.Both,
                        IsSwitchable = true,
                    },
                },
                modsCheckbox = new OsuTabControlCheckbox
                {
                    Anchor = Anchor.BottomRight,
                    Origin = Anchor.BottomRight,
                    Text = @"Selected Mods",
                    Alpha = 0,
                },
            };

            tabs.Current.ValueChanged += _ => invokeOnFilter();
            modsCheckbox.Current.ValueChanged += _ => invokeOnFilter();
        }

        [BackgroundDependencyLoader]
        private void load(OsuColour colour, OsuConfigManager config)
        {
            modsCheckbox.AccentColour = tabs.AccentColour = colour.YellowLight;
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            tabs.Current.Value = tabs.Items.OfType<BeatmapDetailsAreaDetailsTabItem>().Single();
        }

        private void invokeOnFilter()
        {
            OnFilter?.Invoke(tabs.Current.Value, modsCheckbox.Current.Value);

            if (tabs.Current.Value is BeatmapDetailsAreaDetailsTabItem)
            {
                modsCheckbox.FadeTo(0, 200, Easing.OutQuint);
                tabsContainer.Padding = new MarginPadding();
            }
            else
            {
                modsCheckbox.FadeTo(1, 200, Easing.OutQuint);
                tabsContainer.Padding = new MarginPadding { Right = 100 };
            }
        }
    }
}
