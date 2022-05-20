namespace Aristocrat.Monaco.Hardware.SerialTouch
{
    using System.Linq;
    using System.Text;

    public static class M3SerialTouchConstants
    {
        /// <summary>
        ///     The synchronize bit used for messages
        /// </summary>
        public const byte SyncBit = 0x80;

        /// <summary>
        ///     The bit used to determine if a finger is being lifted or pressed
        /// </summary>
        public const byte ProximityBit = 0x40;

        /// <summary>
        ///     Touch mask used for getting the coordinates
        /// </summary>
        public const byte LowOrderBits = 0x0F;

        /// <summary>
        ///     The head for all serial touch commands. <SOH>
        /// </summary>
        public const byte Header = 0x01;

        /// <summary>
        ///     The terminator for all serial touch commands <CR>
        /// </summary>
        public const byte Terminator = 0x0D;

        /// <summary>
        ///     This is what represents a good status response from the controller
        /// </summary>
        public const byte StatusGood = 0x30;

        /// <summary>
        ///     The byte used for acknowledging messages
        /// </summary>
        public const byte TargetAcknowledged = 0x31;

        /// <summary>
        ///     The number of bytes for a touch
        /// </summary>
        public const int TouchDataLength = 4;

        /// <summary>
        ///     The radius for the touch
        /// </summary>
        public const int TouchRadius = 1;

        /// <summary>
        ///     The orientation for the touch
        /// </summary>
        public const int TouchOrientation = 90;

        /// <summary>
        ///     The pressure applied for the touch
        /// </summary>
        public const int TouchPressure = 32000;

        /// <summary>
        ///     Command used to query the controller and wait for a response
        /// </summary>
        public static readonly byte[] NullCommand = ToMessage("Z");

        /// <summary>
        ///     A command used to determine the controller type and version
        /// </summary>
        public static readonly byte[] OutputIdentityCommand = ToMessage("OI");

        /// <summary>
        ///     A command used for getting the name of the controller
        /// </summary>
        public static readonly byte[] NameCommand = ToMessage("NM");

        /// <summary>
        ///     A command for determining if there is a problem with the touch screen
        /// </summary>
        public static readonly byte[] DiagnosticCommand = ToMessage("DX");

        /// <summary>
        ///     A command used for placing the controller into extended calibration mode
        /// </summary>
        public static readonly byte[] CalibrationCommand = ToMessage("CX");

        /// <summary>
        ///     A command used for resetting the controller
        /// </summary>
        public static readonly byte[] ResetCommand = ToMessage("R");

        /// <summary>
        ///     A command used for resetting the controller back to factory defaults
        /// </summary>
        public static readonly byte[] RestoreDefaultsCommand = ToMessage("RD");

        private static byte[] ToMessage(string command)
        {
            var bytes = Encoding.ASCII.GetBytes(command);
            return bytes.Prepend(Header).Append(Terminator).ToArray();
        }
    }
}