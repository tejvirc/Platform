namespace Aristocrat.Monaco.Gaming.Contracts.Payment
{
    using System.Collections.Generic;

    /// <summary>
    ///     An interface that we can use in order to decide what to do with a payment depending on rules that may be
    ///     protocol or jurisdiction specific.
    /// </summary>
    public interface IPaymentDeterminationHandler
    {
        /// <summary>
        ///     Return the results of paying an amount to the player. This is used to figure out where the player's
        ///     payments (wins, bonuses, etc) should go - to the credit meter, to a handpay, to a voucher, etc. Normally
        ///     there will only be one result created for a win, and it will simply indicate whether the whole amount is
        ///     to be credited or hand/cash paid. For some jurisdictions though, this can get more complicated and multiple
        ///     results can be emitted, splitting parts of the win between the credit meter and a handpay or cash out.
        ///     See the PayGameResultsCommandHandler for usage.
        /// </summary>
        /// <param name="winInMillicents">The total amount that the player has to be paid.</param>
        /// <param name="isPayGameResults">
        ///     Is used to indicate whether we are doing a full prize calculation (e.g. for PayGameResults) or whether we
        ///     just need to know whether there will be a handpay happening (e.g. for PresentEventHandler).
        /// </param>
        /// <returns>A list of amounts that need to be paid to different places.</returns>
        List<PaymentDeterminationResult> GetPaymentResults(long winInMillicents, bool isPayGameResults=true);
    }
}