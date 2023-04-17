namespace Aristocrat.Monaco.Hardware.Contracts
{
    using System;

    /// <summary>
    ///     Hardware related constants.
    /// </summary>
    public static class HardwareConstants
    {
        /// <summary>
        ///     Path lookup of the data folder
        /// </summary>
        public const string DataPath = @"/Data";

        /// <summary>
        ///     The extension point for the startup event listener service
        /// </summary>
        public const string StartupEventListenerExtensionPoint = "/Hardware/Service/StartupEventListener";

        /// <summary>
        ///     Key for the simulateEdgeLighting property in property manager. The property value corresponding to
        ///     this key is set by the command line arguments.
        /// </summary>
        public const string SimulateEdgeLighting = "simulateEdgeLighting";

        /// <summary>
        ///     Key for the simulateLcdButtonDeck property in property manager.  The property value corresponding to
        ///     this key is set by the command line arguments.
        /// </summary>
        public const string SimulateLcdButtonDeck = "simulateLcdButtonDeck";

        /// <summary>
        ///     Key for the simulateVirtualButtonDeck property in property manager.  The property value corresponding to
        ///     this key is set by the command line arguments.
        /// </summary>
        public const string SimulateVirtualButtonDeck = "simulateVirtualButtonDeck";

        /// <summary>
        ///     Key for the UsbButtonDeck property in property manager.  The property value corresponding to
        ///     whether button deck is Usb Screen.
        /// </summary>
        public const string UsbButtonDeck = "UsbButtonDeck";

        /// <summary>
        ///     Key used to get the Id of Display 1
        /// </summary>
        public const string Display1 = "Display1";

        /// <summary>
        ///     Key used to get the Id of Display 2
        /// </summary>
        public const string Display2 = "Display2";

        /// <summary>
        ///     Key used to get the Id of Display 3
        /// </summary>
        public const string Display3 = "Display3";

        /// <summary>
        ///     Key used to get the Id of Display 4
        /// </summary>
        public const string Display4 = "Display4";

        /// <summary>
        ///     Key used to get the Id of Display 5
        /// </summary>
        public const string Display5 = "Display5";

        /// <summary>
        ///     Key used to get the Id of Touch Device 1
        /// </summary>
        public const string TouchDevice1 = "TouchDevice1";

        /// <summary>
        ///     Key used to get the Id of Touch Device 2
        /// </summary>
        public const string TouchDevice2 = "TouchDevice2";

        /// <summary>
        ///     Key used to get the Id of Touch Device 3
        /// </summary>
        public const string TouchDevice3 = "TouchDevice3";

        /// <summary>
        ///     Key used to get the Id of Touch Device 4
        /// </summary>
        public const string TouchDevice4 = "TouchDevice4";

        /// <summary>
        ///     Key used to get the Id of Touch Device 5
        /// </summary>
        public const string TouchDevice5 = "TouchDevice5";

        /// <summary>
        ///     Key used to get a value indicating whether or not hardware meters are enabled
        /// </summary>
        public const string HardMetersEnabledKey = "Hardware.HardMetersEnabled";

        /// <summary>
        ///     Key used to get a value indicating whether or not the bell is enabled
        /// </summary>
        public const string BellEnabledKey = "Hardware.BellEnabled";

        /// <summary>
        ///     Key used to get a value indicating whether or not the daoor alarm is enabled
        /// </summary>
        public const string DoorAlarmEnabledKey = "Hardware.DoorAlarmEnabled";

        /// <summary>
        ///     Gets the ISystemDisableManager key used when a Device has a protocol mismatch and needs to revalidate.
        /// </summary>
        public static Guid HardwareProtocolMismatchDisabledKey = new Guid("{42461B16-79AD-44D2-BF61-3C12BE133FD8}");

        /// <summary>
        ///     The key used to disable the system when the audio usb is disconnected.
        /// </summary>
        public static Guid AudioDisconnectedLockKey = new Guid("{62AFC79B-76CC-4CFC-E520-52F26FC87BAD}");

        /// <summary>
        ///     The key used to disable the system when the audio usb is re-connected.
        /// </summary>
        public static Guid AudioReconnectedLockKey = new Guid("{42AFA79B-76CC-4FFC-E520-52F27FC17BCD}");

        /// <summary>
        ///     Operating System component path.
        /// </summary>
        public const string OperatingSystemPath = "Operating System";

        /// <summary>
        ///     Key used to get a bool indicating if battery 1 is low or not
        /// </summary>
        public const string Battery1Low = "Battery1Low";

        /// <summary>
        ///     Key used to get a bool indicating if battery 2 is low or not
        /// </summary>
        public const string Battery2Low = "Battery2Low";

        /// <summary>
        ///     Property manager key for disabled notes
        /// </summary>
        public const string DisabledNotes = "DisabledNotes";

        /// <summary>
        ///     Property manager key for default max failed poll count
        /// </summary>
        public const int DefaultMaxFailedPollCount = 3;

        /// <summary>
        ///     Property manager key for max failed poll count (command line option)
        /// </summary>
        public const string MaxFailedPollCount = "maxFailedPollCount";

        /// <summary>
        ///     Property manager key for serial touch disabled (command line option)
        /// </summary>
        public const string SerialTouchDisabled = "serialTouchDisabled";

        /// <summary>
        ///     Regex value used to identify LS cabinet types
        /// </summary>
        public const string CabinetTypeRegexLs = "^LS";

        /// <summary>
        ///     Array of Display Properties
        /// </summary>
        public static readonly string[] Displays = { Display1, Display2, Display3, Display4, Display5 };

        /// <summary>
        ///     Key used to get the preset volume level
        /// </summary>
        public const string VolumePreset = "VolumePreset";

        /// <summary>
        ///     Key used to get the preset volume scalar
        /// </summary>
        public const string VolumeScalarPreset = "VolumeScalarPreset";
    }
}