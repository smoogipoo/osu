// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osu.Game.Database;
using osu.Game.Graphics.Sprites;
using osu.Game.Graphics.UserInterface;
using osu.Game.Online.API.Requests.Responses;
using osu.Game.Online.Multiplayer;
using osuTK;
using osuTK.Graphics;

namespace osu.Game
{
    public class ChallengeOverlay : CompositeDrawable
    {
        [Resolved]
        private MultiplayerClient client { get; set; } = null!;

        private FillFlowContainer challengesContainer = null!;

        [BackgroundDependencyLoader]
        private void load()
        {
            InternalChildren = new Drawable[]
            {
                new Box
                {
                    RelativeSizeAxes = Axes.Both,
                    Colour = Color4.SlateGray,
                },
                challengesContainer = new FillFlowContainer
                {
                    RelativeSizeAxes = Axes.Both,
                    Padding = new MarginPadding(10),
                }
            };
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            client.MatchmakingChallengeIssued += onChallengeIssued;
        }

        private void onChallengeIssued(int userId)
        {
            challengesContainer.Add(new ChallengeRow(userId));
        }

        private class ChallengeRow : CompositeDrawable
        {
            [Resolved]
            private UserLookupCache userLookupCache { get; set; } = null!;

            [Resolved]
            private MultiplayerClient client { get; set; } = null!;

            private readonly int userId;

            public ChallengeRow(int userId)
            {
                this.userId = userId;

                RelativeSizeAxes = Axes.X;
                Height = 20;
            }

            [BackgroundDependencyLoader]
            private void load()
            {
                APIUser user = userLookupCache.GetUserAsync(userId).GetResultSafely()!;

                InternalChild = new GridContainer
                {
                    RelativeSizeAxes = Axes.Both,
                    ColumnDimensions =
                    [
                        new Dimension(),
                        new Dimension(GridSizeMode.AutoSize),
                        new Dimension(GridSizeMode.AutoSize)
                    ],
                    Content = new[]
                    {
                        new Drawable[]
                        {
                            new OsuSpriteText
                            {
                                Text = user.Username
                            },
                            new IconButton
                            {
                                Size = new Vector2(20),
                                Icon = FontAwesome.Regular.CheckCircle,
                                Colour = Color4.Green,
                                Action = () => client.MatchmakingAcceptChallenge(userId)
                            },
                            new IconButton
                            {
                                Size = new Vector2(20),
                                Icon = FontAwesome.Regular.TimesCircle,
                                Colour = Color4.Red,
                                Action = () => client.MatchmakingDeclineChallenge(userId)
                            }
                        }
                    }
                };
            }
        }
    }
}
