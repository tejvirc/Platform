namespace Aristocrat.Monaco.Hardware.Contracts.Communicator
{
    using System;

    /// <summary>
    ///     Definition of the IHidDriver interface.
    /// </summary>
    public interface IHidDriver : IDisposable
    {
        /// <summary>
        ///     Gets the maximum input report length.
        /// </summary>
        int InputReportLength { get; }

        /// <summary>
        ///     Gets the maximum output report length.
        /// </summary>
        int OutputReportLength { get; }

        /// <summary>
        ///     Gets the maximum feature report length.
        /// </summary>
        int FeatureReportLength { get; }

        /// <summary>
        ///     Event that is fired whenever a report is received from the HID communication channel.
        /// </summary>
        event EventHandler<ReportEventArgs> ReportReceived;

        /// <summary>
        ///     Sends a feature report over the HID communication channel.
        /// </summary>
        /// <param name="buffer">The byte array buffer to send.</param>
        /// <returns><b>true</b> if successful; otherwise <b>false</b>.</returns>
        bool SetFeature(byte[] buffer);

        /// <summary>Gets a feature.</summary>
        /// <param name="reportId">Identifier for the report.</param>
        /// <returns>An array of byte.</returns>
        byte[] GetFeature(byte reportId);

        /// <summary>
        ///     Sends an output report over the HID communication channel.
        /// </summary>
        /// <param name="buffer">The byte array buffer to send.</param>
        /// <returns><b>true</b> if successful; otherwise <b>false</b>.</returns>
        bool SetOutputReport(byte[] buffer);

        /// <summary>Gets input report.</summary>
        /// <param name="reportId">Identifier for the report.</param>
        /// <returns>An array of byte.</returns>
        byte[] GetInputReport(byte reportId);
    }
}