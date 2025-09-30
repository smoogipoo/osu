// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using NUnit.Framework;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Tests.Visual.Matchmaking
{
    public class TestSceneChatBubble : OsuTestScene
    {
        [Test]
        public void SendChatMessage()
        {
            AddStep("post chat bubble", () => Add(new ChatBubble("hello, world!")
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                Scale = new Vector2(3)
            }));
        }

        private class ChatBubble : CompositeDrawable
        {
            private static float bubble_height = 14;
            private static float point_height = 6;

            private readonly Drawable horizontalPoint;
            private readonly Drawable verticalPoint;

            public ChatBubble(string text)
            {
                AutoSizeAxes = Axes.Both;

                InternalChild = new BufferedContainer
                {
                    AutoSizeAxes = Axes.Both,
                    Padding = new MarginPadding
                    {
                        Left = 2,
                        Bottom = 2
                    },
                    Children = new[]
                    {
                        new Container
                        {
                            AutoSizeAxes = Axes.Both,
                            Children = new Drawable[]
                            {
                                new Circle
                                {
                                    RelativeSizeAxes = Axes.Both,
                                },
                                new OsuSpriteText
                                {
                                    Anchor = Anchor.Centre,
                                    Origin = Anchor.Centre,
                                    Text = text,
                                    Colour = Color4.Black,
                                    Font = OsuFont.Default.With(size: 12),
                                    UseFullGlyphHeight = false,
                                    Margin = new MarginPadding
                                    {
                                        Horizontal = 15,
                                        Vertical = 3,
                                    }
                                }
                            }
                        },
                        horizontalPoint = new Triangle
                        {
                            Anchor = Anchor.CentreLeft,
                            Origin = Anchor.TopCentre,
                            Size = new Vector2(8, 4),
                            X = -2,
                            Rotation = -90
                        },
                        verticalPoint = new Triangle
                        {
                            Anchor = Anchor.BottomCentre,
                            Origin = Anchor.TopCentre,
                            Y = 2,
                            Size = new Vector2(8, 4),
                            Rotation = 180,
                        }
                    }
                };
            }
        }
    }
}
