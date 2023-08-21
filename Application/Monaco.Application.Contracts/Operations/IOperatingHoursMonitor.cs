namespace Aristocrat.Monaco.Application.Contracts.Operations
{
    using System;

    /// <summary>
    ///     The definition of OperatingHoursMonitor interface
    /// </summary>
    [CLSCompliant(false)]
    public interface IOperatingHoursMonitor
    {
        /// <summary>
        ///     return true if it is outside operation hours; false if it is in operation hours
        /// </summary>
        bool OutsideOperatingHours { get; }
    }
}
