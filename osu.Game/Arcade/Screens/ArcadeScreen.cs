// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.IO;
using System.Threading.Tasks;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Extensions.ObjectExtensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Rendering;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.Textures;
using osu.Framework.Input;
using osu.Framework.Logging;
using osu.Framework.Screens;
using osu.Game.Graphics;
using osu.Game.Graphics.Containers;
using osu.Game.Graphics.Sprites;
using osu.Game.Graphics.UserInterface;
using osu.Game.Localisation;
using osu.Game.Online.API;
using osu.Game.Online.Multiplayer;
using osu.Game.Overlays;
using osu.Game.Overlays.Login;
using osu.Game.Screens;
using osu.Game.Screens.Backgrounds;
using osuTK;
using osuTK.Graphics;
using QRCoder;

namespace osu.Game.Arcade.Screens
{
    public partial class ArcadeScreen : OsuScreen
    {
        public override bool AllowUserExit => false;

        protected override BackgroundScreen CreateBackground() => new BackgroundScreenDefault();

        [Resolved]
        private IAPIProvider api { get; set; } = null!;

        [Resolved]
        private IRenderer renderer { get; set; } = null!;

        [Resolved]
        private ArcadeClient arcadeClient { get; set; } = null!;

        [Resolved]
        private OsuColour colours { get; set; } = null!;

        [Cached]
        private ArcadeConfigManager arcadeConfig = null!;

        [Cached]
        private readonly OverlayColourProvider colourProvider = new OverlayColourProvider(OverlayColourScheme.Plum);

        private readonly IBindable<APIState> apiState = new Bindable<APIState>();
        private readonly Func<ArcadeIdentity, OsuScreen> createNextScreen;

        private Texture qrTexture = null!;
        private OsuNumberBox? codeTextBox;
        private OsuSpriteText? errorText;

        public ArcadeScreen(Func<ArcadeIdentity, OsuScreen> createNextScreen)
        {
            this.createNextScreen = createNextScreen;
        }

        protected override IReadOnlyDependencyContainer CreateChildDependencies(IReadOnlyDependencyContainer parent)
        {
            arcadeConfig = new ArcadeConfigManager(parent);
            return base.CreateChildDependencies(parent);
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            using (var qrCode = QRCodeGenerator.GenerateQrCode(new PayloadGenerator.Url(arcadeClient.KeyEndpoint)))
            {
                using (var qrRenderer = new PngByteQRCode(qrCode))
                {
                    byte[] qrImage = qrRenderer.GetGraphic(20, drawQuietZones: false);

                    using (var imageMs = new MemoryStream(qrImage))
                    {
                        TextureUpload upload = new TextureUpload(imageMs);
                        qrTexture = renderer.CreateTexture(upload.Width, upload.Height);
                        qrTexture.SetData(upload);
                    }
                }
            }
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            apiState.BindTo(api.State);
            apiState.BindValueChanged(onApiStateChanged, true);

            arcadeClient.UserConnected += onClientConnected;
        }

