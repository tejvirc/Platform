namespace Aristocrat.Monaco.Asp.Client.DataSources
{
    using System.ComponentModel;

    public enum BonusReason
    {
        Unknown,

        StandaloneProgressiveWin,

        LinkProgressiveWin,

        PersonalProgressiveWin,

        StandaloneMysteryWin,

        LinkMysteryWin,

        SharedJackpotWin,

        [Description("Mr. Pokie Bonus Award")]
        MrPokieBonusAward,

        MemberBonusAward,

        RandomBonusAward,

        NearMissAward,

        [Description("Cash Component of Prize + Cash")]
        CashComponentOfPrizeCash,

        MultiplierWin,

        WelcomeBackBonusAward,

        CarJackpotWin,

        HolidayJackpotWin,

        MotorbikeJackpotWin,

        DreamMakerJackpot,

        NonCashMysteryWin,

        NonCashProgressiveWin,

        BonusAward,
    }
}