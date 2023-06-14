namespace Aristocrat.Monaco.Gaming.Runtime.Client
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using System.Threading;
    using Application.Contracts.Extensions;
    using GdkRuntime.V1;
    using Contracts;
    using Contracts.Process;
    using Google.Protobuf.WellKnownTypes;
    using Hardware.Contracts.Reel;
    using Kernel;
    using log4net;
    using Snapp;
    using V1 = GdkRuntime.V1;
    using Empty = GdkRuntime.V1.Empty;
    using Outcome = Contracts.Central.Outcome;

    public class SnappClient : IRuntime, IDisposable, IReelService, IPresentationService
    {
        private static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod()?.DeclaringType);

        private readonly IEventBus _eventBus;
        private readonly IProcessManager _processManager;

        private readonly object _sync = new ();

        private Channel _channel;
        private RuntimeServiceStub _runtimeStub;
        private RuntimeReelServiceStub _runtimeReelStub;
        private RuntimePresentationServiceStub _runtimePresentationStub;
        private bool _shutdownRequested;
        private bool _disposed;

        public SnappClient(IEventBus eventBus, IProcessManager processManager)
        {
            Logger.Debug("Create SnappClient");
            _eventBus = eventBus ?? throw new ArgumentNullException(nameof(eventBus));
            _processManager = processManager ?? throw new ArgumentNullException(nameof(processManager));

            // Create channel using transport (in this case … named pipe)
            _channel = new Channel(new NamedPipeTransport(GamingConstants.IpcRuntimePipeName));

            // Connect to Server - here, it’s assumed that the Runtime server is already running (because the
            // Runtime calls SnappServer.Join() which creates this object, so we know that the Runtime server is up)
            _channel.Connect();

            // Create stubs
            _runtimeStub = new RuntimeServiceStub { ServiceChannel = _channel };
            _runtimeReelStub = new RuntimeReelServiceStub { ServiceChannel = _channel };
            _runtimePresentationStub = new RuntimePresentationServiceStub { ServiceChannel = _channel };

            _eventBus.Subscribe<GameProcessExitedEvent>(this, _ =>
            {
                Logger.Debug("Handle GameProcessExitedEvent");
                _channel = null;
            });
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        public bool Connected => _channel?.IsConnected ?? false;

        public bool GetFlag(RuntimeCondition flag)
        {
            return Invoke(
                client =>
                {
                    var reply = client.GetFlag(new GetFlagRequest { Flag = (RuntimeFlag)flag });

                    return reply.Value;
                });
        }

        public void UpdateFlag(RuntimeCondition flag, bool state)
        {
            Invoke(client => client.UpdateFlag(new UpdateFlagRequest { Flag = (RuntimeFlag)flag, State = state }));
        }

        public RuntimeState GetState()
        {
            return Invoke(
                client =>
                {
                    var reply = client.GetState(new Empty());

                    return (RuntimeState)reply.State;
                });
        }

        public void UpdateState(RuntimeState state)
        {
            Invoke(client => client.UpdateState(new UpdateStateRequest { State = (V1.RuntimeState)state }));
        }

        public void InvokeButton(uint id, int state)
        {
            Invoke(client => client.InvokeButton(new InvokeButtonRequest { ButtonId = id, State = state }));
        }

        public void UpdateBalance(long credits)
        {
            Invoke(client => client.UpdateBalance(new UpdateBalanceNotification { Value = (ulong)credits }));
        }

        public void JackpotNotification()
        {
            Invoke(client => client.OnJackpotUpdated(new Empty()));
        }

        public void JackpotWinNotification(string poolName, IDictionary<int, long> winLevels)
        {
            var request = new JackpotWinAvailableNotification { PoolName = poolName };

            request.Levels.Add(
                winLevels.Select(
                    r => new LevelInfo { LevelId = (uint)r.Key, Value = (ulong)r.Value}));

            Invoke(client => client.JackpotWinAvailable(request));
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

            Invoke(client => client.BeginGameRoundResult(notification));
        }

        public void UpdateVolume(float level)
        {
            Invoke(client => client.UpdateVolume(new VolumeUpdateNotification { Volume = level }));
        }

        public void UpdateHandCount(int handCount)
        {
            Invoke(client => client.UpdateHandCount(new UpdateHandCountNotification { Value = (ulong)handCount }));
        }

        public void UpdateButtonState(uint buttonId, ButtonMask mask, ButtonState state)
        {
            Invoke(
                client => client.UpdateButtonState(
                    new UpdateButtonStateRequest
                    {
                        ButtonId = buttonId, ButtonMask = (int)mask, ButtonState = (int)state
                    }));
        }

        public void UpdateLocalTimeTranslationBias(TimeSpan duration)
        {
            Invoke(
                client => client.UpdateLocalTimeTranslationBias(
                    new UpdateLocalTimeTranslationBiasRequest { Minutes = (int)duration.TotalMinutes }));
        }

        public void UpdateParameters(IDictionary<string, string> parameters, ConfigurationTarget target)
        {
            var request = new UpdateParametersRequest { Target = (V1.ConfigurationTarget)target };

            request.Parameters.Add(parameters);

            Invoke(client => client.UpdateParameters(request));
        }

        public void UpdatePlatformMessages(IEnumerable<string> messages)
        {
            var request = new UpdatePlatformMessageRequest();
            var messagesList = messages?.Where(x => !string.IsNullOrEmpty(x)).ToList();
            if (messagesList != null && messagesList.Any())
            {
                request.Messages.Add(messagesList);
            }
            Invoke(client => client.UpdatePlatformMessage(request));
        }

        public void UpdateTimeRemaining(string message)
        {
            Invoke(client => client.UpdateTimeRemaining(new UpdateTimeRemainingRequest { TimeRemaining = message }));
        }

        public void Shutdown()
        {
            lock (_sync)
            {
                Invoke(client => client.Shutdown(new Empty()));

                _shutdownRequested = true;
            }
        }

        public void UpdateReelState(IDictionary<int, ReelLogicalState> updateData)
        {
            var stateRequest = new UpdateReelStateNotification
            {
                States = { updateData.ToDictionary(x => x.Key, x => HardwareReelExtensions.GetReelState(x.Value)) }
            };

            Invoke(x => x.UpdateReelState(stateRequest));
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

            Invoke(x => x .PresentOverriddenPresentation(overriddenPresentationMessage));
        }

        protected virtual void Dispose(bool disposing)
        {
            if (_disposed)
            {
                return;
            }

            if (disposing)
            {
                _eventBus.UnsubscribeAll(this);

                if (_channel is IDisposable disposableChannel)
                {
                    disposableChannel.Dispose();
                }
            }

            _channel = null;
            _runtimeStub = null;
            _runtimeReelStub = null;
            _runtimePresentationStub = null;

            _disposed = true;
        }

        private static bool IsRuntimePresumedDead(Exception ex)
        {
            return ex is StatusCodeException;
        }

        private void Invoke<T>(Func<RuntimePresentationServiceStub, T> callback)
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
                callback(_runtimePresentationStub);
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

        private void Invoke<T>(Func<RuntimeReelServiceStub, T> callback)
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
                callback(_runtimeReelStub);
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

        private T Invoke<T>(Func<RuntimeServiceStub, T> callback)
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
                return callback(_runtimeStub);
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