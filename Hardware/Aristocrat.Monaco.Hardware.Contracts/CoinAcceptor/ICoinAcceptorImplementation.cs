namespace Aristocrat.Monaco.Hardware.Contracts.CoinAcceptor
{
    using System;

    /// <summary>Interface for coin acceptor implementation.</summary>
    public interface ICoinAcceptorImplementation : IGdsDevice
    {
        /// <summary>Enable Reject Mechanishm.</summary>
        void CoinRejectMechOn();

        /// <summary>Disable Reject Mechanishm</summary>
        void CoinRejectMechOff();

        /// <summary>Enable divertor towards hopper.</summary>
        void DivertToHopper();

        /// <summary>Enable divertor towards cashbox.</summary>
        void DivertToCashbox();

        /// <summary>Reset the device.</summary>
        void DeviceReset();

        /// <summary>Event to report coin in.</summary>
        event EventHandler<CoinEventType> CoinInStatusReported;

        /// <summary>Event fired when fault is detected.</summary>
        event EventHandler<CoinFaultTypes> FaultOccurred;

        /// <summary>Gets the current faults.</summary>
        CoinFaultTypes Faults { get; set; }
    }
}
