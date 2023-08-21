namespace Aristocrat.Monaco.G2S.Handlers.NoteAcceptor
{
    using System;
    using System.Collections.Generic;
    using Accounting.Contracts;
    using Application.Contracts;
    using Aristocrat.G2S.Protocol.v21;
    using Hardware.Contracts.NoteAcceptor;
    using Kernel;

    /// <summary>
    ///     Extension methods for an INoteAcceptor.
    /// </summary>
    public static class NoteAcceptorExtensions
    {
        /// <summary>
        ///     Converts an <see cref="BillTransaction" /> instance to a <see cref="notesAcceptedLog" />
        /// </summary>
        /// <param name="this">The <see cref="BillTransaction" /> instance to convert.</param>
        /// <returns>A <see cref="printLog" /> instance.</returns>
        public static notesAcceptedLog ToNotesAcceptedLog(this BillTransaction @this)
        {
            return new notesAcceptedLog
            {
                logSequence = @this.LogSequence,
                deviceId = @this.DeviceId,
                transactionId = @this.TransactionId,
                currencyId = @this.CurrencyId,
                denomId = @this.Amount,
                baseCashableAmt = @this.Amount,
                noteDateTime = @this.TransactionDateTime
            };
        }

        /// <summary>
        ///     Gets the noteAcceptorData from the Note Acceptor devices
        /// </summary>
        /// <param name="this">An <see cref="INoteAcceptor" /> instance.</param>
        /// <param name="currencyId">The currency identifier</param>
        /// <returns>A noteAcceptorData collection</returns>
        public static IEnumerable<noteAcceptorData> GetNoteAcceptorData(this INoteAcceptor @this, string currencyId)
        {
            if (@this == null)
            {
                throw new ArgumentNullException(nameof(@this));
            }

            var data = new List<noteAcceptorData>();

            var properties = ServiceManager.GetInstance().GetService<IPropertiesManager>();
            var currencyMultiplier = (long)properties.GetValue(
                ApplicationConstants.CurrencyMultiplierKey,
                ApplicationConstants.DefaultCurrencyMultiplier);

            foreach (var note in @this.GetSupportedNotes())
            {
                data.Add(ToNoteAcceptorData(note * currencyMultiplier, currencyId, @this.Denominations.Contains(note)));
            }

            return data;
        }

        private static noteAcceptorData ToNoteAcceptorData(long cashableAmount, string currencyId, bool active)
        {
            return new noteAcceptorData
            {
                currencyId = currencyId,
                denomId = cashableAmount,
                token = false,
                basePromoAmt = 0,
                baseNonCashAmt = 0,
                baseCashableAmt = cashableAmount,
                noteActive = active
            };
        }
    }
}
