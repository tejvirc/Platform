namespace Aristocrat.Monaco.Hardware.Serial.Printer.EpicTTL
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    ///     Constants used by EpicTTL protocol
    /// </summary>
    public class EpicTTLProtocolConstants
    {
        // Control characters used in protocol
        public const byte Cr = 0x0D; // ASCII carriage return
        public const byte Enq = 0x05; // ASCII enquiry
        public const byte Esc = 0x1B; // ASCII escape
        public const byte Ff = 0x0C; // ASCII form-feed
        public const byte Gs = 0x1D; // ASCII group-separator
        public const byte Nl = 0x0A; // ASCII new-line

        // protocol commands and responses
        public static readonly byte[] CommandAbortPrint = { Gs, (byte)'~', (byte)'R', (byte)'P' }; // reset printer (abort)
        public static readonly byte[] CommandAbsoluteVerticalPosition = { Gs, (byte)'$', 0, 0 }; // 0,0 gets replaced by position
        public static readonly byte[] CommandBarCodeElementWidth = { Gs, (byte)'W', 0, 0 }; // 0,0 get bar thicknesses
        public static readonly byte[] CommandBarCodeHeight = { Gs, (byte)'h', 0 }; // replace 0 with height
        public static readonly byte[] CommandBarCodeStartPosition = { Gs, (byte)'A', 0, 0 }; // 0,0 gets replaced by position
        public static readonly byte[] CommandCrcVerification = { Gs, (byte)'?', 0, 0, 0, 0, 0, 0 }; // start address 0, seed 0
        public static readonly byte[] CommandDoubleStrikeOff = { Esc, (byte)'G', 0 };
        public static readonly byte[] CommandDoubleStrikeOn = { Esc, (byte)'G', 1 };
        public static readonly byte[] CommandFieldJustify = { Gs, (byte)'F', 0, 0, 0, 0, 0 }; // first 0 gets replaced by justify code, next 2 start pos, next 2 end pos
        public static readonly byte[] CommandPageMode = { Gs, (byte)'V', 1 };
        public static readonly byte[] CommandPrintBarCodeInterleaved2Of5 = { Gs, (byte)'k', BarcodeInterleaved2Of5, 0 }; // 0 gets replaced with actual barcode length
        public static readonly byte[] CommandPrintDirection = { Esc, (byte)'t', 0 }; // replace 0 with EpicTTLPrintDirection
        public static readonly byte[] CommandRequestCombinedPrinterStatus = { Gs, (byte)'y' };
        public static readonly byte[] CommandResetToDefaults = { Esc, (byte)'@' };
        public static readonly byte[] CommandReturnFirmwareRevision = { Esc, (byte)'V' };
        public static readonly byte[] CommandReturnVersionInfo = { Esc, Enq, (byte)'1' };
        public static readonly byte[] CommandSelectCharacterSize = { Gs, (byte)'!', 0 }; // 0 gets width multiplier in low nibble, height multiplier in high nibble
        public static readonly byte[] CommandSelectFonts = { Esc, (byte)'F', 0, 1, 0 }; // point height, font width ratio (unused), features
        public static readonly byte[] ResponseCrcVerification = { Gs, (byte)'?' };
        public static readonly byte[] ResponseEmpty = { };
        public static readonly byte[] ResponseReturnVersionInfo = { Enq, (byte)'1' };

        // device specifics
        public const int ResolutionDpi = 203;
        public const int PaperWidthDots = 507;
        public const int PaperLengthDots = 1100;
        public const int LandscapeBuffer = 35;
        public const float PointsPerInch = 72f;
        public const float DotsPerPoint = ResolutionDpi / PointsPerInch;
        public const byte BarcodeInterleaved2Of5 = 7;
        public const byte DefaultBarcodeLength = 18;
        public const int ResetTimeMs = 10000;

        public static readonly List<FontDefinition> Fonts = new List<FontDefinition>
        {
            new FontDefinition { Index = 'S', HeightPts = PointsPerInch / 8.4f, PitchCpi = 20.0f },
            new FontDefinition { Index = 'P', HeightPts = PointsPerInch / 8.4f, PitchCpi = 16.9f },
            new FontDefinition { Index = 'M', HeightPts = PointsPerInch / 6.4f, PitchCpi = 12.7f },
            new FontDefinition { Index = 'U', HeightPts = PointsPerInch / 6.4f, PitchCpi = 10.2f },
            new FontDefinition { Index = 'T', HeightPts = PointsPerInch / 3.6f, PitchCpi = 7.3f }
        };

        public static readonly List<FontScaleFactor> FontScaleFactors = new List<FontScaleFactor>
        {
            new FontScaleFactor { Index = 2, ScaleFactor = 0.85f },
            new FontScaleFactor { Index = 5, ScaleFactor = 0.85f },
            new FontScaleFactor { Index = 6, ScaleFactor = 0.85f },
            new FontScaleFactor { Index = 7, ScaleFactor = 1.1f },
            new FontScaleFactor { Index = 8, ScaleFactor = 0.7f }
        };

        /// <summary>
        ///     Direction of printing
        /// </summary>
        public enum EpicTTLPrintDirection : byte
        {
            Right = 0,
            Up,
            Left,
            Down
        }

        /// <summary>
        ///     Text justification within field
        /// </summary>
        public enum EpicTTLPrintJustify : byte
        {
            Left = 0,
            Center,
            Right
        }

        /// <summary>
        ///     Status byte flags for two combined status response bytes.
        ///     Not all bits are used/useful.
        /// </summary>
        [Flags]
        public enum EpicTTLStatus : ushort
        {
            PaperLow = 0x0001,
            PaperInPrinter = 0x0002,
            BarCodeCompleted = 0x0010,
            ValidationCompleted = 0x0020,
            PaperInPath = 0x0040,
            PaperJam = 0x0080,
            PrinterReady = 0x0100,
            TopOfForm = 0x0200,
            HeadIsUp = 0x0800,
            ChassisIsOpen = 0x1000,
            OutOfPaper = 0x2000,
            NotPrinting = 0x8000 // this one isn't in the spec, but seems to indicate "ready to print again"
        }
    }
}
