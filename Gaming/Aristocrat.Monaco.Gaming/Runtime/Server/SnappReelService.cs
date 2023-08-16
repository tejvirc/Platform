namespace Aristocrat.Monaco.Gaming.Runtime.Server
{
    using System;
    using System.Reflection;
    using Aristocrat.Monaco.Hardware.Contracts.Reel.ControlData;
    using Commands;
    using GdkRuntime.V1;
    using log4net;
    using NudgeReelData = Hardware.Contracts.Reel.ControlData.NudgeReelData;
    using ReelSpeedData = Hardware.Contracts.Reel.ControlData.ReelSpeedData;
    using ReelSpinData = Hardware.Contracts.Reel.ControlData.ReelSpinData;
    using SpinDirection = Hardware.Contracts.Reel.SpinDirection;

    public class SnappReelService : IReelServiceCallback
    {
        private static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod()?.DeclaringType);
        private readonly ICommandHandlerFactory _handlerFactory;

        public SnappReelService(ICommandHandlerFactory handlerFactory)
        {
            _handlerFactory = handlerFactory ?? throw new ArgumentNullException(nameof(handlerFactory));
        }

        public override ConnectedReelsResponse GetConnectedReels(ConnectedReelsRequest request)
        {
            Logger.Debug("GetConnectedReels");

            var command = new ConnectedReels();
            _handlerFactory.Create<ConnectedReels>()
                .Handle(command);

            var response = new ConnectedReelsResponse();
            response.ReelId.AddRange(command.ReelIds);
            return response;
        }

        public override GetReelsStateResponse GetReelsState(GetReelsStateRequest request)
        {
            Logger.Debug("GetReelsState");

            var command = new GetReelState();
            _handlerFactory.Create<GetReelState>()
                .Handle(command);

            var response = new GetReelsStateResponse();
            foreach (var reelState in command.States)
            {
                response.States.Add(reelState.Key, reelState.Value);
            }

            return response;
        }

        public override NudgeReelsResponse NudgeReels(NudgeReelsRequest request)
        {
            Logger.Debug("NudgeReels");

            var nudgeSpinData = new NudgeReelData[request.NudgeData.Count];
            for (var i = 0; i < request.NudgeData.Count; ++i)
            {
                var direction = request.NudgeData[i].Direction == Direction.Forward ? SpinDirection.Forward : SpinDirection.Backwards;
                nudgeSpinData[i] = new NudgeReelData(
                    request.NudgeData[i].ReelId,
                    direction,
                    request.NudgeData[i].Speed,
                    request.NudgeData[i].Steps);
            }

            var command = new NudgeReels(nudgeSpinData);
            _handlerFactory.Create<NudgeReels>()
                .Handle(command);

            Logger.Debug($"NudgeReels with request: {request} Result: { command.Success}");

            return new NudgeReelsResponse { Result = command.Success };
        }

        public override SpinReelsResponse SpinReels(SpinReelsRequest request)
        {
            Logger.Debug("SpinReels");

            var spinData = new ReelSpinData[request.SpinData.Count];
            for (var i = 0; i < request.SpinData.Count; ++i)
            {
                var direction = request.SpinData[i].Direction == Direction.Forward ? SpinDirection.Forward : SpinDirection.Backwards;
                spinData[i] = new ReelSpinData(
                    request.SpinData[i].ReelId,
                    direction,
                    request.SpinData[i].Speed,
                    request.SpinData[i].Step);
            }

            var command = new SpinReels(spinData);
            _handlerFactory.Create<SpinReels>()
                .Handle(command);

            Logger.Debug($"SpinReels with request: {request} Result: {command.Success}");

            return new SpinReelsResponse { Result = command.Success };
        }

        public override UpdateReelsSpeedResponse UpdateReelsSpeed(UpdateReelsSpeedRequest request)
        {
            Logger.Debug("UpdateReelsSpeed");

            var speedData = new ReelSpeedData[request.SpeedData.Count];
            for (var i = 0; i < request.SpeedData.Count; ++i)
            {
                speedData[i] = new ReelSpeedData(
                    request.SpeedData[i].ReelId,
                    request.SpeedData[i].Speed);
            }

            var command = new UpdateReelsSpeed(speedData);
            _handlerFactory.Create<UpdateReelsSpeed>()
                .Handle(command);

            Logger.Debug($"UpdateReelsSpeed with request: {request} Result: {command.Success}");

            return new UpdateReelsSpeedResponse { Result = command.Success };
        }

        public override MessageResponse PrepareLightShowAnimations(PrepareLightShowAnimationsRequest request)
        {
            return default;
        }

        public override MessageResponse PrepareStepperCurves(PrepareStepperCurvesRequest request)
        {
            Logger.Debug("PrepareStepperCurves");

            var curveData = new ReelCurveData[request.StepperData.Count];

            for (var i = 0; i < request.StepperData.Count; ++i)
            {
                curveData[i] = new ReelCurveData(
                    (byte)request.StepperData[i].ReelIndex,
                    request.StepperData[i].AnimationName);
            }

            var command = new PrepareStepperCurves(curveData);
            _handlerFactory.Create<PrepareStepperCurves>()
                .Handle(command);

            return new MessageResponse { Result = command.Success };
        }

        public override MessageResponse StartAnimations(Empty request)
        {
            throw new NotImplementedException();
        }

        public override MessageResponse StopLightshowAnimation(StopLightshowAnimationRequest request)
        {
            throw new NotImplementedException();
        }

        public override MessageResponse StopAllLightshowAnimations(Empty request)
        {
            throw new NotImplementedException();
        }

        public override MessageResponse StopAllAnimationTags(StopAllAnimationTagsRequest request)
        {
            Logger.Debug("StopAllAnimationTags");

            var command = new StopAllAnimationTags(request.AnimationName);
            _handlerFactory.Create<StopAllAnimationTags>()
                .Handle(command);

            return new MessageResponse { Result = command.Success };
        }

        public override MessageResponse PrepareStopReel(PrepareStopReelRequest request)
        {
            Logger.Debug("PrepareStopReel");

            var stopData = new ReelStopData[request.StopReelData.Count];

            for (var i = 0; i < request.StopReelData.Count; ++i)
            {
                stopData[i] = new ReelStopData(
                    (byte)request.StopReelData[i].ReelIndex,
                    (short)request.StopReelData[i].Duration,
                    (short)request.StopReelData[i].Step);
            }

            var command = new PrepareStopReels(stopData);
            _handlerFactory.Create<PrepareStopReels>()
                .Handle(command);

            return new MessageResponse { Result = command.Success };
        }

        public override MessageResponse PrepareStepperRule(PrepareStepperRuleRequest request)
        {
            throw new NotImplementedException();
        }

        public override MessageResponse SynchronizeReels(SynchronizeReelsRequest request)
        {
            throw new NotImplementedException();
        }

        public override MessageResponse SetBrightness(SetBrightnessRequest request)
        {
            throw new NotImplementedException();
        }

        public override LightshowQueueAttributesResponse GetLightshowQueueAttributes(Empty request)
        {
            return default;
        }
    }
}