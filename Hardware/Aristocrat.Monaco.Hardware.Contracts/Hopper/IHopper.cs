namespace Aristocrat.Monaco.Hardware.Contracts.Hopper
{
    using SharedDevice;

    /// <summary> Hopper type to decide the type of hopper /// </summary>
    public enum HopperType
    {
        /// <summary> Undefine type</summary>
        Undefine,
        /// <summary> Himec type</summary>
        Himec,
        /// <summary> Aristocrat type</summary>
        Aristocrat
    }

    /// <summary> Interface for Hopper./// </summary>
    public interface IHopper: IDeviceAdapter
    {
        /// <summary>Gets or sets the id of the hopper.</summary>
        int HopperId { get; set; }
   
        /// <summary>Start Hopper motor to dispense coins.</summary>
        void StartHopperMotor();

        /// <summary>Stop Hopper motor.</summary>
        void StopHopperMotor();

        /// <summary>Reset hopper .</summary>
        void Reset();

        /// <summary>Max amount which can hopper payout.</summary>
        void SetMaxCoinoutAllowed(int amount);

        /// <summary>Get the hopper full status.</summary>
        bool IsHopperFull { get; }

        /// <summary>Gets the current faults.</summary>
        /// <value>The current faults.</value>
        HopperFaultTypes Faults { get; set; }
    }
}
