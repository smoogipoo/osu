// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using System.Linq;
using System.Threading;
using osu.Framework.Allocation;
using osu.Framework.Audio;
using osu.Framework.Audio.Sample;
using osu.Framework.Bindables;
using osu.Framework.Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Logging;
using osu.Framework.Screens;
using osu.Game.Audio;
using osu.Game.Beatmaps;
using osu.Game.Configuration;
using osu.Game.Database;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osu.Game.Graphics.UserInterface;
using osu.Game.Online;
using osu.Game.Online.API.Requests.Responses;
using osu.Game.Online.Multiplayer;
using osu.Game.Online.Multiplayer.MatchTypes.RankedPlay;
using osu.Game.Online.Rooms;
using osu.Game.Overlays;
using osu.Game.Overlays.Dialog;
using osu.Game.Rulesets;
using osu.Game.Screens.OnlinePlay.Matchmaking.Match.Gameplay;
using osu.Game.Screens.OnlinePlay.Multiplayer;

namespace osu.Game.Screens.OnlinePlay.Matchmaking.RankedPlay
{
    public partial class RankedPlayScreen2 : OsuScreen, IPreviewTrackOwner, IHandlePresentBeatmap
    {
        public ShearedButton ActionButton { get; }

        [Cached(typeof(OnlinePlayBeatmapAvailabilityTracker))]
        private readonly OnlinePlayBeatmapAvailabilityTracker beatmapAvailabilityTracker = new MultiplayerBeatmapAvailabilityTracker();

        [Resolved]
        private MultiplayerClient client { get; set; } = null!;

        [Resolved]
        private BeatmapManager beatmapManager { get; set; } = null!;

        [Resolved]
        private RulesetStore rulesets { get; set; } = null!;

        [Resolved]
        private BeatmapLookupCache beatmapLookupCache { get; set; } = null!;

        [Resolved]
        private BeatmapModelDownloader beatmapDownloader { get; set; } = null!;

        [Resolved]
        private IDialogOverlay dialogOverlay { get; set; } = null!;

        [Resolved]
        private AudioManager audio { get; set; } = null!;

        [Resolved]
        private OsuConfigManager config { get; set; } = null!;

        [Resolved]
        private PreviewTrackManager previewTrackManager { get; set; } = null!;

        [Resolved]
        private MusicController music { get; set; } = null!;

        private readonly MultiplayerRoom room;
        private readonly Dictionary<RankedPlayCardItem, RevealableCardItem> revealedCards = [];

        private readonly Container<Card> playedCardContainer;
        private readonly OsuSpriteText stageText;
        private readonly Hand localUserHand;

        private Sample? sampleStart;
        private CancellationTokenSource? downloadCheckCancellation;
        private int? lastDownloadCheckedBeatmapId;

        public RankedPlayScreen2(MultiplayerRoom room)
        {
            this.room = room;

            InternalChildren = new Drawable[]
            {
                beatmapAvailabilityTracker,
                ActionButton = new ShearedButton(width: 150)
                {
                    Anchor = Anchor.BottomRight,
                    Origin = Anchor.BottomRight,
                    Y = -100,
                    Alpha = 0,
                    Action = onActionButtonClicked,
                    Enabled = { Value = false }
                },
                stageText = new OsuSpriteText
                {
                    Anchor = Anchor.TopCentre,
                    Origin = Anchor.TopCentre,
                    Font = OsuFont.Style.Title,
                    Y = 50
                },
                playedCardContainer = new Container<Card>
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    RelativeSizeAxes = Axes.X,
                    AutoSizeAxes = Axes.Y
                },
                localUserHand = new Hand
                {
                    Anchor = Anchor.BottomCentre,
                    Origin = Anchor.BottomCentre,
                    RelativeSizeAxes = Axes.Both
                }
            };
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            sampleStart = audio.Samples.Get(@"SongSelect/confirm-selection");
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            client.RoomUpdated += onRoomUpdated;
            client.UserStateChanged += onUserStateChanged;
            client.SettingsChanged += onSettingsChanged;
            client.LoadRequested += onLoadRequested;
            client.MatchRoomStateChanged += onMatchRoomStateChanged;
            client.RankedPlayCardAdded += onRankedPlayCardAdded;
            client.RankedPlayCardRemoved += onRankedPlayCardRemoved;
            client.RankedPlayCardPlayed += onRankedPlayCardPlayed;
            client.RankedPlayCardRevealed += onRankedPlayCardRevealed;

            beatmapAvailabilityTracker.Availability.BindValueChanged(onBeatmapAvailabilityChanged, true);

            var localUserState = (RankedPlayUserState)client.LocalUser!.MatchState!;
            foreach (var card in localUserState.Hand)
                localUserHand.AddCard(getRevealedCard(card));
        }

        private void onRoomUpdated()
        {
            if (this.IsCurrentScreen() && client.Room == null)
            {
                Logger.Log($"{this} exiting due to loss of room or connection");
                exitConfirmed = true;
                this.Exit();
            }
        }

