// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Linq;
using Humanizer;
using osu.Framework.Allocation;
using osu.Framework.Audio;
using osu.Framework.Audio.Sample;
using osu.Framework.Bindables;
using osu.Framework.Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Cursor;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.UserInterface;
using osu.Framework.Threading;
using osu.Game.Graphics;
using osu.Game.Graphics.Backgrounds;
using osu.Game.Graphics.UserInterface;
using osu.Game.Graphics.UserInterfaceV2;
using osu.Game.Online.Multiplayer;
using osu.Game.Screens.OnlinePlay.Components;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Screens.OnlinePlay.Multiplayer.Match
{
    public class MultiplayerReadyButton : MultiplayerRoomComposite
    {
        public Action<TimeSpan?> OnReadyClick;

        [Resolved]
        private OsuColour colours { get; set; }

        [Resolved]
        private OngoingOperationTracker ongoingOperationTracker { get; set; }

        private IBindable<bool> operationInProgress;

        private Sample sampleReady;
        private Sample sampleReadyAll;
        private Sample sampleUnready;

        private readonly ButtonWithTrianglesExposed mainButton;
        private readonly CountdownButton countdownButton;

        private int countReady;

        private ScheduledDelegate readySampleDelegate;

        public MultiplayerReadyButton()
        {
            InternalChild = new GridContainer
            {
                RelativeSizeAxes = Axes.Both,
                ColumnDimensions = new[]
                {
                    new Dimension(),
                    new Dimension(GridSizeMode.AutoSize)
                },
                Content = new[]
                {
                    new Drawable[]
                    {
                        mainButton = new ButtonWithTrianglesExposed
                        {
                            RelativeSizeAxes = Axes.Both,
                            Size = Vector2.One,
                            Enabled = { Value = true },
                            Action = () => OnReadyClick?.Invoke(null)
                        },
                        countdownButton = new CountdownButton
                        {
                            RelativeSizeAxes = Axes.Y,
                            Size = new Vector2(40, 1),
                            Icon = FontAwesome.Solid.CaretDown,
                            IconScale = new Vector2(0.6f),
                            Alpha = 0,
                            Action = t => OnReadyClick?.Invoke(t)
                        }
                    }
                }
            };
        }

        [BackgroundDependencyLoader]
        private void load(AudioManager audio)
        {
            operationInProgress = ongoingOperationTracker.InProgress.GetBoundCopy();
            operationInProgress.BindValueChanged(_ => updateState());

            sampleReady = audio.Samples.Get(@"Multiplayer/player-ready");
            sampleReadyAll = audio.Samples.Get(@"Multiplayer/player-ready-all");
            sampleUnready = audio.Samples.Get(@"Multiplayer/player-unready");
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            CurrentPlaylistItem.BindValueChanged(_ => updateState());
        }

        protected override void OnRoomUpdated()
        {
            base.OnRoomUpdated();

            updateState();
        }

        private void updateState()
        {
            var localUser = Client.LocalUser;

            int newCountReady = Room?.Users.Count(u => u.State == MultiplayerUserState.Ready) ?? 0;
            int newCountTotal = Room?.Users.Count(u => u.State != MultiplayerUserState.Spectating) ?? 0;

            switch (localUser?.State)
            {
                default:
                    countdownButton.Alpha = 0;
                    mainButton.Text = "Ready";
                    updateButtonColour(true);
                    break;

                case MultiplayerUserState.Spectating:
                case MultiplayerUserState.Ready:

                    string countText = $"({newCountReady} / {newCountTotal} ready)";

                    if (Room?.Host?.Equals(localUser) == true)
                    {
                        countdownButton.Alpha = 1;
                        mainButton.Text = $"Start match {countText}";
                        updateButtonColour(true);
                    }
                    else
                    {
                        countdownButton.Alpha = 0;
                        mainButton.Text = $"Waiting for host... {countText}";
                        updateButtonColour(false);
                    }

                    break;
            }

            bool enableButton =
                Room?.State == MultiplayerRoomState.Open
                && CurrentPlaylistItem.Value?.ID == Room.Settings.PlaylistItemId
                && !Room.Playlist.Single(i => i.ID == Room.Settings.PlaylistItemId).Expired
                && !operationInProgress.Value;

            // When the local user is the host and spectating the match, the "start match" state should be enabled if any users are ready.
            if (localUser?.State == MultiplayerUserState.Spectating)
                enableButton &= Room?.Host?.Equals(localUser) == true && newCountReady > 0;

            mainButton.Enabled.Value = enableButton;
            countdownButton.Enabled.Value = enableButton;

            if (newCountReady == countReady)
                return;

            readySampleDelegate?.Cancel();
            readySampleDelegate = Schedule(() =>
            {
                if (newCountReady > countReady)
                {
                    if (newCountReady == newCountTotal)
                        sampleReadyAll?.Play();
                    else
                        sampleReady?.Play();
                }
                else if (newCountReady < countReady)
                {
                    sampleUnready?.Play();
                }

                countReady = newCountReady;
            });
        }

        private void updateButtonColour(bool green)
        {
            if (green)
            {
                countdownButton.BackgroundColour = colours.Green;
                mainButton.BackgroundColour = colours.Green;
                mainButton.Triangles.ColourDark = colours.Green;
                mainButton.Triangles.ColourLight = colours.GreenLight;
            }
            else
            {
                countdownButton.BackgroundColour = colours.YellowDark;
                mainButton.BackgroundColour = colours.YellowDark;
                mainButton.Triangles.ColourDark = colours.YellowDark;
                mainButton.Triangles.ColourLight = colours.Yellow;
            }
        }

        private class ButtonWithTrianglesExposed : ReadyButton
        {
            public new Triangles Triangles => base.Triangles;
        }

        public class CountdownButton : IconButton, IHasPopover
        {
            public new Action<TimeSpan> Action;

            public Color4 BackgroundColour
            {
                get => background.Colour;
                set => background.Colour = value;
            }

            private readonly Drawable background;

            public CountdownButton()
            {
                Add(background = new Box
                {
                    RelativeSizeAxes = Axes.Both,
                    Depth = float.MaxValue
                });

                base.Action = this.ShowPopover;
            }

            private static readonly TimeSpan[] available_delays =
            {
                TimeSpan.FromSeconds(10),
                TimeSpan.FromSeconds(30),
                TimeSpan.FromMinutes(1),
                TimeSpan.FromMinutes(2)
            };

            public Popover GetPopover()
            {
                var flow = new FillFlowContainer
                {
                    Width = 200,
                    AutoSizeAxes = Axes.Y,
                    Direction = FillDirection.Vertical,
                    Spacing = new Vector2(2),
                };

                foreach (var duration in available_delays)
                {
                    flow.Add(new PopoverButton
                    {
                        RelativeSizeAxes = Axes.X,
                        Text = $"Start match in {duration.Humanize()}",
                        BackgroundColour = BackgroundColour,
                        Action = () =>
                        {
                            Action(duration);
                            this.HidePopover();
                        }
                    });
                }

                return new OsuPopover { Child = flow };
            }

            public class PopoverButton : OsuButton
            {
            }
        }
    }
}
