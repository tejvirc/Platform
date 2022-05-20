namespace Aristocrat.Monaco.Gaming.Runtime.Client
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using System.Threading;
    using Application.Contracts.Extensions;
    using Contracts;
    using Contracts.Process;
    using Google.Protobuf.WellKnownTypes;
    using Grpc.Core;
    using Hardware.Contracts.Reel;
    using Kernel;
    using log4net;
    using V1;
    using Empty = V1.Empty;
    using Outcome = Contracts.Central.Outcome;

    public class RpcClient : IRuntime, IDisposable, IReelService, IPresentationService
    {
        private static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        private static readonly TimeSpan ShutdownTimeout = TimeSpan.FromSeconds(30);
        private static readonly TimeSpan SendTimeout = TimeSpan.FromSeconds(30);

        private readonly IEventBus _eventBus;
        private readonly IProcessManager _processManager;

        private readonly object _sync = new object();

        private Channel _channel;
        private RuntimeService.RuntimeServiceClient _client;
        private RuntimeReelService.RuntimeReelServiceClient _reelClient;
        private RuntimePresentationService.RuntimePresentationServiceClient _presentationOverrideClient;
        private CallOptions _defaultCallOptions;
        private bool _shutdownRequested;
        private bool _disposed;
        private CancellationTokenSource _runtimeCancellation = new CancellationTokenSource();

        public RpcClient(IEventBus eventBus, IProcessManager processManager, int port)
        {
            _eventBus = eventBus ?? throw new ArgumentNullException(nameof(eventBus));
            _processManager = processManager ?? throw new ArgumentNullException(nameof(processManager));

            _channel = new Channel($"localhost:{port}", ChannelCredentials.Insecure);

            _client = new RuntimeService.RuntimeServiceClient(_channel);
            _reelClient = new RuntimeReelService.RuntimeReelServiceClient(_channel);
            _presentationOverrideClient = new RuntimePresentationService.RuntimePresentationServiceClient(_channel);

            _defaultCallOptions = new CallOptions().WithWaitForReady();

            _eventBus.Subscribe<GameProcessExitedEvent>(this, _ =>
            {
                _runtimeCancellation?.Cancel(false);
                _runtimeCancellation?.Dispose();

                _runtimeCancellation = new CancellationTokenSource();
            });
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        public bool Connected => _channel?.State == ChannelState.Idle || _channel?.State == ChannelState.Ready;
        
        public bool GetFlag(RuntimeCondition flag)
        {
            return Invoke(
                client =>
                {
                    var reply = client.GetFlag(new GetFlagRequest { Flag = (RuntimeFlag)flag }, Options());

                    return reply.Value;
                });
        }

        public void UpdateFlag(RuntimeCondition flag, bool state)
        {
            Invoke(client => client.UpdateFlag(new UpdateFlagRequest { Flag = (RuntimeFlag)flag, State = state }, Options()));
        }

        public RuntimeState GetState()
        {
            return Invoke(
                client =>
                {
                    var reply = client.GetState(new Empty(), Options());

                    return (RuntimeState)reply.State;
                });
        }

        public void UpdateState(RuntimeState state)
        {
            Invoke(client => client.UpdateState(new UpdateStateRequest { State = (V1.RuntimeState)state }, Options()));
        }

        public void InvokeButton(uint id, int state)
        {
            Invoke(client => client.InvokeButton(new InvokeButtonRequest { ButtonId = id, State = state }, Options()));
        }

        public void UpdateBalance(long credits)
        {
            Invoke(client => client.UpdateBalance(new UpdateBalanceNotification { Value = (ulong)credits }, Options()));
        }

        public void JackpotNotification()
        {
            Invoke(client => client.OnJackpotUpdated(new Empty(), Options()));
        }

        public void JackpotWinNotification(string poolName, IDictionary<int, long> winLevels)
        {
            var request = new JackpotWinAvailableNotification { PoolName = poolName };

            request.Levels.Add(
                winLevels.Select(
                    r => new LevelInfo { LevelId = (uint)r.Key, Value = (ulong)r.Value}));

            Invoke(client => client.JackpotWinAvailable(request, Options()));
        }

        public void BeginGameRoundResponse(BeginGameRoundResult result, IEnumerable<Outcome> outcomes, CancellationTokenSource cancellationTokenSource = null)
        {
            var notification = new BeginGameRoundNotification
            {
                State = (BeginGameRoundNotification.Types.BeginGameRoundState)result
            };

            notification.Outcomes.Add(
                outcomes.Select(
                    o => new V1.Outcome
                    {
                        Type = (V1.Outcome.Types.OutcomeType)o.Type,
                        WinAmount = (ulong)o.Value.MillicentsToCents(),
                        LookupData = o.LookupData,
                        WinLevelIndex = o.WinLevelIndex
                    }));

            Invoke(client => client.BeginGameRoundResult(notification, Options(cancellationTokenSource)));
        }

        public void UpdateVolume(float level)
        {
            Invoke(client => client.UpdateVolume(new VolumeUpdateNotification { Volume = level }, Options()));
        }

        public void UpdateButtonState(uint buttonId, ButtonMask mask, ButtonState state)
        {
            Invoke(
                client => client.UpdateButtonState(
                    new UpdateButtonStateRequest
                    {
                        ButtonId = buttonId, ButtonMask = (int)mask, ButtonState = (int)state
                    },
                    Options()));
        }

        public void UpdateLocalTimeTranslationBias(TimeSpan duration)
        {
            Invoke(
                client => client.UpdateLocalTimeTranslationBias(
                    new UpdateLocalTimeTranslationBiasRequest { Minutes = (int)duration.TotalMinutes }, Options()));
        }

        public void UpdateParameters(IDictionary<string, string> parameters, ConfigurationTarget target)
        {
            var request = new UpdateParametersRequest { Target = (V1.ConfigurationTarget)target };

            request.Parameters.Add(parameters);

            Invoke(client => client.UpdateParameters(request, Options()));
        }

        public void UpdatePlatformMessages(IEnumerable<string> messages)
        {
            var request = new UpdatePlatformMessageRequest();
            var messagesList = messages?.Where(x => !string.IsNullOrEmpty(x)).ToList();
            if (messagesList != null && messagesList.Any())
            {
                request.Messages.Add(messagesList);
            }
            Invoke(client => client.UpdatePlatformMessage(request, Options()));
        }

        public void UpdateTimeRemaining(string message)
        {
            Invoke(client => client.UpdateTimeRemaining(new UpdateTimeRemainingRequest { TimeRemaining = message }, Options()));
        }

        public void Shutdown()
        {
            lock (_sync)
            {
                Invoke(client => client.Shutdown(new Empty(), Options()));

                _shutdownRequested = true;
            }
        }

        public void UpdateReelState(IDictionary<int, ReelLogicalState> updateData)
        {
            var stateRequest = new UpdateReelStateRequest
            {
                States = { updateData.ToDictionary(x => x.Key, x => HardwareReelExtensions.GetReelState(x.Value)) }
            };

            Invoke(x => x.UpdateReelState(stateRequest, Options()));
        }

        public void PresentOverriddenPresentation(IList<PresentationOverrideData> presentations)
        {
            var overriddenPresentationMessage = new OverriddenPresentationMessage();
            
            foreach (var presentation in presentations)
            {
                var presentationMessage = new TextPresentationMessage
                {
                    PresentationType = (PresentationType)presentation.Type,
                    Message = presentation.Message
                };

                overriddenPresentationMessage.OverridingMessages.Add(Any.Pack(presentationMessage));
            }
            
            Invoke(x => x.PresentOverriddenPresentation(overriddenPresentationMessage, Options()));
        }

        protected virtual void Dispose(bool disposing)
        {
            if (_disposed)
            {
                return;
            }

            if (disposing)
            {
                _channel.ShutdownAsync().Wait(ShutdownTimeout);

                // ReSharper disable once UseNullPropagation
                if (_runtimeCancellation != null)
                {
                    _runtimeCancellation.Cancel(false);
                    _runtimeCancellation.Dispose();
                    _runtimeCancellation = null;
                }

                _eventBus.UnsubscribeAll(this);
            }

            _channel = null;
            _client = null;
            _reelClient = null;
            _presentationOverrideClient = null;

            _disposed = true;
        }

        private CallOptions Options(CancellationTokenSource cancellationTokenSource = null)
        {
            return cancellationTokenSource == null
                ? _defaultCallOptions.WithDeadline(DateTime.UtcNow.Add(SendTimeout))
                    .WithCancellationToken(_runtimeCancellation.Token)
                : _defaultCallOptions.WithDeadline(DateTime.UtcNow.Add(SendTimeout))
                    .WithCancellationToken(cancellationTokenSource.Token);
        }

        private static bool IsRuntimePresumedDead(Exception ex)
        {
            return ex is RpcException rpcException &&
                   rpcException.StatusCode != StatusCode.Cancelled && // Game process exited
                   rpcException.StatusCode != StatusCode.InvalidArgument && // Value not supported by this client
                   rpcException.StatusCode != StatusCode.OK;
        }

        private void Invoke<T>(Func<RuntimePresentationService.RuntimePresentationServiceClient, T> callback)
        {
            var localLogger = Logger;

            if (callback == null)
            {
                throw new ArgumentNullException(nameof(callback));
            }

            lock (_sync)
            {
                if (_shutdownRequested)
                {
                    localLogger.Warn($"Failed to invoke {callback.Method.Name} - Shutdown requested");
                    return;
                }
            }

            try
            {
                callback(_presentationOverrideClient);
            }
            catch (Exception ex) when (IsRuntimePresumedDead(ex))
            {
                localLogger.Error($"Failed to invoke {callback.Method.Name}", ex);

                // In the event of one of the above filtered exceptions we're going to assume the runtime has crashed or exited (and we didn't catch it)
                //  This will effectively, get things back to a known good state without crashing the platform
                var processId = _processManager.GetRunningProcesses().FirstOrDefault();

                _eventBus.Publish(new GameProcessHungEvent(processId));
            }
            catch (Exception ex)
            {
                localLogger.Error($"Failed to invoke {callback.Method.Name} runtime not presumed dead", ex);
            }
        }

        private void Invoke<T>(Func<RuntimeReelService.RuntimeReelServiceClient, T> callback)
        {
            var localLogger = Logger;

            if (callback == null)
            {
                throw new ArgumentNullException(nameof(callback));
            }

            lock (_sync)
            {
                if (_shutdownRequested)
                {
                    localLogger.Warn($"Failed to invoke {callback.Method.Name} - Shutdown requested");
                    return;
                }
            }

            try
            {
                callback(_reelClient);
            }
            catch (Exception ex) when (IsRuntimePresumedDead(ex))
            {
                localLogger.Error($"Failed to invoke {callback.Method.Name}", ex);

                // In the event of one of the above filtered exceptions we're going to assume the runtime has crashed or exited (and we didn't catch it)
                //  This will effectively, get things back to a known good state without crashing the platform
                var processId = _processManager.GetRunningProcesses().FirstOrDefault();

                _eventBus.Publish(new GameProcessHungEvent(processId));
            }
            catch (Exception ex)
            {
                localLogger.Error($"Failed to invoke {callback.Method.Name} runtime not presumed dead", ex);
            }
        }

        private T Invoke<T>(Func<RuntimeService.RuntimeServiceClient, T> callback)
        {
            var localLogger = Logger;

            if (callback == null)
            {
                throw new ArgumentNullException(nameof(callback));
            }

            lock (_sync)
            {
                if (_shutdownRequested)
                {
                    localLogger.Warn($"Failed to invoke {callback.Method.Name} - Shutdown requested");
                    return default;
                }
            }

            try
            {
                return callback(_client);
            }
            catch (Exception ex) when (IsRuntimePresumedDead(ex))
            {
                localLogger.Error($"Failed to invoke {callback.Method.Name}", ex);

                // In the event of one of the above filtered exceptions we're going to assume the runtime has crashed or exited (and we didn't catch it)
                //  This will effectively, get things back to a known good state without crashing the platform
                var processId = _processManager.GetRunningProcesses().FirstOrDefault();

                _eventBus.Publish(new GameProcessHungEvent(processId));
            }
            catch (Exception ex)
            {
                localLogger.Error($"Failed to invoke {callback.Method.Name} runtime not presumed dead", ex);
            }

            return default;
        }
    }
}