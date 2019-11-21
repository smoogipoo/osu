// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Lines;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Input.Events;
using osu.Game.Rulesets.Osu.Objects;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Rulesets.Osu.Edit.Blueprints.Sliders.Components
{
    public abstract class PathControlPointPiece : BlueprintPiece<Slider>
    {
        public RequestSelectionDelegate RequestSelection;
        public ControlPointsChangedDelegate ControlPointsChanged;

        public readonly BindableBool IsSelected = new BindableBool();

        protected readonly Slider Slider;
        protected readonly Path Path;

        protected readonly Container Marker;
        protected readonly Drawable MarkerRing;

        private readonly bool allowSelection;

        protected PathControlPointPiece(Slider slider, bool allowSelection)
        {
            Slider = slider;
            this.allowSelection = allowSelection;

            Origin = Anchor.Centre;
            AutoSizeAxes = Axes.Both;

            InternalChildren = new Drawable[]
            {
                Path = new SmoothPath
                {
                    Anchor = Anchor.Centre,
                    PathRadius = 1
                },
                Marker = new Container
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    AutoSizeAxes = Axes.Both,
                    Children = new[]
                    {
                        new Circle
                        {
                            Anchor = Anchor.Centre,
                            Origin = Anchor.Centre,
                            Size = new Vector2(10),
                        },
                        MarkerRing = new CircularContainer
                        {
                            Anchor = Anchor.Centre,
                            Origin = Anchor.Centre,
                            Size = new Vector2(14),
                            Masking = true,
                            BorderThickness = 2,
                            BorderColour = Color4.White,
                            Alpha = 0,
                            Child = new Box
                            {
                                RelativeSizeAxes = Axes.Both,
                                Alpha = 0,
                                AlwaysPresent = true
                            }
                        }
                    }
                }
            };
        }

        protected override void Update()
        {
            base.Update();

            Position = GetPosition();

            updateMarkerDisplay();
        }

        /// <summary>
        /// Updates the state of the circular control point marker.
        /// </summary>
        private void updateMarkerDisplay()
        {
            MarkerRing.Alpha = IsSelected.Value ? 1 : 0;

            Color4 colour = GetColour();
            if (IsHovered || IsSelected.Value)
                colour = Color4.White;
            Marker.Colour = colour;
        }

        protected abstract Vector2 GetPosition();

        protected abstract Color4 GetColour();

        // The connecting path is excluded from positional input
        public override bool ReceivePositionalInputAt(Vector2 screenSpacePos) => Marker.ReceivePositionalInputAt(screenSpacePos);

        protected override bool OnMouseDown(MouseDownEvent e)
        {
            if (allowSelection)
            {
                RequestSelection.Invoke(this);
                return true;
            }

            return false;
        }

        protected override bool OnMouseUp(MouseUpEvent e) => allowSelection;

        protected override bool OnClick(ClickEvent e) => allowSelection;

        protected override bool OnDragStart(DragStartEvent e) => true;

        protected override bool OnDrag(DragEvent e) => true;

        protected override bool OnDragEnd(DragEndEvent e) => true;

        protected void SetControlPoints(int segmentIndex, Vector2[] controlPoints) => ControlPointsChanged?.Invoke(segmentIndex, controlPoints);

        protected ReadOnlySpan<Vector2> GetControlPoints(int segmentIndex) => Slider.Path.Segments[segmentIndex].ControlPoints;
    }
}
