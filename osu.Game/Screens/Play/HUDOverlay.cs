﻿// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Extensions.IEnumerableExtensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Input.Events;
using osu.Game.Configuration;
using osu.Game.Overlays;
using osu.Game.Overlays.Notifications;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Scoring;
using osu.Game.Rulesets.UI;
using osu.Game.Screens.Play.HUD;
using osu.Game.Skinning;
using osuTK;
using osuTK.Input;

namespace osu.Game.Screens.Play
{
    [Cached]
    public class HUDOverlay : Container
    {
        public const float FADE_DURATION = 400;

        public const Easing FADE_EASING = Easing.Out;

        public readonly KeyCounterDisplay KeyCounter;
        public readonly SkinnableComboCounter ComboCounter;
        public readonly SkinnableScoreCounter ScoreCounter;
        public readonly SkinnableAccuracyCounter AccuracyCounter;
        public readonly SkinnableHealthDisplay HealthDisplay;
        public readonly SongProgress Progress;
        public readonly ModDisplay ModDisplay;
        public readonly HitErrorDisplay HitErrorDisplay;
        public readonly HoldForMenuButton HoldToQuit;
        public readonly PlayerSettingsOverlay PlayerSettingsOverlay;
        public readonly FailingLayer FailingLayer;

        public Bindable<bool> ShowHealthbar = new Bindable<bool>(true);

        private readonly ScoreProcessor scoreProcessor;
        private readonly HealthProcessor healthProcessor;
        private readonly DrawableRuleset drawableRuleset;
        private readonly IReadOnlyList<Mod> mods;

        /// <summary>
        /// Whether the elements that can optionally be hidden should be visible.
        /// </summary>
        public Bindable<bool> ShowHud { get; } = new BindableBool();

        private Bindable<bool> configShowHud;

        private readonly Container visibilityContainer;

        private readonly BindableBool replayLoaded = new BindableBool();

        private static bool hasShownNotificationOnce;

        public Action<double> RequestSeek;

        private readonly FillFlowContainer bottomRightElements;
        private readonly FillFlowContainer topRightElements;

        private readonly Container mainUIElements;

        private IEnumerable<Drawable> hideTargets => new Drawable[] { visibilityContainer, KeyCounter };

        public HUDOverlay(ScoreProcessor scoreProcessor, HealthProcessor healthProcessor, DrawableRuleset drawableRuleset, IReadOnlyList<Mod> mods)
        {
            this.scoreProcessor = scoreProcessor;
            this.healthProcessor = healthProcessor;
            this.drawableRuleset = drawableRuleset;
            this.mods = mods;

            RelativeSizeAxes = Axes.Both;

            Children = new Drawable[]
            {
                FailingLayer = CreateFailingLayer(),
                visibilityContainer = new Container
                {
                    RelativeSizeAxes = Axes.Both,
                    Child = new GridContainer
                    {
                        RelativeSizeAxes = Axes.Both,
                        Content = new[]
                        {
                            new Drawable[]
                            {
                                mainUIElements = new Container
                                {
                                    RelativeSizeAxes = Axes.Both,
                                    Children = new Drawable[]
                                    {
                                        HealthDisplay = CreateHealthDisplay(),
                                        AccuracyCounter = CreateAccuracyCounter(),
                                        ScoreCounter = CreateScoreCounter(),
                                        ComboCounter = CreateComboCounter(),
                                        HitErrorDisplay = CreateHitErrorDisplayOverlay(),
                                    }
                                },
                            },
                            new Drawable[]
                            {
                                Progress = CreateProgress(),
                            }
                        },
                        RowDimensions = new[]
                        {
                            new Dimension(),
                            new Dimension(GridSizeMode.AutoSize)
                        }
                    },
                },
                topRightElements = new FillFlowContainer
                {
                    Anchor = Anchor.TopRight,
                    Origin = Anchor.TopRight,
                    Margin = new MarginPadding(10),
                    Spacing = new Vector2(10),
                    AutoSizeAxes = Axes.Both,
                    Direction = FillDirection.Vertical,
                    Children = new Drawable[]
                    {
                        ModDisplay = CreateModsContainer(),
                        PlayerSettingsOverlay = CreatePlayerSettingsOverlay(),
                    }
                },
                bottomRightElements = new FillFlowContainer
                {
                    Anchor = Anchor.BottomRight,
                    Origin = Anchor.BottomRight,
                    Margin = new MarginPadding(10),
                    Spacing = new Vector2(10),
                    AutoSizeAxes = Axes.Both,
                    LayoutDuration = FADE_DURATION / 2,
                    LayoutEasing = FADE_EASING,
                    Direction = FillDirection.Vertical,
                    Children = new Drawable[]
                    {
                        KeyCounter = CreateKeyCounter(),
                        HoldToQuit = CreateHoldForMenuButton(),
                    }
                }
            };
        }

        [BackgroundDependencyLoader(true)]
        private void load(OsuConfigManager config, NotificationOverlay notificationOverlay)
        {
            if (scoreProcessor != null)
                BindScoreProcessor(scoreProcessor);

            if (healthProcessor != null)
                BindHealthProcessor(healthProcessor);

            if (drawableRuleset != null)
            {
                BindDrawableRuleset(drawableRuleset);

                Progress.Objects = drawableRuleset.Objects;
                Progress.RequestSeek = time => RequestSeek(time);
                Progress.ReferenceClock = drawableRuleset.FrameStableClock;
            }

            ModDisplay.Current.Value = mods;

            configShowHud = config.GetBindable<bool>(OsuSetting.ShowInterface);

            if (!configShowHud.Value && !hasShownNotificationOnce)
            {
                hasShownNotificationOnce = true;

                notificationOverlay?.Post(new SimpleNotification
                {
                    Text = @"The score overlay is currently disabled. You can toggle this by pressing Shift+Tab."
                });
            }

            // start all elements hidden
            hideTargets.ForEach(d => d.Hide());
        }

