namespace Aristocrat.Monaco.Gaming.Runtime.Server
{
    using System;
    using System.Reflection;
    using GdkRuntime.V1;
    using Commands;
    using log4net;
    using NudgeReelData = Hardware.Contracts.Reel.NudgeReelData;
    using ReelSpinData = Hardware.Contracts.Reel.ReelSpinData;
    using ReelSpeedData = Hardware.Contracts.Reel.ReelSpeedData;
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
            Logger.Debug($"GetConnectedReels");

            var command = new ConnectedReels();
            _handlerFactory.Create<ConnectedReels>()
                .Handle(command);

            var response = new ConnectedReelsResponse();
            response.ReelId.AddRange(command.ReelIds);
            return response;
        }

        public override GetReelsStateResponse GetReelsState(GetReelsStateRequest request)
        {
            Logger.Debug($"GetReelsState");

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
            Logger.Debug($"NudgeReels");

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
            Logger.Debug($"SpinReels");

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
            Logger.Debug($"UpdateReelsSpeed");

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
    }
}