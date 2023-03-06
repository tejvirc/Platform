namespace Aristocrat.Monaco.Gaming.Runtime.Client
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using System.ServiceModel;
    using System.Threading;
    using Application.Contracts.Extensions;
    using Contracts;
    using Contracts.Process;
    using GDKRuntime.Contract;
    using Hardware.Contracts.Reel;
    using Kernel;
    using log4net;
    using Outcome = GDKRuntime.Contract.Outcome;
    using PresentationOverrideTypes = GDKRuntime.Contract.PresentationOverrideTypes;

    public class WcfClient : IRuntime, IReelService, IPresentationService, IDisposable
    {
        private static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        private static readonly TimeSpan TimeOut = TimeSpan.FromSeconds(30);

        private readonly OperationContext _context;
        private readonly IEventBus _eventBus;
        private readonly IProcessManager _processManager;
        private readonly object _sync = new object();

        private bool _shutdownRequested;
        private bool _disconnected;
        private bool _disposed;

        public WcfClient(OperationContext context, IEventBus eventBus, IProcessManager processManager)
        {
            if (context?.Channel != null)
            {
                context.Channel.OperationTimeout = TimeOut;
            }

            _context = context;
            _eventBus = eventBus ?? throw new ArgumentNullException(nameof(eventBus));
            _processManager = processManager ?? throw new ArgumentNullException(nameof(processManager));

            _eventBus.Subscribe<GameProcessExitedEvent>(this, _ => _disconnected = true);
        }

        public bool Connected
        {
            get
            {
                lock (_sync)
                {
                    return _context.Channel?.State == CommunicationState.Opened && !_disconnected && !_shutdownRequested;
                }
            }
        }

        public bool GetFlag(RuntimeCondition flag)
        {
            return Invoke(runtime => runtime.GetFlag((RuntimeFlag)flag));
        }

        public void UpdateFlag(RuntimeCondition flag, bool state)
        {
            Invoke(runtime => runtime.SetFlag((RuntimeFlag)flag, state));
        }

        public RuntimeState GetState()
        {
            return Invoke(runtime => runtime.GetState().ToRuntimeState());
        }

        public void UpdateState(RuntimeState state)
        {
            Invoke(runtime => runtime.SetState(state.ToGdkRuntimeState()));
        }

        public void InvokeButton(uint id, int state)
        {
            Invoke(runtime => runtime.InvokeButton(id, state));
        }

        public void UpdateBalance(long credits)
        {
            Invoke(runtime => runtime.OnBalanceUpdate((ulong)credits));
        }

        public void JackpotNotification()
        {
            Invoke(runtime => runtime.OnJackpotUpdated());
        }

        public void JackpotWinNotification(string poolName, IDictionary<int, long> winLevels)
        {
            Invoke(
                runtime => runtime.OnJackpotWinAvailable(
                    poolName,
                    winLevels.ToDictionary(l => (uint)l.Key, l => (ulong)l.Value)));
        }

        public void BeginGameRoundResponse(BeginGameRoundResult result, IEnumerable<Contracts.Central.Outcome> outcomes, CancellationTokenSource cancellationTokenSource = null)
        {
            var state = result.ToGdkBeginGameRoundState();

            var outcomeList = new List<Outcome>();
            outcomeList.AddRange(outcomes.Select(o => new Outcome
            {
                Type = o.Type.ToGdkOutcomeType(),
                WinAmount = (ulong)o.Value.MillicentsToCents(),
                LookupData = o.LookupData,
                WinLevelIndex = o.WinLevelIndex
            }));

            Invoke(runtime => runtime.BeginGameRoundResult(state, outcomeList));
        }

        public void UpdateVolume(float level)
        {
            Invoke(runtime => runtime.OnVolumeUpdate(level));
        }
        public void UpdateHandCount(int handCount)
        {
            var parameters = new Dictionary<string, string>
            {
                { "/Runtime/HandCountValue", handCount.ToString() }
            };
            UpdateParameters(parameters, ConfigurationTarget.GameConfiguration);
        }

        public void UpdateButtonState(uint buttonId, ButtonMask mask, ButtonState state)
        {
            Invoke(runtime => runtime.SetButtonState(buttonId, (SystemButtonMask)mask, (SystemButtonState)state));
        }

        public void UpdateLocalTimeTranslationBias(TimeSpan duration)
        {
            Invoke(runtime => runtime.SetLocalTimeTranslationBias((int)duration.TotalMinutes));
        }

        public void UpdateParameters(IDictionary<string, string> parameters, ConfigurationTarget target)
        {
            Invoke(runtime => runtime.SetParameters(parameters, (GDKRuntime.Contract.ConfigurationTarget)target));
        }

        public void UpdatePlatformMessages(IEnumerable<string> messages)
        {
            Invoke(runtime => runtime.SetPlatformMessage(messages?.ToArray()));
        }

        public void UpdateTimeRemaining(string message)
        {
            Invoke(runtime => runtime.SetTimeRemaining(message));
        }

        public void Shutdown()
        {
            lock (_sync)
            {
                Invoke(runtime => runtime.Shutdown());

                _shutdownRequested = true;
            }
        }

        public void UpdateReelState(IDictionary<int, ReelLogicalState> updateData)
        {
            var stateRequest = new Dictionary<int, ReelState>();
            foreach (var update in updateData)
            {
                stateRequest.Add(update.Key, (ReelState)HardwareReelExtensions.GetReelState(update.Value));
            }

            Invoke(runtime => runtime.UpdateReelState(stateRequest));
        }

        public void PresentOverriddenPresentation(IList<Contracts.PresentationOverrideData> presentations)
        {
            var runtimePresentations = presentations.Select(presentation => new GDKRuntime.Contract.PresentationOverrideData() { Message = presentation.Message, Type = (PresentationOverrideTypes)presentation.Type }).ToList();
            Invoke(runtime => runtime.PresentOverriddenPresentation(runtimePresentations));
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
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
            }

            _disposed = true;
        }

        private static bool IsRuntimePresumedDead(Exception ex)
        {
            return ex is TimeoutException || ex is CommunicationObjectAbortedException;
        }

        private void Invoke(Action<IGameRuntime> runtime)
        {
            if (runtime == null)
            {
                throw new ArgumentNullException(nameof(runtime));
            }

            if (!Connected)
            {
                Logger.Error(
                    $"The callback channel is not connected or shutdown requested ({_context?.Channel?.State}). Failed to invoke {runtime.Method.Name}");
                return;
            }

            try
            {
                runtime(_context.GetCallbackChannel<IGameRuntime>());
            }
            catch (Exception ex)
            {
                HandleException(ex, runtime.Method.Name);
            }
        }

        private T Invoke<T>(Func<IGameRuntime, T> runtime)
        {
            if (runtime == null)
            {
                throw new ArgumentNullException(nameof(runtime));
            }

            if (!Connected)
            {
                Logger.Error($"The callback channel is not connected or shutdown requested ({_context?.Channel?.State}). Failed to invoke {runtime.Method.Name}");
                return default(T);
            }

            try
            {
                return runtime(_context.GetCallbackChannel<IGameRuntime>());
            }
            catch (Exception ex)
            {
                HandleException(ex, runtime.Method.Name);
            }

            return default(T);
        }

        private void HandleException(Exception ex, string methodName)
        {
            if (ex is InvalidCastException)
            {
                Logger.Warn($"Runtime method is not supported: {_context.Channel}", ex);
            }
            else if (ex is ArgumentOutOfRangeException)
            {
                Logger.Warn($"Value not supported by this client: {_context.Channel}", ex);
            }
            else if (IsRuntimePresumedDead(ex))
            {
                Logger.Error($"Failed to invoke method: {methodName}", ex);

                // In the event of one of the above filtered exceptions we're going to assume the runtime has crashed or exited (and we didn't catch it)
                //  This will effectively, get things back to a known good state without crashing the platform
                var processId = _processManager.GetRunningProcesses().FirstOrDefault();

                _eventBus.Publish(new GameProcessHungEvent(processId));
            }
            else
            {
                Logger.Error($"Failed to invoke {methodName}", ex);
            }
        }
    }
}