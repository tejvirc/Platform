namespace Aristocrat.Monaco.Hardware.Contracts.Hopper
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    /// <summary>
    /// 
    /// </summary>
    public interface IHopperImplementation : IGdsDevice
    {
        /// <summary>Enable Hopper.</summary>
        bool EnableHopper();
        /// <summary>Disable Hopper.</summary>
        bool DisableHopper();
        /// <summary>Start Hopper motor to dispense coins.</summary>
        void StartHopperMotor();
        /// <summary>Stop Hopper motor.</summary>
        void StopHopperMotor();
        /// <summary>Check if hopper is connected or not.</summary>
        bool HopperTestDisconnection();
        /// <summary>Check if hopper is connected or not.</summary>
        bool SetHopperType(HopperType type);
        /// <summary>Reset hopper .</summary>
        void DeviceReset();
        /// <summary>Max amount which can hopper payout.</summary>
        void SetMaxCoinoutAllowed(int amount);
        /// <summary>Get the status report of hopper.</summary>
        byte GetStatusReport();

        /// <summary>Gets the current faults.</summary>
        /// <value>The current faults.</value>
        HopperFaultTypes Faults { get; set; }
    }
}
