// ReSharper disable InconsistentNaming
namespace Aristocrat.Mgam.Client.Messaging
{
    using System.Diagnostics.CodeAnalysis;

    /// <summary>
    ///     Response status codes.
    /// </summary>
    [SuppressMessage("ReSharper", "UnusedMember.Global")]
    public enum ServerResponseCode
    {
        /// <summary>No response code was sent from server.</summary>
        Unknown = -1,

        /// <summary>SRC_OK</summary>
        Ok = 0,

        /// <summary>SRC_CardIsNotInGroup</summary>
        CardIsNotInGroup = 1,

        /// <summary>SRC_ChecksumFailure</summary>
        ChecksumFailure = 2,

        /// <summary>SRC_InvalidAccessType</summary>
        InvalidAccessType = 3,

        /// <summary>SRC_InvalidAmount</summary>
        InvalidAmount = 4,

        /// <summary>SRC_InvalidBarcode</summary>
        InvalidBarcode = 5,

        /// <summary>SRC_InvalidBarcodeLength</summary>
        InvalidBarcodeLength = 6,

        /// <summary>SRC_InvalidCardString</summary>
        InvalidCardString = 7,

        /// <summary>SRC_InvalidControlType</summary>
        InvalidControlType = 9,

        /// <summary>SRC_InvalidDataType</summary>
        InvalidDataType = 10,

        /// <summary>SRC_InvalidDefaultValue</summary>
        InvalidDefaultValue = 11,

        /// <summary>SRC_ProgressiveMessageAttrNotRegistered</summary>
        ProgressiveMessageAttrNotRegistered = 12,

        /// <summary>SRC_InvalidGroupName</summary>
        InvalidGroupName = 13,

        /// <summary>SRC_InvalidInstanceID</summary>
        InvalidInstanceId = 14,

        /// <summary>SRC_InvalidLocalTransactionID</summary>
        InvalidLocalTransactionId = 15,

        /// <summary>SRC_InvalidMaximumValue</summary>
        InvalidMaximumValue = 16,

        /// <summary>SRC_InvalidMessageAttribute</summary>
        InvalidMessageAttribute = 17,

        /// <summary>SRC_InvalidMinimumValue</summary>
        InvalidMinimumValue = 18,

        /// <summary>SRC_InvalidMinMaxValues</summary>
        InvalidMinMaxValues = 19,

        /// <summary>SRC_InvalidPayTableIndex</summary>
        InvalidPayTableIndex = 20,

        /// <summary>SRC_InvalidDenomination</summary>
        InvalidDenomination = 21,

        /// <summary>SRC_InvalidGameUPCNumber</summary>
        InvalidGameUpcNumber = 22,

        /// <summary>SRC_InvalidPlayerTrackingString</summary>
        InvalidPlayerTrackingString = 23,

        /// <summary>SRC_InvalidSessionBalance</summary>
        InvalidSessionBalance = 25,

        /// <summary>SRC_InvalidSessionID</summary>
        InvalidSessionId = 26,

        /// <summary>SRC_InvalidStringParameterTooLong</summary>
        InvalidStringParameterTooLong = 27,

        /// <summary>SRC_InvalidValueAttribute</summary>
        InvalidValueAttribute = 28,

        /// <summary>SRC_DuplicateVoucherRetry</summary>
        DuplicateVoucherRetry = 29,

        /// <summary>SRC_BarcodeNotFound</summary>
        BarcodeNotFound = 30,

        /// <summary>SRC_SomeGamesExceedMaxBet</summary>
        SomeGamesExceedMaxBet = 31,

        /// <summary>SRC_AttributeAlreadyRegistered</summary>
        AttributeAlreadyRegistered = 32,

        /// <summary>SRC_AttributeInvalidAttributeName</summary>
        AttributeInvalidAttributeName = 33,

        /// <summary>SRC_AttributeInvalidScope</summary>
        AttributeInvalidScope = 35,

        /// <summary>SRC_AttributeNameNotFound</summary>
        AttributeNameNotFound = 36,

        /// <summary>SRC_AttributeWritePermissionDenied</summary>
        AttributeWritePermissionDenied = 37,

        /// <summary>SRC_CommandAlreadyRegistered</summary>
        CommandAlreadyRegistered = 38,

        /// <summary>SRC_CommandInvalidCommandID</summary>
        CommandInvalidCommandId = 39,

        /// <summary>SRC_CommandInvalidParameterName</summary>
        CommandInvalidParameterName = 40,

        /// <summary>SRC_ApplicationAlreadyRegistered</summary>
        ApplicationAlreadyRegistered = 42,

        /// <summary>SRC_ApplicationInvalidGUID</summary>
        ApplicationInvalidGuid = 43,

        /// <summary>SRC_ApplicationInvalidName</summary>
        ApplicationInvalidName = 44,

        /// <summary>SRC_ApplicationInvalidChecksum</summary>
        ApplicationInvalidChecksum = 45,

