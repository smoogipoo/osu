// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using System;
using System.Linq;
using JetBrains.Annotations;
using osu.Framework.Allocation;
using osu.Framework.Logging;
using osu.Framework.Testing;
using osu.Game.Configuration;
using osu.Game.Rulesets;
using osu.Game.Rulesets.Mods;
using osu.Game.Screens.Play;

namespace osu.Game.Tests.Visual
{
    public abstract partial class PlayerTestScene : RateAdjustedBeatmapTestScene
    {
        /// <summary>
        /// Whether custom test steps are provided. Custom tests should invoke <see cref="CreateTest"/> to create the test steps.
        /// </summary>
        protected virtual bool HasCustomSteps => false;

        protected TestPlayer Player;

        protected OsuConfigManager LocalConfig;

        private long? lastFcTotalProcessedFrames;
        private long? lastTrackTotalProcessedFrames;
        private long? lastGcTotalProcessedFrames;

        protected override void Update()
        {
            base.Update();

            if (Player?.GameplayClockContainer != null)
            {
                ClockBackedTestWorkingBeatmap.TrackVirtualManual tvm = Player.GameplayClockContainer.GameplayClock.Source as ClockBackedTestWorkingBeatmap.TrackVirtualManual;

                long fcTotalProcessedFrames = long.Parse(Player.GameplayClockContainer.GameplayClock.GetSnapshot().Split('\n')[0].Split(':', StringSplitOptions.TrimEntries)[1]);
                long trackTotalProcessedFrames = tvm?.TotalProcessedFrames ?? 0;
                long gcTotalProcessedFrames = Player.GameplayClockContainer.TotalProcessedFrames;

                if (fcTotalProcessedFrames > lastFcTotalProcessedFrames
                    && trackTotalProcessedFrames == lastTrackTotalProcessedFrames
                    && gcTotalProcessedFrames > lastGcTotalProcessedFrames
                    && tvm?.IsRunning == true)
                {
                    Logger.Log("Framed fast clock updated but TVM did not!");
                    Logger.Log(Player.GameplayClockContainer.GameplayClock.GetSnapshot());
                }

                lastFcTotalProcessedFrames = fcTotalProcessedFrames;
                lastTrackTotalProcessedFrames = trackTotalProcessedFrames;
                lastGcTotalProcessedFrames = gcTotalProcessedFrames;
            }
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            Dependencies.Cache(LocalConfig = new OsuConfigManager(LocalStorage));
            LocalConfig.GetBindable<double>(OsuSetting.DimLevel).Value = 1.0;
        }

        [SetUpSteps]
        public override void SetUpSteps()
        {
            base.SetUpSteps();

            if (!HasCustomSteps)
                CreateTest();
        }

        protected void CreateTest([CanBeNull] Action action = null)
        {
            if (action != null && !HasCustomSteps)
                throw new InvalidOperationException($"Cannot add custom test steps without {nameof(HasCustomSteps)} being set.");

            action?.Invoke();

            AddStep($"Load player for {CreatePlayerRuleset().Description}", LoadPlayer);
            AddUntilStep("player loaded", () => Player.IsLoaded && Player.Alpha == 1);

            if (AllowBackwardsSeeks)
            {
                AddStep("allow backwards seeking", () =>
                {
                    Player.DrawableRuleset.AllowBackwardsSeeks = AllowBackwardsSeeks;
                });
            }
        }

        protected virtual bool AllowFail => false;

        protected virtual bool AllowBackwardsSeeks => false;

        protected virtual bool Autoplay => false;

        protected void LoadPlayer() => LoadPlayer(Array.Empty<Mod>());

        protected void LoadPlayer(Mod[] mods)
        {
            // if a player screen is present already, we must exit that before loading another one,
            // otherwise it'll crash on SpectatorClient.BeginPlaying being called while client is in "playing" state already.
            if (Stack.CurrentScreen is Player)
                Stack.Exit();

            var ruleset = CreatePlayerRuleset();
            Ruleset.Value = ruleset.RulesetInfo;

            var beatmap = CreateBeatmap(ruleset.RulesetInfo);

            Beatmap.Value = CreateWorkingBeatmap(beatmap);

            SelectedMods.Value = mods;

            if (!AllowFail)
            {
                var noFailMod = ruleset.CreateMod<ModNoFail>();
                if (noFailMod != null)
                    SelectedMods.Value = SelectedMods.Value.Append(noFailMod).ToArray();
            }

            if (Autoplay)
            {
                var mod = ruleset.GetAutoplayMod();
                if (mod != null)
                    SelectedMods.Value = SelectedMods.Value.Append(mod).ToArray();
            }

            Player = CreatePlayer(ruleset);
            LoadScreen(Player);
        }

        protected override void Dispose(bool isDisposing)
        {
            LocalConfig?.Dispose();
            base.Dispose(isDisposing);
        }

        /// <summary>
        /// Creates the ruleset for setting up the <see cref="Player"/> component.
        /// </summary>
        [NotNull]
        protected abstract Ruleset CreatePlayerRuleset();

        protected sealed override Ruleset CreateRuleset() => CreatePlayerRuleset();

        protected virtual TestPlayer CreatePlayer(Ruleset ruleset) => new TestPlayer(false, false, AllowBackwardsSeeks);
    }
}
