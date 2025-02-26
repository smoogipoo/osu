// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using osu.Framework.Allocation;
using osu.Framework.Audio.Track;
using osu.Framework.Bindables;
using osu.Framework.Extensions;
using osu.Framework.Graphics.Textures;
using osu.Framework.Lists;
using osu.Framework.Logging;
using osu.Framework.Threading;
using osu.Game.Configuration;
using osu.Game.Database;
using osu.Game.Rulesets;
using osu.Game.Rulesets.Difficulty;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Scoring;
using osu.Game.Rulesets.UI;
using osu.Game.Scoring;
using osu.Game.Skinning;
using osu.Game.Storyboards;

namespace osu.Game.Beatmaps
{
    /// <summary>
    /// A component which performs and acts as a central cache for difficulty calculations of beatmap/ruleset/mod combinations.
    /// Currently not persisted between game sessions.
    /// </summary>
    public partial class BeatmapDifficultyCache : MemoryCachingComponent<BeatmapDifficultyCache.DifficultyCacheLookup, StarDifficulty?>
    {
        // Too many simultaneous updates can lead to stutters. One thread seems to work fine for song select display purposes.
        private readonly ThreadedTaskScheduler updateScheduler = new ThreadedTaskScheduler(1, nameof(BeatmapDifficultyCache));

        /// <summary>
        /// All bindables that should be updated along with the current ruleset + mods.
        /// </summary>
        private readonly WeakList<BindableStarDifficulty> trackedBindables = new WeakList<BindableStarDifficulty>();

        /// <summary>
        /// Cancellation sources used by tracked bindables.
        /// </summary>
        private readonly List<CancellationTokenSource> linkedCancellationSources = new List<CancellationTokenSource>();

        /// <summary>
        /// Lock to be held when operating on <see cref="trackedBindables"/> or <see cref="linkedCancellationSources"/>.
        /// </summary>
        private readonly object bindableUpdateLock = new object();

        private CancellationTokenSource trackedUpdateCancellationSource = new CancellationTokenSource();

        [Resolved]
        private BeatmapManager beatmapManager { get; set; } = null!;

        [Resolved]
        private Bindable<RulesetInfo> currentRuleset { get; set; } = null!;

        [Resolved]
        private Bindable<IReadOnlyList<Mod>> currentMods { get; set; } = null!;

        [Resolved]
        private RealmAccess realm { get; set; } = null!;

        private ModSettingChangeTracker? modSettingChangeTracker;
        private ScheduledDelegate? debouncedModSettingsChange;

        protected override void LoadComplete()
        {
            base.LoadComplete();

            currentRuleset.BindValueChanged(_ => Scheduler.AddOnce(updateTrackedBindables));

            currentMods.BindValueChanged(mods =>
            {
                modSettingChangeTracker?.Dispose();

                Scheduler.AddOnce(updateTrackedBindables);

                modSettingChangeTracker = new ModSettingChangeTracker(mods.NewValue);
                modSettingChangeTracker.SettingChanged += _ =>
                {
                    debouncedModSettingsChange?.Cancel();
                    debouncedModSettingsChange = Scheduler.AddDelayed(updateTrackedBindables, 100);
                };
            }, true);
        }

        public void Invalidate(BeatmapInfo beatmap)
        {
            base.Invalidate(lookup => lookup.Beatmap.Equals(beatmap.ID));
        }

        /// <summary>
        /// Retrieves a bindable containing the star difficulty of a <see cref="BeatmapInfo"/> that follows the currently-selected ruleset and mods.
        /// </summary>
        /// <param name="beatmapInfo">The <see cref="BeatmapInfo"/> to get the difficulty of.</param>
        /// <param name="cancellationToken">An optional <see cref="CancellationToken"/> which stops updating the star difficulty for the given <see cref="BeatmapInfo"/>.</param>
        /// <returns>A bindable that is updated to contain the star difficulty when it becomes available. Will be null while in an initial calculating state (but not during updates to ruleset and mods if a stale value is already propagated).</returns>
        public IBindable<StarDifficulty?> GetBindableDifficulty(IBeatmapInfo beatmapInfo, CancellationToken cancellationToken = default)
        {
            var bindable = new BindableStarDifficulty(beatmapInfo, cancellationToken);

            updateBindable(bindable, currentRuleset.Value, currentMods.Value, cancellationToken);

            lock (bindableUpdateLock)
                trackedBindables.Add(bindable);

            return bindable;
        }

