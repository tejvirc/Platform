

using Aristocrat.Monaco.Application.Contracts.Localization;
using Aristocrat.Monaco.Localization.Properties;

namespace Aristocrat.Monaco.Accounting.Contracts
{
    /// <summary>
    ///     Currency extensions that need to be in the Accounting layer
    /// </summary>
    public static class CurrencyAccountingExtensions
    {
        /// <summary>Creates a message that tells the last note state.</summary>
        /// <param name="state">The note state</param>
        /// <returns>The details of the state.</returns>
        public static string GetStatusText(CurrencyState state)
        {
            switch (state)
            {
                case CurrencyState.Pending:
                    return Localizer.For(CultureFor.Operator).GetString(ResourceKeys.PendingText);

                case CurrencyState.Accepting:
                    return Localizer.For(CultureFor.Operator).GetString(ResourceKeys.AcceptingText);

                case CurrencyState.Accepted:
                    return Localizer.For(CultureFor.Operator).GetString(ResourceKeys.AcceptedText);

                case CurrencyState.Returned:
                    return Localizer.For(CultureFor.Operator).GetString(ResourceKeys.ReturnedText);

                case CurrencyState.Rejected:
                default:
                    return Localizer.For(CultureFor.Operator).GetString(ResourceKeys.RejectedText);
            }
        }

        /// <summary>Creates a message that tells the bill rejection reason.</summary>
        /// <param name="exceptionCode">The bill rejection exception code.</param>
        /// <returns>The details of the exception.</returns>
        public static string GetDetailsMessage(int exceptionCode)
        {
            switch ((CurrencyInExceptionCode)exceptionCode)
            {
                case CurrencyInExceptionCode.Virtual:
                    return Localizer.For(CultureFor.Operator).GetString(ResourceKeys.VirtualText);

                case CurrencyInExceptionCode.CreditLimitExceeded:
                    return Localizer.For(CultureFor.Operator).GetString(ResourceKeys.CreditLimitExceeded);

                case CurrencyInExceptionCode.CreditInLimitExceeded:
                    return Localizer.For(CultureFor.Operator).GetString(ResourceKeys.CreditInLimitExceeded);

                case CurrencyInExceptionCode.LaundryLimitExceeded:
                    return Localizer.For(CultureFor.Operator).GetString(ResourceKeys.SessionLimitReached);

                case CurrencyInExceptionCode.InvalidBill:
                    return Localizer.For(CultureFor.Operator).GetString(ResourceKeys.InvalidBill);

                case CurrencyInExceptionCode.PowerFailure:
                    return Localizer.For(CultureFor.Operator).GetString(ResourceKeys.PowerFailure);

                case CurrencyInExceptionCode.None:
                    return Localizer.For(CultureFor.Operator).GetString(ResourceKeys.AcceptedText);

                case CurrencyInExceptionCode.Other:
                case CurrencyInExceptionCode.NoteAcceptorFailure:
                default:
                    return Localizer.For(CultureFor.Operator).GetString(ResourceKeys.NoteAcceptorFailure);
            }
        }

    }
}
