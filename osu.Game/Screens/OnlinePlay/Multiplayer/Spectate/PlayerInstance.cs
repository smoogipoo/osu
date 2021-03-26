// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using JetBrains.Annotations;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Input.Events;
using osu.Game.Beatmaps;
using osu.Game.Rulesets;
using osu.Game.Rulesets.Scoring;
using osu.Game.Scoring;
using osu.Game.Screens.Play;
using osu.Game.Users;
using osuTK;

namespace osu.Game.Screens.OnlinePlay.Multiplayer.Spectate
{
    public class PlayerInstance : CompositeDrawable
    {
        public Action<PlayerInstance> ToggleMaximisationState;

        public bool PlayerLoaded => stack?.CurrentScreen is Player;
        public bool IsMaximised;

        public User User => Score.ScoreInfo.User;

        public WorkingBeatmap Beatmap { get; private set; }
        public Ruleset Ruleset { get; private set; }

        public ScoreProcessor ScoreProcessor => player?.ScoreProcessor;

        public readonly Score Score;

        private OsuScreenStack stack;
        private PlayerFacade facade;
        private MultiplayerSpectatorPlayer player;
        private bool isTracking = true;

        public PlayerInstance(Score score, PlayerFacade facade)
        {
            Score = score;
            this.facade = facade;

            Origin = Anchor.Centre;
            Masking = true;
        }

        [BackgroundDependencyLoader]
        private void load(BeatmapManager beatmapManager)
        {
            Beatmap = beatmapManager.GetWorkingBeatmap(Score.ScoreInfo.Beatmap, bypassCache: true);
            Ruleset = Score.ScoreInfo.Ruleset.CreateInstance();

            InternalChild = new GameplayIsolationContainer(Beatmap, Score.ScoreInfo.Ruleset, Score.ScoreInfo.Mods)
            {
                RelativeSizeAxes = Axes.Both,
                Child = new DrawSizePreservingFillContainer
                {
                    RelativeSizeAxes = Axes.Both,
                    Child = stack = new OsuScreenStack()
                }
            };

            stack.Push(new SpectatorPlayerLoader(Score, () => player = new MultiplayerSpectatorPlayer(Score)));
        }

        protected override void Update()
        {
            base.Update();

            if (isTracking)
            {
                Position = getFinalPosition();
                Size = getFinalSize();
            }
        }

        public void SetFacade([NotNull] PlayerFacade newFacade)
        {
            PlayerFacade lastFacade = facade;
            facade = newFacade;

            if (lastFacade == null || lastFacade == newFacade)
                return;

            isTracking = false;

            this.MoveTo(getFinalPosition(), 400, Easing.OutQuint).ResizeTo(getFinalSize(), 400, Easing.OutQuint)
                .Then()
                .OnComplete(_ =>
                {
                    if (facade == newFacade)
                        isTracking = true;
                });
        }

        private Vector2 getFinalPosition()
        {
            var topLeft = Parent.ToLocalSpace(facade.ToScreenSpace(Vector2.Zero));
            return topLeft + facade.DrawSize / 2;
        }

        private Vector2 getFinalSize() => facade.DrawSize;

        // Todo: Temporary?
        protected override bool ShouldBeConsideredForInput(Drawable child) => false;

        protected override bool OnClick(ClickEvent e)
        {
            ToggleMaximisationState(this);
            return true;
        }
    }

    public class MultiplayerSpectatorPlayer : SpectatorPlayer
    {
        public new ScoreProcessor ScoreProcessor => base.ScoreProcessor;

        public MultiplayerSpectatorPlayer(Score score)
            : base(score)
        {
        }
    }
}
