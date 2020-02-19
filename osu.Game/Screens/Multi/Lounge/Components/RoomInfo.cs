// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osu.Game.Screens.Multi.Components;
using osuTK;

namespace osu.Game.Screens.Multi.Lounge.Components
{
    public class RoomInfo : MultiplayerComposite
    {
        private readonly SpriteText roomName;

        public RoomInfo()
        {
            AutoSizeAxes = Axes.Y;

            InternalChild = new FillFlowContainer
            {
                RelativeSizeAxes = Axes.X,
                AutoSizeAxes = Axes.Y,
                Direction = FillDirection.Vertical,
                Spacing = new Vector2(0, 4),
                Children = new Drawable[]
                {
                    new Container
                    {
                        RelativeSizeAxes = Axes.X,
                        AutoSizeAxes = Axes.Y,
                        Children = new Drawable[]
                        {
                            new FillFlowContainer
                            {
                                AutoSizeAxes = Axes.Both,
                                Direction = FillDirection.Vertical,
                                Children = new Drawable[]
                                {
                                    roomName = new OsuSpriteText { Font = OsuFont.GetFont(size: 30) },
                                    new RoomStatusInfo(),
                                }
                            },
                            new ModeTypeInfo
                            {
                                Anchor = Anchor.CentreRight,
                                Origin = Anchor.CentreRight
                            }
                        }
                    },
                    new ParticipantInfo(),
                }
            };
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            RoomName.BindValueChanged(name => roomName.Text = name.NewValue, true);
        }
    }
}
