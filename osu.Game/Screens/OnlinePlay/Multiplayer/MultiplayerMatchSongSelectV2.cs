// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Humanizer;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Logging;
using osu.Framework.Screens;
using osu.Game.Beatmaps;
using osu.Game.Graphics.UserInterface;
using osu.Game.Localisation;
using osu.Game.Online.API;
using osu.Game.Online.Multiplayer;
using osu.Game.Online.Rooms;
using osu.Game.Overlays;
using osu.Game.Overlays.Mods;
using osu.Game.Rulesets;
using osu.Game.Rulesets.Mods;
using osu.Game.Screens.Footer;
using osu.Game.Screens.SelectV2;
using osu.Game.Utils;

namespace osu.Game.Screens.OnlinePlay.Multiplayer
{
    public class MultiplayerMatchSongSelectV2 : SongSelect, IOnlinePlaySubScreen
    {
        public string ShortTitle => "song selection";

        public override string Title => ShortTitle.Humanize();

        protected readonly Bindable<bool> Freestyle = new Bindable<bool>(true);
        protected readonly Bindable<IReadOnlyList<Mod>> FreeMods = new Bindable<IReadOnlyList<Mod>>([]);

        private readonly IBindable<bool> operationInProgress = new Bindable<bool>();

        [Resolved]
        private MultiplayerClient client { get; set; } = null!;

        [Resolved]
        private OngoingOperationTracker operationTracker { get; set; } = null!;

        [Resolved]
        private BeatmapManager beatmapManager { get; set; } = null!;

        [Resolved]
        private RulesetStore rulesets { get; set; } = null!;

        [Resolved]
        private IOverlayManager? overlayManager { get; set; }

        private readonly Room room;
        private readonly PlaylistItem? itemToEdit;

        private ModSelectOverlay modSelect = null!;
        private FreeModSelectOverlay freeModSelect = null!;
        private LoadingLayer loadingLayer = null!;

        private IDisposable? modSelectOverlayRegistration;
        private IDisposable? selectionOperation;

        public MultiplayerMatchSongSelectV2(Room room, PlaylistItem? itemToEdit = null)
        {
            this.room = room;
            this.itemToEdit = itemToEdit;

            Padding = new MarginPadding { Horizontal = HORIZONTAL_OVERFLOW_PADDING };
            LeftPadding = new MarginPadding { Top = CORNER_RADIUS_HIDE_OFFSET + Header.HEIGHT };
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            AddInternal(loadingLayer = new LoadingLayer(true));

            LoadComponent(freeModSelect = new FreeModSelectOverlay
            {
                SelectedMods = { BindTarget = FreeMods },
                IsValidMod = isValidAllowedMod,
            });
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            if (itemToEdit != null)
            {
                // Prefer using a local databased beatmap lookup since OnlineId may be -1 for an invalid beatmap selection.
                BeatmapInfo? beatmapInfo = itemToEdit.Beatmap as BeatmapInfo;

                // And in the case that this isn't a local databased beatmap, query by online ID.
                if (beatmapInfo == null)
                {
                    int onlineId = itemToEdit.Beatmap.OnlineID;
                    beatmapInfo = beatmapManager.QueryBeatmap(b => b.OnlineID == onlineId);
                }

                if (beatmapInfo != null)
                    Beatmap.Value = beatmapManager.GetWorkingBeatmap(beatmapInfo);

                RulesetInfo? ruleset = rulesets.GetRuleset(itemToEdit.RulesetID);

                if (ruleset != null)
                {
                    Ruleset.Value = ruleset;

                    var rulesetInstance = ruleset.CreateInstance();
                    Debug.Assert(rulesetInstance != null);

                    // At this point, Mods contains both the required and allowed mods. For selection purposes, it should only contain the required mods.
                    // Similarly, freeMods is currently empty but should only contain the allowed mods.
                    Mods.Value = itemToEdit.RequiredMods.Select(m => m.ToMod(rulesetInstance)).ToArray();
                    FreeMods.Value = itemToEdit.AllowedMods.Select(m => m.ToMod(rulesetInstance)).ToArray();
                }

                Freestyle.Value = itemToEdit.Freestyle;
            }

            modSelectOverlayRegistration = overlayManager?.RegisterBlockingOverlay(freeModSelect);

            operationInProgress.BindTo(operationTracker.InProgress);
            operationInProgress.BindValueChanged(_ => updateLoadingLayer(), true);

            Mods.BindValueChanged(onGlobalModsChanged);
            Ruleset.BindValueChanged(onRulesetChanged);
            Freestyle.BindValueChanged(onFreestyleChanged);

            updateValidMods();
        }

        private void updateLoadingLayer()
        {
            if (operationInProgress.Value)
                loadingLayer.Show();
            else
                loadingLayer.Hide();
        }

        private void onGlobalModsChanged(ValueChangedEvent<IReadOnlyList<Mod>> mods)
        {
            updateValidMods();
        }

        private void onRulesetChanged(ValueChangedEvent<RulesetInfo> ruleset)
        {
            // Todo: We can probably attempt to preserve across rulesets like the global mods do.
            FreeMods.Value = [];
        }

