// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.Allocation;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Input;
using osu.Game.Graphics;
using osu.Game.Online.API;
using OpenTK;
using OpenTK.Graphics;

namespace osu.Game.Screens.AX.Steps
{
    public class StepContainer : Container<Step>, IOnlineComponent
    {
        protected override Container<Step> Content => content;
        private readonly Container<Step> content;

        private Step currentStep;

        private APIAccess api;
        private UserInputManager inputManager;

        public StepContainer()
        {
            AutoSizeAxes = Axes.Y;
            AutoSizeDuration = 200;
            AutoSizeEasing = EasingTypes.OutQuint;
            Width = 600;

            CornerRadius = 5;
            Masking = true;

            EdgeEffect = new EdgeEffectParameters
            {
                Type = EdgeEffectType.Shadow,
                Colour = Color4.Black.Opacity(0.6f),
                Radius = 10
            };

            AddInternal(new Drawable[]
            {
                content = new Container<Step>
                {
                    RelativeSizeAxes = Axes.X,
                    AutoSizeAxes = Axes.Y,
                    Padding = new MarginPadding(20)
                },
                new Box
                {
                    Anchor = Anchor.BottomLeft,
                    Origin = Anchor.BottomLeft,
                    RelativeSizeAxes = Axes.X,
                    Colour = OsuColour.FromHex("ffcc22"),
                    Height = 3
                }
            });
        }

        [BackgroundDependencyLoader(permitNulls: true)]
        private void load(APIAccess api, UserInputManager inputManager)
        {
            this.api = api;
            this.inputManager = inputManager;

            api?.Register(this);
        }

        public void APIStateChanged(APIAccess api, APIState state)
        {
            switch (state)
            {
                case APIState.Connecting:
                    break;
                case APIState.Offline:
                    using (BeginDelayedSequence(500))
                    {
                        Schedule(() =>
                        {
                            if (api.State != APIState.Offline)
                                return;
                            pushStep(new LoginStep());
                        });
                    }
                    break;
                case APIState.Online:
                    var getMessageRequest = new GetMessageRequest();
                    getMessageRequest.Failure += e => pushStep(new SubmissionResultStep(SubmissionState.Restricted));
                    getMessageRequest.Success += m =>
                    {
                        var writeMessageStep = new WriteMessageStep(m.Entry?.Message ?? string.Empty);
                        writeMessageStep.Submit = s => pushStep(new SubmissionResultStep(s));
                        pushStep(writeMessageStep);
                    };
                    api.Queue(getMessageRequest);
                    break;
            }
        }

        public override bool Contains(Vector2 screenSpacePos) => true;

        public override bool AcceptsFocus => true;

        protected override bool OnClick(InputState state) => true;

        protected override void OnFocus(InputState state) => Schedule(() => inputManager?.ChangeFocus(currentStep));

        private void pushStep(Step step)
        {
            currentStep?.Expire();
            Add(currentStep = step);
        }
    }
}
