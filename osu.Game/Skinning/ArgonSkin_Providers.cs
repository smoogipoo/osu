// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using System.Linq;
using osu.Framework.Audio.Sample;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Textures;
using osu.Game.Audio;
using osu.Game.Beatmaps.Formats;
using osu.Game.Screens.Play;
using osu.Game.Screens.Play.HUD;
using osu.Game.Screens.Play.HUD.HitErrorMeters;
using osu.Game.Skinning.Components;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Skinning
{
    public partial class ArgonSkin : ITextureProvider,
                                     ISampleProvider,
                                     IDrawableProvider<GlobalSkinnableContainerLookup>,
                                     IConfigProvider<GlobalSkinColours>,
                                     IConfigProvider<SkinComboColourLookup>
    {
        public virtual Texture? Get(string lookup, WrapMode wrapModeS, WrapMode wrapModeT)
            => Textures?.Get(lookup, wrapModeS, wrapModeT);

        public virtual ISample? Get(ISampleInfo lookup)
        {
            foreach (string lookupName in lookup.LookupNames)
            {
                var sample = Samples?.Get(lookupName)
                             ?? Resources.AudioManager?.Samples.Get(lookupName.Replace(@"Gameplay/", @"Gameplay/Argon/"))
                             ?? Resources.AudioManager?.Samples.Get(lookupName);

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

        public virtual Drawable? Get(GlobalSkinnableContainerLookup lookup)
        {
            switch (lookup.Lookup)
            {
                case GlobalSkinnableContainers.SongSelect:
                    var songSelectComponents = new DefaultSkinComponentsContainer(_ =>
                    {
                        // do stuff when we need to.
                    });

                    return songSelectComponents;

                case GlobalSkinnableContainers.MainHUDComponents:
                    if (lookup.Ruleset != null)
                    {
                        return new Container
                        {
                            RelativeSizeAxes = Axes.Both,
                            Child = new ArgonComboCounter
                            {
                                Anchor = Anchor.BottomLeft,
                                Origin = Anchor.BottomLeft,
                                Position = new Vector2(36, -66),
                                Scale = new Vector2(1.3f),
                            },
                        };
                    }

                    var mainHUDComponents = new DefaultSkinComponentsContainer(container =>
                    {
                        var health = container.OfType<ArgonHealthDisplay>().FirstOrDefault();
                        var healthLine = container.OfType<BoxElement>().FirstOrDefault();
                        var wedgePieces = container.OfType<ArgonWedgePiece>().ToArray();
                        var score = container.OfType<ArgonScoreCounter>().FirstOrDefault();
                        var accuracy = container.OfType<ArgonAccuracyCounter>().FirstOrDefault();
                        var performancePoints = container.OfType<ArgonPerformancePointsCounter>().FirstOrDefault();
                        var songProgress = container.OfType<ArgonSongProgress>().FirstOrDefault();
                        var keyCounter = container.OfType<ArgonKeyCounterDisplay>().FirstOrDefault();

                        if (health != null)
                        {
                            // elements default to beneath the health bar
                            const float components_x_offset = 50;

                            health.Anchor = Anchor.TopLeft;
                            health.Origin = Anchor.TopLeft;
                            health.UseRelativeSize.Value = false;
                            health.Width = 300;
                            health.BarHeight.Value = 30f;
                            health.Position = new Vector2(components_x_offset, 20f);

                            if (healthLine != null)
                            {
                                healthLine.Anchor = Anchor.TopLeft;
                                healthLine.Origin = Anchor.CentreLeft;
                                healthLine.Y = health.Y + ArgonHealthDisplay.MAIN_PATH_RADIUS;
                                healthLine.Size = new Vector2(45, 3);
                            }

                            foreach (var wedgePiece in wedgePieces)
                                wedgePiece.Position += new Vector2(-50, 15);

                            if (score != null)
                            {
                                score.Origin = Anchor.TopRight;
                                score.Position = new Vector2(components_x_offset + 200, wedgePieces.Last().Y + 30);
                            }

                            if (accuracy != null)
                            {
                                // +4 to vertically align the accuracy counter with the score counter.
                                accuracy.Position = new Vector2(-20, 20);
                                accuracy.Anchor = Anchor.TopRight;
                                accuracy.Origin = Anchor.TopRight;
                            }

                            if (performancePoints != null && accuracy != null)
                            {
                                performancePoints.Position = new Vector2(accuracy.X, accuracy.Y + accuracy.DrawHeight + 10);
                                performancePoints.Anchor = Anchor.TopRight;
                                performancePoints.Origin = Anchor.TopRight;
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

                            if (songProgress != null)
                            {
                                const float padding = 10;
                                // Hard to find this at runtime, so taken from the most expanded state during replay.
                                const float song_progress_offset_height = 36 + padding;

                                songProgress.Position = new Vector2(0, -padding);
                                songProgress.Scale = new Vector2(0.9f, 1);

                                if (keyCounter != null && hitError != null)
                                {
                                    keyCounter.Anchor = Anchor.BottomRight;
                                    keyCounter.Origin = Anchor.BottomRight;
                                    keyCounter.Position = new Vector2(-(hitError.Width + padding), -(padding * 2 + song_progress_offset_height));
                                }
                            }
                        }
                    })
                    {
                        Children = new Drawable[]
                        {
                            new ArgonWedgePiece
                            {
                                Size = new Vector2(380, 72),
                            },
                            new ArgonWedgePiece
                            {
                                Size = new Vector2(380, 72),
                                Position = new Vector2(4, 5)
                            },
                            new ArgonScoreCounter
                            {
                                ShowLabel = { Value = false },
                            },
                            new ArgonHealthDisplay(),
                            new BoxElement
                            {
                                CornerRadius = { Value = 0.5f }
                            },
                            new ArgonAccuracyCounter(),
                            new ArgonPerformancePointsCounter
                            {
                                Scale = new Vector2(0.8f),
                            },
                            new BarHitErrorMeter(),
                            new BarHitErrorMeter(),
                            new ArgonSongProgress(),
                            new ArgonKeyCounterDisplay(),
                        }
                    };

                    return mainHUDComponents;
            }

            return null;
        }

        public virtual IBindable<TValue>? Get<TValue>(GlobalSkinColours lookup) where TValue : notnull
        {
            switch (lookup)
            {
                case GlobalSkinColours.ComboColours:
                {
                    LogLookupDebug(this, lookup, LookupDebugType.Hit);
                    return SkinUtils.As<TValue>(new Bindable<IReadOnlyList<Color4>?>(Configuration.ComboColours));
                }
            }

            return null;
        }

        public virtual IBindable<TValue>? Get<TValue>(SkinComboColourLookup lookup) where TValue : notnull
        {
            LogLookupDebug(this, lookup, LookupDebugType.Hit);
            return SkinUtils.As<TValue>(new Bindable<Color4>(getComboColour(Configuration, lookup.ColourIndex)));
        }

        private static Color4 getComboColour(IHasComboColours source, int colourIndex)
            => source.ComboColours![colourIndex % source.ComboColours.Count];
    }
}
