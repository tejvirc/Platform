////////////////////////////////////////////////////////////////////////////////////////////
// <copyright file="CashSlipEventData.cs" company="ARISTOCRAT TECHNOLOGIES AUSTRALIA PTY LTD">
// COPYRIGHT © 2017 ARISTOCRAT TECHNOLOGIES AUSTRALIA PTY LTD
// Absolutely no use, dissemination or copying in any matter whatsoever
// Of this material or any portion of it is to be made without the prior
// written authorisation of Aristocrat Technologies Australia Pty Ltd.
// All rights in and to this work are fully reserved
// </copyright>
////////////////////////////////////////////////////////////////////////////////////////////

namespace Aristocrat.Monaco.Accounting.UI.Models
{
    /// <summary>
    ///     Definition of the CashSlipEventData class.
    ///     This class stores one cash slip event.
    /// </summary>
    public class CashSlipEventData
    {
        /// <summary>
        ///     Gets or sets the unique transaction identifier assigned by the EGM.
        /// </summary>
        public long TransactionId { get; set; }

        /// <summary>
        ///     Gets or sets the sequence number of the cash slip.
        /// </summary>
        public long SequenceNumber { get; set; }

        /// <summary>
        ///     Gets or sets the date and time the cash slip was issued.
        /// </summary>
        public string TimeStamp { get; set; }

        /// <summary>
        ///     Gets the name copied from a transaction
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        ///     Gets the name copied from a transaction
        /// </summary>
        public string Amount { get; set; }

        /// <summary>
        ///     Gets the status
        /// </summary>
        public string Status { get; set; }

        /// <summary>
        ///     Gets and sets the validation id
        /// </summary>
        public string ValidationId { get; set; }

        /// <summary>
        ///     Gets and sets the visibility
        /// </summary>
        public bool IsVisible { get; set; } = false;
    }
}