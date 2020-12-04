// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Game.Graphics;
using osu.Game.Graphics.UserInterface;
using osuTK.Graphics;

namespace osu.Game.Screens.Multi.Lounge.Components
{
    public class FilterControl : CompositeDrawable
    {
        private const float vertical_padding = 10;
        private const float horizontal_padding = 80;

        private readonly Box tabStrip;

        public readonly SearchTextBox Search;
        public readonly PageTabControl<RoomStatusFilter> Tabs;

        public FilterControl()
        {
            InternalChildren = new Drawable[]
            {
                new Box
                {
                    RelativeSizeAxes = Axes.Both,
                    Colour = Color4.Black,
                    Alpha = 0.5f,
                },
                tabStrip = new Box
                {
                    Anchor = Anchor.BottomLeft,
                    Origin = Anchor.BottomLeft,
                    RelativeSizeAxes = Axes.X,
                    Height = 1,
                },
                new Container
                {
                    RelativeSizeAxes = Axes.Both,
                    Padding = new MarginPadding
                    {
                        Top = vertical_padding,
                        Horizontal = horizontal_padding
                    },
                    Children = new Drawable[]
                    {
                        Search = new FilterSearchTextBox
                        {
                            RelativeSizeAxes = Axes.X,
                        },
                        Tabs = new PageTabControl<RoomStatusFilter>
                        {
                            Anchor = Anchor.BottomLeft,
                            Origin = Anchor.BottomLeft,
                            RelativeSizeAxes = Axes.X,
                        },
                    }
                }
            };

            Tabs.Current.Value = RoomStatusFilter.Open;
            Tabs.Current.TriggerChange();
        }

        [BackgroundDependencyLoader]
        private void load(OsuColour colours)
        {
            tabStrip.Colour = colours.Yellow;
        }

        public bool HoldFocus
        {
            get => Search.HoldFocus;
            set => Search.HoldFocus = value;
        }

        public void TakeFocus() => Search.TakeFocus();

        private class FilterSearchTextBox : SearchTextBox
        {
            [BackgroundDependencyLoader]
            private void load()
            {
                BackgroundUnfocused = OsuColour.Gray(0.06f);
                BackgroundFocused = OsuColour.Gray(0.12f);
            }
        }
    }
}