        private void onUserStateChanged(MultiplayerRoomUser user, MultiplayerUserState state)
        {
            if (user.Equals(client.LocalUser) && state == MultiplayerUserState.Idle)
                this.MakeCurrent();
        }

        private void onSettingsChanged(MultiplayerRoomSettings _) => Scheduler.Add(() =>
        {
            checkForAutomaticDownload();
            updateGameplayState();
        });

        private void onLoadRequested() => Scheduler.Add(() =>
        {
            updateGameplayState();

            if (Beatmap.IsDefault)
            {
                Logger.Log("Aborting gameplay start - beatmap not downloaded.");
                return;
            }

            sampleStart?.Play();

            this.Push(new MultiplayerPlayerLoader(() => new ScreenGameplay(new Room(room), new PlaylistItem(client.Room!.CurrentPlaylistItem), room.Users.ToArray())));
        });

        private void onMatchRoomStateChanged(MatchRoomState state)
        {
            if (state is not RankedPlayRoomState rankedPlayState)
                return;

            stageText.Text = string.Empty;
            ActionButton.Hide();
            localUserHand.AllowSelection.Value = false;

            switch (rankedPlayState.Stage)
            {
                case RankedPlayStage.CardDiscard:
                    stageText.Text = "discard beatmaps from your hand";

                    ActionButton.Show();
                    ActionButton.Text = "Discard";
                    ActionButton.Enabled.Value = true;

                    localUserHand.AllowSelection.Value = true;
                    localUserHand.SelectionLength = int.MaxValue;
                    break;

                case RankedPlayStage.CardPlay:
                    bool isActivePlayer = client.Room!.Users[rankedPlayState.ActivePlayerIndex].Equals(client.LocalUser);

                    if (isActivePlayer)
                    {
                        stageText.Text = "play a card from your hand!";

                        ActionButton.Show();
                        ActionButton.Text = "Play";
                        ActionButton.Enabled.Value = isActivePlayer;

                        localUserHand.AllowSelection.Value = true;
                        localUserHand.SelectionLength = 1;
                    }
                    else
                        stageText.Text = "waiting for the other player to play a card...";

                    break;
            }
        }

        private void onRankedPlayCardAdded(int userId, RankedPlayCardItem card)
        {
            if (userId == client.LocalUser!.UserID)
                localUserHand.AddCard(getRevealedCard(card));
        }

        private void onRankedPlayCardRemoved(int userId, RankedPlayCardItem card)
        {
            if (playedCardContainer.FirstOrDefault()?.Item.Item.Equals(card) == true)
                playedCardContainer.Clear();

            if (userId == client.LocalUser!.UserID)
                localUserHand.RemoveCard(getRevealedCard(card));
        }

