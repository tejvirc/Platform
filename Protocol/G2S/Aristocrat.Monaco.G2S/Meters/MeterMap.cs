namespace Aristocrat.Monaco.G2S.Meters
{
    using System.Collections.Generic;
    using Accounting.Contracts;
    using Application.Contracts;
    using Aristocrat.G2S;
    using Aristocrat.G2S.Extension.ATI;
    using Gaming.Contracts;

    public static class MeterMap
    {
        static MeterMap()
        {
            DeviceMeters = new Dictionary<string, IDictionary<string, string>>
            {
                { DeviceClass.G2S_gamePlay, PerformanceMeters },
                { DeviceClass.G2S_cabinet, CabinetMeters },
                { DeviceClass.G2S_voucher, VoucherMeters },
                { DeviceClass.G2S_noteAcceptor, CurrencyInMeters },
                { DeviceClass.G2S_handpay, HandpayMeters },
                { DeviceClass.G2S_bonus, BonusMeters },
                { DeviceClass.G2S_progressive, ProgressiveMeters },
                // { DeviceClass.G2S_noteDispenser, CurrencyOutMeters }
            };
            // DeviceMeters.Add(DeviceClass.G2S_gamePlay, WagerCategoryMeters);
            // DeviceMeters.Add(DeviceClass.G2S_gamePlay, GameDenomMeters);
        }

        public static IDictionary<string, IDictionary<string, string>> DeviceMeters { get; }

        public static IDictionary<string, string> CurrencyInMeters { get; } = new Dictionary<string, string>
        {
            { CurrencyMeterName.CurrencyInAmount, "TotalCashIn" },
            { CurrencyMeterName.CurrencyInCount, "TotalBillCount" },
            // { CurrencyMeterName.PromoOutAmount, "" },
            // { CurrencyMeterName.DispenserDoorOpenCount, "" },
            // { CurrencyMeterName.CurrencyOutCount, "" },
            // { CurrencyMeterName.CurrencyOutAmount, "" },
            { CurrencyMeterName.PowerOffStackerRemovedCount, "" },
            { CurrencyMeterName.PowerOffDropDoorOpenCount, ApplicationMeters.CashDoorOpenPowerOffCount },
            { CurrencyMeterName.StackerRemovedCount, ApplicationMeters.StackerRemovedCount },
            { CurrencyMeterName.NonCashableToDispAmount, "VoucherOutNonCashablePromotionalValue" },
            { CurrencyMeterName.NonCashableToDropAmount, "VoucherInNonCashablePromotionalValue" },
            { CurrencyMeterName.NonCashableInAmount, "VoucherInNonCashablePromotionalValue" },
            { CurrencyMeterName.PromoToDispAmount, "CashableAndPromoVouchersOutValue" },
            { CurrencyMeterName.PromoToDropAmount, "VoucherInCashablePromotionalValue" },
            // { CurrencyMeterName.PromoInAmount, "" },
            { CurrencyMeterName.DropDoorOpenCount, ApplicationMeters.CashDoorOpenCount },
            // { CurrencyMeterName.CurrencyToDispenserCount, "" },
            { CurrencyMeterName.CurrencyToDropAmount, "TotalCashIn" },
            { CurrencyMeterName.CurrencyToDropCount, "TotalBillCount" }
        };

        public static IDictionary<string, string> CabinetMeters { get; } = new Dictionary<string, string>
        {
            { CabinetMeterName.HandPaidCancelAmount, AccountingMeters.HandpaidCancelAmount },
            { CabinetMeterName.CardPlayedCount, PlayerMeters.CardedPlayedCount },
            { CabinetMeterName.CardedBonusWonAmount, PlayerMeters.CardedBonusWonAmount },
            { CabinetMeterName.GamesSinceInitCount, GamingMeters.PlayedCount },
            { CabinetMeterName.GamesSinceDoorClosedCount, GamingMeters.GamesPlayedSinceDoorClosed },
            { CabinetMeterName.LogicDoorOpenCount, ApplicationMeters.LogicDoorOpenCount },
            { CabinetMeterName.AuxiliaryDoorOpenCount, ApplicationMeters.TopBoxDoorOpenCount },
            { CabinetMeterName.CabinetDoorOpenCount, ApplicationMeters.MainDoorOpenCount },
            { CabinetMeterName.PowerOffCabinetDoorOpenCount, ApplicationMeters.MainDoorOpenPowerOffCount },
            { CabinetMeterName.PowerOffLogicDoorOpenCount, ApplicationMeters.LogicDoorOpenPowerOffCount },
            { CabinetMeterName.PowerOffAuxiliaryDoorOpenCount, ApplicationMeters.TopBoxDoorOpenPowerOffCount },
            { CabinetMeterName.GamesSincePowerResetCount, CabinetMeterName.GamesSincePowerResetCount },
            { CabinetMeterName.HandPaidBonusNonWonAmount, Gaming.Contracts.Bonus.BonusMeters.HandPaidBonusGameNonWonAmount },
            { CabinetMeterName.EgmPaidBonusNonWonAmount, Gaming.Contracts.Bonus.BonusMeters.EgmPaidBonusGameNonWonAmount },
            { CabinetMeterName.HandPaidBonusWonAmount, GamingMeters.HandPaidBonusAmount },
            { CabinetMeterName.EgmPaidBonusWonAmount, Gaming.Contracts.Bonus.BonusMeters.EgmPaidBonusGameWonAmount },
            { CabinetMeterName.PlayerCashableAmount, CabinetMeterName.PlayerCashableAmount },
            { CabinetMeterName.PlayerPromoAmount, CabinetMeterName.PlayerPromoAmount },
            { CabinetMeterName.PlayerNonCashableAmount, CabinetMeterName.PlayerNonCashableAmount },
            { CabinetMeterName.WageredCashableAmount, GamingMeters.WageredCashableAmount },
            { CabinetMeterName.WageredPromoAmount, GamingMeters.WageredPromoAmount },
            { CabinetMeterName.WageredNonCashableAmount, GamingMeters.WageredNonCashableAmount },
            { CabinetMeterName.CardedWageredNonCashableAmount, PlayerMeters.CardedWageredNonCashableAmount },
            { CabinetMeterName.CardedWageredCashableAmount, PlayerMeters.CardedWageredCashableAmount },
            { CabinetMeterName.CardedWageredPromoAmount, PlayerMeters.CardedWageredPromoAmount },
            { CabinetMeterName.EgmPaidGameWonAmount, GamingMeters.TotalEgmPaidGameWonAmount },
            { CabinetMeterName.HandPaidGameWonAmount, GamingMeters.TotalHandPaidGameWonAmount },
            { CabinetMeterName.EgmPaidProgWonAmount, GamingMeters.EgmPaidProgWonAmount },
            { CabinetMeterName.HandPaidProgWonAmount, GamingMeters.HandPaidProgWonAmount },
            { CabinetMeterName.CardedGameWonAmount, PlayerMeters.CardedGameWonAmount },
            { CabinetMeterName.MjtGamesPlayedCount, Gaming.Contracts.Bonus.BonusMeters.MjtGamesPlayedCount },
            { CabinetMeterName.MjtGamesPaidCount, Gaming.Contracts.Bonus.BonusMeters.MjtBonusCount },
            { CabinetMeterName.MjtBonusAmount, Gaming.Contracts.Bonus.BonusMeters.MjtBonusAmount },
            { CabinetMeterName.WagerMatchPlayedCount, GamingMeters.WagerMatchBonusCount },
            { CabinetMeterName.WagerMatchBonusAmount, GamingMeters.WagerMatchBonusAmount }
            // { CabinetMeterName.CardedBonusNonWonAmount, "" },
            // { CabinetMeterName.EgmDispensedCashableAmount, "" },
            // { CabinetMeterName.EgmDispensedPromoAmount, "" },
            // { CabinetMeterName.EgmDispensedNonCashableAmount, "" },
        };

        public static IDictionary<string, string> VoucherMeters { get; } = new Dictionary<string, string>
        {
            { VoucherMeterName.CashableInAmount, @"VoucherInCashableValue" },
            { VoucherMeterName.CashableInCount, @"VoucherInCashableCount" },
            { VoucherMeterName.PromoOutCount, "VoucherOutNonCashablePromotionalCount" },
            { VoucherMeterName.PromoOutAmount, "VoucherOutNonCashablePromotionalValue" },
            { VoucherMeterName.CashableOutCount, "VoucherOutCashableCount" },
            { VoucherMeterName.CashableOutAmount, "VoucherOutCashableValue" },
            // { VoucherMeterName.NonCashableSystemInCount, "" },
            // { VoucherMeterName.NonCashableSystemInAmount, "" },
            { VoucherMeterName.PromoSystemInCount, "VoucherInNonTransferablePromotionalCount" },
            { VoucherMeterName.PromoSystemInAmount, "VoucherInNonTransferablePromotionalValue" },
            // { VoucherMeterName.CashableSystemInCount, "" },
            // { VoucherMeterName.CashableSystemInAmount, "" },
            { VoucherMeterName.NonCashableInCount, "VoucherInNonCashablePromotionalCount" },
            { VoucherMeterName.NonCashableInAmount, "VoucherInNonCashablePromotionalValue" },
            { VoucherMeterName.PromoInCount, "VoucherInCashablePromotionalCount" },
            { VoucherMeterName.PromoInAmount, "VoucherInCashablePromotionalValue" },
            { VoucherMeterName.NonCashableOutAmount, "VoucherOutNonCashablePromotionalValue" },
            { VoucherMeterName.NonCashableOutCount, "VoucherOutCashablePromotionalCount" }
        };

        public static IDictionary<string, string> PerformanceMeters { get; } = new Dictionary<string, string>
        {
            { PerformanceMeterName.WageredAmount, GamingMeters.WageredAmount },
            // { PerformanceMeterName.TournamentGamesPlayedCount, "" },
            { PerformanceMeterName.TheoreticalPaybackAmount, GamingMeters.TheoPayback },
            { PerformanceMeterName.SecondaryFailedCount, GamingMeters.SecondaryFailedCount },
            { PerformanceMeterName.SecondaryTiedCount, GamingMeters.SecondaryTiedCount },
            { PerformanceMeterName.SecondaryLostCount, GamingMeters.SecondaryLostCount },
            { PerformanceMeterName.SecondaryWonCount, GamingMeters.SecondaryWonCount },
            { PerformanceMeterName.SecondaryWonAmount, GamingMeters.SecondaryWonAmount },
            { PerformanceMeterName.SecondaryWageredAmount, GamingMeters.SecondaryWageredAmount },
            { PerformanceMeterName.AveragePaybackPercent, GamingMeters.AveragePayback },
            { PerformanceMeterName.FailedCount, GamingMeters.FailedCount },
            { PerformanceMeterName.TiedCount, GamingMeters.TiedCount },
            { PerformanceMeterName.LostCount, GamingMeters.LostCount },
            { PerformanceMeterName.WonCount, GamingMeters.WonCount },
            { PerformanceMeterName.HandPaidProgWonAmount, GamingMeters.HandPaidProgWonAmount },
            { PerformanceMeterName.EgmPaidProgWonAmount, GamingMeters.EgmPaidProgWonAmount },
            { PerformanceMeterName.HandPaidGameWonAmount, GamingMeters.HandPaidGameWonAmount },
            { PerformanceMeterName.EgmPaidGameWonAmount, GamingMeters.EgmPaidGameWonAmount },
            { PerformanceMeterName.TournamentCreditsWageredCount, "" }
            // { PerformanceMeterName.TournamentCreditsWonCount, "" },
        };

        public static IDictionary<string, string> ProgressiveMeters { get; } = new Dictionary<string, string>
        {
            { ProgressiveMeterName.WageredAmount, Gaming.Contracts.Progressives.ProgressiveMeters.LinkedProgressiveWageredAmount },
            { ProgressiveMeterName.PlayedCount, Gaming.Contracts.Progressives.ProgressiveMeters.LinkedProgressivePlayedCount },
            { ContributionMeterName.AnteBet, Gaming.Contracts.Progressives.ProgressiveMeters.LinkedProgressiveWageredAmountWithAnte },
        };

        //This mapping provides a map between a level id and the appropriate bulk contribution meter name
        public static IDictionary<int, string> BulkContributionMeters { get; } = new Dictionary<int, string>
        {
            { 1, ContributionMeterName.BulkContribution01 },
            { 2, ContributionMeterName.BulkContribution02 },
            { 3, ContributionMeterName.BulkContribution03 },
            { 4, ContributionMeterName.BulkContribution04 },
            { 5, ContributionMeterName.BulkContribution05 },
            { 6, ContributionMeterName.BulkContribution06 },
            { 7, ContributionMeterName.BulkContribution07 },
            { 8, ContributionMeterName.BulkContribution08 },
            { 9, ContributionMeterName.BulkContribution09 },
            { 10, ContributionMeterName.BulkContribution10 },
            { 11, ContributionMeterName.BulkContribution11 },
            { 12, ContributionMeterName.BulkContribution12 },
        };

        public static IDictionary<string, string> BonusMeters { get; } = new Dictionary<string, string>
        {
            { TransferMeterName.CashableInAmount, Gaming.Contracts.Bonus.BonusMeters.BonusCashableInAmount },
            { TransferMeterName.PromoInAmount, Gaming.Contracts.Bonus.BonusMeters.BonusPromoInAmount},
            { TransferMeterName.NonCashableInAmount, Gaming.Contracts.Bonus.BonusMeters.BonusNonCashableInAmount },
            { TransferMeterName.TransferInCount, Gaming.Contracts.Bonus.BonusMeters.BonusTotalCount },
        };

        public static IDictionary<string, string> GameDenomMeters { get; } = new Dictionary<string, string>
        {
            { GameDenomMeterName.WageredAmount, GamingMeters.WageredAmount },
            { GameDenomMeterName.PlayedCount, GamingMeters.PlayedCount },
            { GameDenomMeterName.AveragePaybackPercent, GamingMeters.AveragePayback },
            { GameDenomMeterName.TheoreticalPaybackAmount, GamingMeters.TheoPayback }
        };

        public static IDictionary<string, string> WagerCategoryMeters { get; } = new Dictionary<string, string>
        {
            { WagerCategoryMeterName.WageredAmount, GamingMeters.WagerCategoryWageredAmount },
            { WagerCategoryMeterName.PlayedCount, GamingMeters.WagerCategoryPlayedCount }
            // { WagerCategoryMeterName.PaybackPercent, "" }
        };

        public static IDictionary<string, string> HandpayMeters { get; } = new Dictionary<string, string>
        {
            { TransferMeterName.CashableOutAmount, AccountingMeters.HandpaidCashableAmount },
            { TransferMeterName.PromoOutAmount, AccountingMeters.HandpaidPromoAmount },
            { TransferMeterName.NonCashableOutAmount, AccountingMeters.HandpaidNonCashableAmount },
            { TransferMeterName.TransferOutCount, AccountingMeters.HandpaidOutCount }
        };
    }
}
