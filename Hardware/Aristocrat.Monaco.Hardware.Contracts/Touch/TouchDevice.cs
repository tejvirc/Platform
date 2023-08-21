namespace Aristocrat.Monaco.Hardware.Contracts.Touch
{
    using System;

    /// <summary>A Touch Device.</summary>
    public class TouchDevice
    {
        /// <summary>Display Orientation 0 = Landscape, 1 = Portrait.</summary>
        public int DisplayOrientation { get; set; }

        /// <summary>Win32 Device Handle of Touch device.</summary>
        public IntPtr DeviceHandle { get; set; } /* HANDLE */

        /// <summary>Touch Device Type.</summary>
        public TouchDeviceType TouchDeviceType { get; set; }

        /// <summary>Associated Monitor to the touch device.</summary>
        public IntPtr MonitorHandle { get; set; } /* HMONITOR */

        /// <summary>Start Id of Touch Cursor.</summary>
        public int StartingCursorId { get; set; }

        /// <summary>Number of supported multi touch points.</summary>
        public short MaxActiveContacts { get; set; }

        /// <summary>Product string of the device.</summary>
        public string ProductString { get; set; }

        /// <summary>Device Id (unique).</summary>
        public string DeviceId { get; set; }

        /// <summary>Device ProductID (Hardware ProductID).</summary>
        public int ProductID { get; set; }

        /// <summary>Device VendorID (Hardware VendorID).</summary>
        public int VendorID { get; set; }

        /// <summary>Device Version Number (Hardware VersionNumber).</summary>
        public int VersionNumber { get; set; }

        /// <summary>Device Id (unique).</summary>
        public string UniqueId => DeviceId;
    }
}