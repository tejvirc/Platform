
namespace Aristocrat.Monaco.Hardware.CoinAcceptor
{
    using System;
    using System.Collections.Generic;
    using System.Reflection;
    using Common;
    using Contracts;
    using Contracts.CoinAcceptor;
    using Contracts.Communicator;
    using Contracts.SharedDevice;
    using Kernel;
    using log4net;

    /// <summary>A coin acceptor adapter.</summary>
    /// <seealso
    ///     cref="T:Aristocrat.Monaco.Hardware.Contracts.SharedDevice.DeviceAdapter{Aristocrat.Monaco.Hardware.Contracts.CoinAcceptor.ICoinAcceptorImplementation}" />
    /// <seealso cref="T:Aristocrat.Monaco.Hardware.Contracts.CoinAcceptor.ICoinAcceptor" />
    public class CoinAcceptorAdapter : DeviceAdapter<ICoinAcceptorImplementation>, ICoinAcceptor
    {
        private const long DefaultTokenValue = 100000L;
        private static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod()!.DeclaringType);
        private const string DeviceImplementationsExtensionPath = "/Hardware/CoinAcceptor/CoinAcceptorImplementations";
        private ICoinAcceptorImplementation _coinAcceptor;
        private readonly IEventBus _bus;
        private long _tokenValue;

        /// <summary>
        ///     Initializes a new instance of the Aristocrat.Monaco.Hardware.CoinAcceptor.CoinAcceptorAdapter class.
        /// </summary>
        public CoinAcceptorAdapter()
        {
            _bus = ServiceManager.GetInstance().GetService<IEventBus>();
            var properties = ServiceManager.GetInstance().GetService<IPropertiesManager>();
            _tokenValue = properties.GetValue(HardwareConstants.CoinValue, DefaultTokenValue);
        }

        /// <inheritdoc />
        public override DeviceType DeviceType => DeviceType.CoinAcceptor;

        /// <inheritdoc />
        public override string Name => string.IsNullOrEmpty(ServiceProtocol) == false
            ? $"{ServiceProtocol} Coin Acceptor Service"
            : "Unknown Coin Acceptor Service";

        /// <inheritdoc />
        public override bool Connected => Implementation?.IsConnected ?? false;

        /// <inheritdoc />
        public override ICollection<Type> ServiceTypes => new[] { typeof(ICoinAcceptor) };

        /// <inheritdoc />
        public DivertorState DiverterDirection { get; set; }

        /// <inheritdoc />
        public AcceptorState InputState { get; set; }

        /// <inheritdoc />
        public CoinFaultTypes Faults
        {
            get
            {
                return Implementation?.Faults ?? CoinFaultTypes.None;
            }
            set
            {
                if (Implementation != null)
                {
                    Implementation.Faults = value;
                }
            }
        }

        /// <inheritdoc />
        public int CoinAcceptorId { get; set; } = 1; // Default to deviceId 1 since 0 isn't valid in G2S

        /// <inheritdoc />
        public void CoinRejectMechOn()
        {
            InputState = AcceptorState.Reject;
            Implementation.CoinRejectMechOn();
        }

        /// <inheritdoc />
        public void CoinRejectMechOff()
        {
            InputState = AcceptorState.Accept;
            Implementation.CoinRejectMechOff();
        }

        /// <inheritdoc />
        public void DivertToHopper()
        {
            DiverterDirection = DivertorState.DivertToHopper;
            Implementation.DivertToHopper();
        }

        /// <inheritdoc />
        public void DivertToCashbox()
        {
            DiverterDirection = DivertorState.DivertToCashbox;
            Implementation.DivertToCashbox();
        }

        /// <inheritdoc />
        public void Reset()
        {
            Implementation.DeviceReset();
            DivertMechanismOnOff();
        }

        /// <inheritdoc />
        public void DivertMechanismOnOff()
        {

            //TODO: implement hopper's properties with realtime values once hopper feature is available..
            bool isHopperInstalled = false;
            //bool isHopperFull = false;

            // if (PropertiesManager.GetValue(HardwareConstants.HopperEnabledKey, false) && isHopperInstalled && (!isHopperFull))
            if (!isHopperInstalled)
            {
                DivertToHopper();
            }
            else
            {
                DivertToCashbox();
            }
        }

        /// <inheritdoc />
        protected override ICoinAcceptorImplementation Implementation => _coinAcceptor;

        /// <inheritdoc />
        protected override string Description => "Coin Acceptor";

        /// <inheritdoc />
        protected override string Path => Kernel.Contracts.Components.Constants.CoinAcceptorPath;

        /// <inheritdoc />
        protected override void DisabledDetected()
        {
            PostEvent(new DisabledEvent(CoinAcceptorId, ReasonDisabled));
        }

        /// <inheritdoc />
        protected override void Disabling(DisabledReasons reason)
        {
            CoinRejectMechOn();

        }

