// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.Allocation;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Game.Graphics;
using osu.Game.Online.API;
using OpenTK.Graphics;

namespace osu.Game.Screens.AX.Steps
{
    public class StepContainer : Container<Step>, IOnlineComponent
    {
        protected override Container<Step> Content => content;
        private readonly Container<Step> content;

        private Step currentStep;

        private APIAccess api;

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
        private void load(APIAccess api)
        {
            this.api = api;
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

        private void pushStep(Step step)
        {
            currentStep?.Expire();
            Add(currentStep = step);
        }
    }
}
