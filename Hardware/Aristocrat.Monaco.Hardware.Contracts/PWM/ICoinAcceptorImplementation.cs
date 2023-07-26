namespace Aristocrat.Monaco.Hardware.Contracts
{
    using System;
    using PWM;

    /// <summary>
    /// 
    /// </summary>
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

        /// <summary>Event fired when fault is cleared.</summary>
        event EventHandler<CoinEventType> CoinInStatusReported;

        /// <summary>Event fired when fault is detected.</summary>
        event EventHandler<CoinFaultTypes> FaultOccurred;

        /// <summary>Gets the current faults.</summary>
        /// <value>The current faults.</value>
        CoinFaultTypes Faults { get; set; }
    }
}