        private void onFreestyleChanged(ValueChangedEvent<bool> enabled)
        {
            updateValidMods();

            if (enabled.NewValue)
            {
                freeModSelect.Hide();

                // Freestyle allows all mods to be selected as freemods. This does not play nicely for some components:
                // - We probably don't want to store a gigantic list of acronyms to the database.
                // - The mod select overlay isn't built to handle duplicate mods/mods from all rulesets being shoved into it.
                // Instead, freestyle inherently assumes this list is empty, and must be empty for server-side validation to pass.
                FreeMods.Value = [];
            }
            else
            {
                // When disabling freestyle, enable freemods by default.
                FreeMods.Value = freeModSelect.AllAvailableMods.Where(state => state.ValidForSelection.Value).Select(state => state.Mod).ToArray();
            }
        }

        /// <summary>
        /// Removes invalid mods from <see cref="OsuScreen.Mods"/> and <see cref="FreeMods"/>,
        /// and updates mod selection overlays to display the new mods valid for selection.
        /// </summary>
        private void updateValidMods()
        {
            Mod[] validMods = Mods.Value.Where(isValidRequiredMod).ToArray();
            if (!validMods.SequenceEqual(Mods.Value))
                Mods.Value = validMods;

            Mod[] validFreeMods = FreeMods.Value.Where(isValidAllowedMod).ToArray();
            if (!validFreeMods.SequenceEqual(FreeMods.Value))
                FreeMods.Value = validFreeMods;

            modSelect.IsValidMod = isValidRequiredMod;
            freeModSelect.IsValidMod = isValidAllowedMod;
        }

        protected override void OnStart()
        {
            if (operationInProgress.Value)
            {
                Logger.Log($"{nameof(OnStart)} aborted due to {nameof(operationInProgress)}");
                return;
            }

            PlaylistItem item = new PlaylistItem(Beatmap.Value.BeatmapInfo)
            {
                ID = room.Playlist.Count == 0 ? 0 : room.Playlist.Max(p => p.ID) + 1,
                RulesetID = Ruleset.Value.OnlineID,
                RequiredMods = Mods.Value.Select(m => new APIMod(m)).ToArray(),
                AllowedMods = FreeMods.Value.Select(m => new APIMod(m)).ToArray(),
                Freestyle = Freestyle.Value
            };

            // If the client is already in a room, update via the client.
            // Otherwise, update the playlist directly in preparation for it to be submitted to the API on match creation.
            if (client.Room != null)
            {
                selectionOperation = operationTracker.BeginOperation();

                Task task = itemToEdit != null
                    ? client.EditPlaylistItem(new MultiplayerPlaylistItem(item))
                    : client.AddPlaylistItem(new MultiplayerPlaylistItem(item));

                task.FireAndForget(onSuccess: () =>
                {
                    selectionOperation.Dispose();

                    Schedule(() =>
                    {
                        // If an error or server side trigger occurred this screen may have already exited by external means.
                        if (this.IsCurrentScreen())
                            this.Exit();
                    });
                }, onError: _ =>
                {
                    selectionOperation.Dispose();
                });
            }
            else
            {
                room.Playlist = [item];
                this.Exit();
            }
        }

        public override IReadOnlyList<ScreenFooterButton> CreateFooterButtons()
        {
            var buttons = base.CreateFooterButtons().ToList();

            buttons.Single(i => i is FooterButtonMods).TooltipText = MultiplayerMatchStrings.RequiredModsButtonTooltip;

            buttons.InsertRange(buttons.FindIndex(b => b is FooterButtonMods) + 1,
            [
                new FooterButtonFreeModsV2(freeModSelect)
                {
                    FreeMods = { BindTarget = FreeMods },
                    Freestyle = { BindTarget = Freestyle }
                },
                new FooterButtonFreestyleV2
                {
                    Freestyle = { BindTarget = Freestyle }
                }
            ]);

            return buttons;
        }

        protected override ModSelectOverlay CreateModSelectOverlay() => modSelect = new UserModSelectOverlay(OverlayColourScheme.Plum)
        {
            IsValidMod = isValidRequiredMod
        };

        /// <summary>
        /// Checks whether a given <see cref="Mod"/> is valid to be selected as a required mod.
        /// </summary>
        /// <param name="mod">The <see cref="Mod"/> to check.</param>
        private bool isValidRequiredMod(Mod mod) => ModUtils.IsValidModForMatch(mod, true, room.Type, Freestyle.Value);

        /// <summary>
        /// Checks whether a given <see cref="Mod"/> is valid to be selected as an allowed mod.
        /// </summary>
        /// <param name="mod">The <see cref="Mod"/> to check.</param>
        private bool isValidAllowedMod(Mod mod) => ModUtils.IsValidModForMatch(mod, false, room.Type, Freestyle.Value)
                                                   // Mod must not be contained in the required mods.
                                                   && Mods.Value.All(m => m.Acronym != mod.Acronym)
                                                   // Mod must be compatible with all the required mods.
                                                   && ModUtils.CheckCompatibleSet(Mods.Value.Append(mod).ToArray());

        protected override void Dispose(bool isDisposing)
        {
            base.Dispose(isDisposing);
            modSelectOverlayRegistration?.Dispose();
        }
    }
}
