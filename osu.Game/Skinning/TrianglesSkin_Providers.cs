// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using System.Linq;
using osu.Framework.Audio.Sample;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Textures;
using osu.Game.Audio;
using osu.Game.Beatmaps.Formats;
using osu.Game.Extensions;
using osu.Game.Screens.Play.HUD;
using osu.Game.Screens.Play.HUD.HitErrorMeters;
using osu.Game.Skinning.Triangles;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Skinning
{
    public partial class TrianglesSkin : ITextureProvider,
                                         ISampleProvider,
                                         IDrawableProvider<GlobalSkinnableContainerLookup>,
                                         IConfigProvider<GlobalSkinColours>,
                                         IConfigProvider<SkinComboColourLookup>
    {
        public Texture? Get(string lookup, WrapMode wrapModeS, WrapMode wrapModeT)
            => Textures?.Get(lookup, wrapModeS, wrapModeT);

        public ISample? Get(ISampleInfo lookup)
        {
            foreach (string lookupName in lookup.LookupNames)
            {
                var sample = Samples?.Get(lookupName) ?? resources.AudioManager?.Samples.Get(lookupName);
                if (sample != null)
                    return sample;
            }

            return null;
        }

        internal override Drawable? Get(SkinnableSprite.SpriteComponentLookup lookup)
        {
            // Temporary until default skin has a valid hit lighting.
            if (lookup.LookupName == @"lighting")
                return Drawable.Empty();

            return base.Get(lookup);
        }

        public Drawable? Get(GlobalSkinnableContainerLookup lookup)
        {
            // Only handle global level defaults for now.
            if (lookup.Ruleset != null)
                return null;

            switch (lookup.Lookup)
            {
                case GlobalSkinnableContainers.SongSelect:
                    var songSelectComponents = new DefaultSkinComponentsContainer(_ =>
                    {
                        // do stuff when we need to.
                    });

                    return songSelectComponents;

                case GlobalSkinnableContainers.MainHUDComponents:
                    var skinnableTargetWrapper = new DefaultSkinComponentsContainer(container =>
                    {
                        var score = container.OfType<DefaultScoreCounter>().FirstOrDefault();
                        var accuracy = container.OfType<DefaultAccuracyCounter>().FirstOrDefault();
                        var combo = container.OfType<DefaultComboCounter>().FirstOrDefault();
                        var ppCounter = container.OfType<PerformancePointsCounter>().FirstOrDefault();
                        var songProgress = container.OfType<DefaultSongProgress>().FirstOrDefault();
                        var keyCounter = container.OfType<DefaultKeyCounterDisplay>().FirstOrDefault();

                        if (score != null)
                        {
                            score.Anchor = Anchor.TopCentre;
                            score.Origin = Anchor.TopCentre;

                            // elements default to beneath the health bar
                            const float vertical_offset = 30;

                            const float horizontal_padding = 20;

                            score.Position = new Vector2(0, vertical_offset);

                            if (ppCounter != null)
                            {
                                ppCounter.Y = score.Position.Y + ppCounter.ScreenSpaceDeltaToParentSpace(score.ScreenSpaceDrawQuad.Size).Y - 4;
                                ppCounter.Origin = Anchor.TopCentre;
                                ppCounter.Anchor = Anchor.TopCentre;
                            }

                            if (accuracy != null)
                            {
                                accuracy.Position = new Vector2(-accuracy.ScreenSpaceDeltaToParentSpace(score.ScreenSpaceDrawQuad.Size).X / 2 - horizontal_padding, vertical_offset + 5);
                                accuracy.Origin = Anchor.TopRight;
                                accuracy.Anchor = Anchor.TopCentre;

                                if (combo != null)
                                {
                                    combo.Position = new Vector2(accuracy.ScreenSpaceDeltaToParentSpace(score.ScreenSpaceDrawQuad.Size).X / 2 + horizontal_padding, vertical_offset + 5);
                                    combo.Anchor = Anchor.TopCentre;
                                }
                            }

                            var hitError = container.OfType<HitErrorMeter>().FirstOrDefault();

                            if (hitError != null)
                            {
                                hitError.Anchor = Anchor.CentreLeft;
                                hitError.Origin = Anchor.CentreLeft;
                            }

                            var hitError2 = container.OfType<HitErrorMeter>().LastOrDefault();

                            if (hitError2 != null)
                            {
                                hitError2.Anchor = Anchor.CentreRight;
                                hitError2.Scale = new Vector2(-1, 1);
                                // origin flipped to match scale above.
                                hitError2.Origin = Anchor.CentreLeft;
                            }
                        }

                        if (songProgress != null && keyCounter != null)
                        {
                            const float padding = 10;

                            // Hard to find this at runtime, so taken from the most expanded state during replay.
                            const float song_progress_offset_height = 73;

                            keyCounter.Anchor = Anchor.BottomRight;
                            keyCounter.Origin = Anchor.BottomRight;
                            keyCounter.Position = new Vector2(-padding, -(song_progress_offset_height + padding));
                        }
                    })
                    {
                        Children = new Drawable[]
                        {
                            new DefaultComboCounter(),
                            new DefaultScoreCounter(),
                            new DefaultAccuracyCounter(),
                            new DefaultHealthDisplay(),
                            new DefaultSongProgress(),
                            new DefaultKeyCounterDisplay(),
                            new BarHitErrorMeter(),
                            new BarHitErrorMeter(),
                            new TrianglesPerformancePointsCounter()
                        }
                    };

                    return skinnableTargetWrapper;
            }

            return null;
        }

        public IBindable<TValue>? Get<TValue>(GlobalSkinColours lookup) where TValue : notnull
        {
            LogLookupDebug(this, lookup, LookupDebugType.Hit);
            return SkinUtils.As<TValue>(new Bindable<IReadOnlyList<Color4>?>(Configuration.ComboColours));
        }

        public IBindable<TValue>? Get<TValue>(SkinComboColourLookup lookup) where TValue : notnull
        {
            LogLookupDebug(this, lookup, LookupDebugType.Hit);
            return SkinUtils.As<TValue>(new Bindable<Color4>(getComboColour(Configuration, lookup.ColourIndex)));
        }

        private static Color4 getComboColour(IHasComboColours source, int colourIndex)
            => source.ComboColours![colourIndex % source.ComboColours.Count];
    }
}
