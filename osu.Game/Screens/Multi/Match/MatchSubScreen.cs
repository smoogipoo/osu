// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Colour;
using osu.Framework.Graphics.Containers;
using osu.Framework.Screens;
using osu.Game.Audio;
using osu.Game.Beatmaps;
using osu.Game.Beatmaps.Drawables;
using osu.Game.Online.Multiplayer;
using osu.Game.Online.Multiplayer.GameTypes;
using osu.Game.Rulesets.Mods;
using osu.Game.Screens.Multi.Components;
using osu.Game.Screens.Multi.Match.Components;
using osu.Game.Screens.Multi.Play;
using osu.Game.Screens.Select;
using osuTK.Graphics;
using Footer = osu.Game.Screens.Multi.Match.Components.Footer;
using PlaylistItem = osu.Game.Online.Multiplayer.PlaylistItem;

namespace osu.Game.Screens.Multi.Match
{
    [Cached(typeof(IPreviewTrackOwner))]
    public class MatchSubScreen : MultiplayerSubScreen, IPreviewTrackOwner
    {
        public override bool DisallowExternalBeatmapRulesetChanges => true;

        public override string Title { get; }

        public override string ShortTitle => "room";

        [Resolved(typeof(Room), nameof(Room.RoomID))]
        private Bindable<int?> roomId { get; set; }

        [Resolved(typeof(Room), nameof(Room.Name))]
        private Bindable<string> name { get; set; }

        [Resolved(typeof(Room), nameof(Room.Type))]
        private Bindable<GameType> type { get; set; }

        [Resolved(typeof(Room))]
        protected BindableList<PlaylistItem> Playlist { get; private set; }

        [Resolved(typeof(Room))]
        protected Bindable<PlaylistItem> CurrentItem { get; private set; }

        [Resolved]
        private BeatmapManager beatmapManager { get; set; }

        [Resolved]
        private PreviewTrackManager previewTrackManager { get; set; }

        [Resolved(CanBeNull = true)]
        private OsuGame game { get; set; }

        private MatchSettingsOverlay settingsOverlay;

        public MatchSubScreen(Room room)
        {
            Title = room.RoomID.Value == null ? "New room" : room.Name.Value;
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            InternalChildren = new Drawable[]
            {
                new HeaderBackgroundSprite
                {
                    RelativeSizeAxes = Axes.X,
                    Height = 200,
                    Colour = ColourInfo.GradientVertical(Color4.White.Opacity(0.4f), Color4.White.Opacity(0))
                },
                new GridContainer
                {
                    RelativeSizeAxes = Axes.Both,
                    Content = new[]
                    {
                        new Drawable[]
                        {
                            new Container
                            {
                                RelativeSizeAxes = Axes.Both,
                                Padding = new MarginPadding
                                {
                                    Horizontal = 105,
                                    Vertical = 20
                                },
                                Child = new GridContainer
                                {
                                    RelativeSizeAxes = Axes.Both,
                                    Content = new[]
                                    {
                                        new Drawable[] { new Components.Header() },
                                        new Drawable[]
                                        {
                                            new Container
                                            {
                                                RelativeSizeAxes = Axes.Both,
                                                Padding = new MarginPadding { Top = 65 },
                                                Child = new GridContainer
                                                {
                                                    RelativeSizeAxes = Axes.Both,
                                                    Content = new[]
                                                    {
                                                        new Drawable[]
                                                        {
                                                            new Container
                                                            {
                                                                RelativeSizeAxes = Axes.Both,
                                                                Padding = new MarginPadding { Right = 5 },
                                                                Child = new OverlinedParticipants()
                                                            },
                                                            new Container
                                                            {
                                                                RelativeSizeAxes = Axes.Both,
                                                                Padding = new MarginPadding { Horizontal = 5 },
                                                                Child = new OverlinedPlaylist()
                                                            },
                                                            new Container
                                                            {
                                                                RelativeSizeAxes = Axes.Both,
                                                                Padding = new MarginPadding { Left = 5 },
                                                                Child = new LeaderboardChatDisplay()
                                                            }
                                                        },
                                                    }
                                                }
                                            }
                                        }
                                    },
                                    RowDimensions = new[]
                                    {
                                        new Dimension(GridSizeMode.AutoSize),
                                        new Dimension(),
                                    }
                                }
                            }
                        },
                        new Drawable[]
                        {
                            new Footer()
                        }
                    },
                    RowDimensions = new[]
                    {
                        new Dimension(),
                        new Dimension(GridSizeMode.AutoSize),
                    }
                },
                settingsOverlay = new MatchSettingsOverlay
                {
                    RelativeSizeAxes = Axes.Both,
                    EditPlaylist = onEditPlaylist,
                    State = { Value = roomId.Value == null ? Visibility.Visible : Visibility.Hidden }
                }
            };
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            roomId.BindValueChanged(id =>
            {
                if (id.NewValue == null)
                    settingsOverlay.Show();
                else
                    settingsOverlay.Hide();
            }, true);

            CurrentItem.BindValueChanged(currentItemChanged, true);
        }

