// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Configuration;
using osu.Framework.Extensions.ObjectExtensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Rendering;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.Textures;
using osu.Framework.Input;
using osu.Framework.Input.Bindings;
using osu.Framework.Logging;
using osu.Framework.Screens;
using osu.Game.Configuration;
using osu.Game.Database;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osu.Game.Graphics.UserInterface;
using osu.Game.Input.Bindings;
using osu.Game.Localisation;
using osu.Game.Online.API;
using osu.Game.Online.Multiplayer;
using osu.Game.Overlays;
using osu.Game.Overlays.Login;
using osu.Game.Rulesets;
using osu.Game.Screens;
using osu.Game.Screens.Backgrounds;
using osuTK;
using osuTK.Graphics;
using QRCoder;
using Realms;

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

        [Resolved]
        private FrameworkConfigManager frameworkConfig { get; set; } = null!;

        [Resolved]
        private OsuConfigManager osuConfig { get; set; } = null!;

        [Resolved]
        private IRulesetConfigCache rulesetConfigs { get; set; } = null!;

        [Resolved]
        private RulesetStore rulesetStore { get; set; } = null!;

        [Resolved]
        private RealmAccess realmAccess { get; set; } = null!;

        [Resolved]
        private GlobalActionContainer globalActionContainer { get; set; } = null!;

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
                                Direction = FillDirection.Vertical,
                                Spacing = new Vector2(10),
                                Padding = new MarginPadding(20),
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
                                        Text = "Type the 6-digit code displayed into the box below:"
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

            resetSettings();
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

            resetSettings();
        }

        private void resetSettings()
        {
            Logger.Log("[ARCADE] Resetting framework settings...");

            foreach ((string setting, IBindable bindable) in getSettings(frameworkConfig))
            {
                switch (setting)
                {
                    case "LastDisplayDevice":
                        continue;

                    case "WindowMode":
                        setValue(bindable, WindowMode.Fullscreen);
                        break;

                    case "FrameSync":
                        setValue(bindable, FrameSync.Unlimited);
                        break;

                    default:
                        setDefault(bindable);
                        break;
                }
            }

            Logger.Log("[ARCADE] Resetting global settings...");

            foreach ((string setting, IBindable bindable) in getSettings(osuConfig))
            {
                switch (setting)
                {
                    case "Username":
                    case "Token":
                    case "SavePassword":
                    case "SaveUsername":
                    case "ReleaseStream":
                    case "Version":
                    case "LastProcessedMetadataId":
                    case "LastOnlineTagsPopulation":
                        continue;

                    case "ShowFirstRunSetup":
                        setValue(bindable, false);
                        break;

                    case "MouseDisableButtons":
                        setValue(bindable, true);
                        break;

                    default:
                        setDefault(bindable);
                        break;
                }
            }

            foreach (var ruleset in rulesetStore.AvailableRulesets)
            {
                Logger.Log($"[ARCADE] Resetting settings for ruleset: {ruleset.Name}...");

                var rulesetConfig = rulesetConfigs.GetConfigFor(ruleset.CreateInstance());
                if (rulesetConfig == null)
                    continue;

                foreach ((string setting, IBindable bindable) in getSettings(rulesetConfig))
                {
                    switch (setting)
                    {
                        default:
                            setDefault(bindable);
                            break;
                    }
                }
            }

            Logger.Log("[ARCADE] Resetting keybindings...");

            realmAccess.Run(r =>
            {
                using (var transaction = r.BeginWrite())
                {
                    r.RemoveAll<RealmKeyBinding>();

                    insertDefaultKeyBindings(r, globalActionContainer.DefaultKeyBindings);

                    foreach (var ruleset in rulesetStore.AvailableRulesets)
                    {
                        var instance = ruleset.CreateInstance();
                        foreach (int variant in instance.AvailableVariants)
                            insertDefaultKeyBindings(r, instance.GetDefaultKeyBindings(variant), ruleset.ShortName, variant);
                    }

                    transaction.Commit();
                }
            });

            static Dictionary<string, IBindable> getSettings(IConfigManager config)
            {
                IDictionary configStore = (IDictionary)config.GetType().GetField("ConfigStore", BindingFlags.Instance | BindingFlags.NonPublic)!.GetValue(config)!;

                Dictionary<string, IBindable> dict = new Dictionary<string, IBindable>();
                foreach (object key in configStore.Keys)
                    dict[key.ToString()!] = (IBindable)configStore[key]!;

                return dict;
            }

            static void setDefault(IBindable target)
            {
                target.GetType().GetMethod(nameof(Bindable<int>.SetDefault), BindingFlags.Instance | BindingFlags.Public)!.Invoke(target, null);
            }

            static void setValue<T>(IBindable target, T value)
            {
                target.GetType().GetProperty(nameof(Bindable<int>.Value), BindingFlags.Instance | BindingFlags.Public)!.SetValue(target, value);
            }

            static void insertDefaultKeyBindings(Realm realm, IEnumerable<IKeyBinding> defaults, string? rulesetName = null, int? variant = null)
            {
                foreach (var defaultsForAction in defaults.GroupBy(k => k.Action))
                    realm.Add(defaultsForAction.Select(k => new RealmKeyBinding(k.Action, k.KeyCombination, rulesetName, variant)));
            }
        }

        protected override void Dispose(bool isDisposing)
        {
            base.Dispose(isDisposing);

            if (arcadeClient.IsNotNull())
                arcadeClient.UserConnected -= onClientConnected;
        }
    }
}
