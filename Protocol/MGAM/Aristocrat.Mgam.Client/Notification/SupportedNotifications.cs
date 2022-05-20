namespace Aristocrat.Mgam.Client.Notification
{
    using System.Collections.Generic;

    /// <summary>
    ///     Contains a list of supported notifications.
    /// </summary>
    public class SupportedNotifications
    {
        private static readonly NotificationInfo[] Notifications = {
            new NotificationInfo
            {
                NotificationId = (int)NotificationCode.LockedDoorOpen,
                Description = "LOCKED_DOOR_OPEN",
                ParameterName = "Door name",
                UrgencyLevel = NotificationUrgencyLevel.Critical,
                RecipientCodes = NotificationRecipientCode.Log | NotificationRecipientCode.Site |
                                 NotificationRecipientCode.Security
            },
            new NotificationInfo
            {
                NotificationId = (int)NotificationCode.DoorOpened,
                Description = "DOOR_OPENED",
                ParameterName = "Door name",
                UrgencyLevel = NotificationUrgencyLevel.Low,
                RecipientCodes = NotificationRecipientCode.Log
            },
            new NotificationInfo
            {
                NotificationId = (int)NotificationCode.DoorClosed,
                Description = "DOOR_CLOSED",
                ParameterName = "Door name",
                UrgencyLevel = NotificationUrgencyLevel.Low,
                RecipientCodes = NotificationRecipientCode.Log
            },
            new NotificationInfo
            {
                NotificationId = (int)NotificationCode.EmployeeLoggedIn,
                Description = "EMPLOYEE_LOGGED_IN",
                ParameterName = "Card string",
                UrgencyLevel = NotificationUrgencyLevel.Low,
                RecipientCodes = NotificationRecipientCode.Log | NotificationRecipientCode.Site
            },
            new NotificationInfo
            {
                NotificationId = (int)NotificationCode.EmployeeLoggedOut,
                Description = "EMPLOYEE_LOGGED_OUT",
                UrgencyLevel = NotificationUrgencyLevel.Low,
                RecipientCodes = NotificationRecipientCode.Log | NotificationRecipientCode.Site
            },
            new NotificationInfo
            {
                NotificationId = (int)NotificationCode.LockedBillAcceptorJam,
                Description = "LOCKED_BA_JAM",
                UrgencyLevel = NotificationUrgencyLevel.Critical,
                RecipientCodes = NotificationRecipientCode.Log | NotificationRecipientCode.Site
            },
            new NotificationInfo
            {
                NotificationId = (int)NotificationCode.LockedBillAcceptorFull,
                Description = "LOCKED_BA_FULL",
                UrgencyLevel = NotificationUrgencyLevel.Critical,
                RecipientCodes = NotificationRecipientCode.Log | NotificationRecipientCode.Site
            },
            new NotificationInfo
            {
                NotificationId = (int)NotificationCode.LockedPrinterOutOfPaper,
                Description = "LOCKED_PRINTER_OOP",
                UrgencyLevel = NotificationUrgencyLevel.Critical,
                RecipientCodes = NotificationRecipientCode.Log | NotificationRecipientCode.Site
            },
            new NotificationInfo
            {
                NotificationId = (int)NotificationCode.LockedPrinterJammed,
                Description = "LOCKED_PRINTER_JAMMED",
                UrgencyLevel = NotificationUrgencyLevel.Critical,
                RecipientCodes = NotificationRecipientCode.Log | NotificationRecipientCode.Site
            },
            new NotificationInfo
            {
                NotificationId = (int)NotificationCode.LockedTilt,
                Description = "LOCKED_TILT",
                ParameterName = "Description",
                UrgencyLevel = NotificationUrgencyLevel.Critical,
                RecipientCodes = NotificationRecipientCode.Log | NotificationRecipientCode.Site
            },
            new NotificationInfo
            {
                NotificationId = (int)NotificationCode.LockedCloseSessionFailed,
                Description = "LOCKED_CLOSE_SESSION_FAILED",
                ParameterName = "Session ID",
                UrgencyLevel = NotificationUrgencyLevel.Critical,
                RecipientCodes = NotificationRecipientCode.Log | NotificationRecipientCode.Site
            },
            new NotificationInfo
            {
                NotificationId = (int)NotificationCode.LockedPrintVoucherFailed,
                Description = "LOCKED_PRINT_VOUCHER_FAILED",
                ParameterName = "Voucher ID",
                UrgencyLevel = NotificationUrgencyLevel.Critical,
                RecipientCodes = NotificationRecipientCode.Log | NotificationRecipientCode.Site
            },
            new NotificationInfo
            {
                NotificationId = (int)NotificationCode.LockedBeginSessionWithCashFailed,
                Description = "LOCKED_BEGIN_SESSION_WITH_CASH_FAILED",
                ParameterName = "Cash Value",
                UrgencyLevel = NotificationUrgencyLevel.Critical,
                RecipientCodes = NotificationRecipientCode.Log | NotificationRecipientCode.Site
            },
            new NotificationInfo
            {
                NotificationId = (int)NotificationCode.LockedBeginSessionWithVoucherFailed,
                Description = "LOCKED_BEGIN_SESSION_WITH_VOUCHER_FAILED",
                ParameterName = "Voucher ID",
                UrgencyLevel = NotificationUrgencyLevel.Critical,
                RecipientCodes = NotificationRecipientCode.Log | NotificationRecipientCode.Site
            },
            new NotificationInfo
            {
                NotificationId = (int)NotificationCode.LockedBeginSessionWithSessionIdFailed,
                Description = "LOCKED_BEGIN_SESSION_WITH_SESSION_ID_FAILED",
                ParameterName = "Session ID",
                UrgencyLevel = NotificationUrgencyLevel.Critical,
                RecipientCodes = NotificationRecipientCode.Log | NotificationRecipientCode.Site
            },
            new NotificationInfo
            {
                NotificationId = (int)NotificationCode.LockedRegistrationFailed,
                Description = "LOCKED_REGISTRATION_FAILED",
                UrgencyLevel = NotificationUrgencyLevel.Critical,
                RecipientCodes = NotificationRecipientCode.Log | NotificationRecipientCode.Site
            },
            new NotificationInfo
            {
                NotificationId = (int)NotificationCode.LockedCreditCashFailed,
                Description = "LOCKED_CREDIT_CASH_FAILED",
                ParameterName = "Cash Value",
                UrgencyLevel = NotificationUrgencyLevel.Critical,
                RecipientCodes = NotificationRecipientCode.Log | NotificationRecipientCode.Site
            },
            new NotificationInfo
            {
                NotificationId = (int)NotificationCode.LockedCreditVoucherFailed,
                Description = "LOCKED_CREDIT_VOUCHER_FAILED",
                ParameterName = "Voucher ID",
                UrgencyLevel = NotificationUrgencyLevel.Critical,
                RecipientCodes = NotificationRecipientCode.Log | NotificationRecipientCode.Site
            },
            new NotificationInfo
            {
                NotificationId = (int)NotificationCode.LockedSoftwareError,
                Description = "LOCKED_SOFTWARE_ERROR",
                ParameterName = "Error Information",
                UrgencyLevel = NotificationUrgencyLevel.Critical,
                RecipientCodes = NotificationRecipientCode.Log | NotificationRecipientCode.Site
            },
            new NotificationInfo
            {
                NotificationId = (int)NotificationCode.LockedMalformedMessage,
                Description = "LOCKED_MALFORMED_MESSAGE",
                ParameterName = "Error Description",
                UrgencyLevel = NotificationUrgencyLevel.Critical,
                RecipientCodes = NotificationRecipientCode.Log | NotificationRecipientCode.Site
            },
            new NotificationInfo
            {
                NotificationId = (int)NotificationCode.LockedGameMalfunction,
                Description = "LOCKED_GAME_MALFUNCTION",
                ParameterName = "Error Description",
                UrgencyLevel = NotificationUrgencyLevel.Critical,
                RecipientCodes = NotificationRecipientCode.Log | NotificationRecipientCode.Site
            },
            new NotificationInfo
            {
                NotificationId = (int)NotificationCode.LockedCommanded,
                Description = "LOCKED_COMMANDED",
                UrgencyLevel = NotificationUrgencyLevel.Low,
                RecipientCodes = NotificationRecipientCode.Log
            },
            new NotificationInfo
            {
                NotificationId = (int)NotificationCode.PowerFailure,
                Description = "POWER_FAILURE",
                UrgencyLevel = NotificationUrgencyLevel.Critical,
                RecipientCodes = NotificationRecipientCode.Log | NotificationRecipientCode.Site
            },
            new NotificationInfo
            {
                NotificationId = (int)NotificationCode.CanceledCreditHandPay,
                Description = "CANCELED_CREDIT_HAND_PAY",
                UrgencyLevel = NotificationUrgencyLevel.Medium,
                RecipientCodes = NotificationRecipientCode.Log | NotificationRecipientCode.Site
            },
            new NotificationInfo
            {
                NotificationId = (int)NotificationCode.ProgressiveJackpot,
                Description = "PROGRESSIVE_JACKPOT",
                ParameterName = "Progressive name",
                UrgencyLevel = NotificationUrgencyLevel.High,
                RecipientCodes = NotificationRecipientCode.Log | NotificationRecipientCode.Site
            },
            new NotificationInfo
            {
                NotificationId = (int)NotificationCode.LowRamBattery,
                Description = "LOW_RAM_BATTERY",
                UrgencyLevel = NotificationUrgencyLevel.Medium,
                RecipientCodes = NotificationRecipientCode.Log | NotificationRecipientCode.Site |
                                 NotificationRecipientCode.Vendor
            },
            new NotificationInfo
            {
                NotificationId = (int)NotificationCode.LostConnection,
                Description = "LOST_CONNECTION",
                UrgencyLevel = NotificationUrgencyLevel.Medium,
                RecipientCodes = NotificationRecipientCode.Log | NotificationRecipientCode.Site
            },
            new NotificationInfo
            {
                NotificationId = (int)NotificationCode.RamCorruption,
                Description = "RAM_CORRUPTION",
                ParameterName = "Extended Information",
                UrgencyLevel = NotificationUrgencyLevel.Critical,
                RecipientCodes = NotificationRecipientCode.Log | NotificationRecipientCode.Site
            },
            new NotificationInfo
            {
                NotificationId = (int)NotificationCode.LockedChecksumFailed,
                Description = "LOCKED_CHECKSUM_FAILED",
                ParameterName = "Reason",
                UrgencyLevel = NotificationUrgencyLevel.Critical,
                RecipientCodes = NotificationRecipientCode.Log | NotificationRecipientCode.Site
            },
            new NotificationInfo
            {
                NotificationId = (int)NotificationCode.LockedWinThresholdExceeded,
                Description = "LOCKED_WIN_THRESHOLD_EXCEEDED",
                ParameterName = "Win Amount",
                UrgencyLevel = NotificationUrgencyLevel.Critical,
                RecipientCodes = NotificationRecipientCode.Log | NotificationRecipientCode.Site
            },
            new NotificationInfo
            {
                NotificationId = (int)NotificationCode.LockCleared,
                Description = "LOCK_CLEARED",
                UrgencyLevel = NotificationUrgencyLevel.Critical,
                RecipientCodes = NotificationRecipientCode.Log | NotificationRecipientCode.Site
            },
            new NotificationInfo
            {
                NotificationId = (int)NotificationCode.CallAttendantOn,
                Description = "CALL_ATTENDANT_ON",
                UrgencyLevel = NotificationUrgencyLevel.Critical,
                RecipientCodes = NotificationRecipientCode.Log | NotificationRecipientCode.Site
            },
            new NotificationInfo
            {
                NotificationId = (int)NotificationCode.CallAttendantOff,
                Description = "CALL_ATTENDANT_OFF",
                UrgencyLevel = NotificationUrgencyLevel.Critical,
                RecipientCodes = NotificationRecipientCode.Log | NotificationRecipientCode.Site
            }
        };

        /// <summary>
        ///     Gets a list of supported notifications.
        /// </summary>
        /// <returns><see cref="NotificationInfo"/> enumeration.</returns>
        public static IEnumerable<NotificationInfo> Get() => Notifications;
    }
}
