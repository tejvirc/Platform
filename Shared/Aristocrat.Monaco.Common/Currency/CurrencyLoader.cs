namespace Aristocrat.Monaco.Common.Currency
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using log4net;

    /// <summary>
    /// Load the currencies from windows
    /// </summary>
    public static class CurrencyLoader
    {
        /// <summary>
        /// Load the currencies from Windows cultures
        /// </summary>
        /// <param name="logger"></param>
        /// <returns>The currency list</returns>
        public static Dictionary<string, CultureInfo> GetCurrenciesFromWindows(ILog logger)
        {
            Dictionary<string, CultureInfo> currencyList = new Dictionary<string, CultureInfo>();
            try
            {
                // go through each culture and region to get the currency code
                foreach (var culture in CultureInfo.GetCultures(CultureTypes.AllCultures)
                             .Where(c => !string.IsNullOrEmpty(c.Name) && !c.IsNeutralCulture))
                {
                    var region = new RegionInfo(culture.Name);
                    string currencyCode = region.ISOCurrencySymbol;

                    if (!string.IsNullOrEmpty(currencyCode) && !currencyList.ContainsKey(currencyCode))
                    {
                        currencyList[currencyCode] = culture;
                    }
                }
            }
            catch (Exception e)
            {
                logger?.Error("Failed to load the currency information from Windows system.", e);
            }
            
            return currencyList;
        }
    }
}
