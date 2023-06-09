////////////////////////////////////////////////////////////////////////////////////////////
// <copyright file="IVerificationTicketCreator.cs" company="ARISTOCRAT TECHNOLOGIES AUSTRALIA PTY LTD">
// COPYRIGHT © 2017 ARISTOCRAT TECHNOLOGIES AUSTRALIA PTY LTD
// Absolutely no use, dissemination or copying in any matter whatsoever
// Of this material or any portion of it is to be made without the prior
// written authorisation of Aristocrat Technologies Australia Pty Ltd.
// All rights in and to this work are fully reserved
// </copyright>
////////////////////////////////////////////////////////////////////////////////////////////

namespace Aristocrat.Monaco.Application.Contracts.Tickets
{
    using Hardware.Contracts.Ticket;

    /// <summary>
    ///     Definition of the IVerificationTicketCreator interface.
    /// </summary>
    public interface IVerificationTicketCreator
    {
        /// <summary>
        ///     Creates VERIFICATION Ticket
        /// </summary>
        /// <param name="pageNumber"> The page number to print</param>
        /// <param name="titleOverride">Title override.</param>
        /// <returns>created ticket</returns>
        Ticket Create(int pageNumber, string titleOverride = null);
    }
}