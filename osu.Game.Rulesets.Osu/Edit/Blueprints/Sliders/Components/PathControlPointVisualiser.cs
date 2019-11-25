// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using Humanizer;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Cursor;
using osu.Framework.Graphics.UserInterface;
using osu.Framework.Input;
using osu.Framework.Input.Bindings;
using osu.Framework.Input.Events;
using osu.Game.Graphics.UserInterface;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Objects.Types;
using osu.Game.Rulesets.Osu.Objects;
using osu.Game.Screens.Edit.Compose;
using osuTK;
using osuTK.Input;

namespace osu.Game.Rulesets.Osu.Edit.Blueprints.Sliders.Components
{
    public class PathControlPointVisualiser : CompositeDrawable, IKeyBindingHandler<PlatformAction>, IHasContextMenu
    {
        public ControlPointsChangedDelegate ControlPointsChanged;
        public SegmentsChangedDelegate SegmentsChanged;

        internal readonly Container<SegmentControlPointPiece> SegmentPieces;
        internal readonly HeadControlPointPiece HeadPiece;

        private readonly Slider slider;
        private readonly bool allowSelection;

        [Resolved(CanBeNull = true)]
        private IPlacementHandler placementHandler { get; set; }

        public PathControlPointVisualiser(Slider slider, bool allowSelection)
        {
            this.slider = slider;
            this.allowSelection = allowSelection;

            RelativeSizeAxes = Axes.Both;

            InternalChildren = new Drawable[]
            {
                HeadPiece = new HeadControlPointPiece(slider, allowSelection)
                {
                    ControlPointsChanged = (segmentIndex, points) => ControlPointsChanged?.Invoke(segmentIndex, points),
                    RequestSelection = selectPiece
                },
                SegmentPieces = new Container<SegmentControlPointPiece> { RelativeSizeAxes = Axes.Both }
            };
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

                    if (totalControlPoints > SegmentPieces.Count)
                    {
                        SegmentPieces.Add(new SegmentControlPointPiece(slider, allowSelection)
                        {
                            ControlPointsChanged = (segmentIndex, points) => ControlPointsChanged?.Invoke(segmentIndex, points),
                            RequestSelection = selectPiece
                        });
                    }

                    SegmentPieces[totalControlPoints - 1].SegmentIndex = s;
                    SegmentPieces[totalControlPoints - 1].ControlPointIndex = c;
                }
            }

            while (totalControlPoints < SegmentPieces.Count)
                SegmentPieces.Remove(SegmentPieces[SegmentPieces.Count - 1]);
        }

        protected override bool OnClick(ClickEvent e)
        {
            deselectAll();
            return false;
        }

        public bool OnPressed(PlatformAction action)
        {
            switch (action.ActionMethod)
            {
                case PlatformActionMethod.Delete:
                    return deleteSelected();
            }

            return false;
        }

        public bool OnReleased(PlatformAction action) => action.ActionMethod == PlatformActionMethod.Delete;

        private void selectPiece(PathControlPointPiece piece, MouseButtonEvent e)
        {
            if (e.Button != MouseButton.Left || !e.ControlPressed)
            {
                // Multiple selection disallowed - reset the selection before proceeding to a new one
                deselectAll();
            }

            piece.IsSelected.Toggle();
        }

        private void deselectAll()
        {
            HeadPiece.IsSelected.Value = false;

            foreach (var piece in SegmentPieces)
                piece.IsSelected.Value = false;
        }

        private bool deleteSelected()
        {
            PathSegment[] segments = slider.Path.Segments.ToArray();

            bool anyRemoved = false;

            if (HeadPiece.IsSelected.Value)
            {
                removeHead(segments);
                anyRemoved = true;
            }

            foreach (var piece in SegmentPieces.Where(p => p.IsSelected.Value))
            {
                removePoint(segments, piece.SegmentIndex, piece.ControlPointIndex);
                anyRemoved = true;
            }

            // If no control points were removed, other components should be able to handle the deletion
            if (!anyRemoved)
                return false;

            removeEmptySegments(ref segments);

            deselectAll();

            if (segments.Length == 0)
            {
                // Special case for when all control points are deleted
                placementHandler?.Delete(slider);
                return true;
            }

            SegmentsChanged?.Invoke(segments.ToArray());

            return true;
        }

        private void removeHead(PathSegment[] segments)
        {
            Vector2 offset = segments[0].ControlPoints[0];

            // Offset the slider position
            slider.Position += offset;

            removePoint(segments, 0, 0);
            offsetPoints(segments, -offset);
        }

        private void removePoint(PathSegment[] segments, int segmentIndex, int controlPointIndex)
        {
            PathType newType = segments[segmentIndex].Type == PathType.PerfectCurve ? PathType.Linear : segments[segmentIndex].Type;

            segments[segmentIndex] = new PathSegment(newType, Enumerable.Range(0, segments[segmentIndex].ControlPoints.Length)
                                                                        .Where(i => i != controlPointIndex)
                                                                        .Select(i => segments[segmentIndex].ControlPoints[i]).ToArray());
        }

        private void offsetPoints(PathSegment[] segments, Vector2 offset)
        {
            for (int s = 0; s < segments.Length; s++)
            {
                segments[s] = new PathSegment(segments[s].Type, Enumerable.Range(0, segments[s].ControlPoints.Length)
                                                                          .Select(i => segments[s].ControlPoints[i] + offset).ToArray());
            }
        }

        private void removeEmptySegments(ref PathSegment[] segments) => segments = segments.Where(s => s.ControlPoints.Length > 0).ToArray();

        public MenuItem[] ContextMenuItems
        {
            get
            {
                if (!HeadPiece.IsHovered && !SegmentPieces.Any(p => p.IsHovered))
                    return null;

                int selectedPoints = (HeadPiece.IsSelected.Value ? 1 : 0) + SegmentPieces.Count(p => p.IsSelected.Value);

                if (selectedPoints == 0)
                    return null;

                return new MenuItem[]
                {
                    new OsuMenuItem($"Delete {"control point".ToQuantity(selectedPoints)}", MenuItemType.Destructive, () => deleteSelected())
                };
            }
        }
    }

    public delegate void SegmentsChangedDelegate(PathSegment[] segments);

    public delegate void RequestSelectionDelegate(PathControlPointPiece piece, MouseButtonEvent e);

    public delegate void ControlPointsChangedDelegate(int segmentIndex, Vector2[] controlPoints);
}
