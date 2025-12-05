// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using System.Linq;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Online.Multiplayer.MatchTypes.RankedPlay;
using osuTK;

namespace osu.Game.Screens.OnlinePlay.Matchmaking.RankedPlay
{
    public partial class RankedPlayScreen2
    {
        public partial class Hand : CompositeDrawable
        {
            public readonly Bindable<bool> AllowSelection = new Bindable<bool>();
            public int SelectionLength { get; set; }

            private readonly FillFlowContainer<Card> cards;

            public Hand()
            {
                InternalChild = cards = new FillFlowContainer<Card>
                {
                    RelativeSizeAxes = Axes.Both,
                    Spacing = new Vector2(10),
                };
            }

            public IEnumerable<RankedPlayCardItem> CurrentSelection
                => cards.Where(c => c.Selected.Value).Select(c => c.Item.Card);

            public void AddCard(RankedPlayCardWithPlaylistItem item)
            {
                var card = new Card(item)
                {
                    Anchor = Anchor.BottomCentre,
                    Origin = Anchor.BottomCentre,
                    AllowSelection = { BindTarget = AllowSelection },
                };

                card.Selected.BindValueChanged(e =>
                {
                    if (!e.NewValue)
                        return;

                    while (CurrentSelection.Count() > SelectionLength)
                        cards.First(c => c != card && c.Selected.Value).Selected.Value = false;
                }, true);

                cards.Add(card);
            }

            public void RemoveCard(RankedPlayCardWithPlaylistItem item)
            {
                cards.RemoveAll(c => c.Item.Card.Equals(item.Card), true);
            }
        }
    }
}
