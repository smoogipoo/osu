// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System.Linq;
using osu.Framework.Extensions.IEnumerableExtensions;
using osu.Framework.Graphics;
using osu.Game.Rulesets.Mania.Objects.Drawables.Pieces;
using OpenTK.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Input.Bindings;
using osu.Game.Rulesets.Scoring;
using osu.Game.Rulesets.UI.Scrolling;

namespace osu.Game.Rulesets.Mania.Objects.Drawables
{
    /// <summary>
    /// Visualises a <see cref="HoldNote"/> hit object.
    /// </summary>
    public class DrawableHoldNote : DrawableManiaHitObject<HoldNote>, IKeyBindingHandler<ManiaAction>
    {
        /// <summary>
        /// Lenience of release hit windows. This is to make cases where the hold note release
        /// is timed alongside presses of other hit objects less awkward.
        /// Todo: This shouldn't exist for non-LegacyBeatmapDecoder beatmaps
        /// </summary>
        private const double release_window_lenience = 1.5;

        public override bool DisplayResult => false;

        public readonly DrawableHoldNoteNote Head;
        public readonly DrawableHoldNoteNote Tail;

        private readonly BodyPiece bodyPiece;

        private bool isHolding;
        private bool hasBroken;

        private readonly Container<DrawableHoldNoteTick> tickContainer;

        public DrawableHoldNote(HoldNote hitObject)
            : base(hitObject)
        {
            RelativeSizeAxes = Axes.X;

            InternalChildren = new Drawable[]
            {
                bodyPiece = new BodyPiece
                {
                    RelativeSizeAxes = Axes.X,
                },
                tickContainer = new Container<DrawableHoldNoteTick>
                {
                    RelativeSizeAxes = Axes.Both,
                    ChildrenEnumerable = HitObject.NestedHitObjects.OfType<HoldNoteTick>().Select(tick => new DrawableHoldNoteTick(tick))
                },
                Head = new DrawableHoldNoteNote(hitObject.Head)
                {
                    Anchor = Anchor.TopCentre,
                    Origin = Anchor.TopCentre
                },
                Tail = new DrawableHoldNoteNote(hitObject.Tail)
                {
                    Anchor = Anchor.TopCentre,
                    Origin = Anchor.TopCentre
                }
            };

            foreach (var tick in tickContainer)
                AddNested(tick);

            AddNested(Head);
            AddNested(Tail);
        }

        protected override void OnDirectionChanged(ScrollingDirection direction)
        {
            base.OnDirectionChanged(direction);

            bodyPiece.Anchor = bodyPiece.Origin = direction == ScrollingDirection.Up ? Anchor.TopLeft : Anchor.BottomLeft;
        }

        public override Color4 AccentColour
        {
            get { return base.AccentColour; }
            set
            {
                base.AccentColour = value;

                bodyPiece.AccentColour = value;
                Head.AccentColour = value;
                Tail.AccentColour = value;
                tickContainer.ForEach(t => t.AccentColour = value);
            }
        }

        protected override void Update()
        {
            base.Update();

            // Make the body piece not lie under the head note
            bodyPiece.Y = (Direction.Value == ScrollingDirection.Up ? 1 : -1) * Head.Height / 2;
            bodyPiece.Height = DrawHeight - Head.Height / 2 + Tail.Height / 2;
        }

        protected override void CheckForResult(bool userTriggered, double timeOffset)
        {
            double startTimeOffset = Time.Current - Head.HitObject.StartTime;

            if (!userTriggered)
            {
                // If the time passes the head note's range without the head being hit (or missed), count it as a miss
                if (!Head.Result.HasResult && !Head.HitObject.HitWindows.CanBeHit(startTimeOffset))
                {
                    Head.ApplyResult(r => r.Type = HitResult.Miss);
                    hasBroken = true;
                }

                // Todo: Check this
                // If the time passes the tail note's range without it being hit, count it as a miss
                if (!Tail.Result.HasResult && !Tail.HitObject.HitWindows.CanBeHit(timeOffset))
                {
                    applyTailResult(HitResult.Miss);
                    hasBroken = true;
                }

                if (!HitObject.HitWindows.CanBeHit(timeOffset))
                    ApplyResult(r => r.Type = HitResult.Perfect);

                return;
            }

            if (!Head.Result.HasResult)
            {
                var startResult = Head.HitObject.HitWindows.ResultFor(startTimeOffset);
                if (startResult == HitResult.None)
                    return;

                if (startResult == HitResult.Miss)
                    hasBroken = true;

                Head.ApplyResult(r => r.Type = startResult);
            }

            // Should be holding even if the head result is a miss
            updateHolding(true);
        }

        private void applyTailResult(HitResult type)
        {
            if (hasBroken && type > HitResult.Good)
                type = HitResult.Good;
            Tail.ApplyResult(r => r.Type = type);
        }

        private void updateHolding(bool holding)
        {
            if (isHolding && !holding)
                hasBroken = true;

            isHolding = holding;

            foreach (var tick in tickContainer)
                tick.IsHolding = holding;
            bodyPiece.Hitting = holding;
        }

        public bool OnPressed(ManiaAction action)
        {
            if (action != Action.Value)
                return false;

            if (AllJudged)
                return false;

            if (Time.Current < Head.HitObject.StartTime - Head.HitObject.HitWindows.HalfWindowFor(HitResult.Miss)
                || Time.Current > Tail.HitObject.StartTime + Tail.HitObject.HitWindows.HalfWindowFor(HitResult.Miss))
            {
                return false;
            }

            return UpdateResult(true);
        }

        public bool OnReleased(ManiaAction action)
        {
            if (!isHolding)
                return false;

            if (action != Action.Value)
                return false;

            var tailResult = Tail.HitObject.HitWindows.ResultFor(Time.Current - Tail.HitObject.StartTime);
            if (tailResult > HitResult.Miss)
                applyTailResult(tailResult);

            updateHolding(false);
            return true;
        }
    }
}
