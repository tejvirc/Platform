namespace Aristocrat.Monaco.Sas
{
    using System.Collections.Generic;
    using Aristocrat.Sas.Client;
    using Hardware.Contracts.NoteAcceptor;

    /// <summary>
    /// SAS country codes.
    /// </summary>
    public static class SASCountryCodes
    {
        private static readonly Dictionary<ISOCurrencyCode, BillAcceptorCountryCode> ISOCurrencyCodeToSASCountryCode = new Dictionary<ISOCurrencyCode, BillAcceptorCountryCode>
        {
            { ISOCurrencyCode.ANG, BillAcceptorCountryCode.Holland },
            { ISOCurrencyCode.ARS, BillAcceptorCountryCode.Argentina },
            { ISOCurrencyCode.AUD, BillAcceptorCountryCode.Australia },
            { ISOCurrencyCode.BGN, BillAcceptorCountryCode.Bulgaria },
            { ISOCurrencyCode.BRL, BillAcceptorCountryCode.Brazil },
            { ISOCurrencyCode.CAD, BillAcceptorCountryCode.Canada },
            { ISOCurrencyCode.CHF, BillAcceptorCountryCode.Switzerland },
            { ISOCurrencyCode.COP, BillAcceptorCountryCode.Columbia },
            { ISOCurrencyCode.CZK, BillAcceptorCountryCode.Czechoslovakia },
            { ISOCurrencyCode.DKK, BillAcceptorCountryCode.Denmark },
            { ISOCurrencyCode.EUR, BillAcceptorCountryCode.Euro },
            { ISOCurrencyCode.GBP, BillAcceptorCountryCode.GreatBritain },
            { ISOCurrencyCode.GIP, BillAcceptorCountryCode.Gibraltar },
            { ISOCurrencyCode.HUF, BillAcceptorCountryCode.Hungary },
            { ISOCurrencyCode.MAD, BillAcceptorCountryCode.Morocco },
            { ISOCurrencyCode.MXN, BillAcceptorCountryCode.Mexico },
            { ISOCurrencyCode.NOK, BillAcceptorCountryCode.Norway },
            { ISOCurrencyCode.PLN, BillAcceptorCountryCode.Poland },
            { ISOCurrencyCode.RON, BillAcceptorCountryCode.Romania },
            { ISOCurrencyCode.RUB, BillAcceptorCountryCode.Russia },
            { ISOCurrencyCode.SEK, BillAcceptorCountryCode.Sweden },
            { ISOCurrencyCode.TRY, BillAcceptorCountryCode.Turkey },
            { ISOCurrencyCode.USD, BillAcceptorCountryCode.UnitedStates },
            { ISOCurrencyCode.USN, BillAcceptorCountryCode.UnitedStates },
            { ISOCurrencyCode.USS, BillAcceptorCountryCode.UnitedStates },
            { ISOCurrencyCode.ZAR, BillAcceptorCountryCode.SouthAfrica }
        };

        /// <summary>
        ///     Gets the SAS country code associated with the ISOCurrencyCode
        /// </summary>
        /// <param name="currencyCode">The ISOCurrencyCode</param>
        /// <returns>The SAS country code</returns>
        public static BillAcceptorCountryCode ToSASCountryCode(ISOCurrencyCode currencyCode)
        {
            return ISOCurrencyCodeToSASCountryCode.TryGetValue(currencyCode, out var countryCode)
                ? countryCode
                : BillAcceptorCountryCode.Unknown;
        }
    }
}