        private void onEditPlaylist() => this.Push(new MatchSongSelect());

        public override bool OnExiting(IScreen next)
        {
            RoomManager?.PartRoom();
            Mods.Value = Array.Empty<Mod>();
            previewTrackManager.StopAnyPlaying(this);

            return base.OnExiting(next);
        }

        /// <summary>
        /// Handles propagation of the current playlist item's content to game-wide mechanisms.
        /// </summary>
        private void currentItemChanged(ValueChangedEvent<PlaylistItem> e)
        {
            // Retrieve the corresponding local beatmap, since we can't directly use the playlist's beatmap info
            var localBeatmap = e.NewValue?.Beatmap == null ? null : beatmapManager.QueryBeatmap(b => b.OnlineBeatmapID == e.NewValue.Beatmap.Value.OnlineBeatmapID);

            Beatmap.Value = beatmapManager.GetWorkingBeatmap(localBeatmap);
            Mods.Value = e.NewValue?.RequiredMods?.ToArray() ?? Array.Empty<Mod>();

            if (e.NewValue?.Ruleset != null)
                Ruleset.Value = e.NewValue.Ruleset.Value;

            previewTrackManager.StopAnyPlaying(this);
        }

        /// <summary>
        /// Handle the case where a beatmap is imported (and can be used by this match).
        /// </summary>
        private void beatmapAdded(BeatmapSetInfo model) => Schedule(() =>
        {
            if (Beatmap.Value != beatmapManager.DefaultBeatmap)
                return;

            if (CurrentItem.Value == null)
                return;

            // Try to retrieve the corresponding local beatmap
            var localBeatmap = beatmapManager.QueryBeatmap(b => b.OnlineBeatmapID == CurrentItem.Value.Beatmap.Value.OnlineBeatmapID);

            if (localBeatmap != null)
                Beatmap.Value = beatmapManager.GetWorkingBeatmap(localBeatmap);
        });

        [Resolved(canBeNull: true)]
        private Multiplayer multiplayer { get; set; }

        private void onStart()
        {
            previewTrackManager.StopAnyPlaying(this);

            switch (type.Value)
            {
                default:
                case GameTypeTimeshift _:
                    multiplayer?.Start(() => new TimeshiftPlayer(CurrentItem.Value)
                    {
                    });
                    break;
            }
        }

        protected override void Dispose(bool isDisposing)
        {
            base.Dispose(isDisposing);

            if (beatmapManager != null)
                beatmapManager.ItemAdded -= beatmapAdded;
        }

        private class HeaderBackgroundSprite : MultiplayerBackgroundSprite
        {
            protected override UpdateableBeatmapBackgroundSprite CreateBackgroundSprite() => new BackgroundSprite { RelativeSizeAxes = Axes.Both };

            private class BackgroundSprite : UpdateableBeatmapBackgroundSprite
            {
                protected override double TransformDuration => 200;
            }
        }
    }
}
