using osu.Framework.Testing;
using osu.Framework.Graphics;
using osu.Game.Overlays;
using osu.Game.Screens;
using osu.Game.Graphics.Backgrounds;
using osu.Game.Screens.Backgrounds;
using osu.Framework.Graphics.Shapes;
using OpenTK.Graphics;
using OpenTK;
using osu.Game.Graphics;
using osu.Framework.Allocation;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics.Containers;
using osu.Framework.Extensions.IEnumerableExtensions;
using osu.Game.Graphics.UserInterface;
using osu.Game.Graphics.Sprites;
using System.Linq;
using osu.Framework.Input;
using OpenTK.Input;
using osu.Game.Screens.Menu;
using System;
using osu.Game.Online.API;
using osu.Game.Database;
using osu.Framework.Graphics.UserInterface;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Configuration;
using Newtonsoft.Json;
using osu.Framework.IO.Network;

namespace osu.Desktop.VisualTests.Tests
{
    public class TestCaseAx2017 : TestCase
    {
        public override string Description => "AX 2017";

        public override void Reset()
        {
            base.Reset();

            Add(new AxLoginForm());
        }

        private class AxLoginForm : OsuScreen
        {
            protected override BackgroundScreen CreateBackground() => new AxBackgroundScreen();

            public AxLoginForm()
            {
                Add(new Drawable[]
                {
                    new Box
                    {
                        RelativeSizeAxes = Axes.Both,
                        Colour = Color4.Black,
                        Alpha = 0.75f
                    },
                    new OsuLogo
                    {
                        Scale = new Vector2(0.35f),
                        Interactive = false,
                        Y = -120
                    },
                    new StepContainer
                    {
                        Anchor = Anchor.Centre,
                        Origin = Anchor.Centre,
                        Scale = new Vector2(0.85f),
                        Y = 120,
                    }
                });
            }

            private class AxBackgroundScreen : BackgroundScreenCustom
            {
                public AxBackgroundScreen()
                    : base("AX/background")
                {
                    AddInternal(new Box
                    {
                        RelativeSizeAxes = Axes.Both,
                        Depth = 1
                    });
                }
            }
        }

        private class StepContainer : Container<Step>, IOnlineComponent
        {
            protected override Container<Step> Content => content;
            private readonly Container<Step> content;

            private Step currentStep;

            private APIAccess api;

            public StepContainer()
            {
                AutoSizeAxes = Axes.Y;
                AutoSizeDuration = 200;
                AutoSizeEasing = EasingTypes.OutQuint;
                Width = 450;

                CornerRadius = 5;
                Masking = true;

                EdgeEffect = new EdgeEffectParameters
                {
                    Type = EdgeEffectType.Shadow,
                    Colour = Color4.Black.Opacity(0.6f),
                    Radius = 10
                };

                AddInternal(new Drawable[]
                {
                    content = new Container<Step>
                    {
                        RelativeSizeAxes = Axes.X,
                        AutoSizeAxes = Axes.Y,
                        Padding = new MarginPadding(20)
                    },
                    new Box
                    {
                        Anchor = Anchor.BottomLeft,
                        Origin = Anchor.BottomLeft,
                        RelativeSizeAxes = Axes.X,
                        Colour = OsuColour.FromHex("ffcc22"),
                        Height = 3
                    }
                });
            }

            [BackgroundDependencyLoader(permitNulls: true)]
            private void load(APIAccess api)
            {
                this.api = api;
                api?.Register(this);
            }

            public void APIStateChanged(APIAccess api, APIState state)
            {
                switch (state)
                {
                    case APIState.Connecting:
                        break;
                    case APIState.Offline:
                        using (BeginDelayedSequence(500))
                        {
                            Schedule(() =>
                            {
                                if (api.State != APIState.Offline)
                                    return;
                                pushStep(new LoginStep());
                            });
                        }
                        break;
                    case APIState.Online:
                        var getMessageRequest = new GetMessageRequest();
                        getMessageRequest.Failure += e => pushStep(new SubmissionResultStep(SubmissionState.Restricted));
                        getMessageRequest.Success += m => pushStep(new WriteMessageStep(m.Entry?.Message ?? string.Empty));
                        api.Queue(getMessageRequest);
                        break;
                }
            }

            private void pushStep(Step step)
            {
                if (currentStep != null)
                    currentStep.Expire();

                Add(currentStep = step);
            }
        }

        private abstract class Step : Container
        {
            protected override Container<Drawable> Content => content;
            private readonly FillFlowContainer content;

