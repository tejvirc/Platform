namespace Aristocrat.Monaco.Hardware.Hopper
{
    using Aristocrat.Monaco.Common;
    using Aristocrat.Monaco.Hardware.Contracts;
    using Aristocrat.Monaco.Hardware.Contracts.Communicator;
    using Aristocrat.Monaco.Hardware.Contracts.Hopper;
    using Aristocrat.Monaco.Hardware.Contracts.SharedDevice;
    using Aristocrat.Monaco.Kernel;
    using log4net;
    using System;
    using System.Collections.Generic;
    using System.Reflection;

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
        public override string Name => !string.IsNullOrEmpty(ServiceProtocol)
            ? $"{ServiceProtocol} Hopper Service"
            : "Unknown Hopper Service";

        /// <inheritdoc />
        public override bool Connected => Implementation?.IsConnected ?? false;

        /// <inheritdoc />
        public override ICollection<Type> ServiceTypes => new[] { typeof(IHopper) };

        /// <inheritdoc />
        public int HopperId { get; set; } = 1; // Default to deviceId 1 since 0 isn't valid in G2S

        protected override IHopperImplementation Implementation => _hopper;

        /// <inheritdoc />
        protected override string Description => "Hopper";

        /// <inheritdoc />
        protected override string Path => Kernel.Contracts.Components.Constants.HopperPath;

        public HopperFaultTypes Faults { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        protected override void DisabledDetected()
        {
            PostEvent(new DisabledEvent(HopperId, ReasonDisabled));
        }

        protected override void Disabling(DisabledReasons reason)
        {
            //There is nothing to diable the hopper
        }

        protected override void Enabling(EnabledReasons reason, DisabledReasons remedied)
        {
            //There is nothing to enable the hopper
        }

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
          //  Implementation.FaultOccurred += ImplementationStatusFaultOccurred;
            //Implementation.CoinInStatusReported += ImplementationStatusReportedd;
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
        /// <inheritdoc/>
        protected override void Inspecting(IComConfiguration comConfiguration, int timeout)
        {
        }

        /// <inheritdoc/>
        protected override void SubscribeToEvents(IEventBus eventBus)
        {
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

        /// <inheritdoc/>
        public byte GetStatusReport()
        {
            return Implementation.GetStatusReport();
        }
    }
}
