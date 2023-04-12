namespace Aristocrat.Monaco.Hardware.Contracts.HardMeter
{
    using System.Collections.Generic;
    using SharedDevice;

    /// <summary>Logical <c>HardMeter</c> state enumerations.</summary>
    public enum HardMeterLogicalState
    {
        /// <summary>Indicates HardMeter state uninitialized.</summary>
        Uninitialized = 0,

        /// <summary>Indicates HardMeter state idle.</summary>
        Idle,

        /// <summary>Indicates HardMeter state error.</summary>
        Error,

        /// <summary>Indicates HardMeter state disabled.</summary>
        Disabled
    }

    /// <summary>Definition of the IHardMeter interface.</summary>
    public interface IHardMeter : IDeviceService
    {
        /// <summary>Gets the dictionary of logical hard meters.</summary>
        /// <returns>Dictionary of logical hard meters.</returns>
        Dictionary<int, LogicalHardMeter> LogicalHardMeters { get; }

        /// <summary>Gets the logical hard meter service state.</summary>
        /// <returns>Logical hard meter service state.</returns>
        HardMeterLogicalState LogicalState { get; }

        /// <summary>Advance the hard meter for the given logical hard meter ID.</summary>
        /// <param name="hardMeterId">The logical hard meter ID.</param>
        /// <param name="value">The amount to advance the meter.</param>
        void AdvanceHardMeter(int hardMeterId, long value);

        /// <summary>Gets hard meter action enumeration value for the given logical hard meter ID.</summary>
        /// <param name="hardMeterId">The logical hard meter ID.</param>
        /// <returns>An enumerated hard meter action.</returns>
        HardMeterAction GetHardMeterAction(int hardMeterId);

        /// <summary>Gets the hard meter name for the given logical hard meter ID.</summary>
        /// <param name="hardMeterId">The logical hard meter ID.</param>
        /// <returns>The hard meter name.</returns>
        string GetHardMeterName(int hardMeterId);

        /// <summary>Gets the physical hard meter ID for the given logical hard meter ID.</summary>
        /// <param name="hardMeterId">The logical hard meter ID.</param>
        /// <returns>The physical hard meter ID.</returns>
        int GetHardMeterPhysicalId(int hardMeterId);

        /// <summary>Gets the hard meter state enumeration value for the given logical hard meter ID.</summary>
        /// <param name="hardMeterId">The logical hard meter ID.</param>
        /// <returns>An enumerated hard meter state.</returns>
        HardMeterState GetHardMeterState(int hardMeterId);

        /// <summary>Gets the localized hard meter name for the given logical hard meter ID.</summary>
        /// <param name="hardMeterId">The logical hard meter ID.</param>
        /// <returns>The localized hard meter name.</returns>
        string GetLocalizedHardMeterName(int hardMeterId);

        /// <summary> Scales HardMeter TickValue with appropriate values. </summary>
        /// <param name="listOfMeters"></param>
        void UpdateTickValues(Dictionary<int, long> listOfMeters);

        /// <summary> Gets HardMeter value for a given logical meter id</summary>
        /// <param name="hardMeterId">Logical Id of HardMeter</param>
        /// <returns></returns>
        long GetHardMeterValue(int hardMeterId);

        /// <summary>Check hardware status</summary>
        bool IsHardwareOperational { get; }
    }
}