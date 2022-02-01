// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Diagnostics;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Screens;
using osu.Game.Database;
using osu.Game.Online.API;
using osu.Game.Online.API.Requests.Responses;
using osu.Game.Online.Spectator;
using osu.Game.Screens.OnlinePlay.Match.Components;
using osu.Game.Screens.Play;
using osu.Game.Users;
using osuTK;

namespace osu.Game.Overlays.Dashboard
{
    internal class CurrentlyPlayingDisplay : CompositeDrawable
    {
        private readonly IBindableDictionary<int, SpectatorState> userStates = new BindableDictionary<int, SpectatorState>();

        private FillFlowContainer<PlayingUserPanel> userFlow;

        [Resolved]
        private SpectatorClient spectatorClient { get; set; }

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
        private UserLookupCache users { get; set; }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            userStates.BindTo(spectatorClient.UserStates);
            userStates.BindCollectionChanged(onUserStatesChanged, true);
        }

        private void onUserStatesChanged(object sender, NotifyDictionaryChangedEventArgs<int, SpectatorState> e) => Schedule(() =>
        {
            switch (e.Action)
            {
                case NotifyDictionaryChangedAction.Add:
                case NotifyDictionaryChangedAction.Replace:
                    Debug.Assert(e.NewItems != null);

                    foreach ((int userId, SpectatorState state) in e.NewItems)
                    {
                        if (state.UserState != UserState.Playing)
                        {
                            removePlayingUser(userId);
                            continue;
                        }

                        users.GetUserAsync(userId).ContinueWith(task =>
                        {
                            var user = task.GetResultSafely();

                            if (user != null)
                                Schedule(() => addPlayingUser(user));
                        });
                    }

                    break;

                case NotifyDictionaryChangedAction.Remove:
                    Debug.Assert(e.OldItems != null);

                    foreach ((int userId, _) in e.OldItems)
                        removePlayingUser(userId);
                    break;
            }

            void addPlayingUser(APIUser user)
            {
                // user may no longer be playing.
                if (!userStates.TryGetValue(user.Id, out var state2) || state2.UserState != UserState.Playing)
                    return;

                userFlow.Add(createUserPanel(user));
            }

            void removePlayingUser(int userId) => userFlow.FirstOrDefault(card => card.User.Id == userId)?.Expire();
        });

        private PlayingUserPanel createUserPanel(APIUser user) =>
            new PlayingUserPanel(user).With(panel =>
            {
                panel.Anchor = Anchor.TopCentre;
                panel.Origin = Anchor.TopCentre;
            });

        private class PlayingUserPanel : CompositeDrawable
        {
            public readonly APIUser User;

            [Resolved(canBeNull: true)]
            private OsuGame game { get; set; }

            public PlayingUserPanel(APIUser user)
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