        /// <summary>
        /// Retrieves the difficulty of a <see cref="IBeatmapInfo"/>.
        /// </summary>
        /// <param name="beatmapInfo">The <see cref="IBeatmapInfo"/> to get the difficulty of.</param>
        /// <param name="rulesetInfo">The <see cref="IRulesetInfo"/> to get the difficulty with.</param>
        /// <param name="mods">The <see cref="Mod"/>s to apply the difficulty with.</param>
        /// <param name="cancellationToken">An optional <see cref="CancellationToken"/> which stops computing the star difficulty.</param>
        public virtual async Task<StarDifficulty> GetDifficultyAsync(IBeatmapInfo beatmapInfo, IRulesetInfo? rulesetInfo = null,
                                                                     IEnumerable<Mod>? mods = null, CancellationToken cancellationToken = default)
        {
            // In the case that the user hasn't given us a ruleset, use the beatmap's default ruleset.
            rulesetInfo ??= beatmapInfo.Ruleset;

            // In case the provided models come from online sources, the local difficulty can not be computed.
            if (beatmapInfo is not BeatmapInfo localBeatmap || rulesetInfo is not RulesetInfo localRuleset)
                return getDirectDifficulty(beatmapInfo);

            return await GetAsync(new DifficultyCacheLookup(localBeatmap, localRuleset, mods), cancellationToken).ConfigureAwait(false)
                   ?? getDirectDifficulty(beatmapInfo);

            static StarDifficulty getDirectDifficulty(IBeatmapInfo beatmap)
                => new StarDifficulty(beatmap.StarRating, (beatmap as IBeatmapOnlineInfo)?.MaxCombo ?? 0);
        }

        protected override Task<StarDifficulty?> ComputeValueAsync(DifficultyCacheLookup lookup, CancellationToken cancellationToken = default)
        {
            return Task.Factory.StartNew(() =>
            {
                if (CheckExists(lookup, out var existing))
                    return existing;

                return computeDifficulty(lookup, cancellationToken);
            }, cancellationToken, TaskCreationOptions.HideScheduler | TaskCreationOptions.RunContinuationsAsynchronously, updateScheduler);
        }

        protected override bool CacheNullValues => false;

        public Task<List<TimedDifficultyAttributes>> GetTimedDifficultyAttributesAsync(IWorkingBeatmap beatmap, Ruleset ruleset, Mod[] mods, CancellationToken cancellationToken = default)
        {
            return Task.Factory.StartNew(() => ruleset.CreateDifficultyCalculator(beatmap).CalculateTimed(mods, cancellationToken),
                cancellationToken,
                TaskCreationOptions.HideScheduler | TaskCreationOptions.RunContinuationsAsynchronously,
                updateScheduler);
        }

        /// <summary>
        /// Updates all tracked <see cref="BindableStarDifficulty"/> using the current ruleset and mods.
        /// </summary>
        private void updateTrackedBindables()
        {
            lock (bindableUpdateLock)
            {
                cancelTrackedBindableUpdate();

                foreach (var b in trackedBindables)
                {
                    var linkedSource = CancellationTokenSource.CreateLinkedTokenSource(trackedUpdateCancellationSource.Token, b.CancellationToken);
                    linkedCancellationSources.Add(linkedSource);

                    updateBindable(b, currentRuleset.Value, currentMods.Value, linkedSource.Token);
                }
            }
        }

        /// <summary>
        /// Cancels the existing update of all tracked <see cref="BindableStarDifficulty"/> via <see cref="updateTrackedBindables"/>.
        /// </summary>
        private void cancelTrackedBindableUpdate()
        {
            lock (bindableUpdateLock)
            {
                trackedUpdateCancellationSource.Cancel();
                trackedUpdateCancellationSource = new CancellationTokenSource();

                foreach (var c in linkedCancellationSources)
                    c.Dispose();

                linkedCancellationSources.Clear();
            }
        }

