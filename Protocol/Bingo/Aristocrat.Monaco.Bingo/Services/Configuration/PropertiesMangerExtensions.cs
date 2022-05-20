namespace Aristocrat.Monaco.Bingo.Services.Configuration
{
    using System;
    using Accounting.Contracts;
    using Common.Storage.Model;
    using Gaming.Contracts;
    using Kernel;

    public static class PropertiesMangerExtensions
    {
        public static void ConfigureLargeWinStrategy(this IPropertiesManager properties, JackpotStrategy strategy)
        {
            if (properties == null)
            {
                throw new ArgumentNullException(nameof(properties));
            }

            properties.SetProperty(
                GamingConstants.GameWinLargeWinCashOutStrategy,
                strategy == JackpotStrategy.CreditJackpotWin
                    ? LargeWinCashOutStrategy.None
                    : LargeWinCashOutStrategy.Handpay);
            properties.SetProperty(
                AccountingConstants.AllowGameWinReceipts,
                strategy == JackpotStrategy.HandpayJackpotWin);
        }
    }
}