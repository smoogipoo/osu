// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.Allocation;
using osu.Framework.Extensions.IEnumerableExtensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osu.Game.Online.API;
using OpenTK;

namespace osu.Game.Screens.AX.Steps
{
    public class SubmissionResultStep : Step
    {
        private readonly CircularContainer logoContainer;
        private readonly Box logoBackground;
        private readonly TextAwesome logo;

        private readonly Container<OsuSpriteText> textContainer;

        private APIAccess api;

        public SubmissionResultStep(SubmissionState state)
        {
            var items = new[]
            {
                "circle clicking",
                "drum bashing",
                "fruit catching",
                "key smashing"
            };

            FillFlowContainer textFlow;

            Add(new Drawable[]
            {
                logoContainer = new CircularContainer
                {
                    Anchor = Anchor.TopCentre,
                    Origin = Anchor.TopCentre,
                    Size = new Vector2(150f),
                    Masking = true,
                    Children = new Drawable[]
                    {
                        logoBackground = new Box
                        {
                            RelativeSizeAxes = Axes.Both
                        },
                        logo = new TextAwesome
                        {
                            Anchor = Anchor.Centre,
                            Origin = Anchor.Centre,
                            TextSize = 66
                        }
                    }
                },
                textFlow = new FillFlowContainer
                {
                    Anchor = Anchor.TopCentre,
                    Origin = Anchor.TopCentre,
                    AutoSizeAxes = Axes.Both,
                    Direction = FillDirection.Horizontal,
                    Margin = new MarginPadding { Top = 10 }
                }
            });

            if (state == SubmissionState.Restricted)
                textFlow.Add(new OsuSpriteText { Text = "Sorry, we don't want your message." });
            else
            {
                logo.Icon = FontAwesome.fa_check;

                textFlow.Add(new Drawable[]
                {
                    new OsuSpriteText
                    {
                        Text = "Thanks for being a part of the "
                    },
                    new Container
                    {
                        AutoSizeAxes = Axes.Both,
                        Children = new Drawable[]
                        {
                            new OsuSpriteText
                            {
                                Text = "circle clicking",
                                Alpha = 0,
                                AlwaysPresent = true
                            },
                            textContainer = new Container<OsuSpriteText>
                            {
                                RelativeSizeAxes = Axes.Both,
                                Children = new[]
                                {
                                    new OsuSpriteText
                                    {
                                        Anchor = Anchor.TopCentre,
                                        Origin = Anchor.TopCentre,
                                        Text = items[0]
                                    }
                                }
                            }
                        }
                    },
                    new OsuSpriteText { Text = " adventure!" }
                });

                int currentItem = 0;
                Scheduler.AddDelayed(() =>
                {
                    currentItem = (currentItem + 1) % items.Length;

                    textContainer.Children.ForEach(c =>
                    {
                        c.MoveToOffset(new Vector2(0, -10), 400, EasingTypes.OutQuint);
                        c.FadeOut(400);
                    });

                    var newText = new OsuSpriteText
                    {
                        Anchor = Anchor.TopCentre,
                        Origin = Anchor.TopCentre,
                        Text = items[currentItem],
                        Alpha = 0
                    };

                    textContainer.Add(newText);

                    using (newText.BeginDelayedSequence(400))
                        newText.FadeIn(200, EasingTypes.OutQuint);
                }, 1000, true);
            }

            switch (state)
            {
                case SubmissionState.New:
                    logoBackground.Colour = OsuColour.FromHex("88b300");
                    break;
                case SubmissionState.Update:
                    logoBackground.Colour = OsuColour.FromHex("ffcc22");
                    break;
                case SubmissionState.Restricted:
                    logoBackground.Colour = OsuColour.FromHex("ed1121");
                    logo.Icon = FontAwesome.fa_close;
                    break;
            }
        }

        [BackgroundDependencyLoader(permitNulls: true)]
        private void load(APIAccess api)
        {
            this.api = api;
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            using (BeginDelayedSequence(5000))
                Schedule(() => api?.Logout());

            logo.ScaleTo(0.1f);
            logo.ScaleTo(1f, 600, EasingTypes.OutBounce);
        }
    }
}
