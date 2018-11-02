// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using OpenTK;

namespace osu.Game.Rulesets.Osu.Edit.Masks.SliderMasks.Components
{
    public class PathControlPointDescriptor
    {
        /// <summary>
        /// The segment which the control point lies in.
        /// </summary>
        public int SegmentIndex;

        /// <summary>
        /// The index of the control point in the segment.
        /// </summary>
        public int IndexInSegment;

        /// <summary>
        /// The control point value.
        /// </summary>
        public Vector2 ControlPoint;
    }
}
