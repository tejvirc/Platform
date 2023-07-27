namespace Aristocrat.Monaco.Sas.Handlers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using System.Text;
    using System.Threading;
    using Application.Contracts.Authentication;
    using Aristocrat.Sas.Client;
    using Aristocrat.Sas.Client.LongPollDataClasses;
    using Base;
    using Kernel;
    using Kernel.Contracts.Components;
    using log4net;

    /// <summary>
    ///     Handles authorization requests
    /// </summary>
    public class LP6ESendAuthenticationInfoHandler :
        ISasLongPollHandler<SendAuthenticationInfoResponse, SendAuthenticationInfoCommand>, IDisposable
    {
        private const double AuthenticationResultExceptionReportingIntervalMs = 15_000.0f;

        private static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private readonly ISasExceptionHandler _exceptionHandler;
        private readonly IEventBus _eventBus;
        private readonly IComponentRegistry _componentRegistry;
        private readonly IAuthenticationService _authenticationService;
        private readonly SasExceptionTimer _exceptionTimer;

        private readonly Dictionary<AuthenticationMethods, AlgorithmType> _algorithmMap =
            new Dictionary<AuthenticationMethods, AlgorithmType>
            {
                { AuthenticationMethods.Crc16, AlgorithmType.Crc16 },
                { AuthenticationMethods.Md5, AlgorithmType.Md5 },
                { AuthenticationMethods.Sha1, AlgorithmType.Sha1 },
                { AuthenticationMethods.Sha256, AlgorithmType.Sha256 }
            };

        // From SAS spec, table 17.1c
        private readonly Dictionary<AuthenticationMethods, int> _algorithmMaxSeedAndResultSize =
            new Dictionary<AuthenticationMethods, int>
            {
                { AuthenticationMethods.Crc16, 2 },
                { AuthenticationMethods.Md5, 16 },
                { AuthenticationMethods.Sha1, 20 },
                { AuthenticationMethods.Sha256, 32 }
            };

        private readonly SendAuthenticationInfoResponse _authResponseData = new SendAuthenticationInfoResponse
        {
            ComponentListCrc = 0,
            Status = AuthenticationStatus.InvalidCommand
        };

        private ComponentVerification _currentVerification = new ComponentVerification();
        private CancellationTokenSource _cancellationTokenSource;
        private bool _disposed;

        /// <summary>
        ///     Creates a new instance of the LP6ESendAuthenticationInfoHandler
        /// </summary>
        /// <param name="exceptionHandler">Instance of <see cref="ISasExceptionHandler"/></param>
        /// <param name="eventBus">Instance of <see cref="IEventBus"/></param>
        /// <param name="componentRegistry">Instance of <see cref="IComponentRegistry"/></param>
        /// <param name="authenticationService">Instance of <see cref="IAuthenticationService"/></param>
        public LP6ESendAuthenticationInfoHandler(
            ISasExceptionHandler exceptionHandler,
            IEventBus eventBus,
            IComponentRegistry componentRegistry,
            IAuthenticationService authenticationService)
        {
            _exceptionHandler = exceptionHandler ?? throw new ArgumentNullException(nameof(exceptionHandler));
            _eventBus = eventBus ?? throw new ArgumentNullException(nameof(eventBus));
            _componentRegistry = componentRegistry ?? throw new ArgumentNullException(nameof(componentRegistry));
            _authenticationService =
                authenticationService ?? throw new ArgumentNullException(nameof(authenticationService));

            _exceptionTimer = new SasExceptionTimer(
                _exceptionHandler,
                GetException,
                TimerActive,
                AuthenticationResultExceptionReportingIntervalMs);

            _eventBus.Subscribe<ComponentAddedEvent>(this, _ => UpdateComponentData());
            _eventBus.Subscribe<ComponentRemovedEvent>(this, _ => UpdateComponentData());
            _eventBus.Subscribe<ComponentHashCompleteEvent>(this, HandleComponentHashCompleteEvent);
            _eventBus.Subscribe<AllComponentsHashCompleteEvent>(this, HandleAllComponentsHashCompleteEvent);

            _authResponseData.Handlers = new HostAcknowledgementHandler { ImpliedAckHandler = null, ImpliedNackHandler = HandleNack };

            _algorithmMap.Keys.ToList().ForEach(a => AvailableAuthenticationMethods |= a);

            UpdateComponentData();
        }

        /// <inheritdoc/>
        public List<LongPoll> Commands { get; } = new List<LongPoll> { LongPoll.SendAuthenticationInformation };

        /// <summary>
        ///     Get which authentication methods are allowed.
        /// </summary>
        public AuthenticationMethods AvailableAuthenticationMethods { get; private set; } = AuthenticationMethods.None;

        /// <inheritdoc/>
        public SendAuthenticationInfoResponse Handle(SendAuthenticationInfoCommand data)
        {
            Console.WriteLine($"[{DateTime.Now}] - [{nameof(Handle)}-0] - [{data.Action}] - [{Environment.CurrentManagedThreadId}]");

            Logger.Debug($"Handling action {data.Action}");
            Component component;
            switch (data.Action)
            {
                case AuthenticationAction.InterrogateNumberOfInstalledComponents:
                    _authResponseData.ComponentSize = _componentRegistry.Components.ToList().Count;
                    _authResponseData.AvailableMethods = AuthenticationMethods.None;
                    _authResponseData.Status = AuthenticationStatus.InstalledComponentResponse;
                    Logger.Debug($"Report {_authResponseData.ComponentSize} components");
                    break;

                case AuthenticationAction.ReadStatusOfComponent:
                    component = FindAddressedComponent(data);
                    if (component == null)
                    {
                        _authResponseData.Status = AuthenticationStatus.ComponentDoesNotExist;
                        Logger.Debug(
                            $"Component {(data.AddressingMode == AuthenticationAddressingMode.AddressingByIndex ? data.ComponentIndex.ToString() : data.ComponentName)} not found; status unavailable");
                        break;
                    }

                    if (!component.Available)
                    {
                        _authResponseData.Status = AuthenticationStatus.ComponentDisabledOrUnavailable;
                        _authResponseData.ComponentName = component.ComponentId;
                        Logger.Debug(
                            $"Component {(data.AddressingMode == AuthenticationAddressingMode.AddressingByIndex ? data.ComponentIndex.ToString() : data.ComponentName)} not available; cannot authenticate");
                        break;
                    }

                    GenerateComponentStatus(component);
                    Logger.Debug("Report component status");
                    break;

                case AuthenticationAction.AuthenticateComponent:
                    component = FindAddressedComponent(data);

                    Console.WriteLine($"[{DateTime.Now}] - [{nameof(Handle)}-1] - [{data.Action}] - [{Environment.CurrentManagedThreadId}]");

                    if (component == null)
                    {
                        _authResponseData.Status = AuthenticationStatus.ComponentDoesNotExist;
                        Logger.Debug(
                            $"Component {(data.AddressingMode == AuthenticationAddressingMode.AddressingByIndex ? data.ComponentIndex.ToString() : data.ComponentName)} not found; cannot authenticate");
                        break;
                    }

                    // Component is disconnected.
                    if (!component.Available)
                    {
                        _authResponseData.Status = AuthenticationStatus.ComponentDisabledOrUnavailable;
                        Logger.Debug(
                            $"Component {(data.AddressingMode == AuthenticationAddressingMode.AddressingByIndex ? data.ComponentIndex.ToString() : data.ComponentName)} not available; cannot authenticate");
                        break;
                    }

                    if (component.HasFault)
                    {
                        _authResponseData.Status = AuthenticationStatus.ComponentCannotBeAuthenticatedNow;
                        Logger.Debug(
                            $"Component {(data.AddressingMode == AuthenticationAddressingMode.AddressingByIndex ? data.ComponentIndex.ToString() : data.ComponentName)} has a fault; cannot authenticate");
                        break;
                    }

                    if (!_algorithmMap.ContainsKey(data.Method))
                    {
                        Logger.Debug($"Authentication method {data.Method} is not supported");
                        _authResponseData.Status = AuthenticationStatus.RequestedAuthenticationMethodNotSupported;
                        break;
                    }

                    if (data.AuthenticationSeed.Length > _algorithmMaxSeedAndResultSize[data.Method] ||
                        data.AuthenticationOffset > component.Size)
                    {
                        Logger.Debug(
                            $"Authentication seed {data.AuthenticationSeed} or offset {data.AuthenticationOffset} is invalid");
                        _authResponseData.Status = AuthenticationStatus.InvalidDataForRequestedAuthenticationMethod;
                        break;
                    }

                    CancelCurrentAuthentication(false);

                    _cancellationTokenSource = new CancellationTokenSource();

                    // Start a signature task; wait for event to know when done.
                    _currentVerification = new ComponentVerification
                    {
                        ComponentId = component.ComponentId,
                        AlgorithmType = _algorithmMap[data.Method],
                        Result = new byte[0],
                        ResultTime = DateTime.MinValue
                    };

                    _authenticationService.GetComponentHashesAsync(
                            _currentVerification.AlgorithmType,
                            _cancellationTokenSource.Token,
                            data.AuthenticationSeed,
                            component.ComponentId,
                            data.AuthenticationOffset)
                        .Start();

                    _authResponseData.Status = AuthenticationStatus.AuthenticationCurrentlyInProgress;

                    Console.WriteLine($"[{DateTime.Now}] - [{nameof(Handle)}-2] - [{_authResponseData.Status}] - [{Environment.CurrentManagedThreadId}]");

                    Logger.Debug("Authentication started");
                    break;

                case AuthenticationAction.InterrogateAuthenticationStatus:
                    if (_authResponseData.Status == AuthenticationStatus.AuthenticationAborted)
                    {
                        break;
                    }

                    if (string.IsNullOrEmpty(_currentVerification.ComponentId))
                    {
                        _authResponseData.Status = AuthenticationStatus.NoAuthenticationDataAvailable;
                        Logger.Debug("No authentication component; cannot report status");
                        break;
                    }

                    var comp = _componentRegistry.Get(_currentVerification.ComponentId);

                    if (comp == null)
                    {
                        _authResponseData.Status = AuthenticationStatus.ComponentDoesNotExist;
                        Logger.Debug(
                            $"Component {(data.AddressingMode == AuthenticationAddressingMode.AddressingByIndex ? data.ComponentIndex.ToString() : data.ComponentName)} not found; cannot authenticate");
                        break;
                    }

                    GenerateComponentStatus(comp);
                    CancelTimer();
                    if (!comp.Available)
                    {
                        _authResponseData.Status = AuthenticationStatus.ComponentDisabledOrUnavailable;
                        _currentVerification = new ComponentVerification(); // Clear the details of the current authentication.
                        Logger.Debug(
                            $"Component {(data.AddressingMode == AuthenticationAddressingMode.AddressingByIndex ? data.ComponentIndex.ToString() : data.ComponentName)} not available; cannot authenticate");
                        break;
                    }
                    Logger.Debug("Report authentication status");
                    break;

                default:
                    _authResponseData.Status = AuthenticationStatus.InvalidCommand;
                    break;
            }

            return _authResponseData;
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        ///     Dispose
        /// </summary>
        /// <param name="disposing">Whether or not you are disposing</param>
        protected virtual void Dispose(bool disposing)
        {
            if (_disposed)
            {
                return;
            }

            if (disposing)
            {
                CancelTimer();
                _exceptionTimer.Dispose();

                if (_cancellationTokenSource != null)
                {
                    _cancellationTokenSource.Cancel();
                    _cancellationTokenSource.Dispose();
                    _cancellationTokenSource = null;
                }

                _eventBus.UnsubscribeAll(this);
            }

            _disposed = true;
        }

        private void UpdateComponentData()
        {
            CancelCurrentAuthentication(true);
            _cancellationTokenSource?.Dispose();
            _cancellationTokenSource = new CancellationTokenSource();

            // Create the string upon which the Component List CRC is calculated (p.
            var sb = new StringBuilder();
            _componentRegistry.Components.ToList().ForEach(c => sb.Append(c.ComponentId));

            var listBytes = Encoding.UTF8.GetBytes(sb.ToString());

            // Get CRC of the list
            _authResponseData.ComponentListCrc = Utilities.GenerateCrc(listBytes, (uint)listBytes.Length);

            Logger.Debug("Component list updated");

            _exceptionHandler.ReportException(new GenericExceptionBuilder(GeneralExceptionCode.ComponentListChanged));
        }

        private void GenerateComponentStatus(Component component)
        {
            Console.WriteLine($"[{DateTime.Now}] - [{nameof(GenerateComponentStatus)}-0] - [{_authResponseData.Status}] - [{Environment.CurrentManagedThreadId}]");

            _authResponseData.ComponentName = component.ComponentId;
            _authResponseData.ComponentSize = component.Size;
            _authResponseData.AvailableMethods = AvailableAuthenticationMethods;

            if (component.ComponentId == _currentVerification.ComponentId)
            {
                Console.WriteLine($"[{DateTime.Now}] - [{nameof(GenerateComponentStatus)}-1] - [{_authResponseData.Status}] - [{Environment.CurrentManagedThreadId}]");

                _authResponseData.Status = _currentVerification.ResultTime == DateTime.MinValue
                    ? AuthenticationStatus.AuthenticationCurrentlyInProgress
                    : AuthenticationStatus.AuthenticationComplete;

                // reverse lookup
                _authResponseData.Method = _algorithmMap.First(v => v.Value == _currentVerification.AlgorithmType).Key;
                _authResponseData.AuthenticationData = _currentVerification.Result;
            }
            else
            {
                _authResponseData.Status = (component.Available ? AuthenticationStatus.Success : AuthenticationStatus.ComponentDisabledOrUnavailable);
            }

            Console.WriteLine($"[{DateTime.Now}] - [{nameof(GenerateComponentStatus)}-2] - [{_authResponseData.Status}] - [{Environment.CurrentManagedThreadId}]");

        }

        private Component FindAddressedComponent(SendAuthenticationInfoCommand data)
        {
            var compName = data.ComponentName;
            if (data.AddressingMode == AuthenticationAddressingMode.AddressingByIndex)
            {
                if (data.ComponentIndex < 1 || data.ComponentIndex > _componentRegistry.Components.Count())
                {
                    _authResponseData.Status = AuthenticationStatus.ComponentDoesNotExist;
                    return null;
                }

                compName = _componentRegistry.Components.ElementAt(data.ComponentIndex - 1).ComponentId;
            }

            return _componentRegistry.Get(compName);
        }

        private void HandleNack()
        {
            // cancel in progress authentications
            CancelCurrentAuthentication(false);

            _currentVerification.ComponentId = string.Empty;
        }

        private void HandleComponentHashCompleteEvent(ComponentHashCompleteEvent evt)
        {
            Console.WriteLine($"[{DateTime.Now}] - [{nameof(HandleComponentHashCompleteEvent)}-0] - [{_authResponseData.Status}] - [{Environment.CurrentManagedThreadId}]");

            if (_cancellationTokenSource == null || evt.TaskToken != _cancellationTokenSource.Token)
            {
                Logger.Debug("Unexpected ComponentHashCompleteEvent");
                return;
            }

            Logger.Debug($"receive ComponentHashCompleteEvent with id={evt.ComponentVerification.ComponentId}");
            _currentVerification = evt.ComponentVerification;
            StartTimer();

            Console.WriteLine($"[{DateTime.Now}] - [{nameof(HandleComponentHashCompleteEvent)}-1] - [{_authResponseData.Status}] - [{Environment.CurrentManagedThreadId}]");
        }

        private void HandleAllComponentsHashCompleteEvent(AllComponentsHashCompleteEvent evt)
        {
            if (_cancellationTokenSource == null || evt.TaskToken != _cancellationTokenSource.Token)
            {
                return;
            }

            if (evt.Cancelled)
            {
                CancelCurrentAuthentication(false);
                _authResponseData.Status = AuthenticationStatus.ComponentDisabledOrUnavailable;
            }
        }

        private void CancelCurrentAuthentication(bool cancelledByComponentListChange)
        {
            Console.WriteLine($"[{DateTime.Now}] - [{nameof(CancelCurrentAuthentication)}-0] - [{cancelledByComponentListChange}] - [{Environment.CurrentManagedThreadId}]");

            if (_currentVerification.ResultTime == DateTime.MinValue &&
                !string.IsNullOrEmpty(_currentVerification.ComponentId))
            {
                _cancellationTokenSource?.Cancel();
                CancelTimer();

                Console.WriteLine($"[{DateTime.Now}] - [{nameof(CancelCurrentAuthentication)}-1] - [{_authResponseData.Status}] - [{Environment.CurrentManagedThreadId}]");

                if (cancelledByComponentListChange)
                {
                    _authResponseData.Status = AuthenticationStatus.AuthenticationAborted;

                    Console.WriteLine($"[{DateTime.Now}] - [{nameof(CancelCurrentAuthentication)}-2] - [{_authResponseData.Status}] - [{Environment.CurrentManagedThreadId}]");
                }
            }
        }

        private static GeneralExceptionCode? GetException()
        {
            return GeneralExceptionCode.AuthenticationComplete;
        }

        private void StartTimer()
        {
            _exceptionTimer.StartTimer();
        }

        private void CancelTimer()
        {
            _exceptionTimer.StopTimer();
        }

        private bool TimerActive() => true;
    }
}