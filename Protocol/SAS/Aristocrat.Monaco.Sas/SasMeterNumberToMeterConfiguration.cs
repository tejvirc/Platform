namespace Aristocrat.Monaco.Sas
{
    using Accounting.Contracts;
    using Application.Contracts;
    using Aristocrat.Sas.Client;
    using Contracts.Metering;
    using Gaming.Contracts;
    using System.Collections.Generic;

    /// <summary>
    ///     Converts a meter number to a meter configuration
    /// </summary>
    public static class SasMeterNumberToMeterConfiguration
    {
        private static readonly SasMeterConfiguration NullMeterConfiguration =
            new SasMeterConfiguration(string.Empty, MeterCategory.Occurrence);

        private static readonly Dictionary<SasMeters, SasMeterConfiguration> MeterNumberToName =
            new Dictionary<SasMeters, SasMeterConfiguration>
            {
                {
                    SasMeters.TotalHandPaidCanceledCredits,
                    new SasMeterConfiguration(AccountingMeters.HandpaidCancelAmount, MeterCategory.Credit)
                },
                {
                    SasMeters.TotalCanceledCredits,
                    new SasMeterConfiguration(SasMeterNames.TotalCanceledCredits, MeterCategory.Credit)
                },
                {
                    SasMeters.TotalCoinIn, new SasMeterConfiguration(GamingMeters.WageredAmount, MeterCategory.Credit)
                },
                {
                    SasMeters.TotalCoinOut,
                    new SasMeterConfiguration(GamingMeters.TotalEgmPaidAmt, MeterCategory.Credit)
                },
                {
                    SasMeters.TotalDrop,
                    new SasMeterConfiguration(ApplicationMeters.TotalIn, MeterCategory.Credit)
                },
                {
                    SasMeters.TotalJackpot,
                    new SasMeterConfiguration(GamingMeters.TotalHandPaidAmt, MeterCategory.Credit)
                },
                {
                    SasMeters.GamesPlayed,
                    new SasMeterConfiguration(GamingMeters.PlayedCount, MeterCategory.Occurrence)
                },
                {
                    SasMeters.GamesWon,
                    new SasMeterConfiguration(GamingMeters.WonCount, MeterCategory.Occurrence)
                },
                {
                    SasMeters.GamesLost,
                    new SasMeterConfiguration(GamingMeters.LostCount, MeterCategory.Occurrence)
                },
                {
                    SasMeters.GamesSinceLastPowerUp,
                    new SasMeterConfiguration(GamingMeters.GamesPlayedSinceReboot, MeterCategory.Occurrence)
                },
                {
                    SasMeters.GamesSinceLastDoorClose,
                    new SasMeterConfiguration(GamingMeters.GamesPlayedSinceDoorClosed, MeterCategory.Occurrence)
                },
                {
                    SasMeters.CurrentCredits,
                    new SasMeterConfiguration(AccountingMeters.CurrentCredits, MeterCategory.Credit)
                },
                {
                    SasMeters.TrueCoinIn,
                    new SasMeterConfiguration(AccountingMeters.TrueCoinIn, MeterCategory.Cents)
                },
                {
                    SasMeters.TrueCoinOut,
                    new SasMeterConfiguration(AccountingMeters.TrueCoinOut, MeterCategory.Cents)
                },
                {
                    SasMeters.CurrentHopperLevel,
                    new SasMeterConfiguration(AccountingMeters.CurrentHopperLevel, MeterCategory.Credit)
                },
                {
                    SasMeters.DollarIn1,
                    new SasMeterConfiguration(AccountingMeters.BillCount1s, MeterCategory.Occurrence)
                }, // these meter names are created at startup based on supported denominations in CurrencyInMetersProvider
                {
                    SasMeters.DollarsIn2,
                    new SasMeterConfiguration(AccountingMeters.BillCount2s, MeterCategory.Occurrence)
                },
                {
                    SasMeters.DollarsIn5,
                    new SasMeterConfiguration(AccountingMeters.BillCount5s, MeterCategory.Occurrence)
                },
                {
                    SasMeters.DollarsIn10,
                    new SasMeterConfiguration(AccountingMeters.BillCount10s, MeterCategory.Occurrence)
                },
                {
                    SasMeters.DollarsIn20,
                    new SasMeterConfiguration(AccountingMeters.BillCount20s, MeterCategory.Occurrence)
                },
                {
                    SasMeters.DollarsIn50,
                    new SasMeterConfiguration(AccountingMeters.BillCount50s, MeterCategory.Occurrence)
                },
                {
                    SasMeters.DollarsIn100,
                    new SasMeterConfiguration(AccountingMeters.BillCount100s, MeterCategory.Occurrence)
                },
                {
                    SasMeters.DollarsIn500,
                    new SasMeterConfiguration(AccountingMeters.BillCount500s, MeterCategory.Occurrence)
                },
                {
                    SasMeters.DollarsIn1000,
                    new SasMeterConfiguration(AccountingMeters.BillCount1_000s, MeterCategory.Occurrence)
                },
                {
                    SasMeters.DollarsIn200,
                    new SasMeterConfiguration(AccountingMeters.BillCount200s, MeterCategory.Occurrence)
                },
                {
                    SasMeters.DollarsIn25,
                    new SasMeterConfiguration(AccountingMeters.BillCount25s, MeterCategory.Occurrence)
                },
                {
                    SasMeters.DollarsIn2000,
                    new SasMeterConfiguration(AccountingMeters.BillCount2_000s, MeterCategory.Occurrence)
                },
                {
                    SasMeters.DollarsIn2500,
                    new SasMeterConfiguration(AccountingMeters.BillCount2_500s, MeterCategory.Occurrence)
                },
                {
                    SasMeters.DollarsIn5000,
                    new SasMeterConfiguration(AccountingMeters.BillCount5_000s, MeterCategory.Occurrence)
                },
                {
                    SasMeters.DollarsIn10000,
                    new SasMeterConfiguration(AccountingMeters.BillCount10_000s, MeterCategory.Occurrence)
                },
                {
                    SasMeters.DollarsIn20000,
                    new SasMeterConfiguration(AccountingMeters.BillCount20_000s, MeterCategory.Occurrence)
                },
                {
                    SasMeters.DollarsIn25000,
                    new SasMeterConfiguration(AccountingMeters.BillCount25_000s, MeterCategory.Occurrence)
                },
                {
                    SasMeters.DollarsIn50000,
                    new SasMeterConfiguration(AccountingMeters.BillCount50_000s, MeterCategory.Occurrence)
                },
                {
                    SasMeters.DollarsIn100000,
                    new SasMeterConfiguration(AccountingMeters.BillCount100_000s, MeterCategory.Occurrence)
                },
                {
                    SasMeters.DollarsIn250,
                    new SasMeterConfiguration(AccountingMeters.BillCount250s, MeterCategory.Occurrence)
                },
                {
                    SasMeters.CreditAmountOfAcceptedBills,
                    new SasMeterConfiguration(AccountingMeters.CreditAmountOfAcceptedBills, MeterCategory.Credit)
                },
                {
                    SasMeters.CoinAmountAcceptedFromExternal,
                    new SasMeterConfiguration(AccountingMeters.CoinAmountAcceptedFromExternal, MeterCategory.Credit)
                },
                {
                    SasMeters.TournamentGamesPlayed,
                    new SasMeterConfiguration(GamingMeters.TournamentGamesPlayed, MeterCategory.Occurrence)
                },
                {
                    SasMeters.TournamentGamesWon,
                    new SasMeterConfiguration(GamingMeters.TournamentGamesWon, MeterCategory.Occurrence)
                },
                {
                    SasMeters.TournamentCreditsWagered,
                    new SasMeterConfiguration(GamingMeters.TournamentCreditsWagered, MeterCategory.Credit)
                },
                {
                    SasMeters.TournamentCreditsWon,
                    new SasMeterConfiguration(GamingMeters.TournamentCreditsWon, MeterCategory.Credit)
                },
                {
                    SasMeters.TotalCreditOfBillsInStacker,
                    new SasMeterConfiguration(AccountingMeters.CurrencyInAmount, MeterCategory.Credit)
                },
                {
                    SasMeters.NumberOfBillsInStacker,
                    new SasMeterConfiguration(AccountingMeters.CurrencyInCount, MeterCategory.Occurrence)
                },
                {
                    SasMeters.MainDoorOpened,
                    new SasMeterConfiguration(ApplicationMeters.MainDoorOpenCount, MeterCategory.Occurrence)
                },
                {
                    SasMeters.TopMainDoorOpened,
                    new SasMeterConfiguration(ApplicationMeters.TopBoxDoorOpenCount, MeterCategory.Occurrence)
                },
                {
                    SasMeters.PowerReset,
                    new SasMeterConfiguration(ApplicationMeters.PowerResetCount, MeterCategory.Occurrence)
                },
                {
                    SasMeters.TotalBillIn,
                    new SasMeterConfiguration(AccountingMeters.CurrencyInAmount, MeterCategory.Dollars)
                }
            };

        /// <summary>
        ///     Gets the sas meter configuration associated with the SAS meter number
        /// </summary>
        /// <param name="meterNumber">The SAS meter number</param>
        /// <returns>The sas meter configuration associated with the meter number</returns>
        public static SasMeterConfiguration GetMeterConfiguration(SasMeters meterNumber) =>
            MeterNumberToName.TryGetValue(meterNumber, out var meter) ? meter : NullMeterConfiguration;
    }
}