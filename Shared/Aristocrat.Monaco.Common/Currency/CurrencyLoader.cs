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
        private const string WorldCultureName = "ar-001";
        private const string WorldCurrencySymbol = "XDR";

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
                int total = 0, listed = 0; 
                // go through each culture and region to get the currency code
                foreach (var culture in CultureInfo.GetCultures(CultureTypes.AllCultures)
                             .Where(c => !string.IsNullOrEmpty(c.Name) && !c.IsNeutralCulture))
                {
                    var region = new RegionInfo(culture.Name);
                    //There is one special scenario where culture is "ar-001"
                    //.NET 6 expects correct country name, if its incorrect RegionInfo will return
                    //empty string or special characters.
                    string currencyCode = culture.Name == WorldCultureName ? WorldCurrencySymbol : region.ISOCurrencySymbol;

                    if (!string.IsNullOrEmpty(currencyCode) && currencyCode != "¤¤" && !currencyList.ContainsKey(currencyCode))
                    {
                        currencyList[currencyCode] = (CultureInfo) culture.Clone();
                        Console.WriteLine($"Culture:[{culture.Name}];Currency:[{currencyCode}];Region:[{region.Name}]");
                        listed++;
                    }
                    total++;
                }

                Console.WriteLine($"Total cultures selected:[{total}];Listed currencies:[{listed}]");
            }
            catch (Exception e)
            {
                logger?.Error("Failed to load the currency information from Windows system.", e);
            }
            
            return currencyList;
        }
    }
}
