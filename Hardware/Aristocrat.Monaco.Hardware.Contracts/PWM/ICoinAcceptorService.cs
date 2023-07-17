namespace Aristocrat.Monaco.Hardware.Contracts.PWM
{
    /// <summary>
    ///     The service interface used to manage coin acceptor devices.
    /// </summary>
    public interface ICoinAcceptorService : IPwmDeviceService
    {
        /// <summary>Enable Reject Mechanishm.</summary>
        bool CoinRejectMechOn();

        /// <summary>Disable Reject Mechanishm</summary>
        bool CoinRejectMechOff();

        /// <summary>Enable divertor towards hopper.</summary>
        bool DivertToHopper();

        /// <summary>Enable divertor towards cashbox.</summary>
        bool DivertToCashbox();

        /// <summary>Reset the device.</summary>
        void Reset();

        /// <summary>Enable diverter towards hopper or cashbox.</summary>
        void DivertMechanismOnOff();

        /// <summary>Gets the current direction of the diverter.</summary>
        DivertorState DiverterDirection { get; }

        /// <summary>Gets the current faults.</summary>
        /// <value>The current faults.</value>
        CoinFaultTypes Faults { get; set; }
    }
}
