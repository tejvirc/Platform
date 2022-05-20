namespace Aristocrat.Mgam.Client
{
    /// <summary>
    ///     Supported notification codes.
    /// </summary>
    public enum NotificationCode
    {
        /// <summary>LOCKED_DOOR_OPEN</summary>
        LockedDoorOpen = 1,

        /// <summary>DOOR_OPENED</summary>
        DoorOpened = 2,

        /// <summary>DOOR_CLOSED</summary>
        DoorClosed = 3,

        /// <summary>EMPLOYEE_LOGGED_IN</summary>
        EmployeeLoggedIn = 4,

        /// <summary>EMPLOYEE_LOGGED_OUT</summary>
        EmployeeLoggedOut = 5,

        /// <summary>LOCKED_BA_JAM</summary>
        LockedBillAcceptorJam = 6,

        /// <summary>LOCKED_BA_FULL</summary>
        LockedBillAcceptorFull = 7,

        /// <summary>LOCKED_PRINTER_OOP</summary>
        LockedPrinterOutOfPaper = 8,

        /// <summary>LOCKED_PRINTER_JAMMED</summary>
        LockedPrinterJammed = 9,

        /// <summary>LOCKED_TILT</summary>
        LockedTilt = 10,

        /// <summary>LOCKED_CLOSE_SESSION_FAILED</summary>
        LockedCloseSessionFailed = 11,

        /// <summary>LOCKED_PRINT_VOUCHER_FAILED</summary>
        LockedPrintVoucherFailed = 12,

        /// <summary>LOCKED_BEGIN_SESSION_WITH_CASH_FAILED</summary>
        LockedBeginSessionWithCashFailed = 13,

        /// <summary>LOCKED_BEGIN_SESSION_WITH_VOUCHER_FAILED</summary>
        LockedBeginSessionWithVoucherFailed = 14,

        /// <summary>LOCKED_BEGIN_SESSION_WITH_SESSION_ID_FAILED</summary>
        LockedBeginSessionWithSessionIdFailed = 15,

        /// <summary>LOCKED_REGISTRATION_FAILED</summary>
        LockedRegistrationFailed = 16,

        /// <summary>LOCKED_CREDIT_CASH_FAILED</summary>
        LockedCreditCashFailed = 17,

        /// <summary>LOCKED_CREDIT_VOUCHER_FAILED</summary>
        LockedCreditVoucherFailed = 18,

        /// <summary>LOCKED_SOFTWARE_ERROR</summary>
        LockedSoftwareError = 19,

        /// <summary>LOCKED_MALFORMED_MESSAGE</summary>
        LockedMalformedMessage = 20,

        /// <summary>LOCKED_GAME_MALFUNCTION</summary>
        LockedGameMalfunction = 21,

        /// <summary>LOCKED_COMMANDED</summary>
        LockedCommanded = 22,

        /// <summary>POWER_FAILURE</summary>
        PowerFailure = 23,

        /// <summary>CANCELED_CREDIT_HAND_PAY</summary>
        CanceledCreditHandPay = 24,

        /// <summary>PROGRESSIVE_JACKPOT</summary>
        ProgressiveJackpot = 25,

        /// <summary>LOW_RAM_BATTERY</summary>
        LowRamBattery = 26,

        /// <summary>LOST_CONNECTION</summary>
        LostConnection = 27,

        /// <summary>RAM_CORRUPTION</summary>
        RamCorruption = 28,

        /// <summary>LOCKED_CHECKSUM_FAILED</summary>
        LockedChecksumFailed = 29,

        /// <summary>LOCKED_WIN_THRESHOLD_EXCEEDED</summary>
        LockedWinThresholdExceeded = 30,

        /// <summary>LOCK_CLEARED</summary>
        LockCleared = 31,

        /// <summary>CALL_ATTENDANT_ON</summary>
        CallAttendantOn = 34,

        /// <summary>CALL_ATTENDANT_OFF</summary>
        CallAttendantOff = 35
    }
}