        /// <inheritdoc />
        protected override void Enabling(EnabledReasons reason, DisabledReasons remedied)
        {
            if (Enabled)
            {
                CoinRejectMechOff();
            }
            else
            {
                CoinRejectMechOn();
            }
        }

        /// <inheritdoc />
        protected override void Initializing()
        {
            // Load an instance of the given protocol implementation.
            _coinAcceptor = AddinFactory.CreateAddin<ICoinAcceptorImplementation>(
                DeviceImplementationsExtensionPath,
                ServiceProtocol);
            if (Implementation == null)
            {
                var errorMessage = $"Cannot load {Name}";
                Logger.Fatal(errorMessage);
                throw new ServiceException(errorMessage);
            }

            Implementation.Initialized += ImplementationInitialized;
            Implementation.InitializationFailed += ImplementationInitializationFailed;
            Implementation.FaultOccurred += ImplementationStatusFaultOccurred;
            Implementation.CoinInStatusReported += ImplementationStatusReportedd;
        }

        /// <inheritdoc />
        protected override void Inspecting(IComConfiguration comConfiguration, int timeout)
        {
        }

        /// <inheritdoc />
        protected override void SubscribeToEvents(IEventBus eventBus)
        {
        }

        /// <inheritdoc />
        protected override void Dispose(bool disposing)
        {
            if (Disposed)
            {
                return;
            }

            if (disposing)
            {
                Disable(DisabledReasons.Service);
                if (Implementation != null)
                {
                    Implementation.Connected -= ImplementationConnected;
                    Implementation.Disconnected -= ImplementationDisconnected;
                    Implementation.Initialized -= ImplementationInitialized;
                    Implementation.InitializationFailed -= ImplementationInitializationFailed;
                    Implementation.Dispose();
                    _coinAcceptor = null;
                }
            }

            base.Dispose(disposing);
        }

        private void ImplementationInitialized(object sender, EventArgs e)
        {
            if ((ReasonDisabled & DisabledReasons.Device) != 0)
            {
                Enable(EnabledReasons.Device);
            }

            // If we are also disabled for error, clear it so that we enable for reset below.
            if ((ReasonDisabled & DisabledReasons.Error) != 0)
            {
                ClearError(DisabledReasons.Error);
            }

            SetInternalConfiguration();
            Implementation?.UpdateConfiguration(InternalConfiguration);
            RegisterComponent();
            Initialized = true;

            PostEvent(new InspectedEvent(CoinAcceptorId));
            if (Enabled)
            {
                Implementation?.Enable()?.WaitForCompletion();
            }
            else
            {
                DisabledDetected();
                Implementation?.Disable()?.WaitForCompletion();
            }
        }

        private void ImplementationInitializationFailed(object sender, EventArgs e)
        {
            if (Implementation != null)
            {
                SetInternalConfiguration();

                Implementation.UpdateConfiguration(InternalConfiguration);
            }

            Logger.Warn("Coin Acceptor InitializationFailed - Inspection Failed");
            Disable(DisabledReasons.Device);

            PostEvent(new InspectionFailedEvent(CoinAcceptorId));
        }

        private void ImplementationStatusReportedd(object sender, CoinEventType type)
        {
            Logger.Info("ImplementationStatusReportedd: coin in event reported");

            switch (type)
            {
                case CoinEventType.CoinInEvent:
                    {
                        _bus?.Publish(new CoinInEvent(new Coin { Value = _tokenValue }));
                        break;
                    }
                case CoinEventType.CoinToCashboxInEvent:
                    {
                        _bus?.Publish(new CoinToCashboxInEvent());
                        break;
                    }
                case CoinEventType.CoinToCashboxInsteadOfHopperEvent:
                    {
                        _bus?.Publish(new CoinToCashboxInsteadOfHopperEvent());
                        break;
                    }

                case CoinEventType.CoinToHopperInEvent:
                    {
                        _bus?.Publish(new CoinToHopperInEvent());
                        break;
                    }
                case CoinEventType.CoinToHopperInsteadOfCashboxEvent:
                    {
                        _bus?.Publish(new CoinToHopperInsteadOfCashboxEvent());
                        break;
                    }
                default:
                    throw new NotImplementedException();
            }
        }

        private void ImplementationStatusFaultOccurred(object sender, CoinFaultTypes type)
        {
            Logger.Info("ImplementationStatusFaultOccurred: device fault occured");
            _bus?.Publish(new HardwareFaultEvent(type));
        }

        private void ImplementationConnected(object sender, EventArgs e)
        {
            Logger.Info("ImplementationConnected: device connected");
            Reset();
            CoinRejectMechOn();
        }


        private void ImplementationDisconnected(object sender, EventArgs e)
        {
            Logger.Warn("ImplementationDisconnected: device disconnected");
        }

    }
}