        /// <summary>SRC_GameAlreadyRegistered</summary>
        GameAlreadyRegistered = 46,

        /// <summary>SRC_GameInvalidGameDescription</summary>
        GameInvalidGameDescription = 47,

        /// <summary>SRC_GameInvalidNumberOfCredits</summary>
        GameInvalidNumberOfCredits = 48,

        /// <summary>SRC_ProgressiveValueAttrNotRegistered</summary>
        ProgressiveValueAttrNotRegistered = 49,

        /// <summary>SRC_InstanceInvalidManufacturerName</summary>
        InstanceInvalidManufacturerName = 51,

        /// <summary>SRC_InstanceInvalidDeviceGUID</summary>
        InstanceInvalidDeviceGuid = 52,

        /// <summary>SRC_InstanceInvalidDeviceName</summary>
        InstanceInvalidDeviceName = 53,

        /// <summary>SRC_InstanceInvalidInstallationGUID</summary>
        InstanceInvalidInstallationGuid = 54,

        /// <summary>SRC_InstanceInvalidInstallationName</summary>
        InstanceInvalidInstallationName = 55,

        /// <summary>SRC_InstanceInvalidApplicationGUID</summary>
        InstanceInvalidApplicationGuid = 56,

        /// <summary>SRC_InstanceInvalidApplicationName</summary>
        InstanceInvalidApplicationName = 57,

        /// <summary>SRC_NotificationAlreadyRegistered</summary>
        NotificationAlreadyRegistered = 58,

        /// <summary>SRC_InvalidDescription</summary>
        InvalidDescription = 59,

        /// <summary>SRC_NotificationInvalidNotificationID</summary>
        NotificationInvalidNotificationId = 60,

        /// <summary>SRC_ProgressiveUnknownProgressiveName</summary>
        ProgressiveUnknownProgressiveName = 61,

        /// <summary>SRC_NotificationInvalidPriorityValue</summary>
        NotificationInvalidPriorityValue = 62,

        /// <summary>SRC_NotificationInvalidUrgencyLevel</summary>
        NotificationInvalidUrgencyLevel = 63,

        /// <summary>SRC_NotificationInvalidRecipientCode</summary>
        NotificationInvalidRecipientCode = 64,

        /// <summary>SRC_PlayerAlreadyLoggedOn</summary>
        PlayerAlreadyLoggedOn = 65,

        /// <summary>SRC_PlayerAnotherPlayerLoggedOn</summary>
        PlayerAnotherPlayerLoggedOn = 66,

        /// <summary>SRC_PlayerNoPlayerLoggedOn</summary>
        PlayerNoPlayerLoggedOn = 67,

        /// <summary>SRC_PlayerUnknownPlayerTrackingString</summary>
        PlayerUnknownPlayerTrackingString = 69,

        /// <summary>SRC_ProgressiveAlreadyRegistered</summary>
        ProgressiveAlreadyRegistered = 70,

        /// <summary>SRC_ProgressiveInvalidProgressiveName</summary>
        ProgressiveInvalidProgressiveName = 71,

        /// <summary>SRC_ServiceUnknownServiceName</summary>
        ServiceUnknownServiceName = 72,

        /// <summary>SRC_SessionInProgress</summary>
        SessionInProgress = 73,

        /// <summary>SRC_SessionBalanceMismatch</summary>
        SessionBalanceMismatch = 74,

        /// <summary>SRC_SessionIDNotFound</summary>
        SessionIdNotFound = 76,

        /// <summary>SRC_SessionNoSessionInProgress</summary>
        SessionNoSessionInProgress = 77,

        /// <summary>SRC_VoucherRedeemed</summary>
        VoucherRedeemed = 78,

        /// <summary>SRC_VoucherExpired</summary>
        VoucherExpired = 79,

        /// <summary>SRC_LauncherServiceAlreadyRegistered</summary>
        LauncherServiceAlreadyRegistered = 82,

        /// <summary>SRC_LauncherServiceNotRegistered</summary>
        LauncherServiceNotRegistered = 83,

        /// <summary>SRC_VLTServiceAlreadyRegistered</summary>
        VltServiceAlreadyRegistered = 84,

        /// <summary>SRC_VLTServiceNotRegistered</summary>
        VltServiceNotRegistered = 85,

        /// <summary>SRC_DeviceStillRegisteredWithVLTSvc</summary>
        DeviceStillRegisteredWithVltSvc = 86,

        /// <summary>SRC_DeviceStillRegisteredWithLauncherSvc</summary>
        DeviceStillRegisteredWithLauncherSvc = 87,

        /// <summary>SRC_KnownRegistration</summary>
        KnownRegistration = 88,

        /// <summary>SRC_NoGameRegistered</summary>
        NoGameRegistered = 89,

        /// <summary>SRC_NotReadyToPlay</summary>
        NotReadyToPlay = 90,

