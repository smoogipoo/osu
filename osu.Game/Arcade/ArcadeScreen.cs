// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.IO;
using System.Threading.Tasks;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Rendering;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.Textures;
using osu.Framework.Input;
using osu.Framework.Logging;
using osu.Framework.Screens;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osu.Game.Graphics.UserInterface;
using osu.Game.Localisation;
using osu.Game.Online.API;
using osu.Game.Online.Multiplayer;
using osu.Game.Overlays.Login;
using osu.Game.Screens;
using osuTK;
using QRCoder;

namespace osu.Game.Arcade
{
    public partial class ArcadeScreen : OsuScreen
    {
        private readonly IBindable<APIState> apiState = new Bindable<APIState>();

        [Resolved]
        private IAPIProvider api { get; set; } = null!;

        [Resolved]
        private IRenderer renderer { get; set; } = null!;

        [Resolved]
        private ArcadeClient arcadeClient { get; set; } = null!;

        [Resolved]
        private OsuColour colours { get; set; } = null!;

        private Texture qrTexture = null!;

        private OsuNumberBox? codeTextBox;
        private OsuSpriteText? errorText;

        [BackgroundDependencyLoader]
        private void load()
        {
            using (var qrCode = QRCodeGenerator.GenerateQrCode(new PayloadGenerator.Url(OnlineArcadeClient.ARCADE_SSO_GENERATE_URL)))
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
        }

        private void onApiStateChanged(ValueChangedEvent<APIState> e)
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
                    InternalChild = new FillFlowContainer
                    {
                        Anchor = Anchor.Centre,
                        Origin = Anchor.Centre,
                        AutoSizeAxes = Axes.Both,
                        Direction = FillDirection.Vertical,
                        Spacing = new Vector2(10),
                        Children = new Drawable[]
                        {
                            new OsuSpriteText
                            {
                                Anchor = Anchor.TopCentre,
                                Origin = Anchor.TopCentre,
                                Text = "The play, first open the following link and log-in to osu!"
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
                                Text = "Then, type the 6-digit code displayed into the box below!"
                            },
                            codeTextBox = new OsuNumberBox
                            {
                                Anchor = Anchor.TopCentre,
                                Origin = Anchor.TopCentre,
                                Width = 200,
                                InputProperties = new TextInputProperties(TextInputType.Number),
                                PlaceholderText = LoginPanelStrings.EnterCode,
                            },
                            errorText = new OsuSpriteText
                            {
                                Anchor = Anchor.TopCentre,
                                Origin = Anchor.TopCentre,
                                Colour = colours.Red,
                                AlwaysPresent = true,
                                Text = "Invalid code provided",
                                Alpha = 0
                            }
                        }
                    };

                    codeTextBox.Current.BindValueChanged(onCodeChanged);
                    break;
            }
        }

        private void onCodeChanged(ValueChangedEvent<string> e)
        {
            string trimmedCode = e.NewValue.Trim();

            if (trimmedCode.Length == 6)
            {
                Task.Run(() => attemptLogin(trimmedCode));
                codeTextBox!.Current.Disabled = true;
            }
        }

        private async Task attemptLogin(string code)
        {
            try
            {
                Logger.Log($"[ARCADE] Retrieving user: {code}...");
                ArcadeIdentity user = await arcadeClient.GetUserWithCode(code);

                Logger.Log($"[ARCADE] Mapped user: {user.User.Username}");

                Logger.Log("[ARCADE] Attempting login...");
                arcadeClient.Connect(user).FireAndForget(completeLoginAttempt, failLoginAttempt);
            }
            catch (Exception ex)
            {
                failLoginAttempt(ex);
            }
        }

        private void completeLoginAttempt() => Scheduler.Add(() =>
        {
            Logger.Log("[ARCADE] Login completed");

            // Todo: Forward user to new screen.
        });

        private void failLoginAttempt(Exception ex) => Scheduler.Add(() =>
        {
            Logger.Error(ex, "[ARCADE] Login failed");

            errorText?.FadeIn().Delay(2000).FadeOut(500);

            if (codeTextBox != null)
            {
                codeTextBox.Current.Disabled = false;
                codeTextBox.Text = string.Empty;
            }
        });

        public override void OnResuming(ScreenTransitionEvent e)
        {
            base.OnResuming(e);

            if (codeTextBox != null)
            {
                codeTextBox.Text = string.Empty;
                codeTextBox.Current.Disabled = false;
            }
        }
    }
}
