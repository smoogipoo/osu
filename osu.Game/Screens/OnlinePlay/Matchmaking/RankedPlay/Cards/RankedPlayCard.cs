// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Threading.Tasks;
using osu.Framework.Allocation;
using osu.Framework.Audio;
using osu.Framework.Audio.Sample;
using osu.Framework.Bindables;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Effects;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Logging;
using osu.Game.Audio;
using osu.Game.Database;
using osu.Game.Online.Rooms;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Screens.OnlinePlay.Matchmaking.RankedPlay.Cards
{
    public partial class RankedPlayCard : CompositeDrawable
    {
        public readonly RankedPlayCardWithPlaylistItem Item;

        private readonly IBindable<MultiplayerPlaylistItem?> playlistItem;

        private readonly Container content;
        private readonly Container cardContent;
        private readonly Container shadow;

        public readonly Container OverlayLayer;

        public float Elevation;

        private Sample? cardFlipSample;

        [Resolved]
        private BeatmapLookupCache beatmapLookupCache { get; set; } = null!;

        public RankedPlayCard(RankedPlayCardWithPlaylistItem item)
        {
            Item = item;

            Size = new Vector2(120, 200);

            playlistItem = item.PlaylistItem.GetBoundCopy();

            InternalChildren =
            [
                shadow = new Container
                {
                    RelativeSizeAxes = Axes.Both,
                    Masking = true,
                    CornerRadius = 10,
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    EdgeEffect = new EdgeEffectParameters
                    {
                        Type = EdgeEffectType.Shadow,
                        Radius = 5,
                        Colour = Color4.Black.Opacity(0.1f),
                    },
                    Child = new Box
                    {
                        RelativeSizeAxes = Axes.Both,
                        Alpha = 0,
                        AlwaysPresent = true,
                    }
                },
                content = new Container
                {
                    Masking = true,
                    CornerRadius = 10,
                    RelativeSizeAxes = Axes.Both,
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    Children =
                    [
                        cardContent = new Container
                        {
                            RelativeSizeAxes = Axes.Both,
                            Child = new RankedPlayCardBackSide()
                        },
                        OverlayLayer = new Container
                        {
                            RelativeSizeAxes = Axes.Both,
                        }
                    ]
                }
            ];
        }

        [BackgroundDependencyLoader]
        private void load(AudioManager audio)
        {
            cardFlipSample = audio.Samples.Get(@"Multiplayer/Matchmaking/Ranked/card-flip-1");
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            playlistItem.BindValueChanged(e => onPlaylistItemChanged(e.NewValue));
            if (playlistItem.Value != null)
                loadCardContent(playlistItem.Value, false);
        }

        protected override void UpdateAfterChildren()
        {
            base.UpdateAfterChildren();

            shadow.Scale = content.Scale;
            shadow.Size = new Vector2(1 - Elevation * 0.25f);
            shadow.Position = new Vector2(-25, 20) * Elevation;
        }

        #region beatmap fetching logic & card flip

        private readonly TaskCompletionSource cardRevealed = new TaskCompletionSource();

        public Task CardRevealed => cardRevealed.Task;

        private void onPlaylistItemChanged(MultiplayerPlaylistItem? playlistItem)
        {
            if (playlistItem == null)
            {
                setContent(new RankedPlayCardBackSide(), true);
                return;
            }

            loadCardContent(playlistItem, true);
        }

        private void loadCardContent(MultiplayerPlaylistItem playlistItem, bool flip) => Task.Run(async () =>
        {
            var beatmap = await beatmapLookupCache.GetBeatmapAsync(playlistItem.BeatmapID).ConfigureAwait(false);

            cardRevealed.TrySetResult();

            if (beatmap == null)
            {
                Logger.Log($"Failed to load beatmap {playlistItem.BeatmapID} for playlistItem {playlistItem.ID}.", level: LogLevel.Error);
                return;
            }

            Schedule(() =>
            {
                var drawable = new RankedPlayCardContent(beatmap)
                {
                    Scale = new Vector2(0.4f) // TODO: make both card drawables the same size
                };

                setContent(drawable, flip);
            });
        });

        private void setContent(Drawable newContent, bool flip)
        {
            if (!flip)
            {
                cardContent.Child = newContent;
                return;
            }

            content.ScaleTo(new Vector2(0, 1), 100, Easing.In)
                   .Then()
                   .Schedule(() => cardContent.Child = newContent)
                   .ScaleTo(new Vector2(1), 300, Easing.OutElasticQuarter);

            SamplePlaybackHelper.PlayWithRandomPitch(cardFlipSample);
        }

        #endregion

        public void PopOutAndExpire()
        {
            content.ScaleTo(0, 500, Easing.In);

            this.FadeOut(500)
                .Expire();
        }
    }
}
