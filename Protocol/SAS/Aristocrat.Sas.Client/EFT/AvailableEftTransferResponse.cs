namespace Aristocrat.Sas.Client.EFT
{
    using System;

    public class AvailableEftTransferResponse : LongPollResponse
    {
        /// <summary>
        ///     Provides bit flag locations for Transfer Availability (table 8.10, SAS5.02)
        /// </summary>
        [Flags]
        public enum EftTransferAvailability
        {
            TransferToGamingMachine = 1,

            TransferFromGamingMachine = 1 << 1
            // Reserved bits 2-7
        }

        public byte[] Reserved => new byte[] { 0, 0, 0 };

        public EftTransferAvailability TransferAvailability { get; set; }
    }
}