        private void onApiStateChanged(ValueChangedEvent<APIState> e) => Schedule(() =>
        {
            switch (e.NewValue)
            {
                case APIState.Offline:
                    InternalChild = new Container
                    {
                        Anchor = Anchor.Centre,
                        Origin = Anchor.Centre,
                        Width = 360,
                        AutoSizeAxes = Axes.Y,
                        Child = new LoginForm()
                    };
                    break;

                case APIState.RequiresSecondFactorAuth:
                    InternalChild = new Container
                    {
                        Anchor = Anchor.Centre,
                        Origin = Anchor.Centre,
                        Width = 360,
                        AutoSizeAxes = Axes.Y,
                        Child = new SecondFactorAuthForm()
                    };
                    break;

                case APIState.Connecting:
                case APIState.Failing:
                    InternalChild = new LoadingSpinner
                    {
                        Anchor = Anchor.Centre,
                        Origin = Anchor.Centre,
                        State = { Value = Visibility.Visible }
                    };
                    break;

                case APIState.Online:
                    InternalChild = new Container
                    {
                        Anchor = Anchor.Centre,
                        Origin = Anchor.Centre,
                        AutoSizeAxes = Axes.Both,
                        Masking = true,
                        CornerRadius = 20,
                        Children = new Drawable[]
                        {
                            new Box
                            {
                                RelativeSizeAxes = Axes.Both,
                                Colour = Color4.Black,
                                Alpha = 0.8f
                            },
                            new FillFlowContainer
                            {
                                AutoSizeAxes = Axes.Both,
                                Padding = new MarginPadding(20),
                                Direction = FillDirection.Horizontal,
                                Spacing = new Vector2(20),
                                Children = new Drawable[]
                                {
                                    new FillFlowContainer
                                    {
                                        Anchor = Anchor.CentreLeft,
                                        Origin = Anchor.CentreLeft,
                                        AutoSizeAxes = Axes.Both,
                                        Direction = FillDirection.Vertical,
                                        Spacing = new Vector2(10),
                                        Children = new Drawable[]
                                        {
                                            new OsuSpriteText
                                            {
                                                Anchor = Anchor.TopCentre,
                                                Origin = Anchor.TopCentre,
                                                Text = "Open the following link and log-in to osu!"
                                            },
                                            new Sprite
                                            {
                                                Anchor = Anchor.TopCentre,
                                                Origin = Anchor.TopCentre,
                                                Texture = qrTexture,
                                                Size = new Vector2(100)
                                            },
                                            new OsuSpriteText
                                            {
                                                Anchor = Anchor.TopCentre,
                                                Origin = Anchor.TopCentre,
                                                Margin = new MarginPadding { Top = 30 },
                                                Text = "Type the code displayed into the box below:"
                                            },
                                            codeTextBox = new OsuNumberBox
                                            {
                                                Anchor = Anchor.TopCentre,
                                                Origin = Anchor.TopCentre,
                                                Width = 200,
                                                InputProperties = new TextInputProperties(TextInputType.Code),
                                                PlaceholderText = LoginPanelStrings.EnterCode,
                                            },
                                            errorText = new OsuSpriteText
                                            {
                                                Anchor = Anchor.TopCentre,
                                                Origin = Anchor.TopCentre,
                                                Colour = colours.Red,
                                                AlwaysPresent = true,
                                                Text = "Invalid code",
                                                Alpha = 0
                                            },
                                            new OsuSpriteText
                                            {
                                                Anchor = Anchor.TopCentre,
                                                Origin = Anchor.TopCentre,
                                                Text = "And remember to collect your prize at the store!",
                                                Colour = colours.Pink
                                            }
                                        }
                                    },
                                    new Box
                                    {
                                        Anchor = Anchor.CentreLeft,
                                        Origin = Anchor.CentreLeft,
                                        Size = new Vector2(2, 280),
                                        Colour = Color4.White,
                                        Alpha = 0.5f
                                    },
                                    new FillFlowContainer
                                    {
                                        Anchor = Anchor.CentreLeft,
                                        Origin = Anchor.CentreLeft,
                                        AutoSizeAxes = Axes.Both,
                                        Direction = FillDirection.Vertical,
                                        Spacing = new Vector2(10),
                                        Children = new Drawable[]
                                        {
                                            new FillFlowContainer
                                            {
                                                Anchor = Anchor.TopCentre,
                                                Origin = Anchor.TopCentre,
                                                AutoSizeAxes = Axes.Both,
                                                Direction = FillDirection.Horizontal,
                                                Spacing = new Vector2(5),
                                                Children = new Drawable[]
                                                {
                                                    new OsuSpriteText
                                                    {
                                                        Anchor = Anchor.CentreLeft,
                                                        Origin = Anchor.CentreLeft,
                                                        Text = "Leaderboard"
                                                    },
                                                    new IconButton
                                                    {
                                                        Anchor = Anchor.CentreLeft,
                                                        Origin = Anchor.CentreLeft,
                                                        Scale = new Vector2(0.5f),
                                                        Icon = FontAwesome.Regular.WindowMaximize,
                                                        Action = () =>
                                                        {
                                                            if (this.IsCurrentScreen())
                                                                this.Push(new ArcadeLeaderboardScreen());
                                                        }
                                                    }
                                                }
                                            },
                                            new OsuScrollContainer(Direction.Vertical)
                                            {
                                                Anchor = Anchor.TopCentre,
                                                Origin = Anchor.TopCentre,
                                                Size = new Vector2(300, 300),
                                                ScrollbarOverlapsContent = false,
                                                Child = new ArcadeLeaderboard
                                                {
                                                    RelativeSizeAxes = Axes.X
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    };

                    codeTextBox.Current.BindValueChanged(onCodeChanged);
                    break;
            }
        });

        private void onClientConnected(int clientId, ArcadeIdentity identity) => Schedule(() =>
        {
            if (clientId == api.LocalUser.Value.OnlineID)
                this.Push(createNextScreen(identity));
        });

        private void onCodeChanged(ValueChangedEvent<string> e)
        {
            string trimmedCode = e.NewValue.Trim();
            trimmedCode = trimmedCode[..Math.Min(8, trimmedCode.Length)];

            if (trimmedCode.Length == 8)
            {
                Task.Run(() => attemptLogin(trimmedCode));
                codeTextBox!.Current.Disabled = true;
            }
        }

        private async Task attemptLogin(string code)
        {
            try
            {
                Logger.Log($"[ARCADE] Retrieving user with code '{code}'...");
                ArcadeIdentity user = await arcadeClient.GetUserWithCode(code);

                Logger.Log($"[ARCADE] Connecting as {user.User.Username}...");
                arcadeClient.Connect(user).FireAndForget(() => Logger.Log("[ARCADE] Connected"), failLoginAttempt);
            }
            catch (Exception ex)
            {
                failLoginAttempt(ex);
            }

            void failLoginAttempt(Exception ex) => Schedule(() =>
            {
                Logger.Log($"[ARCADE] Failed to connect: {ex}");

                errorText?.FadeIn().Delay(2000).FadeOut(500);

                if (codeTextBox != null)
                {
                    codeTextBox.Current.Disabled = false;
                    codeTextBox.Current.Value = string.Empty;
                }
            });
        }

        public override void OnEntering(ScreenTransitionEvent e)
        {
            base.OnEntering(e);

            arcadeConfig.Reset();
        }

        public override void OnResuming(ScreenTransitionEvent e)
        {
            base.OnResuming(e);

            Logger.Log("[ARCADE] Disconnecting from arcade server...");
            arcadeClient.Disconnect().FireAndForget(() => Schedule(() =>
            {
                Logger.Log("[ARCADE] Disconnected");

                if (codeTextBox != null)
                {
                    codeTextBox.Current.Disabled = false;
                    codeTextBox.Current.Value = string.Empty;
                }
            }));

            arcadeConfig.Reset();
        }

        protected override void Dispose(bool isDisposing)
        {
            base.Dispose(isDisposing);

            if (arcadeClient.IsNotNull())
                arcadeClient.UserConnected -= onClientConnected;
        }
    }
}
