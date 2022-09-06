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
        public const int TouchDataLength = 5;

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
        public const int TouchPressure = 0; // 0 (default) to 1024
        
        /// <summary>
        ///     The Maximum coordinate range - 14 bits
        /// </summary>
        public const int MaxCoordinateRange = 16383;

        /// <summary>
        ///     The minimum length of a valid command response.
        /// </summary>
        public const int MinimumResponseLength = 3;

        /// <summary>
        ///     The low byte index of the x coordinate in touch data.
        /// </summary>
        public const int TouchDataPacketLowXByte = 1;

        /// <summary>
        ///     The high byte index of the x coordinate in touch data.
        /// </summary>
        public const int TouchDataPacketHighXByte = 2;
        
        /// <summary>
        ///     The low byte index of the y coordinate in touch data.
        /// </summary>
        public const int TouchDataPacketLowYByte = 3;
        
        /// <summary>
        ///     The high byte index of the y coordinate in touch data.
        /// </summary>
        public const int TouchDataPacketHighYByte = 4;
        
        /// <summary>
        ///     The number of bits to shift the high coordinate byte in touch data.
        ///     The MSB of the low byte is not used so we need to shift the high byte 7 bits.
        /// </summary>
        public const int TouchDataPacketHighByteShift = 7;

        #region Kortek And EX II (3M Touch) Controller Commands 

        /// <summary>
        ///     A command used for placing the controller into extended calibration mode.
        /// </summary>
        /// <remarks>Initiates a 2-point calibration, interactive for the EX II, the Kortek
        /// will auto-calibrate (non-interactive).</remarks>
        public static readonly byte[] CalibrateExtendedCommand = ToMessage("CX");

        /// <summary>
        ///     A command used to query the controller and wait for a response
        /// </summary>
        public static readonly byte[] NullCommand = ToMessage("Z");

        /// <summary>
        ///     A command used to get the manufacturer of the controller
        /// </summary>
        public static readonly byte[] NameCommand = ToMessage("NM");

        /// <summary>
        ///     A command used to determine the controller type and firmware version
        /// </summary>
        public static readonly byte[] OutputIdentityCommand = ToMessage("OI");

        /// <summary>
        ///     A command used for resetting the controller
        /// </summary>
        /// <remarks>Initializes the hardware and the firmware, causes the controller to
        /// stop sending data, and recalculates the environmental conditions.</remarks>
        public static readonly byte[] ResetCommand = ToMessage("R");

        /// <summary>
        ///     A command used for resetting the controller back to factory defaults
        /// </summary>
        /// <remarks>Returns the controller to the factory default operating parameters.
        /// The serial port is reset to N81 format tablet and 2-point calibration is lost.</remarks>
        public static readonly byte[] RestoreDefaultsCommand = ToMessage("RD");

        #endregion

        private static byte[] ToMessage(string command)
        {
            var bytes = Encoding.ASCII.GetBytes(command);
            return bytes.Prepend(Header).Append(Terminator).ToArray();
        }
    }
}