        /// <summary>
        /// Updates the value of a <see cref="BindableStarDifficulty"/> with a given ruleset + mods.
        /// </summary>
        /// <param name="bindable">The <see cref="BindableStarDifficulty"/> to update.</param>
        /// <param name="rulesetInfo">The <see cref="IRulesetInfo"/> to update with.</param>
        /// <param name="mods">The <see cref="Mod"/>s to update with.</param>
        /// <param name="cancellationToken">A token that may be used to cancel this update.</param>
        private void updateBindable(BindableStarDifficulty bindable, IRulesetInfo? rulesetInfo, IEnumerable<Mod>? mods, CancellationToken cancellationToken = default)
        {
            // GetDifficultyAsync will fall back to existing data from IBeatmapInfo if not locally available
            // (contrary to GetAsync)
            GetDifficultyAsync(bindable.BeatmapInfo, rulesetInfo, mods, cancellationToken)
                .ContinueWith(task =>
                {
                    // We're on a threadpool thread, but we should exit back to the update thread so consumers can safely handle value-changed events.
                    Schedule(() =>
                    {
                        if (cancellationToken.IsCancellationRequested)
                            return;

                        bindable.Value = task.GetResultSafely();
                    });
                }, cancellationToken);
        }

        /// <summary>
        /// Computes the difficulty defined by a <see cref="DifficultyCacheLookup"/> key, and stores it to the timed cache.
        /// </summary>
        /// <param name="key">The <see cref="DifficultyCacheLookup"/> that defines the computation parameters.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The <see cref="StarDifficulty"/>.</returns>
        private StarDifficulty? computeDifficulty(DifficultyCacheLookup key, CancellationToken cancellationToken = default)
        {
            return realm.Run<StarDifficulty?>(r =>
            {
                BeatmapInfo? beatmapInfo = r.Find<BeatmapInfo>(key.Beatmap);
                RulesetInfo? rulesetInfo = r.Find<RulesetInfo>(key.Ruleset);

                if (beatmapInfo == null || rulesetInfo == null)
                    return null;

                try
                {
                    Ruleset ruleset = rulesetInfo.CreateInstance();
                    PlayableCachedWorkingBeatmap workingBeatmap = new PlayableCachedWorkingBeatmap(beatmapManager.GetWorkingBeatmap(beatmapInfo));
                    IBeatmap playableBeatmap = workingBeatmap.GetPlayableBeatmap(ruleset.RulesetInfo, key.OrderedMods, cancellationToken);

                    var difficulty = ruleset.CreateDifficultyCalculator(workingBeatmap).Calculate(key.OrderedMods, cancellationToken);
                    cancellationToken.ThrowIfCancellationRequested();

                    var performanceCalculator = ruleset.CreatePerformanceCalculator();
                    if (performanceCalculator == null)
                        return new StarDifficulty(difficulty, new PerformanceAttributes());

                    ScoreProcessor scoreProcessor = ruleset.CreateScoreProcessor();
                    scoreProcessor.Mods.Value = key.OrderedMods;
                    scoreProcessor.ApplyBeatmap(playableBeatmap);
                    cancellationToken.ThrowIfCancellationRequested();

                    ScoreInfo perfectScore = new ScoreInfo(beatmapInfo, rulesetInfo)
                    {
                        Passed = true,
                        Accuracy = 1,
                        Mods = key.OrderedMods,
                        MaxCombo = scoreProcessor.MaximumCombo,
                        Combo = scoreProcessor.MaximumCombo,
                        TotalScore = scoreProcessor.MaximumTotalScore,
                        Statistics = scoreProcessor.MaximumStatistics,
                        MaximumStatistics = scoreProcessor.MaximumStatistics
                    };

                    var performance = performanceCalculator.Calculate(perfectScore, difficulty);
                    cancellationToken.ThrowIfCancellationRequested();

                    return new StarDifficulty(difficulty, performance);
                }
                catch (OperationCanceledException)
                {
                    // no need to log, cancellations are expected as part of normal operation.
                    return null;
                }
                catch (BeatmapInvalidForRulesetException invalidForRuleset)
                {
                    if (rulesetInfo.Equals(beatmapInfo.Ruleset))
                        Logger.Error(invalidForRuleset, $"Failed to convert {beatmapInfo.OnlineID} to the beatmap's default ruleset ({beatmapInfo.Ruleset}).");

                    return null;
                }
                catch (Exception unknownException)
                {
                    Logger.Error(unknownException, "Failed to calculate beatmap difficulty");

                    return null;
                }
            });
        }

        protected override void Dispose(bool isDisposing)
        {
            base.Dispose(isDisposing);

            modSettingChangeTracker?.Dispose();

            cancelTrackedBindableUpdate();
            updateScheduler.Dispose();
        }

