// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Extensions.ObjectExtensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Screens;
using osu.Game.Database;
using osu.Game.Online.API;
using osu.Game.Online.Spectator;
using osu.Game.Screens.OnlinePlay.Match.Components;
using osu.Game.Screens.Play;
using osu.Game.Users;
using osuTK;

namespace osu.Game.Overlays.Dashboard
{
    internal class CurrentlyPlayingDisplay : CompositeDrawable
    {
        private readonly IBindableDictionary<int, SpectatorState> playingUsers = new BindableDictionary<int, SpectatorState>();

        private FillFlowContainer<PlayingUserPanel> userFlow;

        [Resolved]
        private SpectatorStreamingClient spectatorStreaming { get; set; }

        [BackgroundDependencyLoader]
        private void load()
        {
            RelativeSizeAxes = Axes.X;
            AutoSizeAxes = Axes.Y;

            InternalChild = userFlow = new FillFlowContainer<PlayingUserPanel>
            {
                RelativeSizeAxes = Axes.X,
                AutoSizeAxes = Axes.Y,
                Padding = new MarginPadding(10),
                Spacing = new Vector2(10),
            };
        }

        [Resolved]
        private IAPIProvider api { get; set; }

        [Resolved]
        private UserLookupCache users { get; set; }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            playingUsers.BindTo(spectatorStreaming.PlayingUsers);
            playingUsers.BindCollectionChanged(onUsersChanged, true);
        }

        private void onUsersChanged(object sender, NotifyDictionaryChangedEventArgs<int, SpectatorState> e) => Schedule(() =>
        {
            switch (e.Action)
            {
                case NotifyDictionaryChangedAction.Add:
                    foreach (var (userId, _) in e.NewItems.AsNonNull())
                    {
                        users.GetUserAsync(userId).ContinueWith(u =>
                        {
                            if (u.Result == null) return;

                            Schedule(() =>
                            {
                                // user may no longer be playing.
                                if (!playingUsers.ContainsKey(u.Result.Id))
                                    return;

                                userFlow.Add(createUserPanel(u.Result));
                            });
                        });
                    }

                    break;

                case NotifyDictionaryChangedAction.Remove:
                    foreach (var (userId, _) in e.OldItems.AsNonNull())
                        userFlow.FirstOrDefault(card => card.User.Id == userId)?.Expire();
                    break;
            }
        });

        private PlayingUserPanel createUserPanel(User user) =>
            new PlayingUserPanel(user).With(panel =>
            {
                panel.Anchor = Anchor.TopCentre;
                panel.Origin = Anchor.TopCentre;
            });

        private class PlayingUserPanel : CompositeDrawable
        {
            public readonly User User;

            [Resolved(canBeNull: true)]
            private OsuGame game { get; set; }

            public PlayingUserPanel(User user)
            {
                User = user;

                AutoSizeAxes = Axes.Both;
            }

            [BackgroundDependencyLoader]
            private void load(IAPIProvider api)
            {
                InternalChildren = new Drawable[]
                {
                    new FillFlowContainer
                    {
                        AutoSizeAxes = Axes.Y,
                        Direction = FillDirection.Vertical,
                        Spacing = new Vector2(2),
                        Width = 290,
                        Children = new Drawable[]
                        {
                            new UserGridPanel(User)
                            {
                                RelativeSizeAxes = Axes.X,
                                Anchor = Anchor.TopCentre,
                                Origin = Anchor.TopCentre,
                            },
                            new PurpleTriangleButton
                            {
                                RelativeSizeAxes = Axes.X,
                                Text = "Watch",
                                Anchor = Anchor.TopCentre,
                                Origin = Anchor.TopCentre,
                                Action = () => game?.PerformFromScreen(s => s.Push(new SoloSpectator(User))),
                                Enabled = { Value = User.Id != api.LocalUser.Value.Id }
                            }
                        }
                    },
                };
            }
        }
    }
}
