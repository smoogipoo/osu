// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Audio;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Testing;
using osu.Framework.Utils;
using osu.Game.Database;
using osu.Game.Online.Rooms;
using osu.Game.Online.Spectator;
using osu.Game.Screens.Play;
using osu.Game.Screens.Play.HUD;
using osu.Game.Screens.Spectate;
using osuTK;

namespace osu.Game.Screens.OnlinePlay.Multiplayer.Spectate
{
    public class MultiplayerSpectator : SpectatorScreen
    {
        private const float player_spacing = 5;
        private const int max_instances = 16;

        private const double min_duration_to_allow_playback = 50;

        private const double max_sync_offset = 2;

        // Isolates beatmap/ruleset to this screen.
        public override bool DisallowExternalBeatmapRulesetChanges => true;

        public bool AllPlayersLoaded => instances.All(p => p?.PlayerLoaded == true);

        private readonly PlayerInstance[] instances;

        // ReSharper disable once NotAccessedField.Local
        private readonly PlaylistItem playlistItem;

        private Container<PlayerInstance> instanceContainer;
        private Container paddingContainer;
        private FillFlowContainer<PlayerFacade> facades;
        private PlayerFacade maximisedFacade;

        // A depth value that gets decremented every time a new instance is maximised in order to reduce underlaps.
        private float maximisedInstanceDepth = 1;

        public MultiplayerSpectator(PlaylistItem playlistItem, int[] userIds)
            : base(userIds.AsSpan().Slice(0, Math.Min(max_instances, userIds.Length)).ToArray())
        {
            this.playlistItem = playlistItem;

            instances = new PlayerInstance[UserIds.Length];
        }

        [BackgroundDependencyLoader]
        private void load(UserLookupCache userLookupCache)
        {
            Container leaderboardContainer;

            InternalChildren = new Drawable[]
            {
                new GridContainer
                {
                    RelativeSizeAxes = Axes.Both,
                    ColumnDimensions = new[]
                    {
                        new Dimension(GridSizeMode.AutoSize)
                    },
                    Content = new[]
                    {
                        new Drawable[]
                        {
                            leaderboardContainer = new Container
                            {
                                RelativeSizeAxes = Axes.Y,
                                AutoSizeAxes = Axes.X,
                            },
                            paddingContainer = new Container
                            {
                                RelativeSizeAxes = Axes.Both,
                                Padding = new MarginPadding(player_spacing),
                                Children = new Drawable[]
                                {
                                    new Container
                                    {
                                        RelativeSizeAxes = Axes.Both,
                                        Child = facades = new FillFlowContainer<PlayerFacade>
                                        {
                                            RelativeSizeAxes = Axes.X,
                                            AutoSizeAxes = Axes.Y,
                                            Spacing = new Vector2(player_spacing),
                                        }
                                    },
                                    maximisedFacade = new PlayerFacade { RelativeSizeAxes = Axes.Both }
                                }
                            },
                        }
                    }
                },
                instanceContainer = new Container<PlayerInstance> { RelativeSizeAxes = Axes.Both }
            };

            for (int i = 0; i < UserIds.Length; i++)
            {
                var facade = new PlayerFacade();

                facades.Add(facade);
                facades.SetLayoutPosition(facade, i);
            }

            LoadComponentAsync(new MultiplayerGameplayLeaderboard(userId => instances[getIndexForUser(userId)].ScoreProcessor, UserIds)
            {
                Expanded = { Value = true }
            }, leaderboardContainer.Add);
        }

        protected override void Update()
        {
            base.Update();

            Vector2 cellsPerDimension;

            switch (facades.Count)
            {
                case 1:
                    cellsPerDimension = Vector2.One;
                    break;

                case 2:
                    cellsPerDimension = new Vector2(2, 1);
                    break;

                case 3:
                case 4:
                    cellsPerDimension = new Vector2(2);
                    break;

                case 5:
                case 6:
                    cellsPerDimension = new Vector2(3, 2);
                    break;

                case 7:
                case 8:
                case 9:
                    // 3 rows / 3 cols.
                    cellsPerDimension = new Vector2(3);
                    break;

                case 10:
                case 11:
                case 12:
                    // 3 rows / 4 cols.
                    cellsPerDimension = new Vector2(4, 3);
                    break;

                default:
                    // 4 rows / 4 cols.
                    cellsPerDimension = new Vector2(4);
                    break;
            }

            // Total spacing between cells
            Vector2 totalCellSpacing = player_spacing * (cellsPerDimension - Vector2.One);

            Vector2 fullSize = paddingContainer.ChildSize - totalCellSpacing;
            Vector2 cellSize = Vector2.Divide(fullSize, new Vector2(cellsPerDimension.X, cellsPerDimension.Y));

            foreach (var facade in facades)
            {
                facade.FullSize = fullSize;
                facade.Size = cellSize;
            }

            updatePlayTime();
        }

