namespace Aristocrat.Sas.Client
{
    using System;

    public static class SasConstants
    {
        /// <summary>The max size for the handpay queue</summary>
        public const int SasHandpayQueueSize = 20;

        /// <summary>The value indicating no Pool ID is set</summary>
        public const ushort NoPoolIdSet = 0;

        /// <summary>Or this with the SAS address to Nack a long poll</summary>
        public const byte Nack = 0x80;

        /// <summary>The bit to indicate the message is a general poll</summary>
        public const byte PollBit = 0x80;

        /// <summary>The mask for obtaining the SAS address</summary>
        public const int SasAddressMask = 0x7F;

        /// <summary>From the SAS Spec Section 2.2.2.4 Type G Long Polls</summary>
        public const int BroadcastAddress = 0x00;

        /// <summary>The index of the SAS address in a long poll</summary>
        public const int SasAddressIndex = 0;

        /// <summary> Length of  the field in byte in a long poll </summary>
        public  const int FieldLength = 1;

        /// <summary>The index of the SAS command in a long poll</summary>
        public const int SasCommandIndex = 1;

        /// <summary>The index of the length byte in a long poll</summary>
        public const int SasLengthIndex = 2;

        /// <summary>The index of the busy in the response</summary>
        public const int SasResponseBusyIndex = 1;

        /// <summary>The minimum number of bytes for a long poll (address byte + long poll command)</summary>
        public const int MinimumBytesForLongPoll = 2;

        /// <summary>Generate 2 BCD digits using 1 byte</summary>
        public const int Bcd2Digits = 1;

        /// <summary>Generate 4 BCD digits using 2 bytes</summary>
        public const int Bcd4Digits = 2;

        /// <summary>Generate 6 BCD digits using 3 bytes</summary>
        public const int Bcd6Digits = 3;

        /// <summary>Generate 8 BCD digits using 4 bytes</summary>
        public const int Bcd8Digits = 4;

        /// <summary>Generate 10 BCD digits using 5 bytes</summary>
        public const int Bcd10Digits = 5;

        /// <summary>Generate 14 BCD digits using 7 bytes</summary>
        public const int Bcd14Digits = 7;

        /// <summary>Generate 16 BCD digits using 8 bytes</summary>
        public const int Bcd16Digits = 8;

        /// <summary>Number of milliseconds in a centisecond.</summary>
        public const uint MillisecondsInHundredthSecond = 10;

        /// <summary>flags a long poll as variable length</summary>
        public const int VariableLength = -1;

        /// <summary>The number of crc bytes in a long poll</summary>
        public const byte NumberOfCrcBytes = 2;

        /// <summary>The maximum number of days for ticket expiration before it is considered a date</summary>
        public const int MaxTicketExpirationDays = 9999;

        /// <summary>The maximum validation sequence number</summary>
        public const uint MaxValidationSequenceNumber = 0xFFFFFF;

        /// <summary>
        ///     The current validation function code
        /// </summary>
        public const int CurrentValidation = 0;

        /// <summary>
        ///     The maximum meter length
        /// </summary>
        public const int MaxMeterLength = 18;

        /// <summary>
        ///     The look ahead function code
        /// </summary>
        public const int LookAhead = 0xFF;

        /// <summary>
        ///     The maximum buffer index
        /// </summary>
        public const int MaxValidationIndex = 0x1F; // TODO : Combine MaxValidationIndex and MaxHostSequence (they are the same).

        /// <summary>
        ///     The max host sequence ID
        /// </summary>
        public const int MaxHostSequence = 31;

        /// <summary>
        ///     The maximum amount of an AFT transfer in cents
        /// </summary>
        public const ulong MaxAftTransferAmount = 9_999_9999_99;

        /// <summary>The default maximum amount of aft transfer in cents.</summary>
        public const ulong MaxAftTransferLimitDefaultAmount = (ulong)100_00M;

        /// <summary>
        ///     Multiplier to convert dollars into cents
        /// </summary>
        public const int DollarsToCentsMultiplier = 100;

        /// <summary>
        ///     Maximum index for the transaction buffer
        /// </summary>
        public const byte MaximumTransactionBufferIndex = 0x7F;

        /// <summary>
        ///     Indicates that the default value should be used
        /// </summary>
        public const string UseDefault = "USE_DEFAULT_VALUE";

        /// <summary>
        ///     Gets the AftLockHandler key.
        /// </summary>
        public static Guid AftLockHandlerGuid => new Guid("{C79F3E67-22B2-4ac9-80FE-F7D9E6C711A4}");

        /// <summary>
        ///     The barcode length on the ticket when using SAS None Validation type.
        /// </summary>
        public const int SasNoneValidationTicketBarcodeLength = 8;
    }
}