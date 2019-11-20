// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Input;
using osu.Framework.Input.Bindings;
using osu.Framework.Input.Events;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Objects.Types;
using osu.Game.Rulesets.Osu.Objects;
using osu.Game.Screens.Edit.Compose;
using osuTK;

namespace osu.Game.Rulesets.Osu.Edit.Blueprints.Sliders.Components
{
    public delegate void SegmentsChangedDelegate(PathSegment[] segments);

    public class PathControlPointVisualiser : CompositeDrawable, IKeyBindingHandler<PlatformAction>
    {
        public ControlPointsChangedDelegate ControlPointsChanged;
        public SegmentsChangedDelegate SegmentsChanged;

        internal readonly Container<PathControlPointPiece> Pieces;
        private readonly Slider slider;
        private readonly bool allowSelection;

        private InputManager inputManager;

        [Resolved(CanBeNull = true)]
        private IPlacementHandler placementHandler { get; set; }

        public PathControlPointVisualiser(Slider slider, bool allowSelection)
        {
            this.slider = slider;
            this.allowSelection = allowSelection;

            RelativeSizeAxes = Axes.Both;

            InternalChild = Pieces = new Container<PathControlPointPiece> { RelativeSizeAxes = Axes.Both };
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            inputManager = GetContainingInputManager();
        }

        protected override void Update()
        {
            base.Update();

            int totalControlPoints = 0;

            for (int s = 0; s < slider.Path.Segments.Length; s++)
            {
                PathSegment segment = slider.Path.Segments[s];

                for (int c = 0; c < segment.ControlPoints.Length; c++)
                {
                    totalControlPoints++;

                    if (totalControlPoints > Pieces.Count)
                    {
                        var piece = new PathControlPointPiece(slider)
                        {
                            ControlPointsChanged = (segmentIndex, points) => ControlPointsChanged?.Invoke(segmentIndex, points),
                        };

                        if (allowSelection)
                            piece.RequestSelection = selectPiece;

                        Pieces.Add(piece);
                    }

                    Pieces[totalControlPoints - 1].SegmentIndex = s;
                    Pieces[totalControlPoints - 1].ControlPointIndex = c;
                }
            }

            while (totalControlPoints < Pieces.Count)
                Pieces.Remove(Pieces[Pieces.Count - 1]);
        }

        protected override bool OnClick(ClickEvent e)
        {
            foreach (var piece in Pieces)
                piece.IsSelected.Value = false;
            return false;
        }

        private void selectPiece(int segmentIndex, int controlPointIndex)
        {
            if (inputManager.CurrentState.Keyboard.ControlPressed)
                Pieces.Single(p => p.SegmentIndex == segmentIndex && p.ControlPointIndex == controlPointIndex).IsSelected.Toggle();
            else
            {
                foreach (var piece in Pieces)
                    piece.IsSelected.Value = piece.SegmentIndex == segmentIndex && piece.ControlPointIndex == controlPointIndex;
            }
        }

        public bool OnPressed(PlatformAction action)
        {
            switch (action.ActionMethod)
            {
                case PlatformActionMethod.Delete:
                    var newSegments = new List<PathSegment>(slider.Path.Segments.ToArray());

                    bool anyDeleted = false;
                    Vector2 offset = Vector2.Zero;

                    for (int s = 0; s < newSegments.Count; s++)
                    {
                        // Find the new control points for this segment by going through all the non-selected pieces
                        Vector2[] newControlPoints = Pieces.Where(p => p.SegmentIndex == s && !p.IsSelected.Value)
                                                           .Select(p => newSegments[s].ControlPoints[p.ControlPointIndex])
                                                           .ToArray();

                        // Make sure any control points were altered before continuing
                        if (newControlPoints.Length == newSegments[s].ControlPoints.Length)
                            continue;

                        anyDeleted = true;

                        // Remove segments with 0 remaining control points
                        if (newControlPoints.Length == 0)
                        {
                            newSegments.RemoveAt(s--);
                            continue;
                        }

                        // If the first segment is altered, it may be required to bring all other control points relative to the first control point
                        // We're iterating through the segments from first to last so this will always touch the first segment first before the offset is applied to other segments
                        if (s == 0)
                            offset = newControlPoints[0];
                        for (int c = 0; c < newControlPoints.Length; c++)
                            newControlPoints[c] = newControlPoints[c] - offset;

                        PathType newType = newSegments[s].Type;

                        switch (newType)
                        {
                            case PathType.PerfectCurve when newControlPoints.Length != 3:
                                newType = PathType.Linear;
                                break;
                        }

                        newSegments[s] = new PathSegment(newType, newControlPoints);
                    }

                    if (!anyDeleted)
                        return false;

                    // Delete the slider if there are no remaining segments
                    if (newSegments.Count == 0)
                    {
                        placementHandler?.Delete(slider);
                        return true;
                    }

                    // In case the first control point was deleted, the slider position must match the new first control point position
                    slider.Position = slider.Position + offset;

                    // Since pieces are re-used, they will not point to the deleted control points while remaining selected
                    foreach (var piece in Pieces)
                        piece.IsSelected.Value = false;

                    SegmentsChanged?.Invoke(newSegments.ToArray());

                    return true;
            }

            return false;
        }

        public bool OnReleased(PlatformAction action) => action.ActionMethod == PlatformActionMethod.Delete;
    }
}