        private void onRankedPlayCardPlayed(RankedPlayCardItem card)
        {
            localUserHand.RemoveCard(getRevealedCard(card));

            playedCardContainer.Child = new Card(getRevealedCard(card))
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre
            };
        }

        private void onRankedPlayCardRevealed(RankedPlayCardItem card, MultiplayerPlaylistItem item)
        {
            getRevealedCard(card).PlaylistItem.Value = item;
        }

        private void onBeatmapAvailabilityChanged(ValueChangedEvent<BeatmapAvailability> e) => Scheduler.Add(() =>
        {
            if (client.Room == null || client.LocalUser == null)
                return;

            client.ChangeBeatmapAvailability(e.NewValue).FireAndForget();

            switch (e.NewValue.State)
            {
                case DownloadState.NotDownloaded:
                case DownloadState.LocallyAvailable:
                    updateGameplayState();
                    break;
            }
        });

        private void onActionButtonClicked()
        {
            RankedPlayCardItem[] selection = localUserHand.CurrentSelection.ToArray();

            bool finished = false;

            switch (((RankedPlayRoomState)client.Room!.MatchState!).Stage)
            {
                case RankedPlayStage.CardDiscard:
                    client.DiscardCards(selection).FireAndForget();
                    finished = true;
                    break;

                case RankedPlayStage.CardPlay:
                    if (selection.Length > 0)
                    {
                        client.PlayCard(selection.First()).FireAndForget();
                        finished = true;
                    }

                    break;
            }

            if (finished)
            {
                ActionButton.Hide();
                ActionButton.Enabled.Value = false;

                localUserHand.AllowSelection.Value = false;
            }
        }

        private RevealableCardItem getRevealedCard(RankedPlayCardItem card)
        {
            if (revealedCards.TryGetValue(card, out var existing))
                return existing;

            return revealedCards[card] = new RevealableCardItem(card);
        }

        private void updateGameplayState()
        {
            MultiplayerPlaylistItem item = client.Room!.CurrentPlaylistItem;

            if (item.Expired)
                return;

            RulesetInfo ruleset = rulesets.GetRuleset(item.RulesetID)!;
            Ruleset rulesetInstance = ruleset.CreateInstance();

            // Update global gameplay state to correspond to the new selection.
            // Retrieve the corresponding local beatmap, since we can't directly use the playlist's beatmap info
            var localBeatmap = beatmapManager.QueryBeatmap($@"{nameof(BeatmapInfo.OnlineID)} == $0 AND {nameof(BeatmapInfo.MD5Hash)} == {nameof(BeatmapInfo.OnlineMD5Hash)}", item.BeatmapID);

            if (localBeatmap != null)
            {
                Beatmap.Value = beatmapManager.GetWorkingBeatmap(localBeatmap);
                Ruleset.Value = ruleset;
                Mods.Value = item.RequiredMods.Select(m => m.ToMod(rulesetInstance)).ToArray();

                // Notify the server that the beatmap has been set and that we are ready to start gameplay.
                if (client.LocalUser!.State == MultiplayerUserState.Idle)
                    client.ChangeState(MultiplayerUserState.Ready).FireAndForget();
            }
            else
            {
                // Notify the server that we don't have the beatmap.
                if (client.LocalUser!.State == MultiplayerUserState.Ready)
                    client.ChangeState(MultiplayerUserState.Idle).FireAndForget();
            }

            client.ChangeBeatmapAvailability(beatmapAvailabilityTracker.Availability.Value).FireAndForget();
        }

        private void checkForAutomaticDownload()
        {
            if (client.Room == null)
                return;

            MultiplayerPlaylistItem item = client.Room.CurrentPlaylistItem;

            // This method is called every time anything changes in the room.
            // This could result in download requests firing far too often, when we only expect them to fire once per beatmap.
            //
            // Without this check, we would see especially egregious behaviour when a user has hit the download rate limit.
            if (lastDownloadCheckedBeatmapId == item.BeatmapID)
                return;

            lastDownloadCheckedBeatmapId = item.BeatmapID;

            downloadCheckCancellation?.Cancel();

            if (beatmapManager.IsAvailableLocally(new APIBeatmap { OnlineID = item.BeatmapID }))
                return;

            // In a perfect world we'd use BeatmapAvailability, but there's no event-driven flow for when a selection changes.
            // ie. if selection changes from "not downloaded" to another "not downloaded" we wouldn't get a value changed raised.
            beatmapLookupCache
                .GetBeatmapAsync(item.BeatmapID, (downloadCheckCancellation = new CancellationTokenSource()).Token)
                .ContinueWith(resolved => Schedule(() =>
                {
                    APIBeatmapSet? beatmapSet = resolved.GetResultSafely()?.BeatmapSet;

                    if (beatmapSet == null)
                        return;

                    beatmapDownloader.Download(beatmapSet, config.Get<bool>(OsuSetting.PreferNoVideo));
                }));
        }

        public override void OnEntering(ScreenTransitionEvent e)
        {
            base.OnEntering(e);

            beginHandlingTrack();
        }

        public override void OnSuspending(ScreenTransitionEvent e)
        {
            endHandlingTrack();

            base.OnSuspending(e);
        }

        private bool exitConfirmed;

        public override bool OnExiting(ScreenExitEvent e)
        {
            if (exitConfirmed)
            {
                if (base.OnExiting(e))
                {
                    exitConfirmed = false;
                    return true;
                }

                endHandlingTrack();

                client.LeaveRoom().FireAndForget();
                return false;
            }

            if (dialogOverlay.CurrentDialog is ConfirmDialog confirmDialog)
                confirmDialog.PerformOkAction();
            else
            {
                dialogOverlay.Push(new ConfirmDialog("Are you sure you want to leave this multiplayer match?", () =>
                {
                    exitConfirmed = true;
                    if (this.IsCurrentScreen())
                        this.Exit();
                }));
            }

            return true;
        }

        public override void OnResuming(ScreenTransitionEvent e)
        {
            base.OnResuming(e);

            beginHandlingTrack();

            if (e.Last is not MultiplayerPlayerLoader playerLoader)
                return;

            if (!playerLoader.GameplayPassed)
            {
                client.AbortGameplay().FireAndForget();
                return;
            }

            client.ChangeState(MultiplayerUserState.Idle).FireAndForget();
        }

        /// <summary>
        /// Handles changes in the track to keep it looping while active.
        /// </summary>
        private void beginHandlingTrack()
        {
            Beatmap.BindValueChanged(applyLoopingToTrack, true);
        }

        /// <summary>
        /// Stops looping the current track and stops handling further changes to the track.
        /// </summary>
        private void endHandlingTrack()
        {
            Beatmap.ValueChanged -= applyLoopingToTrack;
            Beatmap.Value.Track.Looping = false;

            previewTrackManager.StopAnyPlaying(this);
        }

        /// <summary>
        /// Invoked on changes to the beatmap to loop the track. See: <see cref="beginHandlingTrack"/>.
        /// </summary>
        /// <param name="beatmap">The beatmap change event.</param>
        private void applyLoopingToTrack(ValueChangedEvent<WorkingBeatmap> beatmap)
        {
            if (!this.IsCurrentScreen())
                return;

            beatmap.NewValue.PrepareTrackForPreview(true);
            music.EnsurePlayingSomething();
        }

        public void PresentBeatmap(WorkingBeatmap beatmap, RulesetInfo ruleset)
        {
            // Do nothing to prevent the user from potentially being kicked out
            // of gameplay due to the screen performer's internal processes.
        }
    }
}
