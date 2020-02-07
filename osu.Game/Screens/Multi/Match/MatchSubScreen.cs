﻿// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
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

        [Resolved(typeof(Room), nameof(Room.Type))]
        private Bindable<GameType> type { get; set; }

        [Resolved(typeof(Room), nameof(Room.Playlist))]
        private BindableList<PlaylistItem> playlist { get; set; }

        [Resolved]
        private BeatmapManager beatmapManager { get; set; }

        [Resolved(canBeNull: true)]
        private Multiplayer multiplayer { get; set; }

        private readonly Bindable<PlaylistItem> selectedItem = new Bindable<PlaylistItem>();
        private LeaderboardChatDisplay leaderboardChatDisplay;
        private Footer footer;
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
                                                                Child = new OverlinedPlaylist(true) // Temporarily always allow selection
                                                                {
                                                                    SelectedItem = { BindTarget = selectedItem }
                                                                }
                                                            },
                                                            new Container
                                                            {
                                                                RelativeSizeAxes = Axes.Both,
                                                                Padding = new MarginPadding { Left = 5 },
                                                                Child = leaderboardChatDisplay = new LeaderboardChatDisplay()
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
                            footer = new Footer { OnStart = onStart }
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
                    EditPlaylist = () => this.Push(new MatchSongSelect()),
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

            selectedItem.BindValueChanged(selectedItemChanged);
            selectedItem.Value = playlist.FirstOrDefault();

            beatmapManager.ItemAdded += beatmapAdded;
        }

        public override bool OnExiting(IScreen next)
        {
            RoomManager?.PartRoom();
            Mods.Value = Array.Empty<Mod>();

            return base.OnExiting(next);
        }

        private void selectedItemChanged(ValueChangedEvent<PlaylistItem> e)
        {
            updateWorkingBeatmap();

            Mods.Value = e.NewValue?.RequiredMods?.ToArray() ?? Array.Empty<Mod>();

            if (e.NewValue?.Ruleset != null)
                Ruleset.Value = e.NewValue.Ruleset.Value;
        }

        private void updateWorkingBeatmap()
        {
            var beatmap = selectedItem.Value?.Beatmap.Value;

            // Retrieve the corresponding local beatmap, since we can't directly use the playlist's beatmap info
            var localBeatmap = beatmap == null ? null : beatmapManager.QueryBeatmap(b => b.OnlineBeatmapID == beatmap.OnlineBeatmapID);

            Beatmap.Value = beatmapManager.GetWorkingBeatmap(localBeatmap);

            footer.AllowStart.Value = Beatmap.Value != beatmapManager.DefaultBeatmap;
        }

        private void beatmapAdded(BeatmapSetInfo model) => Schedule(() =>
        {
            if (Beatmap.Value != beatmapManager.DefaultBeatmap)
                return;

            updateWorkingBeatmap();
        });

        private void onStart()
        {
            switch (type.Value)
            {
                default:
                case GameTypeTimeshift _:
                    multiplayer?.Start(() => new TimeshiftPlayer(selectedItem.Value)
                    {
                        Exited = () => leaderboardChatDisplay.RefreshScores()
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
