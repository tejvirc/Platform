namespace Aristocrat.Sas.Client.Metering
{
    /// <summary>An enumeration of Sas Meter identifiers.</summary>
    public enum SasMeterId
    {
        /// <summary>An invalid meter (-1).</summary>
        InvalidMeter = -1,

        /// <summary>Total coin in credits per game and per denom (0x00).</summary>
        TotalCoinIn = 0x00,

        /// <summary>Total coin out credits per game and per denom (0x01).</summary>
        TotalCoinOut = 0x01,

        /// <summary>Total jackpot credits per game and per denom (0x02).</summary>
        TotalJackpot = 0x02,

        /// <summary>Total hand paid cancelled credits per game and per denom (0x03).</summary>
        TotalHandPaidCanceledCredits = 0x03,

        /// <summary>Total cancelled credits (0x04).</summary>
        TotalCanceledCredits = 0x04,

        /// <summary>Number of games played per game and per denom (0x05).</summary>
        GamesPlayed = 0x05,

        /// <summary>Number of games won per game and per denom (0x06).</summary>
        GamesWon = 0x06,

        /// <summary>Number of games lost per game and per denom (0x07).</summary>
        GamesLost = 0x07,

        /// <summary>Total credits from coin acceptor (0x08).</summary>
        TotalCreditsFromCoinAcceptor = 0x08,

        /// <summary>Total credits paid from hopper (0x09).</summary>
        TotalCreditsPaidFromHopper = 0x09,

        /// <summary>Total credits from coins to drop (0x10).</summary>
        TotalCreditsFromCoinsToDrop = 0x0A,

        /// <summary>Total credits from bills accepted (0x0B).</summary>
        TotalCreditsFromBillsAccepted = 0x0B,

        /// <summary>Current credits (0x0C).</summary>
        CurrentCredits = 0x0C,

        /// <summary>Total Sas cashable ticket in, including nonrestricted tickets (cents) (0x0D, same as meter 0x80 + 0x84).</summary>
        TotalSasCashableTicketInCents = 0x0D,

        /// <summary>Total Sas cashable ticket out, including debit tickets (cents) (0x0E, same as meter 0x86 + 0x8A).</summary>
        TotalSasCashableTicketOutCents = 0x0E,

        /// <summary>Total Sas restricted ticket in (cents) (0x0F, same as meter 0x82).</summary>
        TotalSasRestrictedTicketInCents = 0x0F,

        /// <summary>Total Sas restricted ticket out (cents) (0x10, same as meter 0x88).</summary>
        TotalSasRestrictedTicketOutCents = 0x10,

        /// <summary>Total Sas cashable ticket in, including nonrestricted tickets (quantity) (0x11, same as meter 0x81 + 0x85).</summary>
        TotalSasCashableTicketInQuantity = 0x11,

        /// <summary>Total Sas cashable ticket out, including debit tickets (quantity) (0x12, same as meter 0x87 + 0x8B).</summary>
        TotalSasCashableTicketOutQuantity = 0x12,

        /// <summary>Total Sas restricted ticket in (quantity) (0x13, same as meter 0x83).</summary>
        TotalSasRestrictedTicketInQuantity = 0x13,

        /// <summary>Total Sas restricted ticet out (quantity) (0x14, same as 0x89).</summary>
        TotalSasRestrictedTicketOutQuantity = 0x14,

        /// <summary>Total ticket in, including cashable, nonrestricted, restricted, and debit tickets (credits) (0x15).</summary>
        TotalTicketIn = 0x15,

        /// <summary>Total ticket out, including cashable, nonrestricted, restricted, and debit tickets (credits) (0x16).</summary>
        TotalTicketOut = 0x16,

        /// <summary>Total electronic transfers to gaming machine (0x17).</summary>
        TotalElectronicTransfersToGamingMachine = 0x17,

        /// <summary>Total electronic transfers to host (0x18).</summary>
        TotalElectronicTransfersToHost = 0x18,

        /// <summary>The credit amount played in restircted credits (0x19).</summary>
        TotalRestrictedAmountPlayed = 0x19,

        /// <summary>The credit amount played in non-restircted credits (0x1A).</summary>
        TotalNonRestrictedAmountPlayed = 0x1A,

        /// <summary>Current restricted credits (0x1B).</summary>
        CurrentRestrictedCredits = 0x1B,

        /// <summary>Total machine paid paytable win excluding progressive or external bonus amounts (0x1C).</summary>
        TotalMachinePaidPaytableWin = 0x1C,

        /// <summary>Total machine paid progressive win amount in credits (0x1D).</summary>
        TotalMachinePaidProgressiveWin = 0x1D,

        /// <summary>Total machine paid bonus (0x1E).</summary>
        TotalMachinePaidBonus = 0x1E,

        /// <summary>Total Hand paid paytable win (0x1F).</summary>
        TotalHandPaidPaytableWin = 0x1F,

        /// <summary>The progressive amount handpaid (0x20).</summary>
        TotalAttendantPaidProgressiveWin = 0x20,

        /// <summary>Total attendant paid external bonus (0x21).</summary>
        TotalAttendantPaidExternalBonus = 0x21,

        /// <summary>Sum of total coin out and total jackpot (0x22).</summary>
        TotalWonCredits = 0x22,

        /// <summary>Sum of total handpaid canceled credits and total jackpot (0x23).</summary>
        TotalHandPaidCredits = 0x23,

        /// <summary>
        /// Total drop, including but not limited to coins to drop, bills to drop, tickets to 
        /// drop, and electronic in (credits) (0x24).
        /// </summary>
        TotalDrop = 0x24,

        /// <summary>Games since last power reset (0x25).</summary>
        GamesSinceLastPowerReset = 0x25,

        /// <summary>Games played since slot door was closed (0x26).</summary>
        GamesSinceSlotClosure = 0x26,

        /// <summary>Total credits from external coin acceptor (0x27).</summary>
        TotalCreditsFromExternalCoinAcceptor = 0x27,

        /// <summary>Cashable ticket in amount including nonrestricted promotional tickets (0x28).</summary>
        TotalCashableTicketIn = 0x28,

        /// <summary>The total amount of cashable ticket in (0x29).</summary>
        TotalRegularCashableTicketIn = 0x29,

        /// <summary>Ticket in amount for restricted promo (0x2A).</summary>
        TotalRestrictedPromoTicketIn = 0x2A,

        /// <summary>Ticket in amount for non-restricted promo (0x2B).</summary>
        TotalNonRestrictedPromoTicketIn = 0x2B,

        /// <summary>Ticket out amount for cashable (0x2C).</summary>
        TotalCashableTicketOut = 0x2C,

        /// <summary>Ticket out amount for restricted promo (0x2D).</summary>
        TotalRestrictedPromoTicketOut = 0x2D,

        /// <summary>Electronic Regular Cashable Transfers To Gaming Machine (same as 00A0) (0x2E).</summary>
        ElectronicCashableTransfersToGamingMachine = 0x2E,

        /// <summary>Electronic Restricted Promotional Transfers To Gaming Machine (same as A2) (0x2F).</summary>
        ElectronicRestrictedPromotionTransfersToGamingMachine = 0x2F,

        /// <summary>Electronic NonRestricted Promotional Transfers To Gaming Machine (Same as A4) (0x30).</summary>
        ElectronicNonRestrictedTransfersToGamingMachine = 0x30,

        /// <summary>Electronic Debit Transfers To Gaming Machine (0x31).</summary>
        ElectronicDebitTransfersToGamingMachine = 0x31,

        /// <summary>Electronic Regular Cashable Transfers To Host (same as B8) (0x32).</summary>
        ElectronicCashableTransfersToHost = 0x32,

        /// <summary>Electronic Restricted Transfers To Host (same as BA) (0x33).</summary>
        ElectronicRestrictedTransfersToHost = 0x33,

        /// <summary>Electronic NonRestricted Promotional Transfers To Host (Same as BC) (0x34).</summary>
        ElectronicNonRestrictedTransfersToHost = 0x34,

        /// <summary>Number of regular cashable ticket in transactions (0x35).</summary>
        TotalRegularCashableTicketInQuantity = 0x35,

        /// <summary>Number of regular restricted ticket in transactions (0x36).</summary>
        TotalRestrictedPromoTicketInQuantity = 0x36,

        /// <summary>Number of regular non-restricted ticket in transactions (0x37).</summary>
        TotalNonRestrictedPromoTicketInQuantity = 0x37,

        /// <summary>Number of regular cashable ticket out transactions, including debit tickets (0x38).</summary>
        TotalRegularCashableTicketOutQuantity = 0x38,

        /// <summary>Number of regular restricted ticket out transactions, including debit tickets (0x39).</summary>
        TotalRestrictedPromoTicketOutQuantity = 0x39,

        /// <summary>Total number of bills currently in the stacker (period meter) (0x3E).</summary>
        TotalBillsInStacker = 0x3E,

        /// <summary>Total amount of bills currently in the stacker (period meter) (0x3F).</summary>
        TotalBillAmountInStacker = 0x3F,

        /// <summary>Total number of $1.00 bills accepted (0x40).</summary>
        TotalNumberBillsAccepted1 = 0x40,

        /// <summary>Total number of $2.00 bills accepted (0x41).</summary>
        TotalNumberBillsAccepted2 = 0x41,

        /// <summary>Total number of $5.00 bills accepted (0x42).</summary>
        TotalNumberBillsAccepted5 = 0x42,

        /// <summary>Total number of $10.00 bills accepted (0x43).</summary>
        TotalNumberBillsAccepted10 = 0x43,

        /// <summary>Total number of $20.00 bills accepted (0x44).</summary>
        TotalNumberBillsAccepted20 = 0x44,

        /// <summary>Total number of $25.00 bills accepted (0x45).</summary>
        TotalNumberBillsAccepted25 = 0x45,

        /// <summary>Total number of $50.00 bills accepted (0x46).</summary>
        TotalNumberBillsAccepted50 = 0x46,

        /// <summary>Total number of $100.00 bills accepted (0x47).</summary>
        TotalNumberBillsAccepted100 = 0x47,

        /// <summary>Total number of $200.00 bills accepted (0x48).</summary>
        TotalNumberBillsAccepted200 = 0x48,

        /// <summary>Total number of $250.00 bills accepted (0x49).</summary>
        TotalNumberBillsAccepted250 = 0x49,

        /// <summary>Total number of $500.00 bills accepted (0x4A).</summary>
        TotalNumberBillsAccepted500 = 0x4A,

        /// <summary>Total number of $1,000.00 bills accepted (0x4B).</summary>
        TotalNumberBillsAccepted1000 = 0x4B,

        /// <summary>Total number of $2,000.00 bills accepted (0x4C).</summary>
        TotalNumberBillsAccepted2000 = 0x4C,

        /// <summary>Total number of $2,500.00 bills accepted (0x4D).</summary>
        TotalNumberBillsAccepted2500 = 0x4D,

        /// <summary>Total number of $5,000.00 bills accepted (0x4E).</summary>
        TotalNumberBillsAccepted5000 = 0x4E,

        /// <summary>Total number of $10,000.00 bills accepted (0x4F).</summary>
        TotalNumberBillsAccepted10000 = 0x4F,

        /// <summary>Total number of $20,000.00 bills accepted (0x50).</summary>
        TotalNumberBillsAccepted20000 = 0x50,

        /// <summary>Total number of $2,5000.00 bills accepted (0x51).</summary>
        TotalNumberBillsAccepted25000 = 0x51,

        /// <summary>Total number of $50,000.00 bills accepted (0x52).</summary>
        TotalNumberBillsAccepted50000 = 0x52,

        /// <summary>Total number of $100,000.00 bills accepted (0x53).</summary>
        TotalNumberBillsAccepted100000 = 0x53,

        /// <summary>Total number of $200,000.00 bills accepted (0x54).</summary>
        TotalNumberBillsAccepted200000 = 0x54,

        /// <summary>Total number of $250,000.00 bills accepted (0x55).</summary>
        TotalNumberBillsAccepted250000 = 0x55,

        /// <summary>Total number of $500,000.00 bills accepted (0x56).</summary>
        TotalNumberBillsAccepted500000 = 0x56,

        /// <summary>Total number of $1,000,000.00 bills accepted (0x57).</summary>
        TotalNumberBillsAccepted1000000 = 0x57,

        /// <summary>Total credits from bills to drop (0x58).</summary>
        TotalCreditsBillsToDrop = 0x58,

        /// <summary>Total number of $1 bills to drop (0x59).</summary>
        TotalNumberBillsToDrop1 = 0x59,

        /// <summary>Total number of $2 bills to drop (0x5A).</summary>
        TotalNumberBillsToDrop2 = 0x5A,

        /// <summary>Total number of $5 bills to drop (0x5B).</summary>
        TotalNumberBillsToDrop5 = 0x5B,

        /// <summary>Total number of $10 bills to drop (0x5C).</summary>
        TotalNumberBillsToDrop10 = 0x5C,

        /// <summary>Total number of $20 bills to drop (0x5D).</summary>
        TotalNumberBillsToDrop20 = 0x5D,

        /// <summary>Total number of $50 bills to drop (0x5E).</summary>
        TotalNumberBillsToDrop50 = 0x5E,

        /// <summary>Total number of $100 bills to drop (0x5F).</summary>
        TotalNumberBillsToDrop100 = 0x5F,

        /// <summary>Total number of $200 bills to drop (0x60).</summary>
        TotalNumberBillsToDrop200 = 0x60,

        /// <summary>Total number of $500 bills to drop (0x61).</summary>
        TotalNumberBillsToDrop500 = 0x61,

        /// <summary>Total number of $1000 bills to drop (0x62).</summary>
        TotalNumberBillsToDrop1000 = 0x62,

        /// <summary>The weighted average theoretical payback percentage in hundredths of a percent (0x7F).</summary>
        AverageTheoreticalPaybackPercentage = 0x7F,

        /// <summary>Regular cashable ticket in (cents) (0x80).</summary>
        CashableTicketInCents = 0x80,

        /// <summary>Regular cashable ticket in (count) (0x81).</summary>
        CashableTicketInCount = 0x81,

        /// <summary>Restricted ticket in (cents) (0x82).</summary>
        RestrictedTicketInCents = 0x82,

        /// <summary>Restricted ticket in (count) (0x83).</summary>
        RestrictedTicketInCount = 0x83,

        /// <summary>Non-restricted ticket in (cents) (0x84).</summary>
        NonRestrictedTicketInCents = 0x84,

        /// <summary>Non-restricted ticket in (count) (0x85).</summary>
        NonRestrictedTicketInCount = 0x85,

        /// <summary>Cashable ticket out (cents) (0x86).</summary>
        CashableTicketOutCents = 0x86,

        /// <summary>Cashable ticket out (count) (0x87).</summary>
        CashableTicketOutCount = 0x87,

        /// <summary>Restricted ticket out (cents) (0x88).</summary>
        RestrictedTicketOutCents = 0x88,

        /// <summary>Restricted ticket out (count) (0x89).</summary>
        RestrictedTicketOutCount = 0x89,

        /// <summary>Validated canceled credit handpay, receipt printed (cents) (0x8C).</summary>
        ValidatedCanceledCreditHandPayReceiptCents = 0x8C,

        /// <summary>Validated canceled credit handpay, receipt printed (quantity) (0x8D).</summary>
        ValidatedCanceledCreditHandPayReceiptCount = 0x8D,

        /// <summary> Validated jackpot handpay, receipt printed (cents) (0x8E).</summary>
        ValidatedJackpotHandPayReceiptCents = 0x8E,

        /// <summary>Validated jackpot handpay, receipt printed (quantity) (0x8F).</summary>
        ValidatedJackpotHandPayReceiptCount = 0x8F,

        /// <summary>Validated canceled credit handpay, no receipt printed (cents) (0x90).</summary>
        ValidatedCanceledCreditHandPayNoReceiptCents = 0x90,

        /// <summary>Validated canceled credit handpay, no receipt printed (quantity) (0x91).</summary>
        ValidatedCanceledCreditHandPayNoReceiptCount = 0x91,

        /// <summary>Validated jackpot handpay, no receipt printed (cents) (0x92).</summary>
        ValidatedJackpotHandPayNoReceiptCents = 0x92,

        /// <summary>Validated jackpot handpay, no receipt printed (quantity) (0x93).</summary>
        ValidatedJackpotHandPayNoReceiptCount = 0x93,

        /// <summary>In-house cashable transfers to gaming machine (cents) (0xA0).</summary>
        AftCashableIn = 0xA0,

        /// <summary>In-house cashable transfers to gaming machine (quantity) (0xA1).</summary>
        AftCashableInQuantity = 0xA1,

        /// <summary>In-house restricted transfers to gaming machine (cents) (0xA2).</summary>
        AftRestrictedIn = 0xA2,

        /// <summary>In-house restricted transfers to gaming machine (quantity) (0xA3).</summary>
        AftRestrictedInQuantity = 0xA3,

        /// <summary>In-house nonrestricted transfers to gaming machine (cents) (0xA4).</summary>
        AftNonRestrictedIn = 0xA4,

        /// <summary>In-house nonrestricted transfers to gaming machine (quantity) (0xA5).</summary>
        AftNonRestrictedInQuantity = 0xA5,

        /// <summary>Bonus cashable transfers to gaming machine (cents) (0xAE).</summary>
        AftCashableBonusIn = 0xAE,

        /// <summary>Bonus cashable transfers to gaming machine (quantity) (0xAF).</summary>
        AftCashableBonusInQuantity = 0xAF,

        /// <summary>Bonus nonrestricted transfers to gaming machine (cents) (0xB0).</summary>
        AftNonRestrictedBonusIn = 0xB0,

        /// <summary>Bonus nonrestricted transfers to gaming machine (quantity) (0xB1).</summary>
        AftNonRestrictedBonusInQuantity = 0xB1,

        /// <summary>In-house cashable transfers to host (cents) (0xB8).</summary>
        AftCashableOut = 0xB8,

        /// <summary>In-house cashable transfers to host (quantity) (0xB9).</summary>
        AftCashableOutQuantity = 0xB9,

        /// <summary>In-house restricted transfers to host (cents) (0xBA).</summary>
        AftRestrictedOut = 0xBA,

        /// <summary>In-house restricted transfers to host (quantity) (0xBB).</summary>
        AftRestrictedOutQuantity = 0xBB,

        /// <summary>In-house nonrestricted transfers to host (cents) (0xBC).</summary>
        AftNonRestrictedOut = 0xBC,

        /// <summary>In-house nonrestricted transfers to host (quantity) (0xBD).</summary>
        AftNonRestrictedOutQuantity = 0xBD,

        /// <summary>Regular cashable keyed-on funds (credits) (0xFA).</summary>
        RegularCashableKeyedOnFunds = 0xFA,

        /// <summary>Restricted promotional keyed-on funds (credits) (0xFB).</summary>
        RestrictedPromotionalKeyedOnFunds = 0xFB,

        /// <summary>Non restricted promotional keyed-on funds (credits) (0xFC).</summary>
        NonRestrictedPromotionalKeyedOnFunds = 0xFC,

        /// <summary>Regular cashable keyed-off funds (credits) (0xFD).</summary>
        RegularCashableKeyedOffFunds = 0xFD,

        /// <summary>Restricted promotional keyed-off funds (credits) (0xFE).</summary>
        RestrictedPromotionalKeyedOffFunds = 0xFE,

        /// <summary>Non restricted promotional keyed-off funds (credits) (0xFF).</summary>
        NonRestrictedPromotionalKeyedOffFunds = 0xFF
    }
}
