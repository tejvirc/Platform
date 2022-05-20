namespace Aristocrat.Monaco.Hardware.Contracts.Display
{
    using System;
    using System.Drawing;
    using System.Windows.Forms;

    /// <summary>Monitor Info.</summary>
    public class Monitor
    {
        /// <summary>Windows Handle to a Monitor.</summary>
        public IntPtr MonitorHandle { get; set; }

        /// <summary>Active Device Context of Monitor.</summary>
        public IntPtr DeviceContextHandle { get; set; }

        /// <summary>The rectangle of monitor.</summary>
        public Rectangle Bounds { get; set; }

        /// <summary>The workable area of the monitor (excluding start bar etc).</summary>
        public Rectangle WorkingArea { get; set; }

        /// <summary>Registry key of the monitor.</summary>
        public string DeviceKey { get; set; }

        /// <summary>The Hardware Id of the monitor (unique).</summary>
        public string DeviceId { get; set; }

        /// <summary>The Dos Device Name of the Monitor.</summary>
        public string DeviceString { get; set; }

        /// <summary>Friendly name of the device.</summary>
        public string DeviceName { get; set; }

        /// <summary>The device states of the monitor.</summary>
        public DisplayDeviceStateFlags DeviceState { get; set; }

        /// <summary>Indicates if the monitor is primary or not.</summary>
        public bool PrimaryMonitor { get; set; }

        /// <summary>Unique name of the device.</summary>
        public string UniqueId => DeviceId;

        /// <summary>Screen orientation.</summary>
        public ScreenOrientation Orientation { get; set; }
    }
}