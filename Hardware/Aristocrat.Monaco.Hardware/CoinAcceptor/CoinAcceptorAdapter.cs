﻿
namespace Aristocrat.Monaco.Hardware.CoinAcceptor
{
    using Aristocrat.Monaco.Common;
    using Aristocrat.Monaco.Hardware.Contracts;
    using Aristocrat.Monaco.Hardware.Contracts.Communicator;
    using Aristocrat.Monaco.Hardware.Contracts.PWM;
    using Aristocrat.Monaco.Hardware.Contracts.SharedDevice;
    using Aristocrat.Monaco.Kernel;
    using Aristocrat.Monaco.Kernel.Contracts.Components;
    using log4net;
    using System;
    using System.Collections.Generic;
    using System.Reflection;
    /// <summary>
    /// 
    /// </summary>
    public class CoinAcceptorAdapter : DeviceAdapter<ICoinAcceptorImplementation>, ICoinAcceptor
    {
        private const string DeviceImplementationsExtensionPath = "/Hardware/CoinAcceptor/CoinAcceptorImplementations";
        private ICoinAcceptorImplementation _coinAcceptor;
        private readonly IEventBus _bus;

        private static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod()!.DeclaringType);

        public CoinAcceptorAdapter()
        {
            _bus = ServiceManager.GetInstance().GetService<IEventBus>();
        }

        public override DeviceType DeviceType => DeviceType.CoinAcceptor;

        public override string Name => string.IsNullOrEmpty(ServiceProtocol) == false
            ? $"{ServiceProtocol} Coin Acceptor Service"
            : "Unknown Coin Acceptor Service";

        public override bool Connected => Implementation?.IsConnected ?? false;

        public override ICollection<Type> ServiceTypes => new[] { typeof(ICoinAcceptor) };

        protected override ICoinAcceptorImplementation Implementation => _coinAcceptor;

        protected override string Description => "Coin Acceptor";

        protected override string Path => Kernel.Contracts.Components.Constants.CoinAcceptorPath;

        public DivertorState DiverterDirection { get; set; }
        public AcceptorState InputState { get; set; }


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


        public int CoinAcceptorId { get; set; } = 1; // Default to deviceId 1 since 0 isn't valid in G2S
        protected override void DisabledDetected()
        {
            PostEvent(new DisabledEvent(CoinAcceptorId, ReasonDisabled));
        }

        protected override void Disabling(DisabledReasons reason)
        {
            CoinRejectMechOn();

        }

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

        protected override void SubscribeToEvents(IEventBus eventBus)
        {
        }

        public void CoinRejectMechOn()
        {
            InputState = AcceptorState.Reject;
            Implementation.CoinRejectMechOn();
        }

        public void CoinRejectMechOff()
        {
            InputState = AcceptorState.Accept;
            Implementation.CoinRejectMechOff();
        }

        public void DivertToHopper()
        {
            DiverterDirection = DivertorState.DivertToHopper;
            Implementation.DivertToHopper();
        }

        public void DivertToCashbox()
        {
            DiverterDirection = DivertorState.DivertToCashbox;
            Implementation.DivertToCashbox();
        }

        public void Reset()
        {
            Implementation.DeviceReset();
            DivertMechanismOnOff();
        }

        public void DivertMechanismOnOff()
        {

            //TODO: implement hopper's properties with realtime values once hopper feature is available..
            bool isHopperInstalled = false;
            //bool isHopperFull = false;

           // if (PropertiesManager.GetValue(HardwareConstants.HopperEnabledKey, false) && isHopperInstalled && (!isHopperFull))
            if(!isHopperInstalled)
            {
                DivertToHopper();
            }
            else
            {
                DivertToCashbox();
            }
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
                        _bus?.Publish(new CoinInEvent(new Coin {Value = 100000}));
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
