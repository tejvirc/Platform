namespace Aristocrat.Monaco.Hardware.Contracts.Hopper
{
    using System;

    /// <summary> Interface for hopper implementation. /// </summary>
    public interface IHopperImplementation : IGdsDevice
    {

        /// <summary>Start Hopper motor to dispense coins.</summary>
        void StartHopperMotor();

        /// <summary>Stop Hopper motor.</summary>
        void StopHopperMotor();

        /// <summary>Reset hopper .</summary>
        void DeviceReset();

        /// <summary>Max amount which can hopper payout.</summary>
        void SetMaxCoinoutAllowed(int amount);

        /// <summary>Get the status report of hopper.</summary>
        byte GetStatusReport();

        /// <summary>Event fired to report coin out status.</summary>
        event EventHandler<CoinOutEventType> CoinOutStatusReported;

        /// <summary>Event fired when fault is detected.</summary>
        event EventHandler<HopperFaultTypes> FaultOccurred;

        /// <summary>Gets the current faults.</summary>
        /// <value>The current faults.</value>
        HopperFaultTypes Faults { get; set; }
    }
}