        public override void Hide() => throw new InvalidOperationException($"{nameof(HUDOverlay)} should not be hidden as it will remove the ability of a user to quit. Use {nameof(ShowHud)} instead.");

        protected override void LoadComplete()
        {
            base.LoadComplete();

            ShowHealthbar.BindValueChanged(healthBar => HealthDisplay.FadeTo(healthBar.NewValue ? 1 : 0, FADE_DURATION, FADE_EASING), true);
            ShowHud.BindValueChanged(visible => hideTargets.ForEach(d => d.FadeTo(visible.NewValue ? 1 : 0, FADE_DURATION, FADE_EASING)));

            configShowHud.BindValueChanged(visible =>
            {
                if (!ShowHud.Disabled)
                    ShowHud.Value = visible.NewValue;
            }, true);

            replayLoaded.BindValueChanged(replayLoadedValueChanged, true);
        }

        protected override void Update()
        {
            base.Update();

            float topRightOffset = 0;

            // fetch the bottom-most position of any main ui element that is anchored to the top of the screen.
            // consider this kind of temporary.
            foreach (var d in mainUIElements)
            {
                if (d is SkinnableDrawable sd && (sd.Drawable.Anchor & Anchor.y0) > 0)
                    topRightOffset = Math.Max(sd.Drawable.ScreenSpaceDrawQuad.BottomRight.Y, topRightOffset);
            }

            topRightElements.Y = ToLocalSpace(new Vector2(0, topRightOffset)).Y;
            bottomRightElements.Y = -Progress.Height;
        }

        private void replayLoadedValueChanged(ValueChangedEvent<bool> e)
        {
            PlayerSettingsOverlay.ReplayLoaded = e.NewValue;

            if (e.NewValue)
            {
                PlayerSettingsOverlay.Show();
                ModDisplay.FadeIn(200);
                KeyCounter.Margin = new MarginPadding(10) { Bottom = 30 };
            }
            else
            {
                PlayerSettingsOverlay.Hide();
                ModDisplay.Delay(2000).FadeOut(200);
                KeyCounter.Margin = new MarginPadding(10);
            }
        }

        protected virtual void BindDrawableRuleset(DrawableRuleset drawableRuleset)
        {
            (drawableRuleset as ICanAttachKeyCounter)?.Attach(KeyCounter);

            replayLoaded.BindTo(drawableRuleset.HasReplayLoaded);

            Progress.BindDrawableRuleset(drawableRuleset);
        }

        protected override bool OnKeyDown(KeyDownEvent e)
        {
            if (e.Repeat) return false;

            if (e.ShiftPressed)
            {
                switch (e.Key)
                {
                    case Key.Tab:
                        configShowHud.Value = !configShowHud.Value;
                        return true;
                }
            }

            return base.OnKeyDown(e);
        }

        protected virtual SkinnableAccuracyCounter CreateAccuracyCounter() => new SkinnableAccuracyCounter();

        protected virtual SkinnableScoreCounter CreateScoreCounter() => new SkinnableScoreCounter();

        protected virtual SkinnableComboCounter CreateComboCounter() => new SkinnableComboCounter();

        protected virtual SkinnableHealthDisplay CreateHealthDisplay() => new SkinnableHealthDisplay();

        protected virtual FailingLayer CreateFailingLayer() => new FailingLayer
        {
            ShowHealth = { BindTarget = ShowHealthbar }
        };

        protected virtual KeyCounterDisplay CreateKeyCounter() => new KeyCounterDisplay
        {
            Anchor = Anchor.BottomRight,
            Origin = Anchor.BottomRight,
        };

        protected virtual SongProgress CreateProgress() => new SongProgress
        {
            Anchor = Anchor.BottomLeft,
            Origin = Anchor.BottomLeft,
            RelativeSizeAxes = Axes.X,
        };

        protected virtual HoldForMenuButton CreateHoldForMenuButton() => new HoldForMenuButton
        {
            Anchor = Anchor.BottomRight,
            Origin = Anchor.BottomRight,
        };

        protected virtual ModDisplay CreateModsContainer() => new ModDisplay
        {
            Anchor = Anchor.TopRight,
            Origin = Anchor.TopRight,
            AutoSizeAxes = Axes.Both,
        };

        protected virtual HitErrorDisplay CreateHitErrorDisplayOverlay() => new HitErrorDisplay(scoreProcessor, drawableRuleset?.FirstAvailableHitWindows);

        protected virtual PlayerSettingsOverlay CreatePlayerSettingsOverlay() => new PlayerSettingsOverlay();

        protected virtual void BindScoreProcessor(ScoreProcessor processor)
        {
            ScoreCounter?.Current.BindTo(processor.TotalScore);
            AccuracyCounter?.Current.BindTo(processor.Accuracy);
            ComboCounter?.Current.BindTo(processor.Combo);

            if (HealthDisplay is IHealthDisplay shd)
            {
                processor.NewJudgement += judgement =>
                {
                    if (judgement.IsHit && judgement.Type != HitResult.IgnoreHit)
                        shd.Flash(judgement);
                };
            }
        }

        protected virtual void BindHealthProcessor(HealthProcessor processor)
        {
            HealthDisplay?.BindHealthProcessor(processor);
            FailingLayer?.BindHealthProcessor(processor);
        }
    }
}
