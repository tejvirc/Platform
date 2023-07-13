namespace Aristocrat.Monaco.TestController.DataModel
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Runtime.Serialization;

    [Flags]
    [DataContract]
    public enum PlatformStateEnum : uint
    {
        [EnumMember] Unknown = 0,
        [EnumMember] InLobby = 1,
        [EnumMember] GameLoaded = 2,
        [EnumMember] GameIdle = 4,
        [EnumMember] GamePlaying = 8,
        [EnumMember] InRecovery = 16,
        [EnumMember] InAudit = 32,
        [EnumMember] SystemDisabled = 64,
        [EnumMember] GamePlayDisabled = 128,
        [EnumMember] InAttractMode = 256
    }

    [DataContract]
    public enum PlatformInfoEnum
    {
        [EnumMember] State,
        [EnumMember] PlayerBalance,
        [EnumMember] Detailed,
        [EnumMember] Printer,
        [EnumMember] NoteAcceptor,
        [EnumMember] Os,
        [EnumMember] Io,
        [EnumMember] Display,
        [EnumMember] Id,
        [EnumMember] Network,
        [EnumMember] Jurisdiction,
        [EnumMember] Protocol,
        [EnumMember] GameInfo,
        [EnumMember] CurrentLockups,
        [EnumMember] TowerLightState,
        [EnumMember] Meters,
        [EnumMember] IsRobotModeRunning,
        [EnumMember] ProcessMetrics
    }

    
    [DataContract]
    public enum ConfigOptionInfo
    {
        [EnumMember] EftAftTransferLimit,
        [EnumMember] EftAftTransferInMode,          //AftInEnabled
        [EnumMember] BillAcceptorDriver,
        [EnumMember] PrinterDriver,
        [EnumMember] VoucherInLimit,
        [EnumMember] VoucherOutLimit,
        [EnumMember] PrintPromoTickets,
        [EnumMember] ValidationType,                //SasValidationType
        [EnumMember] EftAftTransferOutMode,         //AftOutEnabled
        [EnumMember] AftBonusEnabled,
        [EnumMember] AftPartialTransferEnabled,
        [EnumMember] AftWinAmountToHostEnabled,
        [EnumMember] LegacyBonusEnabled,
        [EnumMember] HandpayReportingType,
        [EnumMember] DisableOnCommunicationsLost,
        [EnumMember] SerialNumber,
        [EnumMember] MachineId,
        [EnumMember] Protocol,
        [EnumMember] SasDualHost,
        [EnumMember] SasHost1Address,               //SasAddressHost1
        [EnumMember] SasHost2Address,               //SasAddressHost2
        [EnumMember] SasValidationHost,
        [EnumMember] SasAftHost,
        [EnumMember] SasGeneralControlHost,
        [EnumMember] SasProgressiveHost,
        [EnumMember] GameDenomValidation,
        [EnumMember] G2SHostUri,
        [EnumMember] ZoneId,                        //Zone
        [EnumMember] Bank,
        [EnumMember] Position,
        [EnumMember] Location,
        [EnumMember] Jurisdiction,
        [EnumMember] CreditLimit,
        [EnumMember] LargeWinLimit,
        [EnumMember] HandpayLimit,
        [EnumMember] PrintHandpayReceipt,
        [EnumMember] HostCashoutAction,
        [EnumMember] MaxBetLimit
    }


    [DataContract]
    public enum WaitStatus
    {
        [EnumMember] WaitUnknown,
        [EnumMember] WaitPending,
        [EnumMember] WaitMet,
        [EnumMember] WaitTimedOut
    }

    [DataContract]
    public enum WaitEventEnum
    {
        [EnumMember] LobbyLoaded,
        [EnumMember] GameLoaded,
        [EnumMember] SpinStart,
        [EnumMember] SpinComplete,
        [EnumMember] GameSelected,
        [EnumMember] GameIdle,
        [EnumMember] GameExited,
        [EnumMember] RecoveryStarted,
        [EnumMember] RecoveryComplete,
        [EnumMember] ResponsibleGamingDialogVisible,
        [EnumMember] ResponsibleGamingDialogInvisible,
        [EnumMember] OperatorMenuEntered,
        [EnumMember] GamePlayDisabled,
        [EnumMember] SystemDisabled,
        [EnumMember] InvalidEvent,
        [EnumMember] IdPresented,
        [EnumMember] IdCleared,
        [EnumMember] IdReadError,
        [EnumMember] IdReaderTimeout,
        [EnumMember] IdValid,
        [EnumMember] IdInvalid,
        [EnumMember] IdNull
    }

    [DataContract]
    public enum LockupTypeEnum
    {
        [EnumMember] MainDoor,
        [EnumMember] BellyDoor,
        [EnumMember] CashDoor,
        [EnumMember] Stacker,
        [EnumMember] SecondaryCashDoor,
        [EnumMember] LogicDoor,
        [EnumMember] DropDoor,
        [EnumMember] TopBox,
        [EnumMember] Legitimacy
    }

    [DataContract]
    public enum TransferOutType
    {
        [EnumMember] CashOut,
        [EnumMember] LargeWin,
        [EnumMember] BonusPay,
        [EnumMember] ProgressiveWin
    }

    [DataContract]
    public enum Account
    {
        [EnumMember] Cashable,
        [EnumMember] Promo,
        [EnumMember] NonCash
    }

    [DataContract]
    public enum RuntimeMode
    {
        [EnumMember] Regular,
        [EnumMember] Recovery,
        [EnumMember] Replay,
        [EnumMember] Combination
    }

    public enum SharedInput
    {
        ReserveKey,
        CollectKey,
        MaxLineKey,
        PlayKey,
        Linex1Key,
        Linex2Key,
        Linex3Key,
        Linex4Key,
        Linex5Key,
        Betx1Key,
        Betx2Key,
        Betx3Key,
        Betx4Key,
        Betx5Key,
        Betx6Key,
        Betx7Key,
        Betx8Key,
        Betx9Key,
        Betx10Key,
        JackpotKey,
        AuditKey,
        OperatorKey3,
        OperatorKey4,
        OperatorKey5,
        LogicDoor,
        TopBoxDoor,
        MeterDoor,
        CashboxDoor,
        MainDoor,
        NoteDoor,
        BellyDoor
    }
}