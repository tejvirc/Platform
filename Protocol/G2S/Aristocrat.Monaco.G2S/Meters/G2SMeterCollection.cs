namespace Aristocrat.Monaco.G2S.Meters
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using Accounting.Contracts;
    using Application.Contracts;
    using Aristocrat.G2S;
    using Gaming.Contracts;
    using Gaming.Contracts.Bonus;

    /// <summary>
    ///     G2S meter mapping collection
    /// </summary>
    public class G2SMeterCollection : IEnumerable<G2SMeter>
    {
        private static readonly IDictionary<string, G2SMeter> MeterCollection =
            new Dictionary<string, G2SMeter>
            {
                {
                    "performance." + PerformanceMeterName.AveragePaybackPercent, new G2SMeter(
                        "performance." + PerformanceMeterName.AveragePaybackPercent,
                        GamingMeters.AveragePayback)
                },
                {
                    "performance." + PerformanceMeterName.EgmPaidGameWonAmount, new G2SMeter(
                        "performance." + PerformanceMeterName.EgmPaidGameWonAmount,
                        GamingMeters.TotalEgmPaidGameWonAmount)
                },
                {
                    "performance." + PerformanceMeterName.EgmPaidProgWonAmount,
                    new G2SMeter(
                        "performance." + PerformanceMeterName.EgmPaidProgWonAmount,
                        GamingMeters.EgmPaidProgWonAmount)
                },
                {
                    "performance." + PerformanceMeterName.FailedCount,
                    new G2SMeter("performance." + PerformanceMeterName.FailedCount, GamingMeters.FailedCount)
                },
                {
                    "performance." + PerformanceMeterName.HandPaidGameWonAmount,
                    new G2SMeter(
                        "performance." + PerformanceMeterName.HandPaidGameWonAmount,
                        GamingMeters.TotalHandPaidGameWonAmount)
                },
                {
                    "performance." + PerformanceMeterName.HandPaidProgWonAmount,
                    new G2SMeter(
                        "performance." + PerformanceMeterName.HandPaidProgWonAmount,
                        GamingMeters.HandPaidProgWonAmount)
                },
                {
                    "performance." + PerformanceMeterName.LostCount,
                    new G2SMeter("performance." + PerformanceMeterName.LostCount, GamingMeters.LostCount)
                },
                {
                    "performance." + PerformanceMeterName.TheoreticalPaybackAmount, new G2SMeter(
                        "performance." + PerformanceMeterName.TheoreticalPaybackAmount,
                        GamingMeters.TheoPayback)
                },
                {
                    "performance." + PerformanceMeterName.TiedCount,
                    new G2SMeter("performance." + PerformanceMeterName.TiedCount, GamingMeters.TiedCount)
                },
                {
                    "performance." + PerformanceMeterName.WageredAmount,
                    new G2SMeter("performance." + PerformanceMeterName.WageredAmount, GamingMeters.WageredAmount)
                },
                {
                    "performance." + PerformanceMeterName.WonCount,
                    new G2SMeter("performance." + PerformanceMeterName.WonCount, GamingMeters.WonCount)
                },
                {
                    "performance." + PerformanceMeterName.SecondaryWageredAmount, new G2SMeter(
                        "performance." + PerformanceMeterName.SecondaryWageredAmount,
                        GamingMeters.SecondaryWageredAmount)
                },
                {
                    "performance." + PerformanceMeterName.SecondaryWonAmount, new G2SMeter(
                        "performance." + PerformanceMeterName.SecondaryWonAmount,
                        GamingMeters.SecondaryWonAmount)
                },
                {
                    "performance." + PerformanceMeterName.SecondaryWonCount, new G2SMeter(
                        "performance." + PerformanceMeterName.SecondaryWonCount,
                        GamingMeters.SecondaryWonCount)
                },
                {
                    "performance." + PerformanceMeterName.SecondaryLostCount, new G2SMeter(
                        "performance." + PerformanceMeterName.SecondaryLostCount,
                        GamingMeters.SecondaryLostCount)
                },
                {
                    "performance." + PerformanceMeterName.SecondaryTiedCount, new G2SMeter(
                        "performance." + PerformanceMeterName.SecondaryTiedCount,
                        GamingMeters.SecondaryTiedCount)
                },
                {
                    "performance." + PerformanceMeterName.SecondaryFailedCount, new G2SMeter(
                        "performance." + PerformanceMeterName.SecondaryFailedCount,
                        GamingMeters.SecondaryFailedCount)
                },
                {
                    "wager." + WagerCategoryMeterName.PlayedCount,
                    new G2SMeter("wager." + WagerCategoryMeterName.PlayedCount, GamingMeters.WagerCategoryPlayedCount)
                },
                {
                    "wager." + WagerCategoryMeterName.WageredAmount, new G2SMeter(
                        "wager." + WagerCategoryMeterName.WageredAmount,
                        GamingMeters.WagerCategoryWageredAmount)
                },
                {
                    "game." + GameDenomMeterName.WageredAmount,
                    new G2SMeter("game." + GameDenomMeterName.WageredAmount, GamingMeters.WageredAmount)
                },
                {
                    "game." + GameDenomMeterName.PlayedCount,
                    new G2SMeter("game." + GameDenomMeterName.PlayedCount, GamingMeters.PlayedCount)
                },
                {
                    "game." + GameDenomMeterName.AveragePaybackPercent,
                    new G2SMeter("game." + GameDenomMeterName.AveragePaybackPercent, GamingMeters.AveragePayback)
                },
                {
                    "game." + GameDenomMeterName.TheoreticalPaybackAmount,
                    new G2SMeter("game." + GameDenomMeterName.TheoreticalPaybackAmount, GamingMeters.TheoPayback)
                },
                /* These are used for the Wat device
                 {
                    "transfer." + TransferMeterName.CashableInAmount,
                    new G2SMeter("transfer." + TransferMeterName.CashableInAmount, AccountingMeters.WatOnCashableAmount)
                },
                {
                    "transfer." + TransferMeterName.PromoInAmount,
                    new G2SMeter(
                        "transfer." + TransferMeterName.PromoInAmount,
                        AccountingMeters.WatOnCashablePromoAmount)
                },
                {
                    "transfer." + TransferMeterName.NonCashableInAmount,
                    new G2SMeter(
                        "transfer." + TransferMeterName.NonCashableInAmount,
                        AccountingMeters.WatOnNonCashableAmount)
                },
                {
                    "transfer." + TransferMeterName.CashableOutAmount,
                    new G2SMeter(
                        "transfer." + TransferMeterName.CashableOutAmount,
                        AccountingMeters.WatOffCashableAmount)
                },
                {
                    "transfer." + TransferMeterName.PromoOutAmount,
                    new G2SMeter(
                        "transfer." + TransferMeterName.PromoOutAmount,
                        AccountingMeters.WatOffCashablePromoAmount)
                },
                {
                    "transfer." + TransferMeterName.NonCashableOutAmount,
                    new G2SMeter(
                        "transfer." + TransferMeterName.NonCashableOutAmount,
                        AccountingMeters.WatOffNonCashableAmount)
                },*/
                {
                    "currency." + CurrencyMeterName.CurrencyInAmount,
                    new G2SMeter("currency." + CurrencyMeterName.CurrencyInAmount, AccountingMeters.CurrencyInAmount)
                },
                {
                    "currency." + CurrencyMeterName.CurrencyInCount,
                    new G2SMeter("currency." + CurrencyMeterName.CurrencyInCount, AccountingMeters.CurrencyInCount)
                },
                {
                    "currency." + CurrencyMeterName.CurrencyToDropAmount,
                    new G2SMeter(
                        "currency." + CurrencyMeterName.CurrencyToDropAmount,
                        AccountingMeters.CurrencyInAmount)
                },
                {
                    "currency." + CurrencyMeterName.CurrencyToDropCount,
                    new G2SMeter("currency." + CurrencyMeterName.CurrencyToDropCount, AccountingMeters.CurrencyInCount)
                },
                {
                    "currency." + CurrencyMeterName.DropDoorOpenCount,
                    new G2SMeter("currency." + CurrencyMeterName.DropDoorOpenCount, ApplicationMeters.CashDoorOpenCount)
                },
                {
                    "currency." + CurrencyMeterName.PowerOffDropDoorOpenCount, new G2SMeter(
                        "currency." + CurrencyMeterName.PowerOffDropDoorOpenCount,
                        ApplicationMeters.CashDoorOpenPowerOffCount)
                },
                {
                    "currency." + CurrencyMeterName.StackerRemovedCount, new G2SMeter(
                        "currency." + CurrencyMeterName.StackerRemovedCount,
                        ApplicationMeters.StackerRemovedCount)
                },
                {
                    "currency." + CurrencyMeterName.PromoToDropAmount,
                    new G2SMeter("currency." + CurrencyMeterName.PromoToDropAmount, AccountingMeters.VoucherInCashablePromoAmount)
                },
                {
                    "cabinet." + CabinetMeterName.HandPaidCancelAmount,
                    new G2SMeter(
                        "cabinet." + CabinetMeterName.HandPaidCancelAmount,
                        AccountingMeters.HandpaidCancelAmount)
                },
                {
                    "cabinet." + CabinetMeterName.LogicDoorOpenCount,
                    new G2SMeter("cabinet." + CabinetMeterName.LogicDoorOpenCount, ApplicationMeters.LogicDoorOpenCount)
                },
                {
                    "cabinet." + CabinetMeterName.PowerOffLogicDoorOpenCount, new G2SMeter(
                        "cabinet." + CabinetMeterName.PowerOffLogicDoorOpenCount,
                        ApplicationMeters.LogicDoorOpenPowerOffCount)
                },
                {
                    "cabinet." + CabinetMeterName.AuxiliaryDoorOpenCount, new G2SMeter(
                        "cabinet." + CabinetMeterName.AuxiliaryDoorOpenCount,
                        ApplicationMeters.TopBoxDoorOpenCount)
                },
                {
                    "cabinet." + CabinetMeterName.PowerOffAuxiliaryDoorOpenCount, new G2SMeter(
                        "cabinet." + CabinetMeterName.PowerOffAuxiliaryDoorOpenCount,
                        ApplicationMeters.TopBoxDoorOpenPowerOffCount)
                },
                {
                    "cabinet." + CabinetMeterName.CabinetDoorOpenCount, new G2SMeter(
                        "cabinet." + CabinetMeterName.CabinetDoorOpenCount,
                        ApplicationMeters.MainDoorOpenCount)
                },
                {
                    "cabinet." + CabinetMeterName.PowerOffCabinetDoorOpenCount, new G2SMeter(
                        "cabinet." + CabinetMeterName.PowerOffCabinetDoorOpenCount,
                        ApplicationMeters.MainDoorOpenPowerOffCount)
                },
                {
                    "cabinet." + CabinetMeterName.GamesSinceInitCount,
                    new G2SMeter("cabinet." + CabinetMeterName.GamesSinceInitCount, GamingMeters.PlayedCount)
                },
                {
                    "cabinet." + CabinetMeterName.GamesSincePowerResetCount, new G2SMeter(
                        "cabinet." + CabinetMeterName.GamesSincePowerResetCount,
                        GamingMeters.PlayedCount,
                        MeterValueType.Session)
                },
                {
                    "cabinet." + CabinetMeterName.GamesSinceDoorClosedCount, new G2SMeter(
                        "cabinet." + CabinetMeterName.GamesSinceDoorClosedCount,
                        GamingMeters.GamesPlayedSinceDoorClosed)
                },
                {
                    "cabinet." + CabinetMeterName.EgmPaidGameWonAmount,
                    new G2SMeter("cabinet." + CabinetMeterName.EgmPaidGameWonAmount, GamingMeters.EgmPaidGameWonAmount)
                },
                {
                    "cabinet." + CabinetMeterName.EgmPaidProgWonAmount,
                    new G2SMeter("cabinet." + CabinetMeterName.EgmPaidProgWonAmount, GamingMeters.EgmPaidProgWonAmount)
                },
                {
                    "cabinet." + CabinetMeterName.HandPaidProgWonAmount,
                    new G2SMeter("cabinet." + CabinetMeterName.HandPaidProgWonAmount, GamingMeters.HandPaidProgWonAmount)
                },
                {
                    "cabinet." + CabinetMeterName.HandPaidGameWonAmount,
                    new G2SMeter(
                        "cabinet." + CabinetMeterName.HandPaidGameWonAmount,
                        GamingMeters.HandPaidGameWonAmount)
                },
                {
                    "cabinet." + CabinetMeterName.HandPaidBonusWonAmount,
                    new G2SMeter("cabinet." + CabinetMeterName.HandPaidBonusWonAmount, GamingMeters.HandPaidBonusAmount)
                },
                {
                    "cabinet." + CabinetMeterName.HandPaidBonusNonWonAmount,
                    new G2SMeter(
                        "cabinet." + CabinetMeterName.HandPaidBonusNonWonAmount,
                        BonusMeters.HandPaidBonusGameNonWonAmount)
                },
                {
                    "cabinet." + CabinetMeterName.WageredCashableAmount, new G2SMeter(
                        "cabinet." + CabinetMeterName.WageredCashableAmount,
                        GamingMeters.WageredCashableAmount)
                },
                {
                    "cabinet." + CabinetMeterName.WageredPromoAmount,
                    new G2SMeter("cabinet." + CabinetMeterName.WageredPromoAmount, GamingMeters.WageredPromoAmount)
                },
                {
                    "cabinet." + CabinetMeterName.WageredNonCashableAmount, new G2SMeter(
                        "cabinet." + CabinetMeterName.WageredNonCashableAmount,
                        GamingMeters.WageredNonCashableAmount)
                },
                {
                    "cabinet." + CabinetMeterName.CardPlayedCount,
                    new G2SMeter("cabinet." + CabinetMeterName.CardPlayedCount, PlayerMeters.CardedPlayedCount)
                },
                {
                    "cabinet." + CabinetMeterName.CardedBonusWonAmount,
                    new G2SMeter("cabinet." + CabinetMeterName.CardedBonusWonAmount, PlayerMeters.CardedBonusWonAmount)
                },
                {
                    "cabinet." + CabinetMeterName.CardedGameWonAmount,
                    new G2SMeter("cabinet." + CabinetMeterName.CardedGameWonAmount, PlayerMeters.CardedGameWonAmount)
                },
                {
                    "cabinet." + CabinetMeterName.CardedProgressiveWonAmount,
                    new G2SMeter(
                        "cabinet." + CabinetMeterName.CardedProgressiveWonAmount,
                        PlayerMeters.CardedProgressiveWonAmount)
                },
                {
                    "cabinet." + CabinetMeterName.CardedWageredCashableAmount,
                    new G2SMeter(
                        "cabinet." + CabinetMeterName.CardedWageredCashableAmount,
                        PlayerMeters.CardedWageredCashableAmount)
                },
                {
                    "cabinet." + CabinetMeterName.CardedWageredPromoAmount,
                    new G2SMeter(
                        "cabinet." + CabinetMeterName.CardedWageredPromoAmount,
                        PlayerMeters.CardedWageredPromoAmount)
                },
                {
                    "cabinet." + CabinetMeterName.CardedWageredNonCashableAmount,
                    new G2SMeter(
                        "cabinet." + CabinetMeterName.CardedWageredNonCashableAmount,
                        PlayerMeters.CardedWageredNonCashableAmount)
                },
                {
                    "cabinet." + CabinetMeterName.MjtGamesPlayedCount,
                    new G2SMeter("cabinet." + CabinetMeterName.MjtGamesPlayedCount, BonusMeters.MjtGamesPlayedCount)
                },
                {
                    "cabinet." + CabinetMeterName.MjtGamesPaidCount,
                    new G2SMeter("cabinet." + CabinetMeterName.MjtGamesPaidCount, BonusMeters.MjtBonusCount)
                },
                {
                    "cabinet." + CabinetMeterName.MjtBonusAmount,
                    new G2SMeter("cabinet." + CabinetMeterName.MjtBonusAmount, BonusMeters.MjtBonusAmount)
                },
                {
                    "cabinet." + CabinetMeterName.WagerMatchPlayedCount,
                    new G2SMeter("cabinet." + CabinetMeterName.WagerMatchPlayedCount, GamingMeters.WagerMatchBonusCount)
                },
                {
                    "cabinet." + CabinetMeterName.WagerMatchBonusAmount,
                    new G2SMeter(
                        "cabinet." + CabinetMeterName.WagerMatchBonusAmount,
                        GamingMeters.WagerMatchBonusAmount)
                },
                {
                    "voucher." + VoucherMeterName.CashableInAmount,
                    new G2SMeter(
                        "voucher." + VoucherMeterName.CashableInAmount,
                        AccountingMeters.VoucherInCashableAmount)
                },
                {
                    "voucher." + VoucherMeterName.CashableInCount,
                    new G2SMeter("voucher." + VoucherMeterName.CashableInCount, AccountingMeters.VoucherInCashableCount)
                },
                {
                    "voucher." + VoucherMeterName.PromoInAmount,
                    new G2SMeter(
                        "voucher." + VoucherMeterName.PromoInAmount,
                        AccountingMeters.VoucherInCashablePromoAmount)
                },
                {
                    "voucher." + VoucherMeterName.PromoInCount,
                    new G2SMeter(
                        "voucher." + VoucherMeterName.PromoInCount,
                        AccountingMeters.VoucherInCashablePromoCount)
                },
                {
                    "voucher." + VoucherMeterName.NonCashableInAmount, new G2SMeter(
                        "voucher." + VoucherMeterName.NonCashableInAmount,
                        AccountingMeters.VoucherInNonCashableAmount)
                },
                {
                    "voucher." + VoucherMeterName.NonCashableInCount, new G2SMeter(
                        "voucher." + VoucherMeterName.NonCashableInCount,
                        AccountingMeters.VoucherInNonCashableCount)
                },
                {
                    "voucher." + VoucherMeterName.PromoSystemInAmount, new G2SMeter(
                        "voucher." + VoucherMeterName.PromoSystemInAmount,
                        AccountingMeters.VoucherInNonTransferableAmount)
                },
                {
                    "voucher." + VoucherMeterName.PromoSystemInCount, new G2SMeter(
                        "voucher." + VoucherMeterName.PromoSystemInCount,
                        AccountingMeters.VoucherInNonTransferableCount)
                },
                {
                    "voucher." + VoucherMeterName.CashableOutAmount,
                    new G2SMeter(
                        "voucher." + VoucherMeterName.CashableOutAmount,
                        AccountingMeters.VoucherOutCashableAmount)
                },
                {
                    "voucher." + VoucherMeterName.CashableOutCount,
                    new G2SMeter(
                        "voucher." + VoucherMeterName.CashableOutCount,
                        AccountingMeters.VoucherOutCashableCount)
                },
                {
                    "voucher." + VoucherMeterName.PromoOutAmount,
                    new G2SMeter(
                        "voucher." + VoucherMeterName.PromoOutAmount,
                        AccountingMeters.VoucherOutCashablePromoAmount)
                },
                {
                    "voucher." + VoucherMeterName.PromoOutCount,
                    new G2SMeter(
                        "voucher." + VoucherMeterName.PromoOutCount,
                        AccountingMeters.VoucherOutCashablePromoCount)
                },
                {
                    "voucher." + VoucherMeterName.NonCashableOutAmount, new G2SMeter(
                        "voucher." + VoucherMeterName.NonCashableOutAmount,
                        AccountingMeters.VoucherOutNonCashableAmount)
                },
                {
                    "voucher." + VoucherMeterName.NonCashableOutCount, new G2SMeter(
                        "voucher." + VoucherMeterName.NonCashableOutCount,
                        AccountingMeters.VoucherOutNonCashableCount)
                },
                {
                    "handpay." + TransferMeterName.CashableOutAmount,
                    new G2SMeter(
                        "handpay." + TransferMeterName.CashableOutAmount,
                        AccountingMeters.HandpaidCashableAmount)
                },
                {
                    "handpay." + TransferMeterName.PromoOutAmount,
                    new G2SMeter("handpay." + TransferMeterName.PromoOutAmount, AccountingMeters.HandpaidPromoAmount)
                },
                {
                    "handpay." + TransferMeterName.NonCashableOutAmount,
                    new G2SMeter(
                        "handpay." + TransferMeterName.NonCashableOutAmount,
                        AccountingMeters.HandpaidNonCashableAmount)
                },
                {
                    "handpay." + TransferMeterName.TransferOutCount,
                    new G2SMeter("handpay." + TransferMeterName.TransferOutCount, AccountingMeters.HandpaidOutCount)
                },
                {
                    "bonus." + TransferMeterName.CashableInAmount,
                    new G2SMeter("bonus." + TransferMeterName.CashableInAmount, BonusMeters.BonusCashableInAmount)
                },
                {
                    "bonus." + TransferMeterName.PromoInAmount,
                    new G2SMeter("bonus." + TransferMeterName.PromoInAmount, BonusMeters.BonusPromoInAmount)
                },
                {
                    "bonus." + TransferMeterName.NonCashableInAmount,
                    new G2SMeter("bonus." + TransferMeterName.NonCashableInAmount, BonusMeters.BonusNonCashableInAmount)
                },
                {
                    "bonus." + TransferMeterName.TransferInCount,
                    new G2SMeter("bonus." + TransferMeterName.TransferInCount, BonusMeters.BonusTotalCount)
                }
            };

        /// <summary>
        ///     Gets an IEnumerable&lt;SASMeter&gt; object, for use in foreach statements.
        /// </summary>
        /// <returns>The IEnumerable&lt;G2SMeter&gt; object.</returns>
        public IEnumerator<G2SMeter> GetEnumerator()
        {
            return MeterCollection.Values.GetEnumerator();
        }

        /// <summary>
        ///     Gets a non-generic IEnumerable object.
        /// </summary>
        /// <returns>The IEnumerable object.</returns>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return MeterCollection.Values.GetEnumerator();
        }

        /// <summary>
        ///     Adds G2S meter mapping object
        /// </summary>
        /// <param name="game2SMeterId">G2S meter Id</param>
        /// <param name="meterId">MeterId</param>
        public static void AddG2SMeter(string game2SMeterId, string meterId)
        {
            if (!MeterCollection.ContainsKey(game2SMeterId))
            {
                MeterCollection[game2SMeterId] = new G2SMeter(game2SMeterId, meterId);
            }
        }

        /// <summary>
        ///     Adds G2S meter mapping object
        /// </summary>
        /// <param name="game2SMeterId">G2S meter Id</param>
        /// <param name="callback">Callback</param>
        public static void AddG2SMeter(string game2SMeterId, Func<long> callback)
        {
            if (!MeterCollection.ContainsKey(game2SMeterId))
            {
                MeterCollection.Add(game2SMeterId, new G2SMeter(game2SMeterId, callback));
            }
            else
            {
                MeterCollection[game2SMeterId] = new G2SMeter(game2SMeterId, callback);
            }
        }

        /// <summary>
        ///     Gets G2S meter mapping object
        /// </summary>
        /// <param name="game2SMeterId">G2S meter Id</param>
        /// <param name="readValue">Needs G2S for reading meter value.</param>
        /// <returns>G2S meter mapped object</returns>
        public static G2SMeter GetG2SMeter(string game2SMeterId, bool readValue = true)
        {
            return MeterCollection.ContainsKey(game2SMeterId)
                ? MeterCollection[game2SMeterId]
                : readValue
                    ? new G2SMeter(game2SMeterId)
                    : null;
        }
    }
}