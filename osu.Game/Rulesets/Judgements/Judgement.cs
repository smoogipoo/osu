﻿// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Scoring;

namespace osu.Game.Rulesets.Judgements
{
    /// <summary>
    /// The scoring information provided by a <see cref="HitObject"/>.
    /// </summary>
    public class Judgement
    {
        public const double SMALL_TICK_RESULT = 10;

        public const double LARGE_TICK_RESULT = 30;

        public const double SMALL_BONUS_RESULT = 10;

        public const double LARGE_BONUS_RESULT = 50;

        /// <summary>
        /// The default health increase for a maximum judgement, as a proportion of total health.
        /// By default, each maximum judgement restores 5% of total health.
        /// </summary>
        protected const double DEFAULT_MAX_HEALTH_INCREASE = 0.05;

        /// <summary>
        /// The maximum <see cref="HitResult"/> that can be achieved.
        /// </summary>
        public virtual HitResult MaxResult => HitResult.Perfect;

        /// <summary>
        /// The minimum <see cref="HitResult"/> that can be achieved.
        /// </summary>
        public virtual HitResult MinResult => HitResult.Miss;

        /// <summary>
        /// Whether this <see cref="Judgement"/> should affect the current combo.
        /// </summary>
        public virtual bool AffectsCombo => true;

        /// <summary>
        /// Whether this <see cref="Judgement"/> should be counted as base (combo) or bonus score.
        /// </summary>
        public virtual bool IsBonus => !AffectsCombo;

        public virtual bool IncreaseScore => true;

        /// <summary>
        /// The numeric score representation for the maximum achievable result.
        /// </summary>
        public double MaxNumericResult => NumericResultFor(MaxResult);

        /// <summary>
        /// The health increase for the maximum achievable result.
        /// </summary>
        public double MaxHealthIncrease => HealthIncreaseFor(MaxResult);

        /// <summary>
        /// Retrieves the numeric score representation of a <see cref="HitResult"/>.
        /// </summary>
        /// <param name="result">The <see cref="HitResult"/> to find the numeric score representation for.</param>
        /// <returns>The numeric score representation of <paramref name="result"/>.</returns>
        protected double NumericResultFor(HitResult result)
        {
            switch (result)
            {
                default:
                    return 0;

                case HitResult.SmallTickHit:
                    return SMALL_TICK_RESULT;

                case HitResult.LargeTickHit:
                    return LARGE_TICK_RESULT;

                case HitResult.SmallBonus:
                    return SMALL_BONUS_RESULT;

                case HitResult.LargeBonus:
                    return LARGE_BONUS_RESULT;

                case HitResult.Meh:
                    return 1 / 6d;

                case HitResult.Ok:
                    return 1 / 3d;

                case HitResult.Good:
                    return 2 / 3d;

                case HitResult.Great:
                    return 1d;

                case HitResult.Perfect:
                    return 7 / 6d;
            }
        }

        /// <summary>
        /// Retrieves the numeric score representation of a <see cref="JudgementResult"/>.
        /// </summary>
        /// <param name="result">The <see cref="JudgementResult"/> to find the numeric score representation for.</param>
        /// <returns>The numeric score representation of <paramref name="result"/>.</returns>
        public double NumericResultFor(JudgementResult result) => NumericResultFor(result.Type);

        /// <summary>
        /// Retrieves the numeric health increase of a <see cref="HitResult"/>.
        /// </summary>
        /// <param name="result">The <see cref="HitResult"/> to find the numeric health increase for.</param>
        /// <returns>The numeric health increase of <paramref name="result"/>.</returns>
        protected virtual double HealthIncreaseFor(HitResult result)
        {
            switch (result)
            {
                case HitResult.Miss:
                    return -DEFAULT_MAX_HEALTH_INCREASE;

                case HitResult.Meh:
                    return -DEFAULT_MAX_HEALTH_INCREASE * 0.05;

                case HitResult.Ok:
                    return -DEFAULT_MAX_HEALTH_INCREASE * 0.01;

                case HitResult.Good:
                    return DEFAULT_MAX_HEALTH_INCREASE * 0.5;

                case HitResult.Great:
                    return DEFAULT_MAX_HEALTH_INCREASE;

                case HitResult.Perfect:
                    return DEFAULT_MAX_HEALTH_INCREASE * 1.05;

                default:
                    return 0;
            }
        }

        /// <summary>
        /// Retrieves the numeric health increase of a <see cref="JudgementResult"/>.
        /// </summary>
        /// <param name="result">The <see cref="JudgementResult"/> to find the numeric health increase for.</param>
        /// <returns>The numeric health increase of <paramref name="result"/>.</returns>
        public double HealthIncreaseFor(JudgementResult result) => HealthIncreaseFor(result.Type);

        public override string ToString() => $"AffectsCombo:{AffectsCombo} MaxResult:{MaxResult} MaxScore:{MaxNumericResult}";
    }
}
