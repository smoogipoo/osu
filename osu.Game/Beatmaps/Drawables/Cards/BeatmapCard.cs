// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Input.Events;
using osu.Framework.Localisation;
using osu.Game.Beatmaps.Drawables.Cards.Buttons;
using osu.Game.Beatmaps.Drawables.Cards.Statistics;
using osu.Game.Graphics;
using osu.Game.Graphics.Containers;
using osu.Game.Graphics.Sprites;
using osu.Game.Graphics.UserInterface;
using osu.Game.Online;
using osu.Game.Online.API.Requests.Responses;
using osu.Game.Overlays;
using osu.Game.Overlays.BeatmapSet;
using osuTK;
using osu.Game.Overlays.BeatmapListing.Panels;
using osu.Game.Resources.Localisation.Web;
using osuTK.Graphics;
using DownloadButton = osu.Game.Beatmaps.Drawables.Cards.Buttons.DownloadButton;

namespace osu.Game.Beatmaps.Drawables.Cards
{
    public class BeatmapCard : OsuClickableContainer
    {
        public const float TRANSITION_DURATION = 400;

        private const float width = 408;
        private const float height = 100;
        private const float corner_radius = 10;
        private const float icon_area_width = 30;

        private readonly APIBeatmapSet beatmapSet;
        private readonly Bindable<BeatmapSetFavouriteState> favouriteState;

        private readonly BeatmapDownloadTracker downloadTracker;

        private UpdateableOnlineBeatmapSetCover leftCover;
        private FillFlowContainer leftIconArea;

        private Container rightAreaBackground;
        private Container<BeatmapCardIconButton> rightAreaButtons;

        private Container mainContent;
        private BeatmapCardContentBackground mainContentBackground;

        private GridContainer titleContainer;
        private GridContainer artistContainer;
        private FillFlowContainer<BeatmapCardStatistic> statisticsContainer;

        private FillFlowContainer idleBottomContent;
        private BeatmapCardDownloadProgressBar downloadProgressBar;

        [Resolved]
        private OsuColour colours { get; set; }

        [Resolved]
        private OverlayColourProvider colourProvider { get; set; }

