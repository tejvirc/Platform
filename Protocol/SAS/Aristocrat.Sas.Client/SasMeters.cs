namespace Aristocrat.Sas.Client
{
    /// <summary>
    ///     Defines the number of a meter to associate with a named meter
    ///     The values are the long poll command that requests the meter NOT
    ///     the meter codes given in Appendix C-7 of the SAS Specification.
    ///     The meter codes in Appendix C-7 are only for long polls 2F, 6F, and AF
    /// </summary>
    public enum SasMeters
    {
        TotalCanceledCredits = 0x10,
        TotalCoinIn,
        TotalCoinOut,
        TotalDrop,
        TotalJackpot,
        GamesPlayed,
        GamesWon,
        GamesLost,
        GamesSinceLastPowerUp,
        GamesSinceLastDoorClose,
        CurrentCredits = 0x1A,
        TotalBillIn = 0x20,
        TrueCoinIn = 0x2A,
        TrueCoinOut,
        CurrentHopperLevel,
        TotalHandPaidCanceledCredits,
        DollarIn1 = 0x31,
        DollarsIn2,
        DollarsIn5,
        DollarsIn10,
        DollarsIn20,
        DollarsIn50,
        DollarsIn100,
        DollarsIn500,
        DollarsIn1000,
        DollarsIn200,
        DollarsIn25,
        DollarsIn2000,
        DollarsIn2500 = 0x3E,
        DollarsIn5000,
        DollarsIn10000,
        DollarsIn20000,
        DollarsIn25000,
        DollarsIn50000,
        DollarsIn100000,
        DollarsIn250,
        CreditAmountOfAcceptedBills,
        CoinAmountAcceptedFromExternal,
        NumberOfBillsInStacker = 0x49,
        TotalCreditOfBillsInStacker,
        TournamentGamesPlayed = 0x95,
        TournamentGamesWon,
        TournamentCreditsWagered,
        TournamentCreditsWon,
        MainDoorOpened,
        TopMainDoorOpened,
        PowerReset
    }
}