// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Configuration;
using osu.Framework.Input.Bindings;
using osu.Framework.Logging;
using osu.Framework.Platform;
using osu.Framework.Testing;
using osu.Game.Configuration;
using osu.Game.Database;
using osu.Game.Input.Bindings;
using osu.Game.Overlays;
using osu.Game.Overlays.Settings.Sections.Input;
using osu.Game.Rulesets;
using Realms;

namespace osu.Game.Arcade.Screens
{
    public class ArcadeConfigManager : IniConfigManager<ArcadeSetting>
    {
        private readonly GameHost host;
        private readonly FrameworkConfigManager frameworkConfig;
        private readonly OsuConfigManager osuConfig;
        private readonly RulesetStore rulesetStore;
        private readonly IRulesetConfigCache rulesetConfigs;
        private readonly RealmAccess realmAccess;
        private readonly GlobalActionContainer globalActionContainer;
        private readonly SettingsOverlay settingsOverlay;

        public ArcadeConfigManager(IReadOnlyDependencyContainer dependencies)
            : base(dependencies.Get<Storage>())
        {
            host = dependencies.Get<GameHost>();
            frameworkConfig = dependencies.Get<FrameworkConfigManager>();
            osuConfig = dependencies.Get<OsuConfigManager>();
            rulesetStore = dependencies.Get<RulesetStore>();
            rulesetConfigs = dependencies.Get<IRulesetConfigCache>();
            realmAccess = dependencies.Get<RealmAccess>();
            globalActionContainer = dependencies.Get<GlobalActionContainer>();
            settingsOverlay = dependencies.Get<SettingsOverlay>();
        }

        public void Reset()
        {
            resetFrameworkSettings();
            resetGlobalSettings();
            resetRulesetSettings();
            resetKeyBindings();
            resetInputHandlers();
        }

        private void resetFrameworkSettings()
        {
            Logger.Log("[ARCADE] Resetting framework settings...");

            foreach ((string setting, IBindable bindable) in getSettings(frameworkConfig))
            {
                switch (setting)
                {
                    case "LastDisplayDevice":
                        continue;

                    case "WindowMode":
#if !DEBUG
                        setValue(bindable, WindowMode.Fullscreen);
#endif
                        break;

                    case "FrameSync":
                        setValue(bindable, FrameSync.Unlimited);
                        break;

                    default:
                        setDefault(bindable);
                        break;
                }
            }
        }

        private void resetGlobalSettings()
        {
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

                    case "HitLighting":
                        setValue(bindable, false);
                        break;

                    case "StarFountains":
                        setValue(bindable, false);
                        break;

                    case "DimLevel":
                        setValue(bindable, 0.92f);
                        setMaxValue(bindable, 0.92f);
                        break;

                    case "BeatmapSkins":
                        setValue(bindable, false);
                        break;

                    case "BeatmapColours":
                        setValue(bindable, false);
                        break;

                    default:
                        setDefault(bindable);
                        break;
                }
            }
        }

        private void resetRulesetSettings()
        {
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
        }

        private void resetKeyBindings()
        {
            Logger.Log("[ARCADE] Resetting keybindings...");

            realmAccess.Run(r =>
            {
                using (var transaction = r.BeginWrite())
                {
                    resetBindings(r, adjustBindings(globalActionContainer.DefaultKeyBindings.ToArray(), null));

                    foreach (var ruleset in rulesetStore.AvailableRulesets)
                    {
                        var instance = ruleset.CreateInstance();

                        foreach (int variant in instance.AvailableVariants)
                            resetBindings(r, adjustBindings(instance.GetDefaultKeyBindings(variant).OfType<IKeyBinding>().ToArray(), ruleset.OnlineID), ruleset.ShortName, variant);
                    }

                    transaction.Commit();
                }
            });

            MethodInfo reloadAllBindings = typeof(KeyBindingsSubsection).GetMethod("reloadAllBindings", BindingFlags.Instance | BindingFlags.NonPublic)!;
            foreach (var section in settingsOverlay.ChildrenOfType<KeyBindingsSubsection>())
                reloadAllBindings.Invoke(section, null);

            realmAccess.Write(r =>
            {
                if (r.All<RealmKeyBinding>().SingleOrDefault(b => b.ActionInt == 34 && b.RulesetName == null) is RealmKeyBinding toggleInterfaceBinding)
                    r.Remove(toggleInterfaceBinding);
            });

            static void resetBindings(Realm realm, IEnumerable<IKeyBinding> defaultBindings, string? rulesetName = null, int? variant = null)
            {
                foreach (var bindingGroup in defaultBindings.GroupBy(b => b.Action))
                {
                    int actionInt = (int)bindingGroup.Key;

                    RealmKeyBinding[] existingBindings = realm.All<RealmKeyBinding>()
                                                              .Where(k => k.ActionInt == actionInt && k.RulesetName == rulesetName && k.Variant == variant)
                                                              .ToArray();

                    if (existingBindings.Length == 0)
                        continue;

                    int bindingIndex = 0;

                    foreach (var binding in bindingGroup)
                    {
                        if (!existingBindings[bindingIndex].KeyCombination.Equals(binding.KeyCombination))
                            existingBindings[bindingIndex].KeyCombination = binding.KeyCombination;
                        bindingIndex++;
                    }
                }
            }
        }

        private void resetInputHandlers()
        {
            Logger.Log("[ARCADE] Resetting input settings...");

            host.ResetInputHandlers();
        }

        private IKeyBinding[] adjustBindings(IKeyBinding[] bindings, int? rulesetId)
        {
            switch (rulesetId)
            {
                case null:
                {
                    if (bindings.SingleOrDefault(b => (int)b.Action == 34) is IKeyBinding toggleInterfaceBinding)
                        toggleInterfaceBinding.KeyCombination = new KeyCombination(InputKey.None);
                    break;
                }

                case 0:
                {
                    if (bindings.SingleOrDefault(b => (int)b.Action == 2) is IKeyBinding smokeBinding)
                        smokeBinding.KeyCombination = new KeyCombination(InputKey.None);
                    break;
                }
            }

            return bindings;
        }

        private static Dictionary<string, IBindable> getSettings(IConfigManager config)
        {
            IDictionary configStore = (IDictionary)config.GetType().GetField("ConfigStore", BindingFlags.Instance | BindingFlags.NonPublic)!.GetValue(config)!;

            Dictionary<string, IBindable> dict = new Dictionary<string, IBindable>();
            foreach (object key in configStore.Keys)
                dict[key.ToString()!] = (IBindable)configStore[key]!;

            return dict;
        }

        private static void setDefault(IBindable target)
        {
            try
            {
                target.GetType().GetMethod(nameof(Bindable<int>.SetDefault), BindingFlags.Instance | BindingFlags.Public)!.Invoke(target, null);
            }
            catch
            {
            }
        }

        private static void setValue<T>(IBindable target, T value)
        {
            try
            {
                target.GetType().GetProperty(nameof(Bindable<int>.Value), BindingFlags.Instance | BindingFlags.Public)!.SetValue(target, value);
            }
            catch
            {
            }
        }

        private static void setMaxValue<T>(IBindable target, T value)
        {
            try
            {
                target.GetType().GetProperty(nameof(BindableNumber<int>.MaxValue), BindingFlags.Instance | BindingFlags.Public)!.SetValue(target, value);
            }
            catch
            {
            }
        }
    }

    public enum ArcadeSetting;
}