        public BeatmapCard(APIBeatmapSet beatmapSet)
            : base(HoverSampleSet.Submit)
        {
            this.beatmapSet = beatmapSet;
            favouriteState = new Bindable<BeatmapSetFavouriteState>(new BeatmapSetFavouriteState(beatmapSet.HasFavourited, beatmapSet.FavouriteCount));
            downloadTracker = new BeatmapDownloadTracker(beatmapSet);
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            Width = width;
            Height = height;
            CornerRadius = corner_radius;
            Masking = true;

            InternalChildren = new Drawable[]
            {
                downloadTracker,
                rightAreaBackground = new Container
                {
                    RelativeSizeAxes = Axes.Y,
                    Width = icon_area_width + 2 * corner_radius,
                    Anchor = Anchor.CentreRight,
                    Origin = Anchor.CentreRight,
                    // workaround for masking artifacts at the top & bottom of card,
                    // which become especially visible on downloaded beatmaps (when the icon area has a lime background).
                    Padding = new MarginPadding { Vertical = 1 },
                    Child = new Box
                    {
                        RelativeSizeAxes = Axes.Both,
                        Colour = Colour4.White
                    },
                },
                new Container
                {
                    Name = @"Left (icon) area",
                    Size = new Vector2(height),
                    Children = new Drawable[]
                    {
                        leftCover = new UpdateableOnlineBeatmapSetCover(BeatmapSetCoverType.List)
                        {
                            RelativeSizeAxes = Axes.Both,
                            OnlineInfo = beatmapSet
                        },
                        leftIconArea = new FillFlowContainer
                        {
                            Margin = new MarginPadding(5),
                            AutoSizeAxes = Axes.Both,
                            Direction = FillDirection.Horizontal,
                            Spacing = new Vector2(1)
                        }
                    }
                },
                new Container
                {
                    Name = @"Right (button) area",
                    Width = 30,
                    RelativeSizeAxes = Axes.Y,
                    Origin = Anchor.TopRight,
                    Anchor = Anchor.TopRight,
                    Padding = new MarginPadding { Vertical = 17.5f },
                    Child = rightAreaButtons = new Container<BeatmapCardIconButton>
                    {
                        RelativeSizeAxes = Axes.Both,
                        Children = new BeatmapCardIconButton[]
                        {
                            new FavouriteButton(beatmapSet)
                            {
                                Current = favouriteState,
                                Anchor = Anchor.TopCentre,
                                Origin = Anchor.TopCentre
                            },
                            new DownloadButton(beatmapSet)
                            {
                                Anchor = Anchor.BottomCentre,
                                Origin = Anchor.BottomCentre,
                                State = { BindTarget = downloadTracker.State }
                            },
                            new GoToBeatmapButton(beatmapSet)
                            {
                                Anchor = Anchor.BottomCentre,
                                Origin = Anchor.BottomCentre,
                                State = { BindTarget = downloadTracker.State }
                            }
                        }
                    }
                },
                mainContent = new Container
                {
                    Name = @"Main content",
                    X = height - corner_radius,
                    Height = height,
                    CornerRadius = corner_radius,
                    Masking = true,
                    Children = new Drawable[]
                    {
                        mainContentBackground = new BeatmapCardContentBackground(beatmapSet)
                        {
                            RelativeSizeAxes = Axes.Both,
                        },
                        new FillFlowContainer
                        {
                            RelativeSizeAxes = Axes.Both,
                            Padding = new MarginPadding
                            {
                                Horizontal = 10,
                                Vertical = 4
                            },
                            Direction = FillDirection.Vertical,
                            Children = new Drawable[]
                            {
                                titleContainer = new GridContainer
                                {
                                    RelativeSizeAxes = Axes.X,
                                    AutoSizeAxes = Axes.Y,
                                    ColumnDimensions = new[]
                                    {
                                        new Dimension(),
                                        new Dimension(GridSizeMode.AutoSize)
                                    },
                                    RowDimensions = new[]
                                    {
                                        new Dimension(GridSizeMode.AutoSize)
                                    },
                                    Content = new[]
                                    {
                                        new[]
                                        {
                                            new OsuSpriteText
                                            {
                                                Text = new RomanisableString(beatmapSet.TitleUnicode, beatmapSet.Title),
                                                Font = OsuFont.Default.With(size: 22.5f, weight: FontWeight.SemiBold),
                                                RelativeSizeAxes = Axes.X,
                                                Truncate = true
                                            },
                                            Empty()
                                        }
                                    }
                                },
                                artistContainer = new GridContainer
                                {
                                    RelativeSizeAxes = Axes.X,
                                    AutoSizeAxes = Axes.Y,
                                    ColumnDimensions = new[]
                                    {
                                        new Dimension(),
                                        new Dimension(GridSizeMode.AutoSize)
                                    },
                                    RowDimensions = new[]
                                    {
                                        new Dimension(GridSizeMode.AutoSize)
                                    },
                                    Content = new[]
                                    {
                                        new[]
                                        {
                                            new OsuSpriteText
                                            {
                                                Text = createArtistText(),
                                                Font = OsuFont.Default.With(size: 17.5f, weight: FontWeight.SemiBold),
                                                RelativeSizeAxes = Axes.X,
                                                Truncate = true
                                            },
                                            Empty()
                                        },
                                    }
                                },
                                new LinkFlowContainer(s =>
                                {
                                    s.Shadow = false;
                                    s.Font = OsuFont.GetFont(size: 14, weight: FontWeight.SemiBold);
                                }).With(d =>
                                {
                                    d.AutoSizeAxes = Axes.Both;
                                    d.Margin = new MarginPadding { Top = 2 };
                                    d.AddText("mapped by ", t => t.Colour = colourProvider.Content2);
                                    d.AddUserLink(beatmapSet.Author);
                                }),
                            }
                        },
                        new Container
                        {
                            Name = @"Bottom content",
                            RelativeSizeAxes = Axes.X,
                            AutoSizeAxes = Axes.Y,
                            Anchor = Anchor.BottomLeft,
                            Origin = Anchor.BottomLeft,
                            Padding = new MarginPadding
                            {
                                Horizontal = 10,
                                Vertical = 4
                            },
                            Children = new Drawable[]
                            {
                                idleBottomContent = new FillFlowContainer
                                {
                                    RelativeSizeAxes = Axes.X,
                                    AutoSizeAxes = Axes.Y,
                                    Direction = FillDirection.Vertical,
                                    Spacing = new Vector2(0, 3),
                                    AlwaysPresent = true,
                                    Children = new Drawable[]
                                    {
                                        statisticsContainer = new FillFlowContainer<BeatmapCardStatistic>
                                        {
                                            RelativeSizeAxes = Axes.X,
                                            AutoSizeAxes = Axes.Y,
                                            Direction = FillDirection.Horizontal,
                                            Spacing = new Vector2(10, 0),
                                            Alpha = 0,
                                            AlwaysPresent = true,
                                            ChildrenEnumerable = createStatistics()
                                        },
                                        new FillFlowContainer
                                        {
                                            RelativeSizeAxes = Axes.X,
                                            AutoSizeAxes = Axes.Y,
                                            Direction = FillDirection.Horizontal,
                                            Spacing = new Vector2(4, 0),
                                            Children = new Drawable[]
                                            {
                                                new BeatmapSetOnlineStatusPill
                                                {
                                                    AutoSizeAxes = Axes.Both,
                                                    Status = beatmapSet.Status,
                                                    Anchor = Anchor.CentreLeft,
                                                    Origin = Anchor.CentreLeft
                                                },
                                                new DifficultySpectrumDisplay(beatmapSet)
                                                {
                                                    Anchor = Anchor.CentreLeft,
                                                    Origin = Anchor.CentreLeft,
                                                    DotSize = new Vector2(6, 12)
                                                }
                                            }
                                        }
                                    }
                                },
                                downloadProgressBar = new BeatmapCardDownloadProgressBar
                                {
                                    RelativeSizeAxes = Axes.X,
                                    Height = 6,
                                    Anchor = Anchor.Centre,
                                    Origin = Anchor.Centre,
                                    State = { BindTarget = downloadTracker.State },
                                    Progress = { BindTarget = downloadTracker.Progress }
                                }
                            }
                        }
                    }
                }
            };

            if (beatmapSet.HasVideo)
                leftIconArea.Add(new IconPill(FontAwesome.Solid.Film));

            if (beatmapSet.HasStoryboard)
                leftIconArea.Add(new IconPill(FontAwesome.Solid.Image));

            if (beatmapSet.HasExplicitContent)
            {
                titleContainer.Content[0][1] = new ExplicitContentBeatmapPill
                {
                    Anchor = Anchor.BottomRight,
                    Origin = Anchor.BottomRight,
                    Margin = new MarginPadding { Left = 5 }
                };
            }

            if (beatmapSet.TrackId != null)
            {
                artistContainer.Content[0][1] = new FeaturedArtistBeatmapPill
                {
                    Anchor = Anchor.BottomRight,
                    Origin = Anchor.BottomRight,
                    Margin = new MarginPadding { Left = 5 }
                };
            }
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            downloadTracker.State.BindValueChanged(_ => updateState(), true);
            FinishTransforms(true);
        }