        private bool gameplayStarted;

        private void updatePlayTime()
        {
            if (gameplayStarted)
            {
                ensurePlaying(instances.Select(i => i.Beatmap.Track.CurrentTime).Max());
                return;
            }

            // Make sure all players are loaded.
            if (!AllPlayersLoaded)
            {
                ensureAllStopped();
                return;
            }

            if (!instances.All(i => i.Score.Replay.Frames.Count > 0))
            {
                ensureAllStopped();
                return;
            }

            gameplayStarted = true;
        }

        private void ensureAllStopped()
        {
            foreach (var inst in instances)
                inst.ChildrenOfType<GameplayClockContainer>().SingleOrDefault()?.Stop();
        }

        private readonly BindableDouble catchupFrequencyAdjustment = new BindableDouble(2.0);

        private void ensurePlaying(double targetTime)
        {
            foreach (var inst in instances)
            {
                double lastFrameTime = inst.Score.Replay.Frames.Select(f => f.Time).Last();
                double currentTime = inst.Beatmap.Track.CurrentTime;

                // If we have enough frames to play back, start playback.
                if (Precision.DefinitelyBigger(lastFrameTime, currentTime, min_duration_to_allow_playback))
                {
                    inst.ChildrenOfType<GameplayClockContainer>().Single().Start();

                    if (targetTime < lastFrameTime && targetTime > currentTime + 16)
                        inst.Beatmap.Track.AddAdjustment(AdjustableProperty.Frequency, catchupFrequencyAdjustment);
                    else
                        inst.Beatmap.Track.RemoveAdjustment(AdjustableProperty.Frequency, catchupFrequencyAdjustment);
                }
                else
                    inst.Beatmap.Track.RemoveAdjustment(AdjustableProperty.Frequency, catchupFrequencyAdjustment);
            }
        }

        private void toggleMaximisationState(PlayerInstance target)
        {
            // Iterate through all instances to ensure only one is maximised at any time.
            foreach (var i in instances)
            {
                if (i == null)
                    continue;

                if (i == target)
                    i.IsMaximised = !i.IsMaximised;
                else
                    i.IsMaximised = false;

                if (i.IsMaximised)
                {
                    i.SetFacade(maximisedFacade);
                    instanceContainer.ChangeChildDepth(i, maximisedInstanceDepth -= 0.001f);
                }
                else
                    i.SetFacade(facades[getIndexForUser(i.Score.ScoreInfo.User.Id)]);
            }
        }

        protected override void OnUserStateChanged(int userId, SpectatorState spectatorState)
        {
        }

        protected override void OnGameplayStateChanged(int userId, GameplayState gameplayState)
        {
            if (gameplayState == null)
            {
                SpectatorClient.StopWatchingUser(userId);
                return;
            }

            Schedule(() =>
            {
                int userIndex = getIndexForUser(userId);

                var existingInstance = instances[userIndex];

                if (existingInstance != null)
                {
                    if (existingInstance.IsMaximised)
                        toggleMaximisationState(existingInstance);

                    instanceContainer.Remove(existingInstance);
                    instances[userIndex] = null;
                }

                instances[userIndex] = new PlayerInstance(gameplayState.Score, facades[userIndex])
                {
                    Depth = 1,
                    ToggleMaximisationState = toggleMaximisationState
                };

                LoadComponentAsync(instances[userIndex], d =>
                {
                    if (instances[userIndex] == d)
                        instanceContainer.Add(d);
                });
            });
        }

        private int getIndexForUser(int userId) => Array.IndexOf(UserIds, userId);
    }
}
