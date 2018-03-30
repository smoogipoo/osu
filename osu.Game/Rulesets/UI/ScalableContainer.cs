// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using OpenTK;

namespace osu.Game.Rulesets.UI
{
    /// <summary>
    /// A <see cref="Container"/> which can have its internal coordinate system scaled to a specific size.
    /// </summary>
    public class ScalableContainer : Container
    {
        /// <summary>
        /// A function that converts coordinates from gamefield to screen space.
        /// </summary>
        public Func<Vector2, Vector2> ToScaledScreenSpace => scaledContainer.ToScaledScreenSpace;

        private readonly ScaledContainer scaledContainer;
        protected override Container<Drawable> Content => scaledContainer;

        /// <summary>
        /// A <see cref="Container"/> which can have its internal coordinate system scaled to a specific size.
        /// </summary>
        /// <param name="customWidth">The width to scale the internal coordinate space to.
        /// May be null if scaling based on <paramref name="customHeight"/> is desired. If <paramref name="customHeight"/> is also null, no scaling will occur.
        /// </param>
        /// <param name="customHeight">The height to scale the internal coordinate space to.
        /// May be null if scaling based on <paramref name="customWidth"/> is desired. If <paramref name="customWidth"/> is also null, no scaling will occur.
        /// </param>
        public ScalableContainer(float? customWidth = null, float? customHeight = null)
        {
            AddInternal(scaledContainer = new ScaledContainer
            {
                CustomWidth = customWidth,
                CustomHeight = customHeight,
                Strategy = DrawSizePreservationStrategy.Minimum,
                RelativeSizeAxes = Axes.Both,
            });
        }

        private class ScaledContainer : DrawSizePreservingFillContainer
        {
            /// <summary>
            /// The value to scale the width of the content to match.
            /// If null, <see cref="CustomHeight"/> is used.
            /// </summary>
            public float? CustomWidth;

            /// <summary>
            /// The value to scale the height of the content to match.
            /// if null, <see cref="CustomWidth"/> is used.
            /// </summary>
            public float? CustomHeight;

            /// <summary>
            /// A function that converts coordinates from gamefield to screen space.
            /// </summary>
            public Func<Vector2, Vector2> ToScaledScreenSpace => Content.ToScreenSpace;

            protected override void Update()
            {
                // Must be set before Update()
                TargetDrawSize = new Vector2(CustomWidth ?? DrawWidth, CustomHeight ?? DrawHeight);

                base.Update();

                // Put the origin of the scaled content container where our origin is
                Content.OriginPosition = TargetDrawSize * RelativeOriginPosition;
                Content.Anchor = Origin;
                Content.RelativeChildSize = Content.Scale;
            }
        }
    }
}
