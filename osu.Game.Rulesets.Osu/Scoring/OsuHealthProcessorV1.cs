// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Linq;
using osu.Game.Beatmaps;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Osu.Objects;
using osu.Game.Rulesets.Scoring;

namespace osu.Game.Rulesets.Osu.Scoring
{
    public partial class OsuHealthProcessorV1 : DrainingHealthProcessorV1
    {
        private const double hp_bar_maximum = 200;
        private const double hp_combo_geki = 14;
        private const double hp_hit_300 = 6;
        private const double hp_slider_repeat = 4;
        private const double hp_slider_tick = 3;

        private double lowestHpEver;
        private double lowestHpEnd;
        private double lowestHpComboEnd;
        private double hpRecoveryAvailable;
        private double hpMultiplierNormal;
        private double hpMultiplierComboEnd;

        public OsuHealthProcessorV1(double drainStartTime)
            : base(drainStartTime)
        {
        }

        public override void ApplyBeatmap(IBeatmap beatmap)
        {
            lowestHpEver = IBeatmapDifficultyInfo.DifficultyRange(beatmap.Difficulty.DrainRate, 195, 160, 60);
            lowestHpComboEnd = IBeatmapDifficultyInfo.DifficultyRange(beatmap.Difficulty.DrainRate, 198, 170, 80);
            lowestHpEnd = IBeatmapDifficultyInfo.DifficultyRange(beatmap.Difficulty.DrainRate, 198, 180, 80);
            hpRecoveryAvailable = IBeatmapDifficultyInfo.DifficultyRange(beatmap.Difficulty.DrainRate, 8, 4, 0);

            base.ApplyBeatmap(beatmap);
        }

        protected override void Reset(bool storeResults)
        {
            hpMultiplierNormal = 1;
            hpMultiplierComboEnd = 1;

            base.Reset(storeResults);
        }

        protected override double ComputeDrainRate()
        {
            double testDrop = 0.05;
            double currentHp;
            double currentHpUncapped;

#if LOGGING
            int iteration = 1;
#endif

            do
            {
#if LOGGING
                Console.WriteLine($"V1 testing drop {testDrop / 200} (i = {iteration++})...");
#endif

                currentHp = hp_bar_maximum;
                currentHpUncapped = hp_bar_maximum;

                double lowestHp = currentHp;
                double lastTime = DrainStartTime;
                int currentBreak = 0;
                bool fail = false;
                int comboTooLowCount = 0;

                int i = 0;

                for (; i < Beatmap.HitObjects.Count; i++)
                {
                    HitObject h = Beatmap.HitObjects[i];

                    // Subtract any break time from the duration since the last object
                    if (Beatmap.Breaks.Count > 0 && currentBreak < Beatmap.Breaks.Count)
                    {
                        while (currentBreak + 1 < Beatmap.Breaks.Count && Beatmap.Breaks[currentBreak + 1].EndTime < h.StartTime)
                            currentBreak++;

                        if (currentBreak >= 0)
                            lastTime = Math.Max(lastTime, Beatmap.Breaks[currentBreak].EndTime);
                    }

                    reduceHp(testDrop * (h.StartTime - lastTime));

                    lastTime = h.GetEndTime();

                    if (currentHp < lowestHp)
                        lowestHp = currentHp;

                    if (currentHp <= lowestHpEver)
                    {
                        fail = true;
                        testDrop *= 0.96;
                        break;
                    }

                    double hpReduction = testDrop * (h.GetEndTime() - h.StartTime);
                    double hpOverkill = Math.Max(0, hpReduction - currentHp);
                    reduceHp(hpReduction);

                    if (h is Slider slider)
                    {
                        for (int j = 0; j < slider.RepeatCount + 2; j++)
                            increaseHp(hpMultiplierNormal * hp_slider_repeat);
                        foreach (var _ in slider.NestedHitObjects.OfType<SliderTick>())
                            increaseHp(hpMultiplierNormal * hp_slider_tick);
                    }
                    else if (h is Spinner spinner)
                    {
                        foreach (var _ in spinner.NestedHitObjects.Where(t => t is not SpinnerBonusTick))
                            increaseHp(hpMultiplierNormal * 1.7);
                    }

                    if (hpOverkill > 0 && currentHp - hpOverkill <= lowestHpEver)
                    {
                        fail = true;
                        testDrop *= 0.96;
                        break;
                    }

                    if (i == Beatmap.HitObjects.Count - 1 || ((OsuHitObject)Beatmap.HitObjects[i + 1]).NewCombo)
                    {
                        increaseHp(hpMultiplierComboEnd * hp_combo_geki + hpMultiplierNormal * hp_hit_300);

                        if (currentHp < lowestHpComboEnd)
                        {
                            if (++comboTooLowCount > 2)
                            {
                                hpMultiplierComboEnd *= 1.07;
                                hpMultiplierNormal *= 1.03;
                                fail = true;
                                break;
                            }
                        }
                    }
                    else
                        increaseHp(hpMultiplierNormal * hp_hit_300);
                }

                if (!fail && currentHp < lowestHpEnd)
                {
                    fail = true;
                    testDrop *= 0.94;
                    hpMultiplierComboEnd *= 1.01;
                    hpMultiplierNormal *= 1.01;
                }

                double recovery = (currentHpUncapped - hp_bar_maximum) / Beatmap.HitObjects.Count;

                if (!fail && recovery < hpRecoveryAvailable)
                {
                    fail = true;
                    testDrop *= 0.96;
                    hpMultiplierComboEnd *= 1.02;
                    hpMultiplierNormal *= 1.01;
                }

                if (fail)
                {
#if LOGGING
                    Console.WriteLine($"V1 failed at {Beatmap.HitObjects[i].StartTime}");
#endif
                    continue;
                }

                return testDrop / hp_bar_maximum;
            } while (true);

            void reduceHp(double amount)
            {
                currentHpUncapped = Math.Max(0, currentHpUncapped - amount);
                currentHp = Math.Max(0, currentHp - amount);
            }

            void increaseHp(double amount)
            {
                currentHpUncapped += amount;
                currentHp = Math.Max(0, Math.Min(hp_bar_maximum, currentHp + amount));
            }
        }
    }
}
