namespace Aristocrat.Monaco.Hardware.Contracts.Gds
{
    /// <summary>
    ///     GDS constants
    /// </summary>
    public class GdsConstants
    {
        /// <summary>
        ///     Text justification
        /// </summary>
        public enum PdlJustify
        {
            /// <summary>Left</summary>
            Left = 1,

            /// <summary>Center</summary>
            Center,

            /// <summary>Right</summary>
            Right
        }

        /// <summary>
        ///     Types of PDL regions
        /// </summary>
        public enum PdlRegionType
        {
            /// <summary>Font</summary>
            Font = 'F',

            /// <summary>Graphic</summary>
            Graphic = 'G',

            /// <summary>Barcode</summary>
            Barcode = 'B',

            /// <summary>Line</summary>
            Line = 'L',

            /// <summary>Box</summary>
            Box = 'O'
        }

        /// <summary>
        ///     Clockwise rotation (fraction of full circle)
        /// </summary>
        public enum PdlRotation
        {
            /// <summary>No rotation</summary>
            None = 1,

            /// <summary>90 degrees</summary>
            Quarter = 2,

            /// <summary>180 degrees</summary>
            Half = 3,

            /// <summary>270 degrees</summary>
            ThreeQuarters = 4
        }

        /// <summary>
        ///     Command and event report IDs per GDS specs.
        /// </summary>
        public enum ReportId
        {
            /// <summary>The acknowledge command.</summary>
            Ack = 0x01,

            /// <summary>The enable command.</summary>
            Enable = 0x02,

            /// <summary>The disable command.</summary>
            Disable = 0x03,

            /// <summary>The self test command.</summary>
            SelfTest = 0x04,

            /// <summary>The request GAT report command.</summary>
            RequestGatReport = 0x05,

            /// <summary>The power status event.</summary>
            PowerStatus = 0x06,

            /// <summary>The GAT data event.</summary>
            GatData = 0x07,

            /// <summary>The calculate CRC command.</summary>
            CalculateCrc = 0x08,

            /// <summary>The CRC data event.</summary>
            CrcData = 0x09,

            /// <summary>The device state event.</summary>
            DeviceState = 0x0A,

            /// <summary>The card reader internal-behavior request.</summary>
            CardReaderGetBehavior = 0x40,

            /// <summary>The card reader internal-behavior response.</summary>
            CardReaderBehaviorResponse = 0x41,

            /// <summary>The card reader get config command.</summary>
            CardReaderGetConfig = 0x5A,

            /// <summary>The card reader read card data command.</summary>
            CardReaderReadCardData = 0x5B,

            /// <summary>The card reader get ATR (Answer to reset) command.</summary>
            CardReaderGetAnswerToReset = 0x5C,

            /// <summary>The card reader transfer to card command.</summary>
            CardReaderTransferToIcc = 0x5D,

            /// <summary>The card reader release latch command.</summary>
            CardReaderReleaseLatch = 0x5E,

            /// <summary>The card reader light control command.</summary>
            CardReaderLightControl = 0x5F,

            /// <summary>The card reader clear buffer command.</summary>
            CardReaderClearBuffer = 0x60,

            /// <summary>The card reader get count state command.</summary>
            CardReaderGetCountStatus = 0x61,

            /// <summary>The card reader failure status event.</summary>
            CardReaderFailureStatus = 0x62,

            /// <summary>The card reader config data event.</summary>
            CardReaderConfigData = 0x63,

            /// <summary>The card reader card status event.</summary>
            CardReaderCardStatus = 0x64,

            /// <summary>The card reader card data event.</summary>
            CardReaderCardData = 0x65,

            /// <summary>The card reader error data event.</summary>
            CardReaderErrorData = 0x66,

            /// <summary>The card reader count status event.</summary>
            CardReaderCountStatus = 0x67,

            /// <summary>The card reader latch mode command.</summary>
            CardReaderLatchMode = 0x68,

            /// <summary>The card reader extended light control command.</summary>
            CardReaderExtendedLightControl = 0x69,

            /// <summary>The note acceptor number of note data entries command/event.</summary>
            NoteAcceptorNumberOfNoteDataEntries = 0x80,

            /// <summary>The note acceptor read note table command/event.</summary>
            NoteAcceptorReadNoteTable = 0x81,

            /// <summary>The note acceptor extend timeout command.</summary>
            NoteAcceptorExtendTimeout = 0x82,

            /// <summary>The note acceptor accept note/ticket command.</summary>
            NoteAcceptorAcceptNoteOrTicket = 0x83,

            /// <summary>The note acceptor return note/ticket command.</summary>
            NoteAcceptorReturnNoteOrTicket = 0x84,

            /// <summary>The note acceptor failure status event.</summary>
            NoteAcceptorFailureStatus = 0x85,

