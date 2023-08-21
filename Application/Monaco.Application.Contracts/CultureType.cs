////////////////////////////////////////////////////////////////////////////////////////////
// <copyright file="TicketCultures.cs" company="ARISTOCRAT TECHNOLOGIES AUSTRALIA PTY LTD">
// COPYRIGHT © 2017 ARISTOCRAT TECHNOLOGIES AUSTRALIA PTY LTD
// Absolutely no use, dissemination or copying in any matter whatsoever
// Of this material or any portion of it is to be made without the prior
// written authorisation of Aristocrat Technologies Australia Pty Ltd.
// All rights in and to this work are fully reserved
// </copyright>
////////////////////////////////////////////////////////////////////////////////////////////

namespace Aristocrat.Monaco.Application.Contracts
{
    /// <summary>
    ///     Definition of the CultureType enumeration
    /// </summary>
    public enum CultureType
    {
        /// <summary>
        ///     Specifies use of the culture for the player's use.
        /// </summary>
        PlayerFacingTicket,

        /// <summary>
        ///     Specifies use of the culture for the operator's purposes.
        /// </summary>
        OperatorFacingTicket
    }
}