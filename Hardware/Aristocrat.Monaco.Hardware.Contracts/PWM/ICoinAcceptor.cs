﻿namespace Aristocrat.Monaco.Hardware.Contracts.PWM
{
    using SharedDevice;
    /// <summary>
    /// 
    /// </summary>
    public interface ICoinAcceptor : IDeviceAdapter
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
