// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System.Collections.Generic;
using OpenTK;
using OpenTK.Graphics;
using osu.Framework.Configuration;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Effects;
using osu.Framework.Graphics.Shapes;
using osu.Game.Rulesets.Edit.Layers.Selection;
using osu.Game.Rulesets.Objects.Drawables;

namespace osu.Game.Rulesets.Edit.Layers.Discussion
{
    public class DiscussionLayer : CompositeDrawable
    {
        private readonly Bindable<SelectionInfo> selection = new Bindable<SelectionInfo>();

        private readonly Container compositionBox;

        public DiscussionLayer(SelectionLayer selectionLayer)
        {
            RelativeSizeAxes = Axes.Both;

            selection.BindTo(selectionLayer.Selection);
            selection.ValueChanged += selectionChanged;

            InternalChild = compositionBox = new Container
            {
                Anchor = Anchor.TopLeft,
                Origin = Anchor.TopCentre,
                Alpha = 0,
                Size = new Vector2(500, 100),
                Child = createBox().WithEffect(new BlurEffect
                {
                    DrawOriginal = true,
                    Strength = 2,
                    Colour = Color4.Black,
                    PadExtent = true,
                })
            };
        }

        private Drawable createBox() => new Container
        {
            RelativeSizeAxes = Axes.Both,
            Children = new Drawable[]
            {
                new Triangle
                {
                    Anchor = Anchor.TopCentre,
                    Origin = Anchor.TopCentre,
                    Size = new Vector2(10, 8)
                },
                new Container
                {
                    RelativeSizeAxes = Axes.Both,
                    Padding = new MarginPadding { Top = 7 },
                    Child = new Container
                    {
                        RelativeSizeAxes = Axes.Both,
                        Masking = true,
                        CornerRadius = 4,
                        Child = new Box { RelativeSizeAxes = Axes.Both }
                    }
                }
            }
        };

        private void selectionChanged(SelectionInfo newSelection)
        {
            if (newSelection == null)
            {
                compositionBox.FadeOut(400, Easing.OutQuint);
                return;
            }

            var selectionQuad = Parent.ToLocalSpace(newSelection.SelectionQuad);
            var location = new Vector2(selectionQuad.Centre.X, selectionQuad.BottomLeft.Y);

            compositionBox.MoveTo(location).FadeIn(300, Easing.OutQuint);
        }
    }
}