            /// <summary>The note acceptor note validated event.</summary>
            NoteAcceptorNoteValidated = 0x86,

            /// <summary>The note acceptor ticket validated event.</summary>
            NoteAcceptorTicketValidated = 0x87,

            /// <summary>The note acceptor note/ticket status event.</summary>
            NoteAcceptorNoteOrTicketStatus = 0x88,

            /// <summary>The note acceptor stacker status event.</summary>
            NoteAcceptorStackerStatus = 0x89,

            /// <summary>The note acceptor read metrics command/event.</summary>
            NoteAcceptorReadMetrics = 0x8A,

            /// <summary>The note acceptor UTF ticket validated event.</summary>
            NoteAcceptorUtfTicketValidated = 0x8B,

            /// <summary>The printer define region command.</summary>
            PrinterDefineRegion = 0xC0,

            /// <summary>The printer define template command.</summary>
            PrinterDefineTemplate = 0xC1,

            /// <summary>The printer print ticket command.</summary>
            PrinterPrintTicket = 0xC2,

            /// <summary>The printer form-feed command.</summary>
            PrinterFormFeed = 0xC3,

            /// <summary>The printer failure status event.</summary>
            PrinterFailureStatus = 0xC4,

            /// <summary>The printer ticket print status event.</summary>
            PrinterTicketPrintStatus = 0xC5,

            /// <summary>The printer transfer status event.</summary>
            PrinterTransferStatus = 0xC6,

            /// <summary>The printer request metrics command.</summary>
            PrinterRequestMetrics = 0xC7,

            /// <summary>The printer metrics event.</summary>
            PrinterMetrics = 0xC8,

            /// <summary>The printer graphic transfer setup command.</summary>
            PrinterGraphicTransferSetup = 0xC9,

            /// <summary>The printer file transfer command.</summary>
            PrinterFileTransfer = 0xCA,

            /// <summary>The printer status event.</summary>
            PrinterStatus = 0xCB,

            /// <summary>The printer ticket retract command.</summary>
            PrinterTicketRetract = 0xCC,

            /// <summary> The reel status event </summary>
            ReelControllerStatus = 0xF0,

            /// <summary> The reel controller failure status event </summary>
            ReelControllerFailureStatus = 0xF1,

            /// <summary> The reel controller home reel command </summary>
            ReelControllerHomeReel = 0xF2,

            /// <summary> The reel controller set lights command </summary>
            ReelControllerSetLights = 0xF3,

            /// <summary> The reel controller spin reels command </summary>
            ReelControllerSpinReels = 0xF4,

            /// <summary> The status for a spinning reel </summary>
            ReelControllerSpinStatus = 0xF5,

            /// <summary> The reel controller nudge command </summary>
            ReelControllerNudge = 0xF6,

            /// <summary> The response for a get reel light identifiers command </summary>
            ReelControllerLightIdentifiersResponse = 0xF7,

            /// <summary> The reel controller set reel lights brightness command </summary>
            ReelControllerSetReelBrightness = 0xF8,

            /// <summary> The reel controller response to a set reel lights and brightness command </summary>
            ReelControllerLightResponse = 0xF9,

            /// <summary> The reel controller set speed command </summary>
            ReelControllerSetReelSpeed = 0xFA,

            /// <summary> The reel controller tilt reels command </summary>
            ReelControllerTiltReels = 0xFB,

            /// <summary> The reel controller set offsets command </summary>
            ReelControllerSetOffsets = 0xFC,

            /// <summary> The notice of the controller hardware fully initialized </summary>
            ReelControllerInitialized = 0xFD,

            /// <summary> The reel controller get reel IDs command </summary>
            ReelControllerGetLightIds = 0xFE,

            /// <summary> The reel controller failure status clear event </summary>
            ReelControllerFailureStatusClear = 0xFF,

            /// <summary> The reel controller response to a tilt reels command</summary>
            ReelControllerTiltReelsResponse = 0x100,

            /// <summary> The reel controller response to a tilt reels command</summary>
            CoinValidatorDivertorState = 0x101,

            /// <summary> The reel controller response to a tilt reels command</summary>
            CoinValidatorRejectState = 0x102,

            /// <summary> The reel controller response to a tilt reels command</summary>
            DeviceReset = 0x103,

            /// <summary> The reel controller response to a tilt reels command</summary>
            CoinInStatus = 0x104,
            /// <summary> The reel controller response to a tilt reels command</summary>
            CoinInFaultStatus = 0x105
        }

        /// <summary>The default seed value used to calculate the CRC.</summary>
        public const int DefaultSeed = 0;

        /// <summary>Common report value for successful operation.</summary>
        public const int Success = 1;
    }
}