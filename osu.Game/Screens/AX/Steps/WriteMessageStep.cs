// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osu.Game.Graphics.UserInterface;
using osu.Game.Online.API;
using OpenTK.Graphics;
using osu.Framework.Input;
using OpenTK.Input;

namespace osu.Game.Screens.AX.Steps
{
    public class WriteMessageStep : Step
    {
        private const int max_length = 200;

        public Action<SubmissionState> Submit;

        private OsuTextBox message;
        private OsuSpriteText lengthText;
        private TextAwesomeButton backButton;
        private OsuButton submitButton;

        private APIAccess api;
        private UserInputManager inputManager;

        private bool submitQueued;

        public WriteMessageStep(string existingMessage)
        {
            Add(new Drawable[]
            {
                new OsuSpriteText
                {
                    Text = "In a few words, tell us what osu! means to you!",
                },
                message = new FocusedTextBox
                {
                    RelativeSizeAxes = Axes.X,
                    PlaceholderText = "Message",
                    LengthLimit = max_length,
                    HoldFocus = true,
                    Text = existingMessage,
                    OnCommit = (s, n) => submit()
                },
                lengthText = new OsuSpriteText
                {
                    Anchor = Anchor.TopRight,
                    Origin = Anchor.TopRight
                },
                new Container
                {
                    RelativeSizeAxes = Axes.X,
                    AutoSizeAxes = Axes.Y,
                    Children = new[]
                    {
                        backButton = new TextAwesomeButton
                        {
                            RelativeSizeAxes = Axes.X,
                            Width = 0.1f,
                            Icon = FontAwesome.fa_step_backward,
                            Action = goBack,
                            BackgroundColour = Color4.Yellow
                        },
                        submitButton = new OsuButton
                        {
                            Anchor = Anchor.TopRight,
                            Origin = Anchor.TopRight,
                            RelativeSizeAxes = Axes.X,
                            Width = 0.88f,
                            Text = "Submit",
                            Action = submit
                        }
                    }
                }
            });
        }

        [BackgroundDependencyLoader(permitNulls: true)]
        private void load(APIAccess api, UserInputManager inputManager)
        {
            this.api = api;
            this.inputManager = inputManager;

            backButton.BackgroundColour = OsuColour.FromHex("ffcc22");
            backButton.Triangles.ColourDark = OsuColour.FromHex("eeaa00");
            backButton.Triangles.ColourLight = OsuColour.FromHex("ffdd55");
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            inputManager?.ChangeFocus(message);
        }

        private void goBack()
        {
            api?.Logout();
        }

        private void submit()
        {
            if (string.IsNullOrEmpty(message.Text))
                return;

            if (submitQueued)
                return;
            submitQueued = true;

            var postMessageRequest = new PostMessageRequest(message.Text);
            postMessageRequest.Success += m => Submit?.Invoke(m.FirstVisit ? SubmissionState.New : SubmissionState.Update);
            api.Queue(postMessageRequest);
        }

        protected override bool OnKeyDown(InputState state, KeyDownEventArgs args)
        {
            if (args.Key == Key.Escape)
            {
                goBack();
                return true;
            }

            return base.OnKeyDown(state, args);
        }

        protected override void Update()
        {
            base.Update();

            lengthText.Text = $"{message.Current.Value?.Length ?? 0}/{max_length}";
        }

        private class TextAwesomeButton : OsuButton
        {
            public FontAwesome Icon
            {
                get { return ((TextAwesome)SpriteText).Icon; }
                set { ((TextAwesome)SpriteText).Icon = value; }
            }

            protected override SpriteText CreateText() => new TextAwesome
            {
                Depth = -1,
                Origin = Anchor.Centre,
                Anchor = Anchor.Centre,
            };
        }
    }
}
