namespace Aristocrat.Monaco.Sas
{
    using Application.Contracts;
    using Aristocrat.Monaco.Accounting.Contracts;
    using Aristocrat.Monaco.Gaming.Contracts;
    using Aristocrat.Sas.Client.Metering;
    using Contracts.Metering;
    using System.Collections.Generic;

    /// <summary>
    ///     Definition of the SasMeterCollection class.
    /// </summary>
    public class SasMeterCollection : IEnumerable<SasMeter>
    {
        /// <summary>
        ///     A dictionary containing supported mappings for Sas meter codes and SasMeter objects.
        /// </summary>
        private static readonly IDictionary<SasMeterId, SasMeter> MeterCollection = new Dictionary<SasMeterId, SasMeter>
        {
            {
                SasMeterId.TotalCoinIn, // 0x00
                new SasMeter(
                    SasMeterId.TotalCoinIn,
                    GamingMeters.WageredAmount,
                    false,
                    true,
                    MeterCategory.Credit)
            },
            {
                SasMeterId.TotalCoinOut, // 0x01
                new SasMeter(
                    SasMeterId.TotalCoinOut,
                    GamingMeters.TotalEgmPaidAmt,
                    false,
                    true,
                    MeterCategory.Credit)
            },
            {
                SasMeterId.TotalJackpot, // 0x02
                new SasMeter(
                    SasMeterId.TotalJackpot,
                    GamingMeters.TotalHandPaidAmt,
                    false,
                    true,
                    MeterCategory.Credit)
            },
            {
                SasMeterId.TotalHandPaidCanceledCredits, // 0x03
                new SasMeter(
                    SasMeterId.TotalHandPaidCanceledCredits,
                    AccountingMeters.HandpaidCancelAmount,
                    false,
                    false,
                    MeterCategory.Credit)
            },
            {
                SasMeterId.TotalCanceledCredits, // 0x04
                new SasMeter(
                    SasMeterId.TotalCanceledCredits,
                    SasMeterNames.TotalCanceledCredits,
                    false,
                    false,
                    MeterCategory.Credit)
            },
            {
                SasMeterId.GamesPlayed, // 0x05
                new SasMeter(
                    SasMeterId.GamesPlayed,
                    GamingMeters.PlayedCount,
                    false,
                    true,
                    MeterCategory.Occurrence)
            },
            {
                SasMeterId.GamesWon, // 0x06
                new SasMeter(
                    SasMeterId.GamesWon,
                    GamingMeters.WonCount,
                    false,
                    true,
                    MeterCategory.Occurrence)
            },
            {
                SasMeterId.GamesLost, // 0x07
                new SasMeter(
                    SasMeterId.GamesLost,
                    GamingMeters.LostCount,
                    false,
                    true,
                    MeterCategory.Occurrence)
            },
            {
                SasMeterId.TotalCreditsFromCoinAcceptor, // 0x08
                new SasMeter(
                    SasMeterId.TotalCreditsFromCoinAcceptor,
                    AccountingMeters.TrueCoinIn,
                    false,
                    false,
                    MeterCategory.Credit)
            },
            {
                SasMeterId.TotalCreditsPaidFromHopper, // 0x09
                new SasMeter(
                    SasMeterId.TotalCreditsPaidFromHopper,
                    AccountingMeters.TrueCoinOut,
                    false,
                    false,
                    MeterCategory.Credit)
            },
            {
                SasMeterId.TotalCreditsFromCoinsToDrop, // 0x0A
                new SasMeter(
                    SasMeterId.TotalCreditsFromCoinsToDrop,
                    AccountingMeters.CoinDrop,
                    false,
                    false,
                    MeterCategory.Credit)
            },
            {
                SasMeterId.TotalCreditsFromBillsAccepted, // 0x0B
                new SasMeter(
                    SasMeterId.TotalCreditsFromBillsAccepted,
                    AccountingMeters.CurrencyInAmount,
                    false,
                    false,
                    MeterCategory.Credit)
            },
            {
                SasMeterId.CurrentCredits, // 0x0C
                new SasMeter(
                    SasMeterId.CurrentCredits,
                    "IBank.QueryBalance()",
                    false,
                    false,
                    MeterCategory.Credit)
            },
            {
                SasMeterId.TotalSasCashableTicketInCents, // 0x0D
                new SasMeter(
                    SasMeterId.TotalSasCashableTicketInCents,
                    AccountingMeters.TotalVoucherInCashableAndPromoAmount,
                    false,
                    false,
                    MeterCategory.Cents,
                    5)
            },
            {
                SasMeterId.TotalSasCashableTicketOutCents, // 0x0E
                new SasMeter(
                    SasMeterId.TotalSasCashableTicketOutCents,
                    AccountingMeters.TotalVoucherOutCashableAndPromoAmount,
                    false,
                    false,
                    MeterCategory.Cents,
                    5)
            },
            {
                SasMeterId.TotalSasRestrictedTicketInCents, // 0x0F
                new SasMeter(
                    SasMeterId.TotalSasRestrictedTicketInCents,
                    AccountingMeters.VoucherInNonCashableAmount,
                    false,
                    false,
                    MeterCategory.Cents,
                    5)
            },
            {
                SasMeterId.TotalSasRestrictedTicketOutCents, // 0x10
                new SasMeter(
                    SasMeterId.TotalSasRestrictedTicketOutCents,
                    AccountingMeters.VoucherOutNonCashableAmount,
                    false,
                    false,
                    MeterCategory.Cents,
                    5)
            },
            {
                SasMeterId.TotalSasCashableTicketInQuantity, // 0x11
                new SasMeter(
                    SasMeterId.TotalSasCashableTicketInQuantity,
                    AccountingMeters.TotalVoucherInCashableAndPromoCount,
                    false,
                    false,
                    MeterCategory.Occurrence)
            },
            {
                SasMeterId.TotalSasCashableTicketOutQuantity, // 0x12
                new SasMeter(
                    SasMeterId.TotalSasCashableTicketOutQuantity,
                    AccountingMeters.TotalVoucherOutCashableAndPromoCount,
                    false,
                    false,
                    MeterCategory.Occurrence)
            },
            {
                SasMeterId.TotalSasRestrictedTicketInQuantity, // 0x13
                new SasMeter(
                    SasMeterId.TotalSasRestrictedTicketInQuantity,
                    AccountingMeters.VoucherInNonCashableCount,
                    false,
                    false,
                    MeterCategory.Occurrence)
            },
            {
                SasMeterId.TotalSasRestrictedTicketOutQuantity, // 0x14
                new SasMeter(
                    SasMeterId.TotalSasRestrictedTicketOutQuantity,
                    AccountingMeters.VoucherOutNonCashableCount,
                    false,
                    false,
                    MeterCategory.Occurrence)
            },
            {
                SasMeterId.TotalTicketIn, // 0x15
                new SasMeter(
                    SasMeterId.TotalTicketIn,
                    AccountingMeters.TotalVouchersIn,
                    false,
                    false,
                    MeterCategory.Credit)
            },
            {
                SasMeterId.TotalTicketOut, // 0x16
                new SasMeter(
                    SasMeterId.TotalTicketOut,
                    AccountingMeters.TotalVouchersOut,
                    false,
                    false,
                    MeterCategory.Credit)
            },
            {
                SasMeterId.TotalElectronicTransfersToGamingMachine, // 0x17
                new SasMeter(
                    SasMeterId.TotalElectronicTransfersToGamingMachine,
                    AccountingMeters.ElectronicTransfersOnTotalAmount,
                    false,
                    false,
                    MeterCategory.Credit)
            },
            {
                SasMeterId.TotalElectronicTransfersToHost, // 0x18
                new SasMeter(
                    SasMeterId.TotalElectronicTransfersToHost,
                    AccountingMeters.ElectronicTransfersOffTotalAmount,
                    false,
                    false,
                    MeterCategory.Credit)
            },
            {
                SasMeterId.TotalRestrictedAmountPlayed, // 0x19
                new SasMeter(
                    SasMeterId.TotalRestrictedAmountPlayed,
                    GamingMeters.WageredNonCashableAmount,
                    false,
                    false,
                    MeterCategory.Credit)
            },
            {
                SasMeterId.TotalNonRestrictedAmountPlayed, // 0x1A
                new SasMeter(
                    SasMeterId.TotalNonRestrictedAmountPlayed,
                    GamingMeters.WageredPromoAmount,
                    false,
                    false,
                    MeterCategory.Credit)
            },
            {
                SasMeterId.CurrentRestrictedCredits, // 0x1B
                new SasMeter(
                    SasMeterId.CurrentRestrictedCredits,
                    "IBank.QueryBalance(AccountType.NonCash)",
                    false,
                    false,
                    MeterCategory.Credit)
            },
            {
                SasMeterId.TotalMachinePaidPaytableWin, // 0x1C
                new SasMeter(
                    SasMeterId.TotalMachinePaidPaytableWin,
                    GamingMeters.TotalEgmPaidGameWonAmount,
                    false,
                    true,
                    MeterCategory.Credit)
            },
            {
                SasMeterId.TotalMachinePaidProgressiveWin, // 0x1D
                new SasMeter(
                    SasMeterId.TotalMachinePaidProgressiveWin,
                    GamingMeters.EgmPaidProgWonAmount,
                    false,
                    true,
                    MeterCategory.Credit)
            },
            {
                SasMeterId.TotalMachinePaidBonus, // 0x1E
                new SasMeter(
                    SasMeterId.TotalMachinePaidBonus,
                    GamingMeters.EgmPaidBonusAmount,
                    false,
                    true,
                    MeterCategory.Credit)
            },
            {
                SasMeterId.TotalHandPaidPaytableWin, // 0x1F - per cabinet, game, & denom
                new SasMeter(
                    SasMeterId.TotalHandPaidPaytableWin,
                    GamingMeters.HandPaidGameWonAmount,
                    false,
                    true,
                    MeterCategory.Credit)
            },
            {
                SasMeterId.TotalAttendantPaidProgressiveWin, // 0x20
                new SasMeter(
                    SasMeterId.TotalAttendantPaidProgressiveWin,
                    GamingMeters.HandPaidProgWonAmount,
                    false,
                    true,
                    MeterCategory.Credit)
            },
            {
                SasMeterId.TotalAttendantPaidExternalBonus, // 0x21
                new SasMeter(
                    SasMeterId.TotalAttendantPaidExternalBonus,
                    GamingMeters.HandPaidBonusAmount,
                    false,
                    true,
                    MeterCategory.Credit)
            },
            {
                SasMeterId.TotalWonCredits, // 0x22
                new SasMeter(
                    SasMeterId.TotalWonCredits,
                    GamingMeters.TotalPaidAmt,
                    false,
                    true,
                    MeterCategory.Credit)
            },
            {
                SasMeterId.TotalHandPaidCredits, // 0x23
                new SasMeter(
                    SasMeterId.TotalHandPaidCredits,
                    AccountingMeters.TotalHandpaidCredits,
                    false,
                    false,
                    MeterCategory.Credit)
            },
            {
                SasMeterId.TotalDrop, // 0x24
                new SasMeter(
                    SasMeterId.TotalDrop,
                    ApplicationMeters.TotalIn,
                    false,
                    false,
                    MeterCategory.Credit)
            },
            {
                SasMeterId.GamesSinceLastPowerReset, // 0x25
                new SasMeter(
                    SasMeterId.GamesSinceLastPowerReset,
                    GamingMeters.GamesPlayedSinceReboot,
                    false,
                    false,
                    MeterCategory.Occurrence)
            },
            {
                SasMeterId.GamesSinceSlotClosure, // 0x26
                new SasMeter(
                    SasMeterId.GamesSinceSlotClosure,
                    GamingMeters.GamesPlayedSinceDoorClosed,
                    false,
                    false,
                    MeterCategory.Occurrence)
            },
            {
                SasMeterId.TotalCreditsFromExternalCoinAcceptor, // 0x27
                new SasMeter(
                    SasMeterId.TotalCreditsFromExternalCoinAcceptor,
                    AccountingMeters.CoinAmountAcceptedFromExternal,
                    false,
                    false,
                    MeterCategory.Credit)
            },
            {
                SasMeterId.TotalCashableTicketIn, // 0x28
                new SasMeter(
                    SasMeterId.TotalCashableTicketIn,
                    AccountingMeters.TotalVoucherInCashableAndPromoAmount,
                    false,
                    false,
                    MeterCategory.Credit)
            },
            {
                SasMeterId.TotalRegularCashableTicketIn, // 0x29
                new SasMeter(
                    SasMeterId.TotalRegularCashableTicketIn,
                    AccountingMeters.VoucherInCashableAmount,
                    false,
                    false,
                    MeterCategory.Credit)
            },
            {
                SasMeterId.TotalRestrictedPromoTicketIn, // 0x2a
                new SasMeter(
                    SasMeterId.TotalRestrictedPromoTicketIn,
                    AccountingMeters.VoucherInNonCashableAmount,
                    false,
                    false,
                    MeterCategory.Credit)
            },
            {
                SasMeterId.TotalNonRestrictedPromoTicketIn, // 0x2b
                new SasMeter(
                    SasMeterId.TotalNonRestrictedPromoTicketIn,
                    AccountingMeters.VoucherInCashablePromoAmount,
                    false,
                    false,
                    MeterCategory.Credit)
            },
            {
                SasMeterId.TotalCashableTicketOut, // 0x2c
                new SasMeter(
                    SasMeterId.TotalCashableTicketOut,
                    AccountingMeters.TotalVoucherOutCashableAndPromoAmount,
                    false,
                    false,
                    MeterCategory.Credit)
            },
            {
                SasMeterId.TotalRestrictedPromoTicketOut, // 0x2d
                new SasMeter(
                    SasMeterId.TotalRestrictedPromoTicketOut,
                    AccountingMeters.VoucherOutNonCashableAmount,
                    false,
                    false,
                    MeterCategory.Credit)
            },
            {
                SasMeterId.ElectronicCashableTransfersToGamingMachine, // 0x2E
                new SasMeter(
                    SasMeterId.ElectronicCashableTransfersToGamingMachine,
                    AccountingMeters.ElectronicTransfersOnCashableAmount,
                    false,
                    false,
                    MeterCategory.Credit)
            },
            {
                SasMeterId.ElectronicRestrictedPromotionTransfersToGamingMachine, // 0x2F
                new SasMeter(
                    SasMeterId.ElectronicRestrictedPromotionTransfersToGamingMachine,
                    AccountingMeters.ElectronicTransfersOnNonCashableAmount,
                    false,
                    false,
                    MeterCategory.Credit)
            },
            {
                SasMeterId.ElectronicNonRestrictedTransfersToGamingMachine, // 0x30
                new SasMeter(
                    SasMeterId.ElectronicNonRestrictedTransfersToGamingMachine,
                    AccountingMeters.ElectronicTransfersOnCashablePromoAmount,
                    false,
                    false,
                    MeterCategory.Credit)
            },
            {
                SasMeterId.ElectronicDebitTransfersToGamingMachine, // 0x31
                new SasMeter(
                    SasMeterId.ElectronicDebitTransfersToGamingMachine,
                    SasMeterNames.TotalElectronicDebitTransfers,
                    false,
                    false,
                    MeterCategory.Credit)
            },
            {
                SasMeterId.ElectronicCashableTransfersToHost, // 0x32
                new SasMeter(
                    SasMeterId.ElectronicCashableTransfersToHost,
                    AccountingMeters.ElectronicTransfersOffCashableAmount,
                    false,
                    false,
                    MeterCategory.Credit)
            },
            {
                SasMeterId.ElectronicRestrictedTransfersToHost, // 0x33
                new SasMeter(
                    SasMeterId.ElectronicRestrictedTransfersToHost,
                    AccountingMeters.ElectronicTransfersOffNonCashableAmount,
                    false,
                    false,
                    MeterCategory.Credit)
            },
            {
                SasMeterId.ElectronicNonRestrictedTransfersToHost, // 0x34
                new SasMeter(
                    SasMeterId.ElectronicNonRestrictedTransfersToHost,
                    AccountingMeters.ElectronicTransfersOffCashablePromoAmount,
                    false,
                    false,
                    MeterCategory.Credit)
            },
            {
                SasMeterId.TotalRegularCashableTicketInQuantity, // 0x35
                new SasMeter(
                    SasMeterId.TotalRegularCashableTicketInQuantity,
                    AccountingMeters.VoucherInCashableCount,
                    false,
                    false,
                    MeterCategory.Occurrence)
            },
            {
                SasMeterId.TotalRestrictedPromoTicketInQuantity, // 0x36
                new SasMeter(
                    SasMeterId.TotalRestrictedPromoTicketInQuantity,
                    AccountingMeters.VoucherInNonCashableCount,
                    false,
                    false,
                    MeterCategory.Occurrence)
            },
            {
                SasMeterId.TotalNonRestrictedPromoTicketInQuantity, // 0x37
                new SasMeter(
                    SasMeterId.TotalNonRestrictedPromoTicketInQuantity,
                    AccountingMeters.VoucherInCashablePromoCount,
                    false,
                    false,
                    MeterCategory.Occurrence)
            },
            {
                SasMeterId.TotalRegularCashableTicketOutQuantity, // 0x38
                new SasMeter(
                    SasMeterId.TotalRegularCashableTicketOutQuantity,
                    AccountingMeters.TotalVoucherOutCashableAndPromoCount,
                    false,
                    false,
                    MeterCategory.Occurrence)
            },
            {
                SasMeterId.TotalRestrictedPromoTicketOutQuantity, // 0x39
                new SasMeter(
                    SasMeterId.TotalRestrictedPromoTicketOutQuantity,
                    AccountingMeters.VoucherOutNonCashableCount,
                    false,
                    false,
                    MeterCategory.Occurrence)
            },
            {
                SasMeterId.TotalBillsInStacker, // 0x3E
                new SasMeter(
                    SasMeterId.TotalBillsInStacker,
                    AccountingMeters.CurrencyInCount,
                    true,
                    false,
                    MeterCategory.Occurrence)
            },
            {
                SasMeterId.TotalBillAmountInStacker, // 0x3F
                new SasMeter(
                    SasMeterId.TotalBillAmountInStacker,
                    AccountingMeters.CurrencyInAmount,
                    true,
                    false,
                    MeterCategory.Credit)
            },
            {
                SasMeterId.TotalNumberBillsAccepted1, // 0x40
                new SasMeter(
                    SasMeterId.TotalNumberBillsAccepted1,
                    AccountingMeters.BillCount1s,
                    false,
                    false,
                    MeterCategory.Occurrence)
            },
            {
                SasMeterId.TotalNumberBillsAccepted2, // 0x41
                new SasMeter(
                    SasMeterId.TotalNumberBillsAccepted2,
                    AccountingMeters.BillCount2s,
                    false,
                    false,
                    MeterCategory.Occurrence)
            },
            {
                SasMeterId.TotalNumberBillsAccepted5, // 0x42
                new SasMeter(
                    SasMeterId.TotalNumberBillsAccepted5,
                    AccountingMeters.BillCount5s,
                    false,
                    false,
                    MeterCategory.Occurrence)
            },
            {
                SasMeterId.TotalNumberBillsAccepted10, // 0x43
                new SasMeter(
                    SasMeterId.TotalNumberBillsAccepted10,
                    AccountingMeters.BillCount10s,
                    false,
                    false,
                    MeterCategory.Occurrence)
            },
            {
                SasMeterId.TotalNumberBillsAccepted20, // 0x44
                new SasMeter(
                    SasMeterId.TotalNumberBillsAccepted20,
                    AccountingMeters.BillCount20s,
                    false,
                    false,
                    MeterCategory.Occurrence)
            },
            {
                SasMeterId.TotalNumberBillsAccepted25, // 0x45
                new SasMeter(
                    SasMeterId.TotalNumberBillsAccepted25,
                    AccountingMeters.BillCount25s,
                    false,
                    false,
                    MeterCategory.Occurrence)
            },
            {
                SasMeterId.TotalNumberBillsAccepted50, // 0x46
                new SasMeter(
                    SasMeterId.TotalNumberBillsAccepted50,
                    AccountingMeters.BillCount50s,
                    false,
                    false,
                    MeterCategory.Occurrence)
            },
            {
                SasMeterId.TotalNumberBillsAccepted100, // 0x47
                new SasMeter(
                    SasMeterId.TotalNumberBillsAccepted100,
                    AccountingMeters.BillCount100s,
                    false,
                    false,
                    MeterCategory.Occurrence)
            },
            {
                SasMeterId.TotalNumberBillsAccepted200, // 0x48
                new SasMeter(
                    SasMeterId.TotalNumberBillsAccepted200,
                    AccountingMeters.BillCount200s,
                    false,
                    false,
                    MeterCategory.Occurrence)
            },
            {
                SasMeterId.TotalNumberBillsAccepted250, // 0x49
                new SasMeter(
                    SasMeterId.TotalNumberBillsAccepted250,
                    AccountingMeters.BillCount250s,
                    false,
                    false,
                    MeterCategory.Occurrence)
            },
            {
                SasMeterId.TotalNumberBillsAccepted500, // 0x4A
                new SasMeter(
                    SasMeterId.TotalNumberBillsAccepted500,
                    AccountingMeters.BillCount500s,
                    false,
                    false,
                    MeterCategory.Occurrence)
            },
            {
                SasMeterId.TotalNumberBillsAccepted1000, // 0x4B
                new SasMeter(
                    SasMeterId.TotalNumberBillsAccepted1000,
                    AccountingMeters.BillCount1_000s,
                    false,
                    false,
                    MeterCategory.Occurrence)
            },
            {
                SasMeterId.TotalNumberBillsAccepted2000, // 0x4C
                new SasMeter(
                    SasMeterId.TotalNumberBillsAccepted2000,
                    AccountingMeters.BillCount2_000s,
                    false,
                    false,
                    MeterCategory.Occurrence)
            },
            {
                SasMeterId.TotalNumberBillsAccepted2500, // 0x4D
                new SasMeter(
                    SasMeterId.TotalNumberBillsAccepted2500,
                    AccountingMeters.BillCount2_500s,
                    false,
                    false,
                    MeterCategory.Occurrence)
            },
            {
                SasMeterId.TotalNumberBillsAccepted5000, // 0x4E
                new SasMeter(
                    SasMeterId.TotalNumberBillsAccepted5000,
                    AccountingMeters.BillCount5_000s,
                    false,
                    false,
                    MeterCategory.Occurrence)
            },
            {
                SasMeterId.TotalNumberBillsAccepted10000, // 0x4F
                new SasMeter(
                    SasMeterId.TotalNumberBillsAccepted10000,
                    AccountingMeters.BillCount10_000s,
                    false,
                    false,
                    MeterCategory.Occurrence)
            },
            {
                SasMeterId.TotalNumberBillsAccepted20000, // 0x50
                new SasMeter(
                    SasMeterId.TotalNumberBillsAccepted20000,
                    AccountingMeters.BillCount20_000s,
                    false,
                    false,
                    MeterCategory.Occurrence)
            },
            {
                SasMeterId.TotalNumberBillsAccepted25000, // 0x51
                new SasMeter(
                    SasMeterId.TotalNumberBillsAccepted25000,
                    AccountingMeters.BillCount25_000s,
                    false,
                    false,
                    MeterCategory.Occurrence)
            },
            {
                SasMeterId.TotalNumberBillsAccepted50000, // 0x52
                new SasMeter(
                    SasMeterId.TotalNumberBillsAccepted50000,
                    AccountingMeters.BillCount50_000s,
                    false,
                    false,
                    MeterCategory.Occurrence)
            },
            {
                SasMeterId.TotalNumberBillsAccepted100000, // 0x53
                new SasMeter(
                    SasMeterId.TotalNumberBillsAccepted100000,
                    AccountingMeters.BillCount100_000s,
                    false,
                    false,
                    MeterCategory.Occurrence)
            },
            {
                SasMeterId.TotalNumberBillsAccepted200000, // 0x54
                new SasMeter(
                    SasMeterId.TotalNumberBillsAccepted200000,
                    "BillCount200000s",
                    false,
                    false,
                    MeterCategory.Occurrence)
            },
            {
                SasMeterId.TotalNumberBillsAccepted250000, // 0x55
                new SasMeter(
                    SasMeterId.TotalNumberBillsAccepted250000,
                    "BillCount250000s",
                    false,
                    false,
                    MeterCategory.Occurrence)
            },
            {
                SasMeterId.TotalNumberBillsAccepted500000, // 0x56
                new SasMeter(
                    SasMeterId.TotalNumberBillsAccepted500000,
                    "BillCount500000s",
                    false,
                    false,
                    MeterCategory.Occurrence)
            },
            {
                SasMeterId.TotalNumberBillsAccepted1000000, // 0x57
                new SasMeter(
                    SasMeterId.TotalNumberBillsAccepted1000000,
                    "BillCount1000000s",
                    false,
                    false,
                    MeterCategory.Occurrence)
            },
            {
                SasMeterId.TotalCreditsBillsToDrop, // 0x58
                new SasMeter(
                    SasMeterId.TotalCreditsBillsToDrop,
                    AccountingMeters.CurrencyInAmount,
                    false,
                    false,
                    MeterCategory.Credit)
            },
            {
                SasMeterId.TotalNumberBillsToDrop1, // 0x59
                new SasMeter(
                    SasMeterId.TotalNumberBillsToDrop1,
                    AccountingMeters.BillCount1s,
                    false,
                    false,
                    MeterCategory.Occurrence)
            },
            {
                SasMeterId.TotalNumberBillsToDrop2, // 0x5A
                new SasMeter(
                    SasMeterId.TotalNumberBillsToDrop2,
                    AccountingMeters.BillCount2s,
                    false,
                    false,
                    MeterCategory.Occurrence)
            },
            {
                SasMeterId.TotalNumberBillsToDrop5, // 0x5B
                new SasMeter(
                    SasMeterId.TotalNumberBillsToDrop5,
                    AccountingMeters.BillCount5s,
                    false,
                    false,
                    MeterCategory.Occurrence)
            },
            {
                SasMeterId.TotalNumberBillsToDrop10, // 0x5C
                new SasMeter(
                    SasMeterId.TotalNumberBillsToDrop10,
                    AccountingMeters.BillCount10s,
                    false,
                    false,
                    MeterCategory.Occurrence)
            },
            {
                SasMeterId.TotalNumberBillsToDrop20, // 0x5D
                new SasMeter(
                    SasMeterId.TotalNumberBillsToDrop20,
                    AccountingMeters.BillCount20s,
                    false,
                    false,
                    MeterCategory.Occurrence)
            },
            {
                SasMeterId.TotalNumberBillsToDrop50, // 0x5E
                new SasMeter(
                    SasMeterId.TotalNumberBillsToDrop50,
                    AccountingMeters.BillCount50s,
                    false,
                    false,
                    MeterCategory.Occurrence)
            },
            {
                SasMeterId.TotalNumberBillsToDrop100, // 0x5F
                new SasMeter(
                    SasMeterId.TotalNumberBillsToDrop100,
                    AccountingMeters.BillCount100s,
                    false,
                    false,
                    MeterCategory.Occurrence)
            },
            {
                SasMeterId.TotalNumberBillsToDrop200, // 0x60
                new SasMeter(
                    SasMeterId.TotalNumberBillsToDrop200,
                    AccountingMeters.BillCount200s,
                    false,
                    false,
                    MeterCategory.Occurrence)
            },
            {
                SasMeterId.TotalNumberBillsToDrop500, // 0x61
                new SasMeter(
                    SasMeterId.TotalNumberBillsToDrop500,
                    AccountingMeters.BillCount500s,
                    false,
                    false,
                    MeterCategory.Occurrence)
            },
            {
                SasMeterId.TotalNumberBillsToDrop1000, // 0x62
                new SasMeter(
                    SasMeterId.TotalNumberBillsToDrop1000,
                    AccountingMeters.BillCount1_000s,
                    false,
                    false,
                    MeterCategory.Occurrence)
            },
            {
                SasMeterId.AverageTheoreticalPaybackPercentage, // 0x7F
                new SasMeter(
                    SasMeterId.AverageTheoreticalPaybackPercentage,
                    GamingMeters.AveragePayback,
                    false,
                    true,
                    MeterCategory.Percentage)
            },
            {
                SasMeterId.CashableTicketInCents, // 0x80
                new SasMeter(
                    SasMeterId.CashableTicketInCents,
                    AccountingMeters.VoucherInCashableAmount,
                    false,
                    false,
                    MeterCategory.Cents,
                    5)
            },
            {
                SasMeterId.CashableTicketInCount, // 0x81
                new SasMeter(
                    SasMeterId.CashableTicketInCount,
                    AccountingMeters.VoucherInCashableCount,
                    false,
                    false,
                    MeterCategory.Occurrence)
            },
            {
                SasMeterId.RestrictedTicketInCents, // 0x82
                new SasMeter(
                    SasMeterId.RestrictedTicketInCents,
                    AccountingMeters.VoucherInNonCashableAmount,
                    false,
                    false,
                    MeterCategory.Cents,
                    5)
            },
            {
                SasMeterId.RestrictedTicketInCount, // 0x83
                new SasMeter(
                    SasMeterId.RestrictedTicketInCount,
                    AccountingMeters.VoucherInNonCashableCount,
                    false,
                    false,
                    MeterCategory.Occurrence)
            },
            {
                SasMeterId.NonRestrictedTicketInCents, // 0x84
                new SasMeter(
                    SasMeterId.NonRestrictedTicketInCents,
                    AccountingMeters.VoucherInCashablePromoAmount,
                    false,
                    false,
                    MeterCategory.Cents,
                    5)
            },
            {
                SasMeterId.NonRestrictedTicketInCount, // 0x85
                new SasMeter(
                    SasMeterId.NonRestrictedTicketInCount,
                    AccountingMeters.VoucherInCashablePromoCount,
                    false,
                    false,
                    MeterCategory.Occurrence)
            },
            {
                SasMeterId.CashableTicketOutCents, // 0x86
                new SasMeter(
                    SasMeterId.CashableTicketOutCents,
                    AccountingMeters.TotalVoucherOutCashableAndPromoAmount,
                    false,
                    false,
                    MeterCategory.Cents,
                    5)
            },
            {
                SasMeterId.CashableTicketOutCount, // 0x87
                new SasMeter(
                    SasMeterId.CashableTicketOutCount,
                    AccountingMeters.TotalVoucherOutCashableAndPromoCount,
                    false,
                    false,
                    MeterCategory.Occurrence)
            },
            {
                SasMeterId.RestrictedTicketOutCents, // 0x88
                new SasMeter(
                    SasMeterId.RestrictedTicketOutCents,
                    AccountingMeters.VoucherOutNonCashableAmount,
                    false,
                    false,
                    MeterCategory.Cents,
                    5)
            },
            {
                SasMeterId.RestrictedTicketOutCount, // 0x89
                new SasMeter(
                    SasMeterId.RestrictedTicketOutCount,
                    AccountingMeters.VoucherOutNonCashableCount,
                    false,
                    false,
                    MeterCategory.Occurrence)
            },
            {
                SasMeterId.ValidatedCanceledCreditHandPayReceiptCents, // 0x8C
                new SasMeter(
                    SasMeterId.ValidatedCanceledCreditHandPayReceiptCents,
                    AccountingMeters.HandpaidValidatedCancelReceiptAmount,
                    false,
                    false,
                    MeterCategory.Cents,
                    5)
            },
            {
                SasMeterId.ValidatedCanceledCreditHandPayReceiptCount, // 0x8D
                new SasMeter(
                    SasMeterId.ValidatedCanceledCreditHandPayReceiptCount,
                    AccountingMeters.HandpaidValidatedCancelReceiptCount,
                    false,
                    false,
                    MeterCategory.Occurrence)
            },
            {
                SasMeterId.ValidatedJackpotHandPayReceiptCents, // 0x8E
                new SasMeter(
                    SasMeterId.ValidatedJackpotHandPayReceiptCents,
                    SasMeterNames.ValidatedJackpotHandPayReceiptCents,
                    false,
                    false,
                    MeterCategory.Cents,
                    5)
            },
            {
                SasMeterId.ValidatedJackpotHandPayReceiptCount, // 0x8F
                new SasMeter(
                    SasMeterId.ValidatedJackpotHandPayReceiptCount,
                    SasMeterNames.ValidatedJackpotHandPayReceiptCount,
                    false,
                    false,
                    MeterCategory.Occurrence)
            },
            {
                SasMeterId.ValidatedCanceledCreditHandPayNoReceiptCents, // 0x90
                new SasMeter(
                    SasMeterId.ValidatedCanceledCreditHandPayNoReceiptCents,
                    AccountingMeters.HandpaidValidatedCancelNoReceiptAmount,
                    false,
                    false,
                    MeterCategory.Cents,
                    5)
            },
            {
                SasMeterId.ValidatedCanceledCreditHandPayNoReceiptCount, // 0x91
                new SasMeter(
                    SasMeterId.ValidatedCanceledCreditHandPayNoReceiptCount,
                    AccountingMeters.HandpaidValidatedCancelNoReceiptCount,
                    false,
                    false,
                    MeterCategory.Occurrence)
            },
            {
                SasMeterId.ValidatedJackpotHandPayNoReceiptCents, // 0x92
                new SasMeter(
                    SasMeterId.ValidatedJackpotHandPayNoReceiptCents,
                    SasMeterNames.ValidatedJackpotHandPayNoReceiptCents,
                    false,
                    false,
                    MeterCategory.Cents,
                    5)
            },
            {
                SasMeterId.ValidatedJackpotHandPayNoReceiptCount, // 0x93
                new SasMeter(
                    SasMeterId.ValidatedJackpotHandPayNoReceiptCount,
                    SasMeterNames.ValidatedJackpotHandPayNoReceiptCount,
                    false,
                    false,
                    MeterCategory.Occurrence)
            },
            {
                SasMeterId.AftCashableIn, // 0xA0
                new SasMeter(
                    SasMeterId.AftCashableIn,
                    AccountingMeters.WatOnCashableAmount,
                    false,
                    false,
                    MeterCategory.Cents,
                    5)
            },
            {
                SasMeterId.AftCashableInQuantity, // 0xA1
                new SasMeter(
                    SasMeterId.AftCashableInQuantity,
                    AccountingMeters.WatOnCashableCount,
                    false,
                    false,
                    MeterCategory.Occurrence)
            },
            {
                SasMeterId.AftRestrictedIn, // 0xA2
                new SasMeter(
                    SasMeterId.AftRestrictedIn,
                    AccountingMeters.WatOnNonCashableAmount,
                    false,
                    false,
                    MeterCategory.Cents,
                    5)
            },
            {
                SasMeterId.AftRestrictedInQuantity, // 0xA3
                new SasMeter(
                    SasMeterId.AftRestrictedInQuantity,
                    AccountingMeters.WatOnNonCashableCount,
                    false,
                    false,
                    MeterCategory.Occurrence)
            },
            {
                SasMeterId.AftNonRestrictedIn, // 0xA4
                new SasMeter(
                    SasMeterId.AftNonRestrictedIn,
                    AccountingMeters.WatOnCashablePromoAmount,
                    false,
                    false,
                    MeterCategory.Cents,
                    5)
            },
            {
                SasMeterId.AftNonRestrictedInQuantity, // 0xA5
                new SasMeter(
                    SasMeterId.AftNonRestrictedInQuantity,
                    AccountingMeters.WatOnCashablePromoCount,
                    false,
                    false,
                    MeterCategory.Occurrence)
            },
            {
                SasMeterId.AftCashableBonusIn, // 0xAE
                new SasMeter(
                    SasMeterId.AftCashableBonusIn,
                    SasMeterNames.AftCashableBonusIn,
                    false,
                    false,
                    MeterCategory.Cents,
                    5)
            },
            {
                SasMeterId.AftCashableBonusInQuantity, // 0xAF
                new SasMeter(
                    SasMeterId.AftCashableBonusInQuantity,
                    SasMeterNames.AftCashableBonusInQuantity,
                    false,
                    false,
                    MeterCategory.Occurrence)
            },
            {
                SasMeterId.AftNonRestrictedBonusIn, // 0xB0
                new SasMeter(
                    SasMeterId.AftNonRestrictedBonusIn,
                    SasMeterNames.AftNonRestrictedBonusIn,
                    false,
                    true,
                    MeterCategory.Cents,
                    5)
            },
            {
                SasMeterId.AftNonRestrictedBonusInQuantity, // 0xB1
                new SasMeter(
                    SasMeterId.AftNonRestrictedBonusInQuantity,
                    SasMeterNames.AftNonRestrictedBonusInQuantity,
                    false,
                    true,
                    MeterCategory.Occurrence)
            },
            {
                SasMeterId.AftCashableOut, // 0xB8
                new SasMeter(
                    SasMeterId.AftCashableOut,
                    AccountingMeters.WatOffCashableAmount,
                    false,
                    false,
                    MeterCategory.Cents,
                    5)
            },
            {
                SasMeterId.AftCashableOutQuantity, // 0xB9
                new SasMeter(
                    SasMeterId.AftCashableOutQuantity,
                    AccountingMeters.WatOffCashableCount,
                    false,
                    false,
                    MeterCategory.Occurrence)
            },
            {
                SasMeterId.AftRestrictedOut, // 0xBA
                new SasMeter(
                    SasMeterId.AftRestrictedOut,
                    AccountingMeters.WatOffNonCashableAmount,
                    false,
                    false,
                    MeterCategory.Cents,
                    5)
            },
            {
                SasMeterId.AftRestrictedOutQuantity, // 0xBB
                new SasMeter(
                    SasMeterId.AftRestrictedOutQuantity,
                    AccountingMeters.WatOffNonCashableCount,
                    false,
                    false,
                    MeterCategory.Occurrence)
            },
            {
                SasMeterId.AftNonRestrictedOut, // 0xBC
                new SasMeter(
                    SasMeterId.AftNonRestrictedOut,
                    AccountingMeters.WatOffCashablePromoAmount,
                    false,
                    false,
                    MeterCategory.Cents,
                    5)
            },
            {
                SasMeterId.AftNonRestrictedOutQuantity, // 0xBD
                new SasMeter(
                    SasMeterId.AftNonRestrictedOutQuantity,
                    AccountingMeters.WatOffCashablePromoCount,
                    false,
                    false,
                    MeterCategory.Occurrence)
            },
            {
                SasMeterId.RegularCashableKeyedOnFunds, // 0xFA
                new SasMeter(
                    SasMeterId.RegularCashableKeyedOnFunds,
                    AccountingMeters.KeyedOnCashableAmount,
                    false,
                    false,
                    MeterCategory.Credit)
            },
            {
                SasMeterId.RestrictedPromotionalKeyedOnFunds, // 0xFB
                new SasMeter(
                    SasMeterId.RestrictedPromotionalKeyedOnFunds,
                    AccountingMeters.KeyedOnCashablePromoAmount,
                    false,
                    false,
                    MeterCategory.Credit)
            },
            {
                SasMeterId.NonRestrictedPromotionalKeyedOnFunds, // 0xFC
                new SasMeter(
                    SasMeterId.NonRestrictedPromotionalKeyedOnFunds,
                    AccountingMeters.KeyedOnNonCashableAmount,
                    false,
                    false,
                    MeterCategory.Credit)
            },
            {
                SasMeterId.RegularCashableKeyedOffFunds, // 0xFD
                new SasMeter(
                    SasMeterId.RegularCashableKeyedOffFunds,
                    AccountingMeters.KeyedOffCashableAmount,
                    false,
                    false,
                    MeterCategory.Credit)
            },
            {
                SasMeterId.RestrictedPromotionalKeyedOffFunds, // 0xFE
                new SasMeter(
                    SasMeterId.RestrictedPromotionalKeyedOffFunds,
                    AccountingMeters.KeyedOffCashablePromoAmount,
                    false,
                    false,
                    MeterCategory.Credit)
            },            {
                SasMeterId.NonRestrictedPromotionalKeyedOffFunds, // 0xFF
                new SasMeter(
                    SasMeterId.NonRestrictedPromotionalKeyedOffFunds,
                    AccountingMeters.KeyedOffNonCashableAmount,
                    false,
                    false,
                    MeterCategory.Credit)
            },
        };

        /// <summary>
        ///     Gets the SasMeter object for the given code.
        /// </summary>
        /// <param name="meterId">Sas meter code for the given info.</param>
        /// <returns>A SasMeter object for the given code, or null if it cannot be found.</returns>
        public static SasMeter SasMeterForCode(SasMeterId meterId)
        {
            return MeterCollection.TryGetValue(meterId, out var meter) ? meter : null;
        }

        /// <summary>
        ///     Gets an IEnumerable&lt;SasMeter&gt; object, for use in foreach statements.
        /// </summary>
        /// <returns>The IEnumerable&lt;SasMeter&gt; object.</returns>
        public IEnumerator<SasMeter> GetEnumerator()
        {
            return MeterCollection.Values.GetEnumerator();
        }

        /// <summary>
        ///     Gets a non-generic IEnumerable object.
        /// </summary>
        /// <returns>The IEnumerable object.</returns>
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return MeterCollection.Values.GetEnumerator();
        }
    }
}
