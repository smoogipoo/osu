// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Input.Events;
using osu.Game.Graphics.Sprites;
using osu.Game.Online.Rooms;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Screens.OnlinePlay.Matchmaking.RankedPlay
{
    public partial class RankedPlayScreen2
    {
        public partial class Card : CompositeDrawable
        {
            public readonly Bindable<bool> AllowSelection = new Bindable<bool>();
            public readonly BindableBool Selected = new BindableBool();

            public readonly RevealableCardItem Item;

            private readonly Bindable<MultiplayerPlaylistItem?> playlistItem = new Bindable<MultiplayerPlaylistItem?>();

            private readonly Box background;
            private readonly OsuSpriteText beatmapIdText;

            public Card(RevealableCardItem item)
            {
                Item = item;

                Size = new Vector2(100, 200);
                Masking = true;
                BorderColour = Color4.Yellow;
                BorderThickness = 0;

                InternalChildren = new Drawable[]
                {
                    background = new Box
                    {
                        RelativeSizeAxes = Axes.Both,
                        Colour = Color4.DimGray
                    },
                    new FillFlowContainer
                    {
                        Anchor = Anchor.Centre,
                        Origin = Anchor.Centre,
                        Children = new Drawable[]
                        {
                            new OsuSpriteText
                            {
                                Anchor = Anchor.Centre,
                                Origin = Anchor.Centre,
                                Text = $"ID: {item.Item.ID.GetHashCode()}"
                            },
                            beatmapIdText = new OsuSpriteText
                            {
                                Anchor = Anchor.Centre,
                                Origin = Anchor.Centre,
                                Text = "Hidden"
                            }
                        }
                    }
                };
            }

            protected override void LoadComplete()
            {
                base.LoadComplete();

                playlistItem.BindTo(Item.PlaylistItem);
                playlistItem.BindValueChanged(onPlaylistItemChanged, true);

                AllowSelection.BindValueChanged(onAllowSelectionChanged, true);
                Selected.BindValueChanged(onSelectedChanged, true);
            }

            private void onPlaylistItemChanged(ValueChangedEvent<MultiplayerPlaylistItem?> e)
            {
                if (e.NewValue != null)
                {
                    background.Colour = Color4.SlateGray;
                    beatmapIdText.Text = $"Beatmap: {e.NewValue.BeatmapID}";
                }
            }

            private void onAllowSelectionChanged(ValueChangedEvent<bool> e)
            {
                if (!e.NewValue)
                    Selected.Value = false;
            }

            private void onSelectedChanged(ValueChangedEvent<bool> e)
            {
                BorderThickness = e.NewValue ? 5 : 0;
            }

            protected override bool OnClick(ClickEvent e)
            {
                if (AllowSelection.Value)
                    Selected.Toggle();

                return true;
            }
        }
    }
}
