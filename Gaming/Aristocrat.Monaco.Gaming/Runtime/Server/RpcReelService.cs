namespace Aristocrat.Monaco.Gaming.Runtime.Server
{
    using System;
    using System.Reflection;
    using System.Threading.Tasks;
    using Commands;
    using Grpc.Core;
    using log4net;
    using V1;
    using NudgeReelData = Hardware.Contracts.Reel.NudgeReelData;
    using ReelSpinData = Hardware.Contracts.Reel.ReelSpinData;
    using ReelSpeedData = Hardware.Contracts.Reel.ReelSpeedData;
    using SpinDirection = Hardware.Contracts.Reel.SpinDirection;

    public class RpcReelService : ReelService.ReelServiceBase
    {
        private static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        private readonly ICommandHandlerFactory _handlerFactory;

        public RpcReelService(ICommandHandlerFactory handlerFactory)
        {
            _handlerFactory = handlerFactory ?? throw new ArgumentNullException(nameof(handlerFactory));
        }

        public override Task<ConnectedReelsResponse> GetConnectedReels(ConnectedReelsRequest request, ServerCallContext context)
        {
            Logger.Debug($"GetConnectedReels");

            var command = new ConnectedReels();
            _handlerFactory.Create<ConnectedReels>()
                .Handle(command);

            var response = new ConnectedReelsResponse();
            response.ReelId.AddRange(command.ReelIds);
            return Task.FromResult(response);
        }

        public override Task<GetReelsStateResponse> GetReelsState(GetReelsStateRequest request, ServerCallContext context)
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

            return Task.FromResult(response);
        }

        public override Task<NudgeReelsResponse> NudgeReels(NudgeReelsRequest request, ServerCallContext context)
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

            return Task.FromResult(new NudgeReelsResponse { Result = command.Success });
        }

        public override Task<SpinReelsResponse> SpinReels(SpinReelsRequest request, ServerCallContext context)
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

            return Task.FromResult(new SpinReelsResponse { Result = command.Success });
        }

        public override Task<UpdateReelsSpeedResponse> UpdateReelsSpeed(UpdateReelsSpeedRequest request, ServerCallContext context)
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

            return Task.FromResult(new UpdateReelsSpeedResponse { Result = command.Success });
        }
    }
}