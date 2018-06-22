// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System.Collections.Generic;
using System.Linq;

namespace osu.Game.Rulesets.Osu.Replays
{
    /// <summary>
    /// A slightly more informative version of <see cref="OsuAction"/>.
    /// Keeps track of which buttons are the primary or secondary buttons, to more easily determine how we should alternate buttons.
    /// </summary>
    public class ButtonPlan
    {
        public Button Primary;
        public Button Secondary;

        private static IEnumerable<OsuAction> toRbs(Button button)
        {
            switch (button)
            {
                default:
                case Button.None:
                    break;
                case Button.Left:
                    yield return OsuAction.LeftButton;
                    break;
                case Button.Right:
                    yield return OsuAction.RightButton;
                    break;
            }
        }

        public IEnumerable<OsuAction> Rbs => toRbs(Primary).Concat(toRbs(Secondary));
        public IEnumerable<OsuAction> PrimaryRbs => toRbs(Primary);
    }
}
