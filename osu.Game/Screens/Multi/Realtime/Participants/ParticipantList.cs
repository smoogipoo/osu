// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Graphics.Containers;
using osuTK;

namespace osu.Game.Screens.Multi.Realtime.Participants
{
    public class ParticipantList : MatchComposite
    {
        private FillFlowContainer<ParticipantPanel> panels;

        [BackgroundDependencyLoader]
        private void load()
        {
            InternalChild = new OsuScrollContainer
            {
                RelativeSizeAxes = Axes.Both,
                Child = panels = new FillFlowContainer<ParticipantPanel>
                {
                    RelativeSizeAxes = Axes.X,
                    AutoSizeAxes = Axes.Y,
                    Direction = FillDirection.Vertical,
                    Spacing = new Vector2(0, 2)
                }
            };
        }

        protected override void OnRoomChanged()
        {
            base.OnRoomChanged();

            panels.RemoveAll(p => !Room.Users.Contains(p.User));
            foreach (var user in Room.Users.Except(panels.Select(p => p.User)))
                panels.Add(new ParticipantPanel(user));
        }
    }
}
