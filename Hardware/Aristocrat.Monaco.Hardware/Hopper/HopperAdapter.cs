namespace Aristocrat.Monaco.Hardware.Hopper
{
    using System;
    using System.Collections.Generic;
    using System.Reflection;
    using Common;
    using Contracts;
    using Contracts.Communicator;
    using Hardware.Contracts.Hopper;
    using Contracts.SharedDevice;
    using Kernel;
    using log4net;

    /// <summary>A hopper adapter.</summary>
    /// <seealso
    ///     cref="T:Aristocrat.Monaco.Hardware.Contracts.SharedDevice.DeviceAdapter{Aristocrat.Monaco.Hardware.Contracts.Hopper.IHopperImplementation}" />
    /// <seealso cref="T:Aristocrat.Monaco.Hardware.Contracts.Hopper.IHopper" />
    public class HopperAdapter : DeviceAdapter<IHopperImplementation>, IHopper
    {
        private const long DefaultTokenValue = 100000L;
        private static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod()!.DeclaringType);
        private const string DeviceImplementationsExtensionPath = "/Hardware/Hopper/HopperImplementations";
        private IHopperImplementation _hopper;
        private readonly IEventBus _bus;
        private readonly long _tokenValue;

        /// <summary>
        ///     Initializes a new instance of the Aristocrat.Monaco.Hardware.CoinAcceptor.CoinAcceptorAdapter class.
        /// </summary>
        public HopperAdapter()
        {
            _bus = ServiceManager.GetInstance().GetService<IEventBus>();
            var properties = ServiceManager.GetInstance().GetService<IPropertiesManager>();
            _tokenValue = properties.GetValue(HardwareConstants.CoinValue, DefaultTokenValue);
        }

        /// <inheritdoc />
        public override DeviceType DeviceType => DeviceType.Hopper;

        /// <inheritdoc />
        public bool IsHopperFull => Implementation?.IsHopperFull ?? false;

        /// <inheritdoc />
        public override string Name => !string.IsNullOrEmpty(ServiceProtocol)
            ? $"{ServiceProtocol} Hopper Service"
            : "Unknown Hopper Service";

        /// <inheritdoc />
        public override bool Connected => Implementation?.IsConnected ?? false;

        /// <inheritdoc />
        public override ICollection<Type> ServiceTypes => new[] { typeof(IHopper) };

        /// <inheritdoc />
        public int HopperId { get; set; } = 1; // Default to deviceId 1 since 0 isn't valid in G2S

        /// <inheritdoc />
        public HopperFaultTypes Faults
        {
            get
            {
                return Implementation?.Faults ?? HopperFaultTypes.None;
            }
            set
            {
                if (Implementation != null)
                {
                    Implementation.Faults = value;
                }
            }
        }

        /// <inheritdoc/>
        public void StartHopperMotor()
        {
            Implementation.StartHopperMotor();
        }

        /// <inheritdoc/>

        public void StopHopperMotor()
        {
            Implementation.StopHopperMotor();
        }

        /// <inheritdoc/>
        public void Reset()
        {
            Implementation.DeviceReset();
        }

        /// <inheritdoc/>
        public void SetMaxCoinoutAllowed(int amount)
        {
            Implementation.SetMaxCoinoutAllowed(amount);
        }

        /// <inheritdoc />
        protected override IHopperImplementation Implementation => _hopper;

        /// <inheritdoc />
        protected override string Description => "Hopper";

        /// <inheritdoc />
        protected override string Path => Kernel.Contracts.Components.Constants.HopperPath;

        /// <inheritdoc />
        protected override void DisabledDetected()
        {
            PostEvent(new DisabledEvent(HopperId, ReasonDisabled));
        }

        /// <inheritdoc />
        protected override void Disabling(DisabledReasons reason)
        {
            //There is nothing to disable the hopper
        }

        /// <inheritdoc />
        protected override void Enabling(EnabledReasons reason, DisabledReasons remedied)
        {
            //There is nothing to enable the hopper
        }

        /// <inheritdoc />
        protected override void Initializing()
        {
            // Load an instance of the given protocol implementation.
            _hopper = AddinFactory.CreateAddin<IHopperImplementation>(
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
            Implementation.CoinOutStatusReported += ImplementationStatusReported;
        }

        /// <inheritdoc/>
        protected override void Inspecting(IComConfiguration comConfiguration, int timeout)
        {
        }

        /// <inheritdoc/>
        protected override void SubscribeToEvents(IEventBus eventBus)
        {
        }

        private void ImplementationStatusReported(object sender, CoinOutEventType type)
        {
            Logger.Info("ImplementationStatusReportedd: coin in event reported");

            switch (type)
            {
                case CoinOutEventType.LegalCoinOut:
                    {
                        _bus?.Publish(new CoinOutEvent(new Contracts.CoinAcceptor.Coin { Value = _tokenValue }));
                        break;
                    }
                case CoinOutEventType.IllegalCoinOut:
                    {
                        ImplementationStatusFaultOccurred(sender, HopperFaultTypes.IllegalCoinOut);
                        break;
                    }
                default:
                    throw new NotSupportedException();
            }
        }

        private void ImplementationStatusFaultOccurred(object sender, HopperFaultTypes type)
        {
            Logger.Info("ImplementationStatusFaultOccurred: device fault occured");
            _bus?.Publish(new HardwareFaultEvent(type));
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

            PostEvent(new InspectedEvent(HopperId));
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
            PostEvent(new InspectionFailedEvent(HopperId));
        }
    }
}