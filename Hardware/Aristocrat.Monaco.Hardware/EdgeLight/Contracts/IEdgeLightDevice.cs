namespace Aristocrat.Monaco.Hardware.EdgeLight.Contracts
{
    using System;
    using System.Collections.Generic;
    using Hardware.Contracts.EdgeLighting;

    public interface IEdgeLightDevice : IDisposable
    {
        /// <summary>
        ///     The List of IControllerStripInfo strips
        /// </summary>
        IReadOnlyList<IStrip> PhysicalStrips { get; }

        /// <summary>
        ///     the Board Id of the device.
        /// </summary>
        BoardIds BoardId { get; }

        /// <summary>
        ///     Gets the Serial Number, Product and Manufacturer details of the device
        /// </summary>
        /// <returns></returns>
        ICollection<EdgeLightDeviceInfo> DevicesInfo { get; }

        /// <summary>
        ///     Puts device in low power mode.
        /// </summary>
        bool LowPowerMode { set; }

        /// <summary>
        ///     Returns true if the device is in open state.
        /// </summary>
        bool IsOpen { get; }

        /// <summary>
        ///     Sending the data for the strip
        /// </summary>
        void RenderAllStripData();

        /// <summary>
        ///     Raised when new strips are discovered.
        /// </summary>
        event EventHandler<EventArgs> StripsChanged;

        /// <summary>
        ///     Raised when the device connection is changed.
        /// </summary>
        event EventHandler<EventArgs> ConnectionChanged;

        /// <summary>
        ///     Closes the device for further use.
        /// </summary>
        void Close();

        /// <summary>
        ///     Checks for device presence or absence.
        /// </summary>
        /// <returns>True if device is present.</returns>
        bool CheckForConnection();

        /// <summary>
        ///     Sets system brightness. This sets the internal info then RenderAllStripData changes brightness
        /// <param name="brightness">Brightness from 1-100</param>
        /// </summary>
        void SetSystemBrightness(int brightness);
    }
}