        /// <summary>SRC_ServerError</summary>
        ServerError = 91,

        /// <summary>SRC_AllGamesExceedMaxBet</summary>
        AllGamesExceedMaxBet = 92,

        /// <summary>SRC_GameNotFound</summary>
        GameNotFound = 93,

        /// <summary>SRC_MaxNumberOfAttributesRegistered</summary>
        MaxNumberOfAttributesRegistered = 94,

        /// <summary>SRC_NotAllRequiredAttributesRegistered</summary>
        NotAllRequiredAttributesRegistered = 95,

        /// <summary>SRC_PlayCommandNotSent</summary>
        PlayCommandNotSent = 96,

        /// <summary>SRC_DenominationAlreadyRegistered</summary>
        DenominationAlreadyRegistered = 97,

        /// <summary>SRC_VoucherFromDifferentSite</summary>
        VoucherFromDifferentSite = 98,

        /// <summary>SRC_VoucherCanceled</summary>
        VoucherCanceled = 99,

        /// <summary>SRC_VoucherIsUnissued</summary>
        VoucherIsUnissued = 100,

        /// <summary>SRC_VoucherIsLimited</summary>
        VoucherIsLimited = 101,

        /// <summary>SRC_CreditFailedSessionBalanceLimit</summary>
        CreditFailedSessionBalanceLimit = 102,

        /// <summary>SRC_DoNotPrintVoucher</summary>
        DoNotPrintVoucher = 103,

        /// <summary>SRC_InsufficientFunds</summary>
        InsufficientFunds = 104,

        /// <summary>SRC_InvalidTicketCostValue</summary>
        InvalidTicketCostValue = 105,

        /// <summary>SRC_IsOfflineVoucher</summary>
        IsOfflineVoucher = 106,

        /// <summary>SRC_SessionNoPreviousSession</summary>
        SessionNoPreviousSession = 107,

        /// <summary>SRC_UnknownCardString</summary>
        UnknownCardString = 108,

        /// <summary>SRC_UnknownGroupName</summary>
        UnknownGroupName = 109,

        /// <summary>SRC_CardNotAssigned</summary>
        CardNotAssigned = 110,

        /// <summary>SRC_UnknownEmployee</summary>
        UnknownEmployee = 111,

        /// <summary>SRC_NotificationIDNotRegistered</summary>
        NotificationIdNotRegistered = 112,

        /// <summary>SRC_ExceedsMaxBet</summary>
        ExceedsMaxBet = 114,

        /// <summary>SRC_NotAllRequiredNotificationsRegistered</summary>
        NotAllRequiredNotificationsRegistered = 115,

        /// <summary>SRC_NotAllRequiredCommandsRegistered</summary>
        NotAllRequiredCommandsRegistered = 116,

        /// <summary>SRC_NoApplicationsRegistered</summary>
        NoApplicationsRegistered = 117,

        /// <summary>SRC_MustPlayExistingSession</summary>
        MustPlayExistingSession = 118,

        /// <summary>SRC_SessionEndedVoucherPrintedOffLine</summary>
        SessionEndedVoucherPrintedOffLine = 119,

        /// <summary>SRC_OfflineVoucherBarcodeMismatch</summary>
        OfflineVoucherBarcodeMismatch = 120,

        /// <summary>SRC_CardPinMismatch</summary>
        CardPinMismatch = 121,

        /// <summary>SRC_InvalidAllowedValues</summary>
        InvalidAllowedValues = 122,

        /// <summary>SRC_VoucherCouponRestricted</summary>
        VoucherCouponRestricted = 123,

        /// <summary>SRC_PlayerTrackingLoginPending</summary>
        PlayerTrackingLoginPending = 131,

        /// <summary>SRC_ActionInvalidGUID</summary>
        ActionInvalidGuid = 132,

        /// <summary>SRC_ActionNotAuthorized</summary>
        ActionNotAuthorized = 133,

        /// <summary>SRC_ActionInvalidName</summary>
        ActionInvalidName = 134,

        /// <summary>SRC_ActionAlreadyRegistered</summary>
        ActionAlreadyRegistered = 135,

        /// <summary>SRC_ActionNotAssigned</summary>
        ActionNotAssigned = 137,

        /// <summary>SRC_AccountUnknownAccount</summary>
        AccountUnknownAccount = 145,

        /// <summary>SRC_AccountUnlocked</summary>
        AccountUnlocked = 146,

        /// <summary>SRC_InvalidICD</summary>
        InvalidIcd = 147,

        /// <summary>SRC_XADFNoXADFFileForDevice</summary>
        NoXadfFileForDevice = 156,

        /// <summary>SRC_XADFInvalidDeviceName</summary>
        XadfInvalidDeviceName = 157,

        /// <summary>SRC_XADFInvalidManufacturerName</summary>
        XadfInvalidManufacturerName = 158
    }
}
