namespace Aristocrat.Monaco.Asp.Client.Contracts
{
    /// <summary>
    ///     Interface to check for if a configuration of the set event report to a specific parameter is applicable to all device types
    ///     see chapter 4. Device Class 0x03 (EGM Game) ASP5000 Device Definition Tables
    /// </summary>

    public interface IDeviceClassFeature
    {
        /// <summary>
        ///     Gets a value indicating whether an event report for a parameter required to share with all other device types or not
        /// </summary>
        bool SharedDeviceTypeEventReport { get; }
    }
}

