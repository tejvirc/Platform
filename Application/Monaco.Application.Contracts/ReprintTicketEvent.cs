////////////////////////////////////////////////////////////////////////////////////////////
// <copyright file="ReprintTicketEvent.cs" company="Video Gaming Technologies, Inc.">
// Copyright © 2011-2013 Video Gaming Technologies, Inc.  All rights reserved.
// Confidential and proprietary information.
// </copyright>
////////////////////////////////////////////////////////////////////////////////////////////

namespace Aristocrat.Monaco.Application.Contracts
{
    using System;
    using Kernel;

    /// <summary>
    ///     A ReprintTicketEvent is posted when a ticket a ticket with a validation number needs to be reprinted.
    /// </summary>
    [Serializable]
    public class ReprintTicketEvent : BaseEvent
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="ReprintTicketEvent" /> class.
        /// </summary>
        public ReprintTicketEvent()
        {
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="ReprintTicketEvent" /> class.
        /// </summary>
        /// <param name="validationNumber">The validation number of the ticket to reprint.</param>
        /// <param name="amount">The amount.</param>
        public ReprintTicketEvent(string validationNumber, long amount)
        {
            ValidationNumber = validationNumber;
            Amount = amount;
        }

        /// <summary>
        ///     Gets or sets the ticket validation number to print.
        /// </summary>
        public string ValidationNumber { get; set; }

        /// <summary>
        ///     Gets or sets the ticket amount.
        /// </summary>
        public long Amount { get; set; }
    }
}
