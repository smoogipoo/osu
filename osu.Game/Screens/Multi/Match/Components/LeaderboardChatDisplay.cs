// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Game.Graphics;
using osu.Game.Graphics.Containers;
using osu.Game.Graphics.Sprites;
using osu.Game.Overlays;
using osuTK.Graphics;

namespace osu.Game.Screens.Multi.Match.Components
{
    public class LeaderboardChatDisplay : MultiplayerComposite
    {
        private const double fade_duration = 100;

        private readonly Button leaderboardButton;
        private readonly Button chatButton;
        private readonly MatchLeaderboard leaderboard;
        private readonly MatchChatDisplay chat;

        public LeaderboardChatDisplay()
        {
            RelativeSizeAxes = Axes.Both;

            InternalChild = new GridContainer
            {
                RelativeSizeAxes = Axes.Both,
                Content = new[]
                {
                    new Drawable[]
                    {
                        new GridContainer
                        {
                            RelativeSizeAxes = Axes.X,
                            AutoSizeAxes = Axes.Y,
                            Content = new[]
                            {
                                new Drawable[]
                                {
                                    leaderboardButton = new Button("Leaderboard")
                                    {
                                        Padding = new MarginPadding { Right = 1 },
                                        Action = showLeaderboard
                                    },
                                    chatButton = new Button("Chat")
                                    {
                                        Padding = new MarginPadding { Left = 1 },
                                        Action = showChat
                                    },
                                },
                            },
                            RowDimensions = new[] { new Dimension(GridSizeMode.AutoSize) }
                        }
                    },
                    new Drawable[]
                    {
                        new Container
                        {
                            RelativeSizeAxes = Axes.Both,
                            Padding = new MarginPadding { Top = 10 },
                            Children = new Drawable[]
                            {
                                leaderboard = new MatchLeaderboard { RelativeSizeAxes = Axes.Both },
                                chat = new MatchChatDisplay
                                {
                                    RelativeSizeAxes = Axes.Both,
                                    Alpha = 0
                                }
                            }
                        }
                    },
                },
                RowDimensions = new[]
                {
                    new Dimension(GridSizeMode.AutoSize),
                }
            };

            leaderboardButton.SetSelected(true);
        }

        private void showLeaderboard()
        {
            chatButton.SetSelected(false);
            leaderboardButton.SetSelected(true);

            chat.FadeTo(0, fade_duration);
            leaderboard.FadeTo(1, fade_duration);
        }

        private void showChat()
        {
            chatButton.SetSelected(true);
            leaderboardButton.SetSelected(false);

            chat.FadeTo(1, fade_duration);
            leaderboard.FadeTo(0, fade_duration);
        }

        private class Button : OsuClickableContainer
        {
            private readonly Drawable colouredContents;
            private readonly Box fill;

            public Button(string text)
            {
                RelativeSizeAxes = Axes.X;
                Height = 35;

                AddRange(new Drawable[]
                {
                    colouredContents = new Container
                    {
                        RelativeSizeAxes = Axes.Both,
                        Masking = true,
                        CornerRadius = 10,
                        BorderThickness = 3,
                        BorderColour = Color4.White,
                        Children = new Drawable[]
                        {
                            fill = new Box
                            {
                                RelativeSizeAxes = Axes.Both,
                                AlwaysPresent = true,
                                Alpha = 0
                            },
                        }
                    },
                    new OsuSpriteText
                    {
                        Anchor = Anchor.Centre,
                        Origin = Anchor.Centre,
                        Text = text,
                        Font = OsuFont.GetFont(size: 14)
                    }
                });
            }

            [BackgroundDependencyLoader]
            private void load(OverlayColourProvider colourProvider)
            {
                colouredContents.Colour = colourProvider.Dark1;
            }

            public void SetSelected(bool selected) => fill.FadeTo(selected ? 1 : 0, fade_duration, Easing.OutQuint);
        }
    }
}
