namespace Aristocrat.Monaco.Gaming.Runtime.Server
{
    using System;
    using System.Collections.Generic;
    using System.Reflection;
    using Aristocrat.Monaco.Hardware.Contracts.Reel.ControlData;
    using Commands;
    using GdkRuntime.V1;
    using Hardware.Contracts.Reel;
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
            Logger.Debug("PrepareLightShows");

            var lightShowDataCollection = new LightShowData[request.LightShowData.Count];

            for (var i = 0; i < request.LightShowData.Count; ++i)
            {
                var lightShowData = request.LightShowData[i];
                var lightShowIdentifier = lightShowData.LightShowIdentifier;
                var loopCount = EvaluateLoopBehavior(lightShowData.LoopBehavior, (sbyte)lightShowData.RepeatCount);
                lightShowDataCollection[i] = new LightShowData(
                    (sbyte)lightShowData.ReelIndex,
                    lightShowIdentifier.AnimationName,
                    lightShowIdentifier.Tag,
                    loopCount,
                    (short)lightShowData.Step);
            }

            var command = new PrepareLightShows(lightShowDataCollection);
            _handlerFactory.Create<PrepareLightShows>()
                .Handle(command);

            return new MessageResponse { Result = command.Success };

            sbyte EvaluateLoopBehavior(LoopBehavior behavior, sbyte loopCount)
            {
                return behavior switch
                {
                    LoopBehavior.Once => ReelConstants.RepeatOnce,
                    LoopBehavior.Forever => ReelConstants.RepeatForever,
                    LoopBehavior.RepeatFor => loopCount,
                    _ => throw (new ArgumentOutOfRangeException(nameof(behavior)))
                };
            }
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
            Logger.Debug("StartAnimations");

            var command = new StartAnimations();
            _handlerFactory.Create<StartAnimations>().Handle(command);

            return new MessageResponse { Result = command.Success };
        }

        public override MessageResponse StopLightshowAnimation(StopLightshowAnimationRequest request)
        {
            Logger.Debug("StopLightShowAnimations");

            var lightShowData = new LightShowData[request.LightShowData.Count];

            for (var i = 0; i < request.LightShowData.Count; ++i)
            {
                lightShowData[i] = new LightShowData(
                    request.LightShowData[i].AnimationName,
                    request.LightShowData[i].Tag);
            }

            var command = new StopLightShowAnimations(lightShowData);
            _handlerFactory.Create<StopLightShowAnimations>().Handle(command);

            return new MessageResponse { Result = command.Success };
        }

        public override MessageResponse StopAllLightshowAnimations(Empty request)
        {
            Logger.Debug("StopAllLightShowAnimations");

            var command = new StopAllLightShowAnimations();
            _handlerFactory.Create<StopAllLightShowAnimations>().Handle(command);

            return new MessageResponse { Result = command.Success };
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
            Logger.Debug("PrepareStepperRule");

            var ruleData = new StepperRuleData
            {
                ReelIndex = (byte)request.RuleData.ReelIndex,
                Cycle = (short)request.RuleData.Cycle,
                EventId = (int)request.RuleData.EventIdentifier,
                ReferenceStep = (byte)request.RuleData.ReferenceStep,
                StepToFollow = (byte)request.RuleData.StepToFollow
            };

            if (request.RuleTypeSpecificData.Is(PrepareStepperAnticipationRuleData.Descriptor))
            {
                var anticipationRuleData = request.RuleTypeSpecificData.Unpack<PrepareStepperAnticipationRuleData>();
                ruleData.Delta = (byte)anticipationRuleData.Delta;
                ruleData.RuleType = StepperRuleType.AnticipationRule;
            }
            else
            {
                ruleData.RuleType = StepperRuleType.FollowRule;
            }

            var command = new PrepareStepperRule(ruleData);
            _handlerFactory.Create<PrepareStepperRule>()
                .Handle(command);

            return new MessageResponse { Result = command.Success };
        }

        public override MessageResponse SynchronizeReels(SynchronizeReelsRequest request)
        {
            Logger.Debug("SynchronizeReels");

            var reelSyncStepData = new List<ReelSyncStepData>();
            var syncData = new ReelSynchronizationData();

            if (request.SynchronizationData.Is(SynchronizeReelsData.Descriptor))
            {
                var snappSyncData = request.SynchronizationData.Unpack<SynchronizeReelsData>();

                foreach (var data in request.Data)
                {
                    reelSyncStepData.Add(new(
                            (byte)data.ReelIndex,
                            (short)data.SynchronizationStep));
                }

                syncData.Duration = (short)snappSyncData.Duration;
                syncData.ReelSyncStepData = reelSyncStepData;
                syncData.SyncType = SynchronizeType.Regular;
            }
            else if (request.SynchronizationData.Is(EnhancedSynchronizeReelsData.Descriptor))
            {
                var snappEnhancedSyncData = request.SynchronizationData.Unpack<EnhancedSynchronizeReelsData>();

                foreach (var data in request.Data)
                {
                    reelSyncStepData.Add(new(
                        (byte)data.ReelIndex,
                        (short)data.SynchronizationStep));
                }

                for(var i = 0; i < snappEnhancedSyncData.Duration.Count; i++)
                {
                    reelSyncStepData[i].Duration = (short)snappEnhancedSyncData.Duration[i];
                }

                syncData.MasterReelIndex = (byte)snappEnhancedSyncData.MasterReelIndex;
                syncData.MasterReelStep = (short)snappEnhancedSyncData.MasterReelSynchronizationStep;
                syncData.ReelSyncStepData = reelSyncStepData;
                syncData.SyncType = SynchronizeType.Enhanced;
            }

            var command = new PrepareSynchronizeReels(syncData);
            _handlerFactory.Create<PrepareSynchronizeReels>()
                .Handle(command);

            return new MessageResponse { Result = command.Success };
        }

        public override MessageResponse SetBrightness(SetBrightnessRequest request)
        {
            Logger.Debug("SetBrightness");

            var brightnessData = request.Brightness;
            var command = new SetBrightness(brightnessData);
            _handlerFactory.Create<SetBrightness>().Handle(command);

            return new MessageResponse { Result = command.Success };
        }
    }
}