        public readonly struct DifficultyCacheLookup : IEquatable<DifficultyCacheLookup>
        {
            /// <summary>
            /// The database beatmap <see cref="BeatmapInfo.ID">key</see>.
            /// </summary>
            public readonly Guid Beatmap;

            /// <summary>
            /// The database ruleset <see cref="RulesetInfo.ShortName">key</see>.
            /// </summary>
            public readonly string Ruleset;

            /// <summary>
            /// The mods to be applied.
            /// </summary>
            public readonly Mod[] OrderedMods;

            public DifficultyCacheLookup(BeatmapInfo beatmapInfo, RulesetInfo ruleset, IEnumerable<Mod>? mods)
            {
                Beatmap = beatmapInfo.ID;
                Ruleset = ruleset.ShortName;
                OrderedMods = mods?.OrderBy(m => m.Acronym).Select(mod => mod.DeepClone()).ToArray() ?? Array.Empty<Mod>();
            }

            public bool Equals(DifficultyCacheLookup other)
                => Beatmap.Equals(other.Beatmap)
                   && string.Equals(Ruleset, other.Ruleset, StringComparison.Ordinal)
                   && OrderedMods.SequenceEqual(other.OrderedMods);

            public override int GetHashCode()
            {
                var hashCode = new HashCode();

                hashCode.Add(Beatmap);
                hashCode.Add(Ruleset);
                foreach (var mod in OrderedMods)
                    hashCode.Add(mod);

                return hashCode.ToHashCode();
            }
        }

        private class BindableStarDifficulty : Bindable<StarDifficulty?>
        {
            public readonly IBeatmapInfo BeatmapInfo;
            public readonly CancellationToken CancellationToken;

            public BindableStarDifficulty(IBeatmapInfo beatmapInfo, CancellationToken cancellationToken)
            {
                BeatmapInfo = beatmapInfo;
                CancellationToken = cancellationToken;
            }
        }

        /// <summary>
        /// A working beatmap that caches its playable representation.
        /// This is intended as single-use for when it is guaranteed that the playable beatmap can be reused.
        /// </summary>
        private class PlayableCachedWorkingBeatmap : IWorkingBeatmap
        {
            private readonly IWorkingBeatmap working;
            private IBeatmap? playable;

            public PlayableCachedWorkingBeatmap(IWorkingBeatmap working)
            {
                this.working = working;
            }

            public IBeatmap GetPlayableBeatmap(IRulesetInfo ruleset, IReadOnlyList<Mod> mods)
                => playable ??= working.GetPlayableBeatmap(ruleset, mods);

            public IBeatmap GetPlayableBeatmap(IRulesetInfo ruleset, IReadOnlyList<Mod> mods, CancellationToken cancellationToken)
                => playable ??= working.GetPlayableBeatmap(ruleset, mods, cancellationToken);

            IBeatmapInfo IWorkingBeatmap.BeatmapInfo => working.BeatmapInfo;
            bool IWorkingBeatmap.BeatmapLoaded => working.BeatmapLoaded;
            bool IWorkingBeatmap.TrackLoaded => working.TrackLoaded;
            IBeatmap IWorkingBeatmap.Beatmap => working.Beatmap;
            Texture IWorkingBeatmap.GetBackground() => working.GetBackground();
            Texture IWorkingBeatmap.GetPanelBackground() => working.GetPanelBackground();
            Waveform IWorkingBeatmap.Waveform => working.Waveform;
            Storyboard IWorkingBeatmap.Storyboard => working.Storyboard;
            ISkin IWorkingBeatmap.Skin => working.Skin;
            Track IWorkingBeatmap.Track => working.Track;
            Track IWorkingBeatmap.LoadTrack() => working.LoadTrack();
            Stream IWorkingBeatmap.GetStream(string storagePath) => working.GetStream(storagePath);
            void IWorkingBeatmap.BeginAsyncLoad() => working.BeginAsyncLoad();
            void IWorkingBeatmap.CancelAsyncLoad() => working.CancelAsyncLoad();
            void IWorkingBeatmap.PrepareTrackForPreview(bool looping, double offsetFromPreviewPoint) => working.PrepareTrackForPreview(looping, offsetFromPreviewPoint);
        }
    }
}
