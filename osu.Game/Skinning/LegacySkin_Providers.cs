// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using osu.Framework.Audio.Sample;
using osu.Framework.Bindables;
using osu.Framework.Extensions.ObjectExtensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Textures;
using osu.Game.Audio;
using osu.Game.Beatmaps.Formats;
using osu.Game.Extensions;
using osu.Game.Rulesets.Objects.Legacy;
using osu.Game.Rulesets.Objects.Types;
using osu.Game.Rulesets.Scoring;
using osu.Game.Screens.Play.HUD;
using osu.Game.Screens.Play.HUD.HitErrorMeters;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Skinning
{
    public partial class LegacySkin : ITextureProvider,
                                      ISampleProvider,
                                      IDrawableProvider<GlobalSkinnableContainerLookup>,
                                      IDrawableProvider<SkinComponentLookup<HitResult>>,
                                      IConfigProvider<GlobalSkinColours>,
                                      IConfigProvider<SkinComboColourLookup>,
                                      IConfigProvider<SkinCustomColourLookup>,
                                      IConfigProvider<LegacyManiaSkinConfigurationLookup>,
                                      IConfigProvider<SkinConfiguration.LegacySetting>
    {
        protected virtual bool AllowManiaConfigLookups => true;

        /// <summary>
        /// Whether this skin can use samples with a custom bank (custom sample set in stable terminology).
        /// Added in order to match sample lookup logic from stable (in stable, only the beatmap skin could use samples with a custom sample bank).
        /// </summary>
        protected virtual bool UseCustomSampleBanks => false;

        /// <summary>
        /// Whether high-resolution textures ("@2x"-suffixed) are allowed to be used by <see cref="Get(string, WrapMode, WrapMode)"/> when available.
        /// </summary>
        protected virtual bool AllowHighResolutionSprites => true;

        public virtual Texture? Get(string lookup, WrapMode wrapModeS, WrapMode wrapModeT)
        {
            switch (lookup)
            {
                case "Menu/fountain-star":
                    lookup = "star2";
                    break;
            }

            Texture? texture = null;
            float ratio = 1;

            if (AllowHighResolutionSprites)
            {
                // some component names (especially user-controlled ones, like `HitX` in mania)
                // may contain `@2x` scale specifications.
                // stable happens to check for that and strip them, so do the same to match stable behaviour.
                lookup = lookup.Replace(@"@2x", string.Empty);

                string twoTimesFilename = $"{Path.ChangeExtension(lookup, null)}@2x{Path.GetExtension(lookup)}";

                texture = Textures?.Get(twoTimesFilename, wrapModeS, wrapModeT);

                if (texture != null)
                    ratio = 2;
            }

            texture ??= Textures?.Get(lookup, wrapModeS, wrapModeT);

            if (texture != null)
                texture.ScaleAdjust = ratio;

            return texture;
        }

        public virtual ISample? Get(ISampleInfo lookup)
        {
            if (!(lookup is ConvertHitObjectParser.LegacyHitSampleInfo legacySample))
                return get(lookup);

            var playLayeredHitSounds = Get<bool>(SkinConfiguration.LegacySetting.LayeredHitSounds);
            if (legacySample.IsLayered && playLayeredHitSounds?.Value == false)
                return new SampleVirtual();

            return get(lookup);
        }

        private ISample? get(ISampleInfo lookup)
        {
            IEnumerable<string> lookupNames;

            if (lookup is HitSampleInfo hitSample)
                lookupNames = getLegacyLookupNames(hitSample);
            else
            {
                lookupNames = lookup.LookupNames.SelectMany(getFallbackSampleNames);
            }

            foreach (string lookupName in lookupNames)
            {
                var sample = Samples?.Get(lookupName);

                if (sample != null)
                    return sample;
            }

            return null;
        }

        public virtual Drawable? Get(GlobalSkinnableContainerLookup lookup)
        {
            switch (lookup.Lookup)
            {
                case GlobalSkinnableContainers.MainHUDComponents:
                    if (lookup.Ruleset != null)
                    {
                        return new DefaultSkinComponentsContainer(container =>
                        {
                            var combo = container.OfType<LegacyDefaultComboCounter>().FirstOrDefault();

                            if (combo != null)
                            {
                                combo.Anchor = Anchor.BottomLeft;
                                combo.Origin = Anchor.BottomLeft;
                                combo.Scale = new Vector2(1.28f);
                            }
                        })
                        {
                            new LegacyDefaultComboCounter()
                        };
                    }

                    return new DefaultSkinComponentsContainer(container =>
                    {
                        var score = container.OfType<LegacyScoreCounter>().FirstOrDefault();
                        var accuracy = container.OfType<GameplayAccuracyCounter>().FirstOrDefault();

                        if (score != null && accuracy != null)
                        {
                            accuracy.Y = container.ToLocalSpace(score.ScreenSpaceDrawQuad.BottomRight).Y;
                        }

                        var songProgress = container.OfType<LegacySongProgress>().FirstOrDefault();

                        if (songProgress != null && accuracy != null)
                        {
                            songProgress.Anchor = Anchor.TopRight;
                            songProgress.Origin = Anchor.CentreRight;
                            songProgress.X = -accuracy.ScreenSpaceDeltaToParentSpace(accuracy.ScreenSpaceDrawQuad.Size).X - 18;
                            songProgress.Y = container.ToLocalSpace(accuracy.ScreenSpaceDrawQuad.TopLeft).Y + (accuracy.ScreenSpaceDeltaToParentSpace(accuracy.ScreenSpaceDrawQuad.Size).Y / 2);
                        }

                        var hitError = container.OfType<HitErrorMeter>().FirstOrDefault();

                        if (hitError != null)
                        {
                            hitError.Anchor = Anchor.BottomCentre;
                            hitError.Origin = Anchor.CentreLeft;
                            hitError.Rotation = -90;
                        }
                    })
                    {
                        Children = new Drawable[]
                        {
                            new LegacyScoreCounter(),
                            new LegacyAccuracyCounter(),
                            new LegacySongProgress(),
                            new LegacyHealthDisplay(),
                            new BarHitErrorMeter(),
                        }
                    };
            }

            return null;
        }

        public virtual Drawable? Get(SkinComponentLookup<HitResult> lookup)
        {
            // kind of wasteful that we throw this away, but should do for now.
            if (getJudgementAnimation(lookup.Component) != null)
            {
                // TODO: this should be inside the judgement pieces.
                Func<Drawable> createDrawable = () => getJudgementAnimation(lookup.Component).AsNonNull();

                var particle = getParticleTexture(lookup.Component);

                if (particle != null)
                    return new LegacyJudgementPieceNew(lookup.Component, createDrawable, particle);

                return new LegacyJudgementPieceOld(lookup.Component, createDrawable);
            }

            return null;
        }

        public virtual IBindable<TValue>? Get<TValue>(GlobalSkinColours lookup) where TValue : notnull
        {
            switch (lookup)
            {
                case GlobalSkinColours.ComboColours:
                    var comboColours = Configuration.ComboColours;
                    if (comboColours != null)
                        return SkinUtils.As<TValue>(new Bindable<IReadOnlyList<Color4>>(comboColours));

                    return null;

                default:
                    return SkinUtils.As<TValue>(getCustomColour(Configuration, lookup.ToString()));
            }
        }

        public virtual IBindable<TValue>? Get<TValue>(SkinComboColourLookup lookup) where TValue : notnull
        {
            return SkinUtils.As<TValue>(GetComboColour(Configuration, lookup.ColourIndex, lookup.Combo));
        }

        public virtual IBindable<TValue>? Get<TValue>(SkinCustomColourLookup lookup) where TValue : notnull
        {
            return SkinUtils.As<TValue>(getCustomColour(Configuration, lookup.Lookup.ToString() ?? string.Empty));
        }

        public virtual IBindable<TValue>? Get<TValue>(LegacyManiaSkinConfigurationLookup lookup) where TValue : notnull
        {
            if (!AllowManiaConfigLookups)
                return null;

            if (!maniaConfigurations.TryGetValue(lookup.TotalColumns, out var existing))
                maniaConfigurations[lookup.TotalColumns] = existing = new LegacyManiaSkinConfiguration(lookup.TotalColumns);

            switch (lookup.Lookup)
            {
                case LegacyManiaSkinConfigurationLookups.ColumnWidth:
                    Debug.Assert(lookup.ColumnIndex != null);
                    return SkinUtils.As<TValue>(new Bindable<float>(existing.ColumnWidth[lookup.ColumnIndex.Value]));

                case LegacyManiaSkinConfigurationLookups.WidthForNoteHeightScale:
                    Debug.Assert(lookup.ColumnIndex != null);
                    return SkinUtils.As<TValue>(new Bindable<float>(existing.WidthForNoteHeightScale));

                case LegacyManiaSkinConfigurationLookups.ColumnSpacing:
                    Debug.Assert(lookup.ColumnIndex != null);
                    return SkinUtils.As<TValue>(new Bindable<float>(existing.ColumnSpacing[lookup.ColumnIndex.Value]));

                case LegacyManiaSkinConfigurationLookups.HitPosition:
                    return SkinUtils.As<TValue>(new Bindable<float>(existing.HitPosition));

                case LegacyManiaSkinConfigurationLookups.ComboPosition:
                    return SkinUtils.As<TValue>(new Bindable<float>(existing.ComboPosition));

                case LegacyManiaSkinConfigurationLookups.ScorePosition:
                    return SkinUtils.As<TValue>(new Bindable<float>(existing.ScorePosition));

                case LegacyManiaSkinConfigurationLookups.LightPosition:
                    return SkinUtils.As<TValue>(new Bindable<float>(existing.LightPosition));

                case LegacyManiaSkinConfigurationLookups.ShowJudgementLine:
                    return SkinUtils.As<TValue>(new Bindable<bool>(existing.ShowJudgementLine));

                case LegacyManiaSkinConfigurationLookups.ExplosionImage:
                    return SkinUtils.As<TValue>(getManiaImage(existing, "LightingN"));

                case LegacyManiaSkinConfigurationLookups.ExplosionScale:
                    Debug.Assert(lookup.ColumnIndex != null);

                    if (Get<decimal>(SkinConfiguration.LegacySetting.Version)?.Value < 2.5m)
                        return SkinUtils.As<TValue>(new Bindable<float>(1));

                    if (existing.ExplosionWidth[lookup.ColumnIndex.Value] != 0)
                        return SkinUtils.As<TValue>(new Bindable<float>(existing.ExplosionWidth[lookup.ColumnIndex.Value] / LegacyManiaSkinConfiguration.DEFAULT_COLUMN_SIZE));

                    return SkinUtils.As<TValue>(new Bindable<float>(existing.ColumnWidth[lookup.ColumnIndex.Value] / LegacyManiaSkinConfiguration.DEFAULT_COLUMN_SIZE));

                case LegacyManiaSkinConfigurationLookups.ColumnLineColour:
                    return SkinUtils.As<TValue>(getCustomColour(existing, "ColourColumnLine"));

                case LegacyManiaSkinConfigurationLookups.JudgementLineColour:
                    return SkinUtils.As<TValue>(getCustomColour(existing, "ColourJudgementLine"));

                case LegacyManiaSkinConfigurationLookups.ColumnBackgroundColour:
                    Debug.Assert(lookup.ColumnIndex != null);
                    return SkinUtils.As<TValue>(getCustomColour(existing, $"Colour{lookup.ColumnIndex + 1}"));

                case LegacyManiaSkinConfigurationLookups.ColumnLightColour:
                    Debug.Assert(lookup.ColumnIndex != null);
                    return SkinUtils.As<TValue>(getCustomColour(existing, $"ColourLight{lookup.ColumnIndex + 1}"));

                case LegacyManiaSkinConfigurationLookups.ComboBreakColour:
                    return SkinUtils.As<TValue>(getCustomColour(existing, "ColourBreak"));

                case LegacyManiaSkinConfigurationLookups.MinimumColumnWidth:
                    return SkinUtils.As<TValue>(new Bindable<float>(existing.MinimumColumnWidth));

                case LegacyManiaSkinConfigurationLookups.NoteBodyStyle:

                    if (existing.NoteBodyStyle != null)
                        return SkinUtils.As<TValue>(new Bindable<LegacyNoteBodyStyle>(existing.NoteBodyStyle.Value));

                    if (Get<decimal>(SkinConfiguration.LegacySetting.Version)?.Value < 2.5m)
                        return SkinUtils.As<TValue>(new Bindable<LegacyNoteBodyStyle>(LegacyNoteBodyStyle.Stretch));

                    return SkinUtils.As<TValue>(new Bindable<LegacyNoteBodyStyle>(LegacyNoteBodyStyle.RepeatBottom));

                case LegacyManiaSkinConfigurationLookups.NoteImage:
                    Debug.Assert(lookup.ColumnIndex != null);
                    return SkinUtils.As<TValue>(getManiaImage(existing, $"NoteImage{lookup.ColumnIndex}"));

                case LegacyManiaSkinConfigurationLookups.HoldNoteHeadImage:
                    Debug.Assert(lookup.ColumnIndex != null);
                    return SkinUtils.As<TValue>(getManiaImage(existing, $"NoteImage{lookup.ColumnIndex}H"));

                case LegacyManiaSkinConfigurationLookups.HoldNoteTailImage:
                    Debug.Assert(lookup.ColumnIndex != null);
                    return SkinUtils.As<TValue>(getManiaImage(existing, $"NoteImage{lookup.ColumnIndex}T"));

                case LegacyManiaSkinConfigurationLookups.HoldNoteBodyImage:
                    Debug.Assert(lookup.ColumnIndex != null);
                    return SkinUtils.As<TValue>(getManiaImage(existing, $"NoteImage{lookup.ColumnIndex}L"));

                case LegacyManiaSkinConfigurationLookups.HoldNoteLightImage:
                    return SkinUtils.As<TValue>(getManiaImage(existing, "LightingL"));

                case LegacyManiaSkinConfigurationLookups.HoldNoteLightScale:
                    Debug.Assert(lookup.ColumnIndex != null);

                    if (Get<decimal>(SkinConfiguration.LegacySetting.Version)?.Value < 2.5m)
                        return SkinUtils.As<TValue>(new Bindable<float>(1));

                    if (existing.HoldNoteLightWidth[lookup.ColumnIndex.Value] != 0)
                        return SkinUtils.As<TValue>(new Bindable<float>(existing.HoldNoteLightWidth[lookup.ColumnIndex.Value] / LegacyManiaSkinConfiguration.DEFAULT_COLUMN_SIZE));

                    return SkinUtils.As<TValue>(new Bindable<float>(existing.ColumnWidth[lookup.ColumnIndex.Value] / LegacyManiaSkinConfiguration.DEFAULT_COLUMN_SIZE));

                case LegacyManiaSkinConfigurationLookups.KeyImage:
                    Debug.Assert(lookup.ColumnIndex != null);
                    return SkinUtils.As<TValue>(getManiaImage(existing, $"KeyImage{lookup.ColumnIndex}"));

                case LegacyManiaSkinConfigurationLookups.KeyImageDown:
                    Debug.Assert(lookup.ColumnIndex != null);
                    return SkinUtils.As<TValue>(getManiaImage(existing, $"KeyImage{lookup.ColumnIndex}D"));

                case LegacyManiaSkinConfigurationLookups.LeftStageImage:
                    return SkinUtils.As<TValue>(getManiaImage(existing, "StageLeft"));

                case LegacyManiaSkinConfigurationLookups.RightStageImage:
                    return SkinUtils.As<TValue>(getManiaImage(existing, "StageRight"));

                case LegacyManiaSkinConfigurationLookups.BottomStageImage:
                    return SkinUtils.As<TValue>(getManiaImage(existing, "StageBottom"));

                case LegacyManiaSkinConfigurationLookups.LightImage:
                    return SkinUtils.As<TValue>(getManiaImage(existing, "StageLight"));

                case LegacyManiaSkinConfigurationLookups.HitTargetImage:
                    return SkinUtils.As<TValue>(getManiaImage(existing, "StageHint"));

                case LegacyManiaSkinConfigurationLookups.LeftLineWidth:
                    Debug.Assert(lookup.ColumnIndex != null);
                    return SkinUtils.As<TValue>(new Bindable<float>(existing.ColumnLineWidth[lookup.ColumnIndex.Value]));

                case LegacyManiaSkinConfigurationLookups.RightLineWidth:
                    Debug.Assert(lookup.ColumnIndex != null);
                    return SkinUtils.As<TValue>(new Bindable<float>(existing.ColumnLineWidth[lookup.ColumnIndex.Value + 1]));

                case LegacyManiaSkinConfigurationLookups.Hit0:
                case LegacyManiaSkinConfigurationLookups.Hit50:
                case LegacyManiaSkinConfigurationLookups.Hit100:
                case LegacyManiaSkinConfigurationLookups.Hit200:
                case LegacyManiaSkinConfigurationLookups.Hit300:
                case LegacyManiaSkinConfigurationLookups.Hit300g:
                    return SkinUtils.As<TValue>(getManiaImage(existing, lookup.Lookup.ToString()));

                case LegacyManiaSkinConfigurationLookups.KeysUnderNotes:
                    return SkinUtils.As<TValue>(new Bindable<bool>(existing.KeysUnderNotes));

                case LegacyManiaSkinConfigurationLookups.LightFramePerSecond:
                    return SkinUtils.As<TValue>(new Bindable<int>(existing.LightFramePerSecond));
            }

            return null;
        }

        public virtual IBindable<TValue>? Get<TValue>(SkinConfiguration.LegacySetting lookup) where TValue : notnull
        {
            switch (lookup)
            {
                case SkinConfiguration.LegacySetting.Version:
                    return SkinUtils.As<TValue>(new Bindable<decimal>(Configuration.LegacyVersion ?? SkinConfiguration.LATEST_VERSION));

                case SkinConfiguration.LegacySetting.InputOverlayText:
                    return SkinUtils.As<TValue>(new Bindable<Colour4>(Configuration.CustomColours.TryGetValue(@"InputOverlayText", out var colour) ? colour : Colour4.Black));

                default:
                    try
                    {
                        if (Configuration.ConfigDictionary.TryGetValue(lookup.ToString(), out string? val))
                        {
                            // special case for handling skins which use 1 or 0 to signify a boolean state.
                            // ..or in some cases 2 (https://github.com/ppy/osu/issues/18579).
                            if (typeof(TValue) == typeof(bool))
                            {
                                val = bool.TryParse(val, out bool boolVal)
                                    ? Convert.ChangeType(boolVal, typeof(bool)).ToString()
                                    : Convert.ChangeType(Convert.ToInt32(val), typeof(bool)).ToString();
                            }

                            var bindable = new Bindable<TValue>();
                            if (val != null)
                                bindable.Parse(val, CultureInfo.InvariantCulture);
                            return bindable;
                        }
                    }
                    catch
                    {
                    }

                    return null;
            }
        }

        /// <summary>
        /// Retrieves the correct combo colour for a given colour index and information on the combo.
        /// </summary>
        /// <param name="source">The source to retrieve the combo colours from.</param>
        /// <param name="colourIndex">The preferred index for retrieving the combo colour with.</param>
        /// <param name="combo">Information on the combo whose using the returned colour.</param>
        protected virtual IBindable<Color4>? GetComboColour(IHasComboColours source, int colourIndex, IHasComboInformation combo)
        {
            var colour = source.ComboColours?[colourIndex % source.ComboColours.Count];
            return colour.HasValue ? new Bindable<Color4>(colour.Value) : null;
        }

        private IBindable<Color4>? getCustomColour(IHasCustomColours source, string lookup)
            => source.CustomColours.TryGetValue(lookup, out var col) ? new Bindable<Color4>(col) : null;

        private IBindable<string>? getManiaImage(LegacyManiaSkinConfiguration source, string lookup)
            => source.ImageLookups.TryGetValue(lookup, out string? image) ? new Bindable<string>(image) : null;

        private Texture? getParticleTexture(HitResult result)
        {
            switch (result)
            {
                case HitResult.Meh:
                    return this.GetTexture("particle50");

                case HitResult.Ok:
                    return this.GetTexture("particle100");

                case HitResult.Great:
                    return this.GetTexture("particle300");
            }

            return null;
        }

        private Drawable? getJudgementAnimation(HitResult result)
        {
            switch (result)
            {
                case HitResult.Miss:
                    return this.GetAnimation("hit0", true, false);

                case HitResult.LargeTickMiss:
                    return this.GetAnimation("slidertickmiss", true, false);

                case HitResult.IgnoreMiss:
                    return this.GetAnimation("sliderendmiss", true, false);

                case HitResult.Meh:
                    return this.GetAnimation("hit50", true, false);

                case HitResult.Ok:
                    return this.GetAnimation("hit100", true, false);

                case HitResult.Great:
                    return this.GetAnimation("hit300", true, false);
            }

            return null;
        }

        private IEnumerable<string> getLegacyLookupNames(HitSampleInfo hitSample)
        {
            var lookupNames = hitSample.LookupNames.SelectMany(getFallbackSampleNames);

            if (!UseCustomSampleBanks && !string.IsNullOrEmpty(hitSample.Suffix))
            {
                // for compatibility with stable, exclude the lookup names with the custom sample bank suffix, if they are not valid for use in this skin.
                // using .EndsWith() is intentional as it ensures parity in all edge cases
                // (see LegacyTaikoSampleInfo for an example of one - prioritising the taiko prefix should still apply, but the sample bank should not).
                lookupNames = lookupNames.Where(name => !name.EndsWith(hitSample.Suffix, StringComparison.Ordinal));
            }

            foreach (string l in lookupNames)
                yield return l;

            // also for compatibility, try falling back to non-bank samples (so-called "universal" samples) as the last resort.
            // going forward specifying banks shall always be required, even for elements that wouldn't require it on stable,
            // which is why this is done locally here.
            yield return hitSample.Name;
        }

        private IEnumerable<string> getFallbackSampleNames(string name)
        {
            // May be something like "Gameplay/normal-hitnormal" from lazer.
            yield return name;

            // Fall back to using the last piece for components coming from lazer (e.g. "Gameplay/normal-hitnormal" -> "normal-hitnormal").
            yield return name.Split('/').Last();
        }
    }
}
