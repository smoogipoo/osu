// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Game.Rulesets.Judgements;
using osu.Game.Rulesets.Objects.Drawables;
using osu.Game.Skinning;
using osuTK.Graphics;

namespace osu.Game.Rulesets.Osu.Objects.Drawables
{
    public class SkinnableLighting : SkinnableSprite
    {
        private DrawableHitObject targetObject;
        private JudgementResult targetResult;

        public SkinnableLighting()
            : base("lighting")
        {
        }

        protected override void SkinChanged(ISkinSource skin, bool allowFallback)
        {
            base.SkinChanged(skin, allowFallback);
            updateColour();
        }

        public void SetColourFrom(DrawableHitObject targetObject, JudgementResult targetResult)
        {
            this.targetObject = targetObject;
            this.targetResult = targetResult;

            updateColour();
        }

        private void updateColour()
        {
            if (targetObject == null || targetResult == null)
                Colour = Color4.White;
            else
                Colour = targetResult.IsHit ? targetObject.AccentColour.Value : Color4.Transparent;
        }
    }
}
