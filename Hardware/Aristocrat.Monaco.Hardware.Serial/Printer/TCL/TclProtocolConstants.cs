namespace Aristocrat.Monaco.Hardware.Serial.Printer.TCL
{
    using System;

    public class TclProtocolConstants
    {
        public const byte CrcCharacter = (byte)'G';  // CRC message indicator
        public const byte Enq = 0x05; // ASCII enquiry
        public const byte Esc = 0x1B; // ASCII Escape
        public const byte Cr = 0x0D;
        public const byte Lf = 0x0A;
        public const byte Ff = 0x0C;
        public const byte GroupSeparatorCharacter = (byte)'|';  // Group separator
        public const byte RightBraceCharacter = (byte)'}';
        public const byte StatusCharacter = (byte)'S';  // Status message indicator
        public const byte TransmitCharacter = (byte)'^';  // Transmit message indicator
        public const byte UnderscoreCharacter = (byte)'_';
        public const byte UndefinedPrintAreaIdCharacter = (byte)'/'; // 0 character - 1;

        public const int CrcResponseLength = 7;
        public const int CrcOffset = 3;
        public const int CrcSize = 2;
        public const int EmptyResponseLength = 0;
        public const int StatusOffset = 15;
        public const int StatusResponseLength = 29;
        public const int SwitchModesResponseLength = 0;
        public const int VersionLength = 9;
        public const int VersionOffset = 5;

        public const string BarcodeInterleaved2Of5Code = "l";
        public const string GroupSeparatorCode = "|";
        public const string PrintAttributeDynamicTextCode = "0";
        public const string PrintCode = "P";
        public const string PrintOneCopyCode = "1";
        public const string RegionCode = "R";
        public const string TransmitCode = "^";
        public const string TemplateCode = "T";
        public const string RamMemoryTargetCode = "R";

        public const string ClearOnlineErrorsCommand = "^C|^";
        public const string DeleteAllRegionsCommand = "^R|$|DR|^";
        public const string EnableLinePrintingModeCommand = "^j|^";
        public const string FlushAllCommand = "^F|A|^";
        public const string FormFeedSingleCommand = "^f|I|^";
        public const string AbortBarcodePrintCommand = "^F|C|^";

        public static readonly byte[] EnableTemplatePrintingModeCommand = { Esc, 0x5B, 0x5E, 0x5D, Esc }; // Escape [^] Escape
        public static readonly byte[] RequestStatusCommand = { Enq };
        public static readonly byte[] ReadCrcCommand =
        {
            TransmitCharacter,
            CrcCharacter,
            GroupSeparatorCharacter,
            0, 0, 0, 0, 0, 0,
            GroupSeparatorCharacter,
            TransmitCharacter
        };

        public static readonly byte[] CrLf = { Cr, Lf };
        public static readonly byte[] CommandInitialize = { Esc, (byte)'@' };
        public static readonly byte[] CommandInitializePageMode = { Esc, (byte)'(', (byte)'G' };

        // set the unit of measure to 1/180"
        public static readonly byte[] CommandSetUnitOfMeasure = { Esc, (byte)'(', (byte)'U', 01, 00, 20 };

        // set to use the font with 20.5 characters per inch
        public static readonly byte[] CommandSet20_5CharactersPerInch = { Esc, (byte)'[', (byte)'F', 7 };

        // set 1/8" line spacing
        public static readonly byte[] CommandSetLineSpacing = { Esc, (byte)'0' };

        // turn on bold printing
        public static readonly byte[] CommandSetBoldFont = { Esc, (byte)'E' };

        public static readonly byte[] EmptyResponse = { };

        /// <summary>
        ///     Direction of printing
        /// </summary>
        public enum TclPrintDirection
        {
            Right = 0,
            Down,
            Left,
            Up
        }

        /// <summary>
        ///     Text justification within field
        /// </summary>
        public enum TclPrintJustification
        {
            Left = 0,
            Center,
            Right
        }

        /// <summary>
        ///     Status byte flags for five combined status response bytes.
        ///     Not all bits are used/useful.
        /// </summary>
        [Flags]
        public enum TclStatus : ulong
        {
            // status byte 5, 4, 3, 2, 1
            VoltageLow = 0x00_00_00_00_01,
            HeadError = 0x00_00_00_00_02,
            OutOfPaper = 0x00_00_00_00_04,
            HeadIsUp = 0x00_00_00_00_08,
            SystemError = 0x00_00_00_00_10,
            Busy = 0x00_00_00_00_20,
            JobMemoryOverflow = 0x00_00_00_01_00,
            BufferOverflow = 0x00_00_00_02_00,
            LibraryLoadError = 0x00_00_00_04_00,
            DataError = 0x00_00_00_08_00,
            LibraryRefError = 0x00_00_00_10_00,
            TemperatureError = 0x00_00_00_20_00,
            MissingSupplyIndex = 0x00_00_01_00_00,
            PrinterOffline = 0x00_00_02_00_00,
            FlashProgError = 0x00_00_04_00_00,
            PaperInPath = 0x00_00_08_00_00,
            PrintLibrariesCorrupted = 0x00_00_10_00_00,
            CommandError = 0x00_00_20_00_00,
            PaperLow = 0x00_01_00_00_00,
            PaperJam = 0x00_02_00_00_00,
            CutterError = 0x00_04_00_00_00,
            JournalPrinting = 0x00_08_00_00_00,
            Reset = 0x01_00_00_00_00,
            BarcodeDataIsAccessed = 0x02_00_00_00_00,
            ChassisIsOpen = 0x04_00_00_00_00,
            XOff = 0x08_00_00_00_00,
            TopOfForm = 0x10_00_00_00_00,
            ValidationNumberDone = 0x20_00_00_00_00,
        }
    }
}