            public Step()
            {
                Anchor = Anchor.Centre;
                Origin = Anchor.Centre;

                RelativeSizeAxes = Axes.X;
                AutoSizeAxes = Axes.Y;

                AddInternal(new Drawable[]
                {
                    content = new FillFlowContainer
                    {
                        RelativeSizeAxes = Axes.X,
                        AutoSizeAxes = Axes.Y,
                        Direction = FillDirection.Vertical,
                        Spacing = new Vector2(0, 5),
                        Masking = true
                    },
                });
            }
        }

        private class LoginStep : Step
        {
            private OsuTextBox username;
            private OsuTextBox password;
            private OsuButton loginButton;

            private APIAccess api;
            private UserInputManager inputManager;

            private bool loginRequested;

            public LoginStep()
            {
                Add(new Drawable[]
                {
                    new OsuSpriteText
                    {
                        Text = "ACCOUNT",
                        Font = "Exo2.0-Bold"
                    },
                    username = new OsuTextBox
                    {
                        RelativeSizeAxes = Axes.X,
                        PlaceholderText = "Username",
                        TabbableContentContainer = this
                    },
                    password = new OsuPasswordTextBox
                    {
                        RelativeSizeAxes = Axes.X,
                        PlaceholderText = "Password",
                        TabbableContentContainer = this,
                        OnCommit = (s, n) => performLogin()
                    },
                    loginButton = new OsuButton
                    {
                        RelativeSizeAxes = Axes.X,
                        Text = "Login",
                        Action = performLogin
                    }
                });
            }

            [BackgroundDependencyLoader(permitNulls: true)]
            private void load(APIAccess api, UserInputManager inputManager)
            {
                this.api = api;
                this.inputManager = inputManager;

                inputManager?.ChangeFocus(username);
            }

            private void performLogin()
            {
                if (string.IsNullOrEmpty(username.Text) || string.IsNullOrEmpty(password.Text))
                    return;

                if (loginRequested)
                    return;

                api?.Login(username.Text, password.Text);
                loginRequested = true;
            }
        }

        private class WriteMessageStep : Step
        {
            private const int max_length = 200;

            private OsuTextBox message;
            private OsuSpriteText lengthText;
            private TextAwesomeButton backButton;
            private OsuButton submitButton;

            private APIAccess api;

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
                        Text = existingMessage
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
            private void load(APIAccess api)
            {
                this.api = api;

                backButton.BackgroundColour = OsuColour.FromHex("ffcc22");
                backButton.Triangles.ColourDark = OsuColour.FromHex("eeaa00");
                backButton.Triangles.ColourLight = OsuColour.FromHex("ffdd55");
            }

            private void goBack()
            {
                api?.Logout();
            }

            private void submit()
            {
                if (string.IsNullOrEmpty(message.Text))
                    return;

                var postMessageRequest = new PostMessageRequest(message.Text);
                api.Queue(postMessageRequest);
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

        private class SubmissionResultStep : Step
        {
            public SubmissionResultStep(SubmissionState state)
            {
                Add(new Drawable[]
                {
                    new CircularContainer
                    {
                        Anchor = Anchor.TopCentre,
                        Origin = Anchor.TopCentre,
                        Size = new Vector2(150f),
                        Masking = true,
                        Children = new Drawable[]
                        {
                            new Box
                            {
                                RelativeSizeAxes = Axes.Both
                            }
                        }
                    }
                });
            }
        }

        public class GetMessageRequest : APIRequest<GetMessageResponse>
        {
            protected override WebRequest CreateWebRequest()
            {
                var req = base.CreateWebRequest();
                return req;
            }

            protected override string Target => $@"guestbook";
        }

        public class GetMessageResponse
        {
            [JsonProperty(@"entry")]
            public MessageEntry Entry;
        }

        public class PostMessageRequest : APIRequest<PostMessageResponse>
        {
            private readonly string message;

            public PostMessageRequest(string message)
            {
                this.message = message;
            }

            protected override WebRequest CreateWebRequest()
            {
                var req = base.CreateWebRequest();
                req.Method = HttpMethod.POST;
                req.AddParameter("message", message);
                return req;
            }

            protected override string Target => $@"guestbook";
        }

        public class PostMessageResponse
        {
            [JsonProperty("entry")]
            public MessageEntry Entry;

            [JsonProperty("first_visit")]
            public bool FirstVisit;
        }

        public class MessageEntry
        {
            [JsonProperty("user_id")]
            public int UserId;

            [JsonProperty("message")]
            public string Message;
        }

        private enum SubmissionState
        {
            New,
            Update,
            Restricted
        }
    }
}