        protected override bool OnHover(HoverEvent e)
        {
            updateState();
            return base.OnHover(e);
        }

        protected override void OnHoverLost(HoverLostEvent e)
        {
            updateState();
            base.OnHoverLost(e);
        }

        private LocalisableString createArtistText()
        {
            var romanisableArtist = new RomanisableString(beatmapSet.ArtistUnicode, beatmapSet.Artist);
            return BeatmapsetsStrings.ShowDetailsByArtist(romanisableArtist);
        }

        private IEnumerable<BeatmapCardStatistic> createStatistics()
        {
            if (beatmapSet.HypeStatus != null)
                yield return new HypesStatistic(beatmapSet.HypeStatus);

            // web does not show nominations unless hypes are also present.
            // see: https://github.com/ppy/osu-web/blob/8ed7d071fd1d3eaa7e43cf0e4ff55ca2fef9c07c/resources/assets/lib/beatmapset-panel.tsx#L443
            if (beatmapSet.HypeStatus != null && beatmapSet.NominationStatus != null)
                yield return new NominationsStatistic(beatmapSet.NominationStatus);

            yield return new FavouritesStatistic(beatmapSet) { Current = favouriteState };
            yield return new PlayCountStatistic(beatmapSet);

            var dateStatistic = BeatmapCardDateStatistic.CreateFor(beatmapSet);
            if (dateStatistic != null)
                yield return dateStatistic;
        }

        private void updateState()
        {
            float targetWidth = width - height;
            if (IsHovered)
                targetWidth = targetWidth - icon_area_width + corner_radius;

            mainContent.ResizeWidthTo(targetWidth, TRANSITION_DURATION, Easing.OutQuint);
            mainContentBackground.Dimmed.Value = IsHovered;

            leftCover.FadeColour(IsHovered ? OsuColour.Gray(0.2f) : Color4.White, TRANSITION_DURATION, Easing.OutQuint);
            statisticsContainer.FadeTo(IsHovered ? 1 : 0, TRANSITION_DURATION, Easing.OutQuint);

            rightAreaBackground.FadeColour(downloadTracker.State.Value == DownloadState.LocallyAvailable ? colours.Lime0 : colourProvider.Background3, TRANSITION_DURATION, Easing.OutQuint);
            rightAreaButtons.FadeTo(IsHovered ? 1 : 0, TRANSITION_DURATION, Easing.OutQuint);

            foreach (var button in rightAreaButtons)
            {
                button.IdleColour = downloadTracker.State.Value != DownloadState.LocallyAvailable ? colourProvider.Light1 : colourProvider.Background3;
                button.HoverColour = downloadTracker.State.Value != DownloadState.LocallyAvailable ? colourProvider.Content1 : colourProvider.Foreground1;
            }

            bool showProgress = downloadTracker.State.Value == DownloadState.Downloading || downloadTracker.State.Value == DownloadState.Importing;

            idleBottomContent.FadeTo(showProgress ? 0 : 1, TRANSITION_DURATION, Easing.OutQuint);
            downloadProgressBar.FadeTo(showProgress ? 1 : 0, TRANSITION_DURATION, Easing.OutQuint);
        }
    }
}
