// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using JetBrains.Annotations;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Input.Events;
using osu.Game.Scoring;
using osu.Game.Screens.Play;
using osu.Game.Users;
using osuTK;

namespace osu.Game.Screens.OnlinePlay.Multiplayer.Spectate
{
    public class PlayerInstance : CompositeDrawable
    {
        public Action<PlayerInstance> ToggleMaximisationState;

        public bool PlayerLoaded => stack.CurrentScreen is Player;
        public bool IsMaximised;

        public User User => Score.ScoreInfo.User;

        public readonly Score Score;
        private readonly OsuScreenStack stack;

        private PlayerFacade facade;
        private bool isTracking = true;

        public PlayerInstance(Score score, PlayerFacade facade)
        {
            Score = score;
            this.facade = facade;

            Origin = Anchor.Centre;
            Masking = true;

            InternalChild = new Container
            {
                RelativeSizeAxes = Axes.Both,
                Child = new DrawSizePreservingFillContainer
                {
                    RelativeSizeAxes = Axes.Both,
                    Child = stack = new OsuScreenStack()
                }
            };

            stack.Push(new SpectatorPlayerLoader(score